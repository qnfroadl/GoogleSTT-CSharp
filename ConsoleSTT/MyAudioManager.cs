using NAudio.Wave;

namespace STTConsole
{
    internal class MyAudioManager
    {
        private BufferedWaveProvider waveBuffer;
        private WaveInEvent waveInEvent;
        int valuableLength;

        public MyAudioManager()
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
        private void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            this.waveBuffer.AddSamples(e.Buffer, 0, e.BytesRecorded);
            valuableLength += e.BytesRecorded;
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
            valuableLength = 0;//67200 // 76800
            // this.waveBuffer.ClearBuffer();

            return ret;
        }

        public int GetValuableBufferLength()
        {
            return this.valuableLength;
        }

        public void WriteFile(string fileName, byte[] buffer, int length)
        {
            WaveFileWriter waveFileWriter = new WaveFileWriter(fileName, this.waveInEvent.WaveFormat);
            waveFileWriter.Write(buffer, 0, length);
            waveFileWriter.Dispose();
        }

        public byte[] ReadFile(string fileName)
        {
            WaveFileReader waveFileReader = new WaveFileReader(fileName);
            
            byte[] buffer = new byte[waveFileReader.Length];
            waveFileReader.Read(buffer, 0, buffer.Length);
            waveFileReader.Close();

            return buffer;
        }
    }
}
