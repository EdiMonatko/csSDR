using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using MathNet.Numerics.Statistics;
using System.Numerics;

namespace SDR
{
    public static class CalculateIQData
    {

        public static Dictionary<double, double> CreateSpectrum(double[] iData,
                                                                double[] qData,
                                                                double offset = 0,
                                                                double scale = 2047,
                                                                double sampling = 61.38e6,
                                                                string mode = "ps")
        {
            var results = new Dictionary<double, double>();
            var samples = new Complex[iData.Length];
            var window = Array.Empty<double>();
            var sum = 0d;

            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] = new Complex(iData[i] / scale / 2, qData[i] / scale / 2);
            }

            switch (mode)
            {
                case "ps":

                    window = Window.FlatTop(samples.Length);

                    break;

                case "psd": // rectangale window 

                    window = Generate.LinearSpaced(samples.Length, 1, 1);

                    break;
            }

            for (int i = 0; i < samples.Length; i++)
            {
                var w = new Complex(window[i], 0);
                var s = samples[i];
                var r = w * s;

                samples[i] = r;
            }

            Fourier.Forward(samples, FourierOptions.NoScaling);
            var fy = Fffshift(samples);
            var py = new double[samples.Length];

            for (int i = 0; i < fy.Length; i++)
            {
                var item = fy[i];
                py[i] = (item * item.Conjugate()).Real;
            }

            sum = window.Sum();

            switch (mode)
            {
                case "ps":

                    sum = window.Sum();

                    for (int i = 0; i < py.Length; i++)
                    {
                        py[i] = py[i] * 4 / Math.Pow(sum, 2);
                    }
                    break;

                case "psd":

                    sum = window.Sum(i => i = Math.Pow(i, 2));

                    for (int i = 0; i < py.Length; i++)
                    {
                        py[i] = py[i] * 4 / (sampling * sum);
                    }

                    break;
            }

            var freqs = Generate.LinearSpaced(py.Length, -sampling / 2, sampling / 2);

            for (int i = 0; i < freqs.Length; i++)
            {
                var freq = freqs[i];
                var specDbm = 10 * Math.Log10(py[i] + Double.Epsilon);

                results.Add(freq, specDbm + offset);
            }


            return results;
        }

        public static (double, double) PeakSearch(Dictionary<double, double> spectrum)
        {
            var max = double.MinValue;
            var freq = default(double);

            foreach (var pair in spectrum)
            {
                if (pair.Value > max)
                {
                    max = pair.Value;
                    freq = pair.Key;
                }
            }

            return (freq, max);
        }

        public static (double, double) GetSpecificAmplitude(Dictionary<double, double> spectrum, double freq)
        {
            var uutAmplitude = default(double);

            try
            {
                uutAmplitude = spectrum[freq];
            }
            catch (Exception)
            {
                var keys = spectrum.Keys;
            }

            return (freq, uutAmplitude);
        }

        public static Dictionary<double, double> ClearPeaks(Dictionary<double, double> spectrum,
                                                            double threshold,
                                                            double span = 0.001)
        {
            var peakAmp = double.MaxValue;
            var peakFreq = default(double);

            while (peakAmp >= threshold)
            {
                (peakFreq, peakAmp) = PeakSearch(spectrum);

                if (peakAmp >= threshold)
                {
                    spectrum = SliceSpectrum(spectrum: spectrum, centerFreq: peakFreq, span: span, isPeak: false);
                }
            }

            return spectrum;

        }

        public static Dictionary<double, double> SliceSpectrum(Dictionary<double, double> spectrum,
                                                               double centerFreq,
                                                               double span,
                                                               bool isPeak = true,
                                                               double florLevel = -200)
        {
            var slice = new Dictionary<double, double>();

            var startFreq = centerFreq - span / 2;
            var stopFreq = centerFreq + span / 2;

            var minFreq = spectrum.Keys.First();
            var maxFreq = spectrum.Keys.Last();

            if (startFreq < minFreq)
            {
                startFreq = minFreq;
            }

            if (stopFreq > maxFreq)
            {
                stopFreq = maxFreq;
            }

            if (isPeak)
            {
                foreach (var pair in spectrum)
                {
                    if (pair.Key >= startFreq && pair.Key <= stopFreq)
                    {
                        slice[pair.Key] = pair.Value;
                    }
                }
            }
            else
            {
                foreach (var pair in spectrum)
                {
                    if (pair.Key >= startFreq && pair.Key <= stopFreq)
                    {
                        slice[pair.Key] = florLevel;
                    }
                    else
                    {
                        slice[pair.Key] = pair.Value;
                    }
                }
            }

            return slice;
        }


        public static double NoiseFloorCalculation(Dictionary<double, double> spectrum,
                                                   double span,
                                                   int minimumBuckets = 200,
                                                   double ripplePersent = 0.05)
        {
            var rbw = spectrum.Keys.Skip(1).Take(1).First() - spectrum.Keys.First();
            var sliceSize = Math.Ceiling(span / rbw);
            var histogramBucketNum = (int)Math.Floor(sliceSize / 10);

            if (histogramBucketNum > minimumBuckets || histogramBucketNum <= 0)
            {
                histogramBucketNum = minimumBuckets;
            }

            var amplitudes = spectrum.Values;

            var histogram = new Histogram(amplitudes, histogramBucketNum);

            var diff = new List<double>();
            var prev = 0d;

            for (int i = 0; i < histogramBucketNum; i++)
            {
                var backet = histogram[i];
                var actual = backet.Count;

                if (i != 0)
                {
                    diff.Add(prev - actual);
                }

                prev = actual;
            }

            var ripple = diff.Max() * ripplePersent;
            var noiseInx = 0;

            for (int i = diff.Count - 1; i >= 0; i--)
            {
                var abs = Math.Abs(diff[i]);

                if (abs > ripple)
                {
                    noiseInx = i;

                    break;
                }
            }

            var noiseBucket = histogram[noiseInx];
            var noise = (noiseBucket.UpperBound + noiseBucket.LowerBound) / 2;

            return noise;
        }

        private static Complex[] Fffshift(Complex[] spectrum)
        {
            int n = spectrum.Length;
            int m = (int)Math.Floor((double)n / 2);

            Complex[] tmp = new Complex[n];

            Array.Copy(spectrum, m, tmp, 0, n - m);
            Array.Copy(spectrum, 0, tmp, n - m, m);
            Array.Copy(tmp, spectrum, n);

            return spectrum;
        }

    }
}
