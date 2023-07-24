
// https://www.youtube.com/watch?v=jHrIi71pGvs&list=LL&index=1

using Google.Cloud.Speech.V1;
using NAudio.Wave;
using NAudio.Mixer;

using System.Runtime.InteropServices;
using System;

namespace WaktaverseSTT
{
    public partial class Form1 : Form
    {
        private List<string> devices = new List<string>();
        private AudioRecorder audioRecorder = new AudioRecorder();
        private bool monitoring = false;

        private WaveInEvent waveIn = new NAudio.Wave.WaveInEvent();
        private BufferedWaveProvider waveBuffer;

        private bool sendReady = false;
        private int peakCount = 0;
        private int mutePeakCount = 0;
        public Form1()
        {
            InitializeComponent();

            if (NAudio.Wave.WaveIn.DeviceCount < 1)
            {
                MessageBox.Show("No microphone! ... exiting");
                return;
            }

            // Set authKey
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "C:\\Users\\qnfro\\Workspace\\waktaversestt-195be6d5847e.json");

            //Mixer
            //Hook Up Audio Mic for sound peak detection
            audioRecorder.SampleAggregator.MaximumCalculated += OnRecorderMaximumCalculated;

            for (int n = 0; n < WaveIn.DeviceCount; n++)
            {
                devices.Add(WaveIn.GetCapabilities(n).ProductName);
            }

            //Set up NAudio waveIn object and events
            waveIn.DeviceNumber = 0;        // 디바이스 선택하는 기능 추가해야함.
            waveIn.WaveFormat = new NAudio.Wave.WaveFormat(16000, 1);
            //Need to catch this event to fill our audio beffer up
            waveIn.DataAvailable += WaveIn_DataAvailable;
            //the actuall wave buffer we will be sending to googles for voice to text conv
            waveBuffer = new BufferedWaveProvider(waveIn.WaveFormat);
            waveBuffer.DiscardOnBufferOverflow = true;

        }
        void OnRecorderMaximumCalculated(object sender, MaxSampleEventArgs e)
        {
            float peak = Math.Max(e.MaxSample, Math.Abs(e.MinSample));

            // multiply by 100 because the Progress bar's default maximum value is 100
            peak *= 100;
            progressBar1.Value = (int)peak;
            // textBox1.AppendText("Recording Level " + peak.ToString() + "\r\n");
            peakCount++;

            if (false == sendReady && peak > 5)
            {
                //Timer should not be enabled, meaning, we are not already recording
                sendReady = true;
                peakCount = 0;
                mutePeakCount = 0;
                waveIn.StartRecording();

            }
            else if(peak <= 5)
            {
                mutePeakCount++;
            }
            
            
            if(sendReady && 3 < mutePeakCount)
            {
                sendReady = false;
                mutePeakCount = 0;
                peakCount = 0;
                waveIn.StopRecording();

                //writeFile Test.

                Task send = StreamBufferToGoogleAsync();

            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (devices.Count > 0)
            {
                if (monitoring == false)
                {
                    monitoring = true;
                    //Begin
                    audioRecorder.BeginMonitoring(0);
                    button1.Text = "Streaming Stop";
                }
                else
                {
                    monitoring = false;
                    audioRecorder.Stop();
                    button1.Text = "Streaming Start";

                }
            }
        }

        private void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            waveBuffer.AddSamples(e.Buffer, 0, e.BytesRecorded);
        }

        private async Task<object> StreamBufferToGoogleAsync()
        {
            byte[] buffer = new byte[waveBuffer.BufferLength];
            int offset = 0;
            int count = waveBuffer.BufferLength;

            waveBuffer.Read(buffer, offset, count);
            waveBuffer.ClearBuffer();

            var speech = SpeechClient.Create();
            var streamingCall = speech.StreamingRecognize();

            await streamingCall.WriteAsync(new StreamingRecognizeRequest()
            {
                StreamingConfig = new StreamingRecognitionConfig()
                {
                    Config = new RecognitionConfig()
                    {
                        Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
                        SampleRateHertz = 16000,
                        LanguageCode = "ko-KR",
                        Model = "latest_short",  //latest_long, latest_short, command_and_search, phone_call, video
                    },

                    //Note: play with this value
                    // InterimResults = true,  // this needs to be true for real time
                    SingleUtterance = true,
                }
            });

            try
            {
                //Sending to Googles .... finally
                streamingCall.WriteAsync(new StreamingRecognizeRequest()
                {
                    AudioContent = Google.Protobuf.ByteString.CopyFrom(buffer, 0, count)
                }).Wait();
            }
            catch (Exception wtf)
            {
                string wtfMessage = wtf.Message;
            }

            //Again, this is googles code example below, I tried unrolling this stuff
            //and the google api stopped working, so stays like this for now

            //Print responses as they arrive. Need to move this into a method for cleanslyness
            Task printResponses = Task.Run(async () =>
            {
                string saidWhat = "";
                string lastSaidWhat = "";
                while (await streamingCall.GetResponseStream().MoveNextAsync(default(CancellationToken)))
                {
                    foreach (var result in streamingCall.GetResponseStream().Current.Results)
                    {
                        foreach (var alternative in result.Alternatives)
                        {
                            saidWhat = alternative.Transcript;
                            if (lastSaidWhat != saidWhat)
                            {
                                //Console.WriteLine(saidWhat);
                                lastSaidWhat = saidWhat;
                                //Need to call this on UI thread ....
                                textBox1.Invoke((MethodInvoker)delegate { textBox1.AppendText(/*textBox1.Text + */ saidWhat + " \r\n"); });
                            }
                        }  // end for
                    } // end for
                }

                // textBox1.Invoke((MethodInvoker)delegate { textBox1.AppendText("End while \r\n"); });
            });

            

            //Tell googles we are done for now
            await streamingCall.WriteCompleteAsync();

            return 0;
        }

    }
}