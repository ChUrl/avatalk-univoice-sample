using Adrenak.UniVoice;
using System;
using UnityEngine;

namespace Voice.Radio
{
    public class AudioClipInput : IAudioInput
    {
        private readonly AudioClipMic mic;

        public event Action<int, float[]> OnSegmentReady;

        public int Frequency => mic.frequency;

        public int ChannelCount => mic.Channels;

        public int SegmentRate => 1000 / mic.bufferLengthMS;

        public AudioClipInput(AudioClipMic audioClipMic, int sampleLen = 100)
        {
            mic = audioClipMic;
            mic.OnSampleReady += Mic_OnSampleReady;

            Debug.unityLogger.Log("AudioClipInput: Starting recording.");
            mic.StartRecording(sampleLen);
        }

        private void Mic_OnSampleReady(int segmentIndex, float[] samples)
        {
            OnSegmentReady?.Invoke(segmentIndex, samples);
        }

        public void Dispose()
        {
            mic.OnSampleReady -= Mic_OnSampleReady;
            mic.StopRecording();
        }
    }
}
