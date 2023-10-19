using iio;
using ScottPlot;
using System.Numerics;

namespace SDR
{
    public class ExampleFromAnalog
    {
        public static void Main()
        {
            Context ctx = new Context("192.168.2.1");

            //Context ctx = new Context("10.100.102.108");

            if (ctx == null)
            {
                Console.WriteLine("Unable to create IIO context");
                return;
            }

            string TXLO = Convert.ToString(2e9);
            string TXBW = Convert.ToString(1e3);
            string TXFS = Convert.ToString(20e6);
            string RXLO = TXLO;
            string RXBW = TXBW;
            string RXFS = TXFS;
            //string gain_ctrl_mode = "slow_attack";
            string gain_ctrl_mode = "manual";

            string hw_gain = "-70";
            string rx_gain = "70";


            // get reference to devices
            var ctrl = ctx.find_device("ad9361-phy"); //configure transciever
            var tx_dac = ctx.find_device("cf-ad9361-dds-core-lpc");
            var rx_adc = ctx.find_device("cf-ad9361-lpc");

            tx_config(ctrl, TXLO, TXBW, TXFS, hw_gain);
            rx_config(ctrl, RXLO, RXBW, RXFS, gain_ctrl_mode, rx_gain);

            // Enable all IQ channels
            enable_tx(tx_dac);
            enable_rx(rx_adc);

            // Cyclic buffer to output perdiodic waveforms
            //int samples_per_channel = (int)Math.Pow(2, 14);
            var samples_per_channel = 1_000_000;

            var tx_buf = new IOBuffer(tx_dac, (uint)samples_per_channel, true);
            var rx_buff = new IOBuffer(rx_adc, (uint)samples_per_channel, false);

            //var iqBytes = generate_sine(samples_per_channel, RXFS, 10e3);
            var iqBytes = GenerateQPSK(samples_per_channel, 1000);
            //byte[] iqBytes = generate_qpsk(samples_per_channel, RXFS, 1e9);
            //byte[] iqBytes = generate_qam4(samples_per_channel, RXFS, 10000);
            // Send data to TX buffer
            tx_buf.fill(iqBytes);
            tx_buf.push();

            // read data on rx buffer
            (List<double> reals, List<double> imags) = read_rx_data(rx_buff, 10, samples_per_channel);

            var plt = new Plot(1920, 1080);
            //plt.PlotSignal(reals.ToArray(), samples_per_channel);
            //plt.PlotSignal(imags.ToArray(), samples_per_channel);

            var a = CalculateIQData.CreateSpectrum(reals.ToArray(), imags.ToArray(), sampling: samples_per_channel, mode:"psd");

            plt.PlotSignalXY(a.Keys.ToArray(), a.Values.ToArray(), label: "Spectrum");

            plt.SaveFig("spectrum.png");

            var showPlt = new FormsPlotViewer(plt);


            showPlt.Refresh();
            showPlt.ShowDialog();
        }

        static void rx_config(Device device, string rx_lo, string rx_bw, string rx_fs, string gain_ctrl_mode, string hardwareGain)
        {
            var rx_chn = device.find_channel("voltage0", false);

            device.find_channel("RX_LO", true).find_attribute("frequency").write(rx_lo);
            rx_chn.find_attribute("rf_bandwidth").write(rx_bw);
            rx_chn.find_attribute("sampling_frequency").write(rx_fs);
            rx_chn.find_attribute("gain_control_mode").write(gain_ctrl_mode);

            if (string.Equals(gain_ctrl_mode, "manual"))
            {
                rx_chn.find_attribute("hardwaregain").write(hardwareGain);
            }
        }

        static void tx_config(Device device, string tx_lo, string tx_bw, string tx_fs, string hw_gain)
        {
            var tx_chn = device.find_channel("voltage0", true);

            device.find_channel("TX_LO", true).find_attribute("frequency").write(tx_lo);
            tx_chn.find_attribute("rf_bandwidth").write(tx_bw);
            tx_chn.find_attribute("sampling_frequency").write(tx_fs);
            tx_chn.find_attribute("hardwaregain").write(hw_gain);
        }

        static void enable_tx(Device tx_adc)
        {
            tx_adc.find_channel("voltage0", true).enable();
            tx_adc.find_channel("voltage1", true).enable();
        }

        static void enable_rx(Device rx_dac)
        {
            rx_dac.find_channel("voltage0", false).enable();
            rx_dac.find_channel("voltage1", false).enable();
        }

        static byte[] generate_sine(int samples_per_channel, string rx_fs)
        {
            int fc = 10000;
            double ts = 1.0 / int.Parse(rx_fs);
            double[] t = Enumerable.Range(0, samples_per_channel).Select(i => i * ts).ToArray();

            double[] i = t.Select(x => (Math.Sin(2 * Math.PI * x * fc) * Math.Pow(2, 14))).ToArray();
            double[] q = t.Select(x => (Math.Cos(2 * Math.PI * x * fc) * Math.Pow(2, 14))).ToArray();

            int[] iq = new int[i.Length + q.Length];
            for (int j = 0; j < i.Length; j++)
            {
                iq[j * 2] = Convert.ToInt16(i[j]);
                iq[j * 2 + 1] = Convert.ToInt16(q[j]);
            }

            byte[] iqBytes = iq.SelectMany(BitConverter.GetBytes).ToArray();
            return iqBytes;
        }

