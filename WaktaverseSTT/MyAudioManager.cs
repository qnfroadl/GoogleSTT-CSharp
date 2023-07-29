using NAudio.Wave;
using System.Diagnostics;

namespace WaktaverseSTT
{
    internal class MyAudioManager
    {
        public BufferedWaveProvider waveBuffer;
        public WaveInEvent waveInEvent;
        public event EventHandler<SentenceEventArgs> SendSentense;
        
        // for test
        int count = 0;
        internal MyAudioManager()
        {
            waveInEvent = new WaveInEvent
            {
                DeviceNumber = 0,   // default 0
                WaveFormat = new WaveFormat(16000, 1),
            };

            waveInEvent.DataAvailable += WaveIn_DataAvailable;
            
            waveBuffer = new BufferedWaveProvider(waveInEvent.WaveFormat)
            {
                // BufferLength = waveInEvent.waveFormat.AverageBytesPerSecond * 5;
                BufferLength = waveInEvent.WaveFormat.AverageBytesPerSecond * 60,   // 버퍼 크기 중가
                DiscardOnBufferOverflow = true,
            };

            
        }
        private void WaveIn_DataAvailable(object? sender, WaveInEventArgs e)
        {
            // for test
            count++;

            this.waveBuffer.AddSamples(e.Buffer, 0, e.BytesRecorded);


            if(100 < count)
            {
                // mute
                SendSentense(this, new SentenceEventArgs());
            }
        }


        public void StartRecording()
        {
            this.waveInEvent.StartRecording();
        }

        public void StopRecording()
        {
            this.waveInEvent.StopRecording();
        }

        public int ReadRecording(byte[] buffer, int offset, int count)
        {
            int ret = this.waveBuffer.Read(buffer, offset, count);
            // this.waveBuffer.ClearBuffer();

            return ret;
        }

        public void WriteFile(string fileName, byte[] buffer, int length)
        {
            WaveFileWriter waveFileWriter = new WaveFileWriter(fileName, this.waveInEvent.WaveFormat);
            waveFileWriter.Write(buffer, 0, length);
            waveFileWriter.Dispose();
        }

        public byte[]? ReadWave(string fileName)
        {
            WaveFileReader waveFileReader = new WaveFileReader(fileName);
            byte[]? buffer = null;
            if (0 < waveFileReader.Length)
            {
                buffer = new byte[waveFileReader.Length];
                waveFileReader.Read(buffer, 0, buffer.Length);
                waveFileReader.Close();
            }

            return buffer;
        }

        public byte[]? ReadMp3(string filePath)
        {
            Mp3FileReader reader = new Mp3FileReader(filePath);
            byte[]? buffer = null;
            if (0 < reader.Length)
            {
                buffer = new byte[reader.Length];
                reader.Read(buffer, 0, buffer.Length);
                reader.Close();
            }

            return buffer;
        }
    }


    public class SentenceEventArgs : EventArgs
    {
        [DebuggerStepThrough]
        public SentenceEventArgs()
        {
           
        }
    }
}
