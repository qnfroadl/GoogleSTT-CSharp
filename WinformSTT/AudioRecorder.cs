// https://www.youtube.com/watch?v=jHrIi71pGvs&list=LL&index=1


using NAudio.Mixer;
using NAudio.Wave;

namespace WaktaverseSTT
{
    public enum RecordingState
    {
        Stopped,
        Monitoring,
        Recording,
        RequestedStop
    }
    public class AudioRecorder
    {
        WaveIn waveIn;
       
        RecordingState recordingState;
        WaveFormat recordingFormat;

        UnsignedMixerControl volumeControl;
        double desiredVolume = 100;

        private SampleAggregator sampleAggregator;
        public event EventHandler Stopped = delegate { };

        public AudioRecorder()
        {
            sampleAggregator = new SampleAggregator();
            RecordingFormat = new WaveFormat(16000, 1);
        }

        public WaveFormat RecordingFormat
        {
            get
            {
                return recordingFormat;
            }
            set
            {
                recordingFormat = value;
                sampleAggregator.NotificationCount = value.SampleRate / 10;
            }
        }

        public void BeginMonitoring(int recordingDevice)
        {
            if (RecordingState.Stopped != recordingState/* && RecordingState.RequestedStop != recordingState*/)
            {
                throw new InvalidOperationException("Can't begin monitoring while we are in this state: " + recordingState.ToString());
            }

            waveIn = new WaveIn();
            waveIn.DeviceNumber = recordingDevice;
            waveIn.DataAvailable += OnDataAvailable;
            waveIn.RecordingStopped += OnRecordingStopped;
            waveIn.WaveFormat = recordingFormat;
            waveIn.StartRecording();
            TryGetVolumeControl();
            recordingState = RecordingState.Monitoring;
        }

        void OnRecordingStopped(object sender, StoppedEventArgs e)
        {
            recordingState = RecordingState.Stopped;
            Stopped(this, EventArgs.Empty);
        }

        public void Stop()
        {
            if (RecordingState.Recording == recordingState || RecordingState.Monitoring == recordingState)
            {
                recordingState = RecordingState.RequestedStop;
                waveIn.StopRecording();
            }
        }

        private void TryGetVolumeControl()
        {
            int waveInDeviceNumber = waveIn.DeviceNumber;
            if (Environment.OSVersion.Version.Major >= 6) // Vista and over
            {
                var mixerLine = waveIn.GetMixerLine();
                //new MixerLine((IntPtr)waveInDeviceNumber, 0, MixerFlags.WaveIn);
                foreach (var control in mixerLine.Controls)
                {
                    if (control.ControlType == MixerControlType.Volume)
                    {
                        this.volumeControl = control as UnsignedMixerControl;
                        MicrophoneLevel = desiredVolume;
                        break;
                    }
                }
            }
            else
            {
                var mixer = new Mixer(waveInDeviceNumber);
                foreach (var destination in mixer.Destinations
                    .Where(d => d.ComponentType == MixerLineComponentType.DestinationWaveIn))
                {
                    foreach (var source in destination.Sources
                        .Where(source => source.ComponentType == MixerLineComponentType.SourceMicrophone))
                    {
                        foreach (var control in source.Controls
                            .Where(control => control.ControlType == MixerControlType.Volume))
                        {
                            volumeControl = control as UnsignedMixerControl;
                            MicrophoneLevel = desiredVolume;
                            break;
                        }
                    }
                }
            }

        }

        public double MicrophoneLevel
        {
            get
            {
                return desiredVolume;
            }
            set
            {
                desiredVolume = value;
                if (volumeControl != null)
                {
                    volumeControl.Percent = value;
                }
            }
        }

        public SampleAggregator SampleAggregator
        {
            get
            {
                return sampleAggregator;
            }
        }

        public RecordingState RecordingState
        {
            get
            {
                return recordingState;
            }
        }

        void OnDataAvailable(object sender, WaveInEventArgs e)
        {
            byte[] buffer = e.Buffer;
            int bytesRecorded = e.BytesRecorded;
            // WriteToFile(buffer, bytesRecorded);

            for (int index = 0; index < e.BytesRecorded; index += 2)
            {
                short sample = (short)((buffer[index + 1] << 8) |
                                        buffer[index + 0]);
                float sample32 = sample / 32768f;
                sampleAggregator.Add(sample32);
            }
        }

    }
}
