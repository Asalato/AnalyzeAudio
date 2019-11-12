using System;
using System.Collections.Generic;

namespace AnalyzeAudio
{
    public static class Freq2Note
    {
        private static float[] noteBaseFreqs;
        
        public static void Init()
        {
            var baseFreqs = new float[]
                {16.35f, 17.32f, 18.35f, 19.45f, 20.60f, 21.83f, 23.12f, 24.50f, 25.96f, 27.50f, 29.14f, 30.87f};
            var freqList = new List<float>();
            for (var i = 0; i < 10; ++i)
            {
                for (var j = 0; j < baseFreqs.Length; ++j)
                {
                    freqList.Add((float) (baseFreqs[j] * Math.Pow(2, i)));
                }
            }
            
            noteBaseFreqs = freqList.ToArray();
        }
        
        public static NoteAccuracy GetNote(double freq)
        {
            if (freq < noteBaseFreqs[0] || freq > noteBaseFreqs[noteBaseFreqs.Length - 1])
                return new NoteAccuracy() {accuracy = 0};
            
            var rightNoteIndex = -1;
            for (var i = 0; i < noteBaseFreqs.Length; ++i)
            {
                if (noteBaseFreqs[i] > freq)
                {
                    rightNoteIndex = i;
                    break;
                }
            }

            var leftDiff = freq - noteBaseFreqs[rightNoteIndex - 1];
            var rightDiff = noteBaseFreqs[rightNoteIndex] - freq;
            var midFreq = (noteBaseFreqs[rightNoteIndex] + noteBaseFreqs[rightNoteIndex - 1]) / 2;

            var noteAccuracy = new NoteAccuracy();
            if (leftDiff > rightDiff)
            {
                noteAccuracy.note = (Note) (rightNoteIndex % 12);
                noteAccuracy.octave = rightNoteIndex / 12 + 1;
                noteAccuracy.accuracy = (freq - midFreq) / (noteBaseFreqs[rightNoteIndex] - midFreq);
            }
            else
            {
                noteAccuracy.note = (Note) ((rightNoteIndex - 1) % 12);
                noteAccuracy.octave = (rightNoteIndex - 1) / 12 + 1;
                noteAccuracy.accuracy = (midFreq - freq) / (midFreq - noteBaseFreqs[rightNoteIndex - 1]);
            }

            return noteAccuracy;
        }

        public struct NoteAccuracy
        {
            public Note note;
            public int octave;
            public double accuracy;
        }
    }
    
    public enum Note
    {
        C = 0,
        Cs,
        D,
        Ds,
        E,
        F,
        Fs,
        G,
        Gs,
        A,
        As,
        B
    }
}