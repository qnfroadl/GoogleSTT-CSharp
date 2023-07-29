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
            openFileDialog1.Filter = "wav ����|*.wav|��� ����|*.*";

            waveIn = new WaveIn
            {
                DeviceNumber = 0,   // default 0
                WaveFormat = new WaveFormat(16000, 1),
            };
            waveIn.DataAvailable += WaveIn_DataAvailable;

            waveBuffer = new BufferedWaveProvider(waveIn.WaveFormat)
            {
                // BufferLength = waveIn.waveFormat.AverageBytesPerSecond * 5;
                BufferLength = waveIn.WaveFormat.AverageBytesPerSecond * 60,   // ���� ũ�� �߰�
                DiscardOnBufferOverflow = true,
            };
        }

        private void fileOpenButton_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                // ���õ� ���� ���
                this.pathLabel.Text = openFileDialog1.FileName;
            }
        }


        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            //window ������ ����� ��Ʈ�ѵ� ������Ʈ
            volumeMeter1.Invalidate();
        }

        private void fileSTTTestButton_Click(object sender, EventArgs e)
        {
            // ���� �о. google speech�� ��û����.
            // �ǽð� STT���̸� �ش� ��� �ߴ� ��Ű�� �����ؾ���. (���� ���߿� ����)

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
            // ����� ���۸� ��Ʈ�������� �����մϴ�.
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
                        LanguageCode = LanguageCodes.Korean.SouthKorea, // �ν��� ��� �ڵ带 �����մϴ�.
                        Model = "latest_long"//"latest_long",
                    },

                    // InterimResults = true,
                    // SingleUtterance = true,
                }
            });

            // ����� ���۸� ��Ʈ�������� �����մϴ�.
            await streamingRecognize.WriteAsync(
                new StreamingRecognizeRequest()
                {
                    AudioContent = Google.Protobuf.ByteString.CopyFrom(waveData, 0, waveData.Length)
                }
            );

            // �ν� ��� ���� (�񵿱� �б�)
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

            // ����� ��Ʈ���� ����
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

                // �ν��ϰ��� �ϴ� �ܾ� ����Ʈ�� �����մϴ�.
               
                // SpeechContext�� �ܾ� ����Ʈ�� �߰��մϴ�.
                SpeechContext speechContext = new SpeechContext();
                speechContext.Phrases.AddRange(new string[] { "���", "�ٳ���", "ũ��ÿ�", "�����縮�Ƹ�����"});
                speechContext.Boost = 20;



                StreamingRecognizeRequest request = new StreamingRecognizeRequest()
                {
                    StreamingConfig = new StreamingRecognitionConfig()
                    {
                        Config = new RecognitionConfig()
                        {
                            Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
                            SampleRateHertz = 16000,
                            LanguageCode = LanguageCodes.Korean.SouthKorea, // �ν��� ��� �ڵ带 �����մϴ�.
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
                                    if (alternative.Transcript.Replace(" ", "").Contains("����"))
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

                    this.statusLabel1.Text = "���� ���� ����";
                });

                // ����͸� �����ϱ�
                waveIn.StartRecording();
                this.statusLabel1.Text = "�ǽð� TTS ������";
                this.micButton.Text = "Monitoring Stop";
                this.isMonitoring = true;
            }
            else
            {
                // �����ϱ�.
                waveIn.StopRecording();
                streamingRecognize.WriteCompleteAsync();

                // �ߴ� ������.(�ٵ� �̰� ����������) 
                this.micButton.Text = "Monitoring Start";
                this.isMonitoring = false;

            }
        }
    }
}