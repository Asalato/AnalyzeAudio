using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AnalyzeAudio
{
    public static class DeserializeSongs
    {
        public static Song[] LoadSongs(string[] address)
        {
            if (address.Length != 0) return null;
            var songList = new List<Song>();
            foreach (var addr in address)
            {
                songList.AddRange(LoadSongs(addr));
            }
            return songList.ToArray();
        }
        
        public static Song[] LoadSongs(string address)
        {
            var songList = new List<Song>();
            using (var reader = new StreamReader(address, Encoding.UTF8))
            {
                var line = "";
                while (true)
                {
                    line = reader.ReadLine();
                    if (line == null) break;
                    var segments = line.Split(' ');
                    if (segments.Length < 2) continue;
                    var song = new Song {name = segments[0], score = new NoteOctave[segments.Length - 1][]};
                    for (var i = 1; i < segments.Length; i++)
                    {
                        var noteOctaveList = new List<NoteOctave>();
                        var noteOctave = new NoteOctave(){octave = -1};
                        var noteString = "";
                        for (var j = 0; j < segments[i].Length; j++)
                        {
                            if (segments[i].Length > j + 1 && segments[i][j + 1] == 's')
                                noteOctave.note = NoteUtility.StringToNote($"{segments[i][j]}s");
                            else if (NoteUtility.IsNote($"{segments[i][j]}"))
                                noteOctave.note = NoteUtility.StringToNote($"{segments[i][j]}");
                            else if (noteOctave.octave == -1 && int.TryParse($"{segments[i][j]}", out var octave))
                            {
                                noteOctave.octave = octave;
                                noteOctaveList.Add(noteOctave);
                                noteOctave = new NoteOctave() {octave = -1};
                            }
                        }
                        song.score[i - 1] = noteOctaveList.ToArray();
                    }
                    songList.Add(song);
                }
            }
            return songList.ToArray();
            try
            {
                
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }

    public struct Song
    {
        public string name;
        public NoteOctave[][] score;
    }
}