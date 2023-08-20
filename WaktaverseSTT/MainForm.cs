using Google.Cloud.Speech.V1;
using Google.Protobuf;
using NAudio.Wave;
using static Google.Cloud.Speech.V1.PhraseSet.Types;
using static Google.Cloud.Speech.V1.SpeechClient;

namespace WaktaverseSTT
{
    public partial class MainForm : Form
    {
        const string ttsRunning = "�ǽð� TTS ������";

        bool isMonitoring = false;

        WaveIn waveIn;
        BufferedWaveProvider waveBuffer;

        SpeechClient speech;
        StreamingRecognizeStream streamingRecognize;
        TextToKey ttk;

        int resultNum = 0;
        public MainForm()
        {
            InitializeComponent();

            ttk = new TextToKey();
            ttk.Init("./ttk.info");

            // Set GoogleAPI Key
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "./waktaversestt-195be6d5847e.json");

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
            // ����� ���۸� ��Ʈ�������� ����
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

                // SpeechContext�� �ܾ� ����Ʈ�� �߰�
                //SpeechContext speechContext = new SpeechContext();
                //speechContext.Phrases.AddRange(new string[] { "���", "�ٳ���", "ũ��ÿ�", "�����縮�Ƹ�����"});
                //speechContext.Boost = 20;

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
                            // SpeechContexts = { speechContext },
                        },

                        InterimResults = true,
                        //SingleUtterance = true,
                    }
                };

                streamingRecognize.WriteAsync(request);

                Task printResponses = Task.Run(async () =>
                {
                    // �ߺ��� ����� response�ϰ� �ֱ� ������ ��Ī�Ǵ� ���ڿ��� �ѹ� Ȯ�εǸ� �׵ڷδ� Ȯ������ �ʵ���
                    bool bSendKey = false;
                    int key;
                    while (await streamingRecognize.GetResponseStream().MoveNextAsync(default))
                    {
                        foreach (var result in streamingRecognize.GetResponseStream().Current.Results)
                        {
                            // begin test
                            //foreach (var alternative in result.Alternatives)
                            //{
                            //    listBox1.Invoke((MethodInvoker)delegate
                            //    {
                            //        listBox1.Items.Add(result.Stability.ToString() + ", " + result.IsFinal + ", " + alternative.Transcript.TrimStart());
                            //    });
                            //}
                            //end test


                            if (0.5f <= result.Stability || result.IsFinal)
                            {
                                foreach (var alternative in result.Alternatives)
                                {
                                    if (false == bSendKey)
                                    {
                                        key = ttk.GetKey(alternative.Transcript.Replace(" ", ""));
                                        if (0 != key)
                                        {
                                            bSendKey = true;
                                            InputKeyboard.SendKeyPress((InputKeyboard.KeyCode)key);
                                            this.statusLabel1.Text = ttsRunning + ", SendKey: 0x" + key.ToString("x");
                                        }
                                    }

                                    listBox1.Invoke((MethodInvoker)delegate
                                    {
                                        if (listBox1.Items.Count == resultNum)
                                        {
                                            listBox1.Items.Add("");
                                        }
                                        listBox1.Items[resultNum] = alternative.Transcript.TrimStart();
                                        listBox1.TopIndex = listBox1.Items.Count - 1;
                                    });
                                }
                            }

                            if (result.IsFinal)
                            {
                                bSendKey = false;
                                resultNum++;
                            }

                        }
                    }

                    this.statusLabel1.Text = "���� ���� ����";
                });

                // ����͸� �����ϱ�
                waveIn.StartRecording();
                this.statusLabel1.Text = ttsRunning;
                this.micButton.Text = "Monitoring Stop";
                this.isMonitoring = true;
                this.fileSTTTestButton.Enabled = false;

            }
            else
            {
                // �����ϱ�.
                waveIn.StopRecording();
                streamingRecognize.WriteCompleteAsync();
                this.micButton.Text = "Monitoring Start";
                this.isMonitoring = false;
                this.fileSTTTestButton.Enabled = true;

            }
        }
    }
}