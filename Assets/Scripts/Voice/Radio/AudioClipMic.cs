using System;
using System.Collections;
using UnityEngine;

namespace Voice.Radio
{
    public class AudioClipMic : MonoBehaviour
    {
        [SerializeField] private AudioClip audioClip;

        [HideInInspector] public int frequency; // The frequency of the audio clip
        [HideInInspector] public int bufferLengthMS; // The length of the sample buffer

        private AudioSource audioSource; // Manages playing the audioClip
        private float[] sampleBuffer; // Last populated audio sample
        private int sampleCount; // Current number of samples taken

        // TODO: I think this currently only works with mono audio
        public int Channels => audioSource.clip.channels;

        // Invoked everytime an audio frame is collected. Includes the frame.
        public event Action<int, float[]> OnSampleReady;


        private void Awake()
        {
            Debug.Log("AudioClipMic: Loading AudioSource.");
            audioSource = new GameObject("RadioAudioSource").AddComponent<AudioSource>();
            
            audioSource.mute = false;
            audioSource.bypassEffects = true;
            audioSource.bypassListenerEffects = true;
            audioSource.bypassReverbZones = true;
            audioSource.playOnAwake = false;
            audioSource.loop = true;
            audioSource.volume = 0;
            
            audioSource.clip = audioClip;
            audioSource.clip.LoadAudioData();
        }

        private void Start()
        {
            Debug.Log("AudioClipMic: Starting AudioSource playback.");
            audioSource.Play();
        }

        public void StartRecording(int sampleDurationMS = 100)
        {
            frequency = audioSource.clip.frequency;
            bufferLengthMS = sampleDurationMS;
            sampleBuffer = new float[frequency / 1000 * bufferLengthMS * Channels];

            Debug.Log("AudioClipMic: Starting recording with " + frequency + "hz and " + bufferLengthMS + "ms buffer (" + sampleBuffer.Length + "samples per buffer with " + Channels + " channel(s)).");
            StartCoroutine(ReadRawAudio());
        }

        public void StopRecording()
        {
            Debug.Log("AudioClipMic: Stopping recording.");
            StopCoroutine(ReadRawAudio());
        }


        private IEnumerator ReadRawAudio()
        {
            int loops = 0; // The number of clip wrap-arounds
            int readAbsPos = 0;
            int prevPos = 0;
            float[] temp = new float[sampleBuffer.Length];

            while (audioSource.isPlaying)
            {
                bool isNewDataAvailable = true;

                while (isNewDataAvailable) // Send packets until each full played sample is sent
                {
                    int currPos = audioSource.timeSamples; // Current playing position
                    if (currPos < prevPos)
                    {
                        loops++; // We finished the clip and started from the beginning
                    }
                    prevPos = currPos;

                    var currAbsPos = loops * audioSource.clip.samples + currPos; // The total sample we're at, including clip wrap-arounds
                    var nextReadAbsPos = readAbsPos + temp.Length; // The next position where a sample should be taken

                    if (nextReadAbsPos < currAbsPos) // Take a sample only if a full sample has been played
                    {
                        audioSource.clip.GetData(temp, readAbsPos % audioSource.clip.samples); // Take a sample, starting from the current readPos

                        sampleBuffer = temp;
                        sampleCount++; // Sequence number

                        // Debug.Log("AudioclipMic: Sample ready.");
                        OnSampleReady?.Invoke(sampleCount, sampleBuffer);

                        readAbsPos = nextReadAbsPos;
                        isNewDataAvailable = true; // There might be another full sample waiting
                    }
                    else
                    {
                        isNewDataAvailable = false; // Wait until a full sample has been played
                    }
                }
                yield return null;
            }
        }
    }
}