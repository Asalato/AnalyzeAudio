using System.Collections.Generic;

namespace AnalyzeAudio
{
    public static class DetectSongs
    {
        private static List<Song> _songs;

        // (TargetSong, ScoreIndex), (NumberOfArrival, WrongCount)
        private static readonly List<SongCandidate> _candidates = new List<SongCandidate>();
        private const int WrongCapacity = 2;

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
                var targetNoteOctave = candidate.song.score[candidate.scoreIndex][candidate.numberOfArrival + 1];
                if (targetNoteOctave.note == noteOctave.note && targetNoteOctave.octave == noteOctave.octave)
                {
                    ++candidate.numberOfArrival;
                    if (candidate.song.score[candidate.scoreIndex].Length == candidate.numberOfArrival)
                    {
                        foundSong = candidate.song;
                        _candidates.RemoveAt(i);
                    }
                    else
                    {
                        _candidates[i] = candidate;
                    }
                }
                else
                {
                    ++candidate.wrongCount;
                    if (candidate.wrongCount >= WrongCapacity)
                    {
                        _candidates.RemoveAt(i);
                    }
                    else
                    {
                        _candidates[i] = candidate;
                    }
                }
            }

            foreach (var song in _songs)
            {
                for (var i = 0; i < song.score.Length; ++i)
                {
                    var targetNoteOctave = song.score[i][0];
                    if (targetNoteOctave.note == noteOctave.note && targetNoteOctave.octave == noteOctave.octave)
                        _candidates.Add(new SongCandidate()
                            {numberOfArrival = 0, scoreIndex = i, song = song, wrongCount = 0});
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
                foundSong = new Song();
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