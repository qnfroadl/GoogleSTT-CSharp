using Google.Cloud.Speech.V1;
using Google.Protobuf;
using NAudio.Wave;
using static Google.Cloud.Speech.V1.PhraseSet.Types;
using static Google.Cloud.Speech.V1.SpeechClient;

namespace WaktaverseSTT
{
    public partial class MainForm : Form
    {
        bool isMonitoring = false;

        WaveIn waveIn;
        BufferedWaveProvider waveBuffer;

        SpeechClient speech;
        StreamingRecognizeStream streamingRecognize;

        int resultNum = 0;
        public MainForm()
        {
            InitializeComponent();

            // Set GoogleAPI Key
            //Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "./waktaversestt-195be6d5847e.json");
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "C:\\Users\\qnfro\\License\\waktaversestt-195be6d5847e.json");

            volumeMeter1.Amplitude = 3f;
            openFileDialog1.Filter = "wav 파일|*.wav|모든 파일|*.*";

            waveIn = new WaveIn
            {
                DeviceNumber = 0,   // default 0
                WaveFormat = new WaveFormat(16000, 1),
            };
            waveIn.DataAvailable += WaveIn_DataAvailable;

            waveBuffer = new BufferedWaveProvider(waveIn.WaveFormat)
            {
                // BufferLength = waveIn.waveFormat.AverageBytesPerSecond * 5;
                BufferLength = waveIn.WaveFormat.AverageBytesPerSecond * 60,   // 버퍼 크기 중가
                DiscardOnBufferOverflow = true,
            };
        }

