using System;
using System.Collections.Generic;
using System.Text;
using NAudio.Wave;

namespace LanaTextToSpeech.Sherpa
{
    internal class FloatArraySampleProvider : ISampleProvider
    {
        private readonly float[] _samples;
        private int _position;

        public WaveFormat WaveFormat { get; }

        public FloatArraySampleProvider(float[] samples, int sampleRate)
        {
            _samples = samples;
            // Sherpa-ONNX outputs mono (1 channel) 32-bit float audio
            WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 1);
        }

        public int Read(Span<float> buffer)
        {
            // Calculate how many samples we have left
            int samplesAvailable = _samples.Length - _position;

            // We can only copy as much as the buffer can hold, or as much as we have left
            int samplesToCopy = Math.Min(samplesAvailable, buffer.Length);

            if (samplesToCopy > 0)
            {
                // Slice our source array to get the exact chunk we need
                Span<float> sourceChunk = _samples.AsSpan(_position, samplesToCopy);

                // Slice the destination buffer to match the chunk size and copy it over
                sourceChunk.CopyTo(buffer.Slice(0, samplesToCopy));

                _position += samplesToCopy;
            }

            // Return the number of samples we actually provided
            return samplesToCopy;
        }
    }
}