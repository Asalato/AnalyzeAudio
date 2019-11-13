using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using NAudio.Wave;

namespace AnalyzeAudio
{
    class Program
    {
        private static readonly int SamplingRate = 44100;
        private static readonly int BufferSize = (int) Math.Pow(2, 13);

        private static BufferedWaveProvider bwp;

        private static bool keyPressed = false;
        private static object lockObj = new object();

        private static string DefaultSongAddress = @"Songs.txt";

        static void Main(string[] args)
        {
            var songs = new List<Song>();
            songs.AddRange(DeserializeSongs.LoadSongs(args));
            songs.AddRange(DeserializeSongs.LoadSongs(DefaultSongAddress));
            DetectSongs.Initialize(songs);

            var sb = new StringBuilder();
            songs.Select(item => sb.Append($"{item.name}, ")).ToArray();
            Console.WriteLine($"{songs.Count} songs loaded: {sb.ToString()}");
            
            var deviceCount = WaveInEvent.DeviceCount;
            if(deviceCount == 0)
            {
                Console.WriteLine("No microphone device found.");
                return;
            }

            for (int i = 0; i < deviceCount; i++)
            {
                var deviceInfo = WaveInEvent.GetCapabilities(i);
                Console.WriteLine("Device {0}: {1}, {2} channels",
                    i, deviceInfo.ProductName, deviceInfo.Channels);
            }
            
            Console.WriteLine($"Select device from {0} to {deviceCount - 1}");

            int index;
            while(!int.TryParse(Console.ReadLine(), out index))
            {
                Console.WriteLine($"Select device from {0} to {deviceCount - 1}");
            }

            Console.WriteLine("Press escape to finish process.");
            
            Freq2Note.Init();
            var waveIn = new WaveInEvent() {DeviceNumber = index};
            waveIn.DataAvailable += AudioDataAvailable;
            waveIn.WaveFormat = new WaveFormat(SamplingRate, 1);
            waveIn.BufferMilliseconds = (int) ((double) BufferSize / (double) SamplingRate * 1000.0);

            bwp = new BufferedWaveProvider(waveIn.WaveFormat)
            {
                BufferLength = BufferSize * 2, DiscardOnBufferOverflow = true
            };
            
            try
            {
                waveIn.StartRecording();
            }
            catch
            {
                string msg = "Could not record from audio device!\n\n";
                msg += "Is your microphone plugged in?\n";
                msg += "Is it set as your default recording device?";
                Console.WriteLine(msg, "ERROR");
            }

            ThreadPool.QueueUserWorkItem(EscapeLoad, null);
            while (true)
            {
                lock (lockObj)
                {
                    if (keyPressed) break;
                }
                
                var note = Process();
                if (note != null)
                {
                    DetectSongs.NoteStreamLoader(note.Value);
                }

                var song = DetectSongs.FindSong();
                if (song != null)
                {
                    Console.WriteLine($"Song detect: {song.Value.name}");
                }
            }
            
            waveIn.StopRecording();
            waveIn.Dispose();
        }
        
        private static void EscapeLoad(object userState)
        {
            var keyInfo = Console.ReadKey(true);
            if (keyInfo.Key == ConsoleKey.Escape)
            {
                lock (lockObj)
                {
                    keyPressed = true;
                }
            }
        }
        
        private static void AudioDataAvailable(object sender, WaveInEventArgs e)
        {
            bwp.AddSamples(e.Buffer, 0, e.BytesRecorded);
        }

        private static NoteOctave? Process()
        {
            // check the incoming microphone audio
            int frameSize = BufferSize;
            var audioBytes = new byte[frameSize];
            bwp.Read(audioBytes, 0, frameSize);

            // return if there's nothing new to plot
            if (audioBytes.Length == 0)
                return null;
            if (audioBytes[frameSize - 2] == 0)
                return null;

            // incoming data is 16-bit (2 bytes per audio point)
            int BYTES_PER_POINT = 2;

            // create a (32-bit) int array ready to fill with the 16-bit data
            int graphPointCount = audioBytes.Length / BYTES_PER_POINT;

            // create double arrays to hold the data we will graph
            double[] pcm = new double[graphPointCount];
            double[] fft = new double[graphPointCount];
            double[] fftReal = new double[graphPointCount/2];
            
            // populate Xs and Ys with double data
            for (int i = 0; i < graphPointCount; i++)
            {
                // read the int16 from the two bytes
                Int16 val = BitConverter.ToInt16(audioBytes, i * 2);

                // store the value in Ys as a percent (+/- 100% = 200%)
                pcm[i] = (double)(val) / Math.Pow(2,16) * 200.0;
            }

            // calculate the full FFT
            fft = FourierProcess.FFT(pcm);
            
            // determine horizontal axis units for graphs
            double fftMaxFreq = SamplingRate / 2;
            double fftPointSpacingHz = fftMaxFreq / graphPointCount;
            
            // just keep the real half (the other half imaginary)
            Array.Copy(fft, fftReal, fftReal.Length);
            
            /*var sb = new StringBuilder();
            fftReal.Select(item => sb.Append($"{item.ToString("F2")}, ")).ToArray();
            Console.WriteLine(sb.ToString());*/

            var peekFrequency = FourierProcess.FindMaxPeekIndex(fftReal) * fftPointSpacingHz;
            var noteAccuracy = Freq2Note.GetNote(peekFrequency);

            if (noteAccuracy.accuracy > 0.5)
            {
                Console.WriteLine($"Frequency: {peekFrequency:F2}Hz, Note: {noteAccuracy.note}{noteAccuracy.octave}, Accuracy: {noteAccuracy.accuracy * 100:F0}%");
                return new NoteOctave() {note = noteAccuracy.note, octave = noteAccuracy.octave};
            }

            return null;
        }
    }
}