        // create a function which returns a byte[] of IQ data for a given frequency
        static byte[] generate_sine(int samples_per_channel, string rx_fs, double fc)
        {
            double ts = 1.0 / int.Parse(rx_fs);
            double[] t = Enumerable.Range(0, samples_per_channel).Select(i => i * ts).ToArray();

            double[] i = t.Select(x => (Math.Sin(2 * Math.PI * x * fc) * 1)).ToArray();
            double[] q = t.Select(x => (Math.Cos(2 * Math.PI * x * fc) * 1)).ToArray();

            //plot i and q on the same graph
            //var plt = new ScottPlot.Plot(1920, 1080);
            //plt.PlotSignal(i, 20e6);
            //plt.PlotSignal(q, 20e6);
            //plt.SaveFig("iq.png");

            //var showPlt = new FormsPlotViewer(plt);


            //showPlt.Refresh();
            //showPlt.ShowDialog();

            int[] iq = new int[i.Length + q.Length];
            for (int j = 0; j < i.Length; j++)
            {
                iq[j * 2] = Convert.ToInt16(i[j]);
                iq[j * 2 + 1] = Convert.ToInt16(q[j]);
            }

            byte[] iqBytes = iq.SelectMany(BitConverter.GetBytes).ToArray();



            return iqBytes;
        }

        //create a function which returns a byte[] of IQ data with QAM 4 modulation
        static byte[] generate_qam4(int samples_per_channel, string rx_fs, double fc)
        {
            double ts = 1.0 / int.Parse(rx_fs);
            double[] t = Enumerable.Range(0, samples_per_channel).Select(i => i * ts).ToArray();

            double[] i = t.Select(x => (Math.Sin(2 * Math.PI * x * fc) * Math.Pow(2, 14))).ToArray();
            double[] q = t.Select(x => (Math.Cos(2 * Math.PI * x * fc) * Math.Pow(2, 14))).ToArray();

            int[] iq = new int[i.Length + q.Length];
            for (int j = 0; j < i.Length; j++)
            {
                iq[j * 2] = Convert.ToInt16(i[j]);
                iq[j * 2 + 1] = Convert.ToInt16(q[j]);
            }

            byte[] iqBytes = iq.SelectMany(BitConverter.GetBytes).ToArray();

            return iqBytes;
        }

        // Create transmit waveform (QPSK, 16 samples per symbol)
        static byte[] generate_qpsk(int samples_per_channel, string rx_fs, double fc, int numSymbols = 1000)
        {
            Random random = new Random();
            int waveformLength = numSymbols * samples_per_channel;
            byte[] waveform = new byte[waveformLength];

            double symbolRate = double.Parse(rx_fs); // Symbol rate
            double ts = 1.0 / symbolRate; // Symbol duration

            for (int i = 0; i < numSymbols; i++)
            {
                // Generate a random QPSK symbol index (0, 1, 2, or 3)
                int symbolIndex = random.Next(4);

                // Calculate the phase for the symbol (in radians)
                double phase = (2 * Math.PI * symbolIndex) / 4.0; // 4 symbols in QPSK

                for (int j = 0; j < samples_per_channel; j++)
                {
                    double t = j * ts;
                    double sample = Math.Cos(2 * Math.PI * fc * t + phase);

                    // Scale the sample to a byte value (assuming 8-bit samples)
                    waveform[i * samples_per_channel + j] = (byte)(sample * 127);
                }
            }

            return waveform;
        }

        public static byte[] GenerateQPSK(int samplesPerSymbol, int numSymbols = 1000)
        {
            Random random = new Random();
            Complex[] symbols = new Complex[numSymbols];
            double[] xRadians = new double[numSymbols];

            for (int i = 0; i < numSymbols; i++)
            {
                int xInt = random.Next(4); // Random integer between 0 and 3
                double xDegrees = xInt * 360.0 / 4.0 + 45.0; // Map to 45, 135, 225, 315 degrees
                xRadians[i] = xDegrees * Math.PI / 180.0; // Convert to radians
                symbols[i] = new Complex(Math.Cos(xRadians[i]), Math.Sin(xRadians[i]));
            }

            // Upsample by repeating symbols and scale
            Complex[] samples = new Complex[numSymbols * samplesPerSymbol];
            for (int i = 0; i < numSymbols; i++)
            {
                for (int j = 0; j < samplesPerSymbol; j++)
                {
                    samples[i * samplesPerSymbol + j] = symbols[i];
                }
            }

            // Scale the samples
            double scale = Math.Pow(2, 14);
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] *= scale;
            }

            // Convert complex samples to bytes (I/Q components)
            byte[] byteSamples = new byte[samples.Length * 2];
            for (int i = 0; i < samples.Length; i++)
            {
                byteSamples[i * 2] = (byte)(samples[i].Real);
                byteSamples[i * 2 + 1] = (byte)(samples[i].Imaginary);
            }

            return byteSamples;
        }


        static (List<double> reals, List<double> imags) read_rx_data(IOBuffer rx_buff, int num_readings, int samples_per_channel)
        {
            List<double> reals = new();
            List<double> imags = new();

            byte[] data = new byte[samples_per_channel];
            for (int k = 0; k < num_readings; k++)
            {
                rx_buff.refill();
                rx_buff.read(data);

                short[] data_casted = new short[data.Length / 2];
                Buffer.BlockCopy(data, 0, data_casted, 0, data.Length);
                for (int idx = 0; idx < data_casted.Length; idx += 2)
                {
                    reals.Add(data_casted[idx]);
                    imags.Add(data_casted[idx + 1]);
                }
            }
            return (reals, imags);
        }

        static void SetTxDds(Device tx_adc, double freq)
        {
            //tx_adc.find_buffer_attribute("altvoltage0").write($"{freq.ToString()}");
            tx_adc.find_buffer_attribute("frequency").write($"{freq.ToString()}");

            tx_adc.find_channel("altvoltage0", true).find_attribute("frequency").write($"{freq.ToString()}");
            tx_adc.find_channel("altvoltage1", true).find_attribute("frequency").write($"{freq.ToString()}");
        }
    }
}