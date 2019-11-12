namespace AnalyzeAudio
{
    public static class FourierProcess
    {
        private static double SilenceThreshold = 1;
        
        public static double[] FFT(double[] data)
        {
            double[] fft = new double[data.Length];
            System.Numerics.Complex[] fftComplex = new System.Numerics.Complex[data.Length];
            for (int i = 0; i < data.Length; i++)
                fftComplex[i] = new System.Numerics.Complex(data[i], 0.0);
            Accord.Math.FourierTransform.FFT(fftComplex, Accord.Math.FourierTransform.Direction.Forward);
            for (int i = 0; i < data.Length; i++)
                fft[i] = fftComplex[i].Magnitude;
            return fft;
        }
        
        public static int FindMaxPeekIndex(double[] data)
        {
            var maxPeekIndex = 0;
            var maxPeekValue = 0d;
            for (var i = 1; i < data.Length - 1; ++i)
            {
                var left = data[i] / data[i - 1];
                var right = data[i] / data[i + 1];
                if (left < 1 || right < 1) continue;
                var max = left > right ? left : right;
                if (max < maxPeekValue || data[i] < SilenceThreshold) continue;
                maxPeekValue = max;
                maxPeekIndex = i;
            }

            return maxPeekIndex;
        }
    }
}