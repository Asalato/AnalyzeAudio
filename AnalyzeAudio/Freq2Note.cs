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
            
            var targetNoteIndex = leftDiff > rightDiff ? rightNoteIndex : rightNoteIndex - 1;
            
            return new NoteAccuracy
            {
                note = (Note) (targetNoteIndex % 12),
                octave = targetNoteIndex / 12 + 1,
                accuracy = (freq - midFreq) / (noteBaseFreqs[targetNoteIndex] - midFreq)
            };
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

    public static class NoteUtility
    {
        public static Note StringToNote(string str)
        {
            if (str == "C")
                return Note.C;
            else if (str == "Cs")
                return Note.Cs;
            else if (str == "D")
                return Note.D;
            else if (str == "Ds")
                return Note.Ds;
            else if (str == "E")
                return Note.E;
            else if (str == "F")
                return Note.F;
            else if (str == "Fs")
                return Note.Fs;
            else if (str == "G")
                return Note.G;
            else if (str == "Gs")
                return Note.Gs;
            else if (str == "A")
                return Note.A;
            else if (str == "As")
                return Note.As;
            else if (str == "B")
                return Note.B;
            else throw new KeyNotFoundException();
        }

        public static bool IsNote(string str)
        {
            try
            {
                StringToNote(str);
                return true;
            }
            catch (KeyNotFoundException e)
            {
                return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }

    public struct NoteOctave
    {
        public Note note;
        public int octave;
    }
}