        private void fileOpenButton_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                // 선택된 파일 경로
                this.pathLabel.Text = openFileDialog1.FileName;
            }
        }


        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            //window 사이즈 변경시 컨트롤들 업데이트
            volumeMeter1.Invalidate();
        }

        private void fileSTTTestButton_Click(object sender, EventArgs e)
        {
            // 파일 읽어서. google speech에 요청까지.
            // 실시간 STT중이면 해당 기능 중단 시키고 진행해야함. (오류 나중에 고쳐)

            string path = this.pathLabel.Text;
            if (path.Length > 0)
            {
                WaveFileReader waveFileReader = new WaveFileReader(path);
                byte[]? buffer = null;
                if (0 < waveFileReader.Length)
                {
                    buffer = new byte[waveFileReader.Length];
                    waveFileReader.Read(buffer, 0, buffer.Length);
                    waveFileReader.Close();

                    Task request = RequestGoogleSTT(buffer);
                    PlayWaveFile(path);
                }
            }
        }

        async void PlayWaveFile(string filePath)
        {
            var audioFile = new AudioFileReader(filePath);
            var outputDevice = new WaveOutEvent();

            outputDevice.Init(audioFile);
            outputDevice.Play();
            while (outputDevice.PlaybackState == PlaybackState.Playing)
            {
                await Task.Delay(100);
            }
        }

        private void WaveIn_DataAvailable(object? sender, WaveInEventArgs e)
        {
            //Console.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff"));
            // 오디오 버퍼를 스트리밍으로 전송합니다.
            streamingRecognize.WriteAsync(new StreamingRecognizeRequest()
            {
                AudioContent = ByteString.CopyFrom(e.Buffer, 0, e.Buffer.Length)
            });
        }

        async Task RequestGoogleSTT(byte[] waveData)
        {
            speech = SpeechClient.Create();
            streamingRecognize = speech.StreamingRecognize();

            await streamingRecognize.WriteAsync(new StreamingRecognizeRequest()
            {
                StreamingConfig = new StreamingRecognitionConfig()
                {
                    Config = new RecognitionConfig()
                    {
                        Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
                        SampleRateHertz = 16000,
                        LanguageCode = LanguageCodes.Korean.SouthKorea, // 인식할 언어 코드를 설정합니다.
                        Model = "latest_long"//"latest_long",
                    },

                    // InterimResults = true,
                    // SingleUtterance = true,
                }
            });

            // 오디오 버퍼를 스트리밍으로 전송합니다.
            await streamingRecognize.WriteAsync(
                new StreamingRecognizeRequest()
                {
                    AudioContent = Google.Protobuf.ByteString.CopyFrom(waveData, 0, waveData.Length)
                }
            );

            // 인식 결과 수신 (비동기 읽기)
            Task printResponses = Task.Run(async () =>
            {
                while (await streamingRecognize.GetResponseStream().MoveNextAsync(default))
                {
                    foreach (var result in streamingRecognize.GetResponseStream().Current.Results)
                    {
                        foreach (var alternative in result.Alternatives)
                        {
                            this.statusLabel1.Text = alternative.Transcript.TrimStart();
                        }
                    }
                }
            });

            // 오디오 스트리밍 종료
            await streamingRecognize.WriteCompleteAsync();
        }

        private void micButton_Click(object sender, EventArgs e)
        {
            if (false == isMonitoring)
            {
                resultNum = 0;
                listBox1.Items.Clear();

                speech = SpeechClient.Create();
                streamingRecognize = speech.StreamingRecognize();

                // 인식하고자 하는 단어 리스트를 설정합니다.
               
                // SpeechContext에 단어 리스트를 추가합니다.
                SpeechContext speechContext = new SpeechContext();
                speechContext.Phrases.AddRange(new string[] { "사과", "바나나", "크루시오", "엑스펠리아르무스"});
                speechContext.Boost = 20;



                StreamingRecognizeRequest request = new StreamingRecognizeRequest()
                {
                    StreamingConfig = new StreamingRecognitionConfig()
                    {
                        Config = new RecognitionConfig()
                        {
                            Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
                            SampleRateHertz = 16000,
                            LanguageCode = LanguageCodes.Korean.SouthKorea, // 인식할 언어 코드를 설정합니다.
                            Model = "latest_long",//"latest_long",
                            SpeechContexts = { speechContext },
                        },

                        InterimResults = true,
                        //SingleUtterance = true,
                    }
                };

                streamingRecognize.WriteAsync(request);

                Task printResponses = Task.Run(async () =>
                {
                    while (await streamingRecognize.GetResponseStream().MoveNextAsync(default))
                    {
                        foreach (var result in streamingRecognize.GetResponseStream().Current.Results)
                        {
                            if (0.5f <= result.Stability || result.IsFinal)
                            {
                                foreach (var alternative in result.Alternatives)
                                {
                                    if (alternative.Transcript.Replace(" ", "").Contains("점프"))
                                    {
                                        // SendKeys.SendWait("t");
                                    }

                                    if (listBox1.Items.Count == resultNum)
                                    {
                                        listBox1.Invoke((MethodInvoker)delegate
                                        {
                                            listBox1.Items.Add(alternative.Transcript.TrimStart());
                                            listBox1.TopIndex = listBox1.Items.Count - 1;
                                        });
                                    }
                                    else
                                    {
                                        listBox1.Invoke((MethodInvoker)delegate
                                        {
                                            listBox1.Items[resultNum] = alternative.Transcript.TrimStart();
                                            listBox1.TopIndex = listBox1.Items.Count - 1;
                                        });
                                    }

                                    if (result.IsFinal)
                                    {
                                        //listBox1.Invoke((MethodInvoker)delegate
                                        //{
                                        //    listBox1.Items[resultNum] = alternative.Transcript.TrimStart();
                                        //});
                                        resultNum++;
                                    }
                                }
                            }
                        }
                    }

                    this.statusLabel1.Text = "구글 응답 종료";
                });

                // 모니터링 시작하기
                waveIn.StartRecording();
                this.statusLabel1.Text = "실시간 TTS 진행중";
                this.micButton.Text = "Monitoring Stop";
                this.isMonitoring = true;
            }
            else
            {
                // 중지하기.
                waveIn.StopRecording();
                streamingRecognize.WriteCompleteAsync();

                // 중단 성공시.(근데 이건 무조건이지) 
                this.micButton.Text = "Monitoring Start";
                this.isMonitoring = false;

            }
        }
    }
}