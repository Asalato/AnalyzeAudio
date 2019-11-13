using System;
using System.Collections.Generic;
using System.Linq;

namespace AnalyzeAudio
{
    public static class DetectSongs
    {
        private static List<Song> _songs;

        // (TargetSong, ScoreIndex), (NumberOfArrival, WrongCount)
        private static readonly List<SongCandidate> _candidates = new List<SongCandidate>();
        private const int WrongCapacity = 3;

        private static Song? foundSong;
        
        public static void Initialize(List<Song> songs)
        {
            _songs = songs;
            foundSong = null;
        }

        public static void NoteStreamLoader(NoteOctave noteOctave)
        {
            for (var i = 0; i < _candidates.Count; ++i)
            {
                var candidate = _candidates[i];
                var currentNoteOctave = candidate.song.score[candidate.scoreIndex][candidate.numberOfArrival];
                if (currentNoteOctave.note == noteOctave.note && currentNoteOctave.octave == noteOctave.octave)
                    continue;
                var targetNoteOctave = candidate.song.score[candidate.scoreIndex][candidate.numberOfArrival + 1];
                if (targetNoteOctave.note == noteOctave.note && targetNoteOctave.octave == noteOctave.octave)
                {
                    ++candidate.numberOfArrival;
                    if (candidate.song.score[candidate.scoreIndex].Length - 1 == candidate.numberOfArrival)
                    {
                        foundSong = candidate.song;
                        _candidates.RemoveAt(i);
                        Console.WriteLine($"Find and Remove from candidates: {candidate.song.name} of {candidate.scoreIndex}");
                    }
                    else
                    {
                        candidate.wrongCount = 0;
                        _candidates[i] = candidate;
                        Console.WriteLine(
                            $"Add number of arrival: {candidate.song.name} of {candidate.scoreIndex}, {candidate.numberOfArrival}/{candidate.song.score[candidate.scoreIndex].Length - 1}");
                    }
                }
                else
                {
                    ++candidate.wrongCount;
                    if (candidate.wrongCount >= WrongCapacity)
                    {
                        _candidates.RemoveAt(i);
                        Console.WriteLine($"Not found and Remove from candidates: {candidate.song.name} of {candidate.scoreIndex}");
                    }
                    else
                    {
                        _candidates[i] = candidate;
                        Console.WriteLine($"Add number of wrong: {candidate.song.name} of {candidate.scoreIndex}, {candidate.wrongCount}/{WrongCapacity}");
                    }
                }
            }

            foreach (var song in _songs)
            {
                for (var i = 0; i < song.score.Length; ++i)
                {
                    var targetNoteOctave = song.score[i][0];
                    if (_candidates.Any(item => item.song.name == song.name && item.scoreIndex == i)) continue;
                    if (targetNoteOctave.note == noteOctave.note && targetNoteOctave.octave == noteOctave.octave)
                    {
                        _candidates.Add(new SongCandidate()
                            {numberOfArrival = 0, scoreIndex = i, song = song, wrongCount = 0});
                        Console.WriteLine($"Add to candidates: {song.name} of {i}");
                    }
                }
            }
        }

        public static Song? FindSong()
        {
            if (foundSong == null)
            {
                return null;
            }
            else
            {
                var song = foundSong;
                foundSong = null;
                return song;
            }
        }

        private struct SongCandidate
        {
            public Song song;
            public int scoreIndex;
            public int numberOfArrival;
            public int wrongCount;
        }
    }
}