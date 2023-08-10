using Google.Cloud.Speech.V1;
using Google.Protobuf;
using NAudio.Midi;
using NAudio.Wave;
using static Google.Cloud.Speech.V1.SpeechClient;


namespace STTConsole // Note: actual namespace depends on the project name.
{
    internal class Program
    {

        static void Main(string[] args)
        {
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "C:\\Users\\qnfro\\License\\waktaversestt-195be6d5847e.json");
           // Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "./waktaversestt-195be6d5847e.json");

            BufferedWaveProvider waveBuffer;
            WaveInEvent waveInEvent = new WaveInEvent
            {
                DeviceNumber = 0,   // default 0
                WaveFormat = new WaveFormat(16000, 1),
            };

            waveBuffer = new BufferedWaveProvider(waveInEvent.WaveFormat)
            {
                BufferLength = waveInEvent.WaveFormat.AverageBytesPerSecond * 5,
                //BufferLength = waveInEvent.WaveFormat.AverageBytesPerSecond * 60,   // 버퍼 크기 중가
                DiscardOnBufferOverflow = true,
            };
            
            // Google Cloud STT 설정
            SpeechClient speech = SpeechClient.Create();
            StreamingRecognizeStream streamingCall = speech.StreamingRecognize();
            
            // 마이크 데이터를 버퍼에 저장하고, 버퍼에 쌓을때마다 STT요청.
            int count = 0;
            int availableLength = 0;
            waveInEvent.DataAvailable += (sender, e) =>
            {
                count++;
                availableLength += e.Buffer.Length;

                waveBuffer.AddSamples(e.Buffer, 0, e.Buffer.Length);
                
                //if(count == 0)
                //{
                //    Console.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff"));
                //}
                if(0 <= count)  //0.1초에 한번씩 DataAbailable이 호출되고있다...
                {
                    //Console.WriteLine(DateTime.Now.ToString("HH:mm:ss.fff"));
                    // 오디오 버퍼를 스트리밍으로 전송합니다.
                    byte[] waveData = new byte[availableLength];
                    waveBuffer.Read(waveData, 0, availableLength);
                    waveBuffer.ClearBuffer();

                    streamingCall.WriteAsync(new StreamingRecognizeRequest()
                    {
                        AudioContent = ByteString.CopyFrom(waveData, 0, waveData.Length)
                    });

                    count = 0;
                    availableLength = 0;
                }

            };


            while(true)
            {
                Console.WriteLine("시작하려면 아무키나 입력하세요");
                Console.ReadKey();

                streamingCall = speech.StreamingRecognize();
                streamingCall.WriteAsync(new StreamingRecognizeRequest()
                {
                    StreamingConfig = new StreamingRecognitionConfig()
                    {
                        Config = new RecognitionConfig()
                        {
                            Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
                            SampleRateHertz = 16000,
                            LanguageCode = LanguageCodes.Korean.SouthKorea, // 언어 코드
                            Model = "latest_long"//"latest_long",
                        },

                        InterimResults = true,
                        // SingleUtterance = false,    // WriteAsync(reqeust) : response = 1:1 을 원할때만 true로 설정.
                    }
                });

                waveInEvent.StartRecording();

                // 인식 결과 수신 (비동기 읽기)
                Task printResponses = Task.Run(async () =>
                {
                    while (await streamingCall.GetResponseStream().MoveNextAsync(default))
                    {
                        foreach (var result in streamingCall.GetResponseStream().Current.Results)
                        {
                            Console.WriteLine($"뭐지이건: {result.Alternatives} {result.IsFinal} {result.Stability}  {result.ResultEndTime} {result.ChannelTag} {result.LanguageCode}");
                            if(0.5f <= result.Stability || result.IsFinal)
                            {
                                foreach (var alternative in result.Alternatives)
                                {
                                    //Console.Clear();
                                    Console.WriteLine($"인식 결과: {alternative.Transcript}");

                                }
                            }
                        }
                    }
                    Console.WriteLine("구글 응답받기 종료");
                });

                Console.WriteLine("종료하려면 아무키나 입력하세요");
                Console.ReadKey();

                waveInEvent.StopRecording();

                // 구글 응답받기 종료. TTS요청 완료.
                streamingCall.WriteCompleteAsync();

            }
            
        }
        
    }
}
