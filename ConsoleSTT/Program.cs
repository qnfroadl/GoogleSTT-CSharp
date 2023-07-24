using Google.Cloud.Speech.V1;

namespace STTConsole // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        
        static async Task Main(string[] args)
        {
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "C:\\Users\\qnfro\\License\\waktaversestt-195be6d5847e.json");

            MyAudioManager audioReader = new MyAudioManager();

            // Google Cloud STT 설정
            var speech = SpeechClient.Create();
            
            // 오디오 스트리밍 시작
            audioReader.StartRecording();

            int count = 0;
            byte[] buffer;
            
            int fileNum = 0;
            while (true)
            {
                Console.WriteLine("말을 하고 아무키나 누르세요");
                Console.ReadKey();

                count = audioReader.GetValuableBufferLength();
                buffer = new byte[count];

                fileNum++;

                audioReader.ReadRecording(buffer,0, count);
                audioReader.WriteFile("text" + fileNum + ".wav", buffer, buffer.Length);
                buffer = audioReader.ReadFile("text" + fileNum + ".wav");

                var streamingCall = speech.StreamingRecognize();
               
                await streamingCall.WriteAsync(new StreamingRecognizeRequest()
                {
                    StreamingConfig = new StreamingRecognitionConfig()
                    {
                        Config = new RecognitionConfig()
                        {
                            Encoding = RecognitionConfig.Types.AudioEncoding.Linear16,
                            SampleRateHertz = 16000,
                            LanguageCode = LanguageCodes.Korean.SouthKorea, // 인식할 언어 코드를 설정합니다.
                            Model = "latest_short"//"latest_long",
                        },

                         //InterimResults = true,
                        SingleUtterance = true,
                    }
                });
                //RecognitionAudio.FromBytes()
                // 오디오 버퍼를 스트리밍으로 전송합니다.
                await streamingCall.WriteAsync(
                    new StreamingRecognizeRequest()
                    {
                        AudioContent = Google.Protobuf.ByteString.CopyFrom(buffer, 0, buffer.Length)
                    }
                );

                // 인식 결과 수신 (비동기 읽기)
                Task printResponses = Task.Run(async () =>
                {
                    while (await streamingCall.GetResponseStream().MoveNextAsync(default))
                    {
                        foreach (var result in streamingCall.GetResponseStream().Current.Results)
                        {
                            foreach (var alternative in result.Alternatives)
                            {
                                Console.WriteLine($"인식 결과: {alternative.Transcript}");
                            }
                        }
                    }
                    //Console.WriteLine("구글 응답받기 종료");
                });

                // 오디오 스트리밍 종료
                await streamingCall.WriteCompleteAsync();
            }
        }
    }
}
