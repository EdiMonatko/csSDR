using iio;
using SDR.Contracts;

namespace SDR
{
    public class AdalmPluto936X : IAdalmPluto936X
    {

        #region Fields

        private Context _context = null!;

        private Device _device = null!;
        private Device _deviceTx = null!;
        private Device _deviceRx = null!;

        public string _ipAddress { get; private set; }
        public bool isInitialized = false;


        #endregion

        #region Properties

        public double TxFrequency { get; set; }
        public double TxSampleRate { get; set; }
        public double TxBandwidth { get; set; }
        public double TxGain { get; set; }

        public double RxFrequency { get; set; }
        public double RxSampleRate { get; set; }
        public double RxBandwidth { get; set; }
        public double RxGain { get; set; }

        public double ChannelSample { get; set; }
        public string GainControlMode { get; set; }
        
        #endregion

        #region Constructors

        public AdalmPluto936X(string ipAddress)
        {
            #region Init with default values

            TxFrequency = 1e9;
            TxSampleRate = 3e6;
            TxBandwidth = 5e6;
            TxGain = -40;

            RxFrequency = 1e9;
            RxSampleRate = 3e6;
            RxBandwidth = 5e6;
            RxGain = 25;

            ChannelSample = 1e6;
            GainControlMode = "manual";

            #endregion
            _ipAddress = ipAddress;
        }

        #endregion

        #region Methods

        public bool Init()
        {
            Context ctx = new Context(_ipAddress);

            if (ctx is not null)
            {
                _context = ctx;
                isInitialized = true;

                _device = ctx.find_device("ad9361-phy");
                _deviceTx = ctx.find_device("cf-ad9361-dds-core-lpc");
                _deviceRx = ctx.find_device("cf-ad9361-lpc");
            }


            return isInitialized;
        }
        public bool Dispose()
        {
            if (!isInitialized)
                return false;

            _device = null!;
            _deviceTx = null!;
            _deviceRx = null!;

            _context.Dispose();
            isInitialized = false;

            return true;
        }

        #region Tx

        private Channel SetTx()
        {
            try
            {
                return _device.find_channel("voltage0", true);
            }
            catch (Exception)
            {
                return null!;
            }
        }
        public bool SetTxFrequency(double frequency)
        {
            if (!isInitialized)
                return false;

            TxFrequency = frequency;

            try
            {
                _device.find_channel("TX_LO", true).find_attribute("frequency").write(TxFrequency);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public bool SetTxSampleRate(double sampleRate)
        {
            if (!isInitialized)
                return false;

            TxSampleRate = sampleRate;

            try
            {
                SetTx().find_attribute("sampling_frequency").write(TxSampleRate);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public bool SetTxBandwidth(double bandwidth)
        {
            if (!isInitialized)
                return false;

            TxBandwidth = bandwidth;
            try
            {
                SetTx().find_attribute("rf_bandwidth").write(TxBandwidth);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public bool SetTxGain(double gain)
        {
            if (!isInitialized)
                return false;

            TxGain = gain;
            try
            {
                SetTx().find_attribute("hardwaregain").write(TxGain);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public bool SetTxDds(double frequency0 = 1e5,
            double frequency2 = 1e5,
            double raw0 = 1, 
            double raw2 = 1, 
            double scale0 = 1,
            double scale2 = 1,
            double phase0 = 90e3,
            double phase2 = 0)
        {
            if (!isInitialized)
                return false;

            try
            {
                var dds0 = _deviceTx.find_channel("TX1_I_F1", true);
                var dds2 = _deviceTx.find_channel("TX1_Q_F1", true);

                dds0.find_attribute("raw").write(raw0);
                dds0.find_attribute("frequency").write(frequency0);
                dds0.find_attribute("scale").write(scale0);
                dds0.find_attribute("phase").write(phase0);

                dds2.find_attribute("raw").write(raw2);
                dds2.find_attribute("frequency").write(frequency2);
                dds2.find_attribute("scale").write(scale2);
                dds2.find_attribute("phase").write(phase2);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public bool EnableTx(byte[] iqBytes, bool cirtular = true)
        {
            if (!isInitialized)
                return false;

            try
            {
                _deviceTx.find_channel("voltage0", true).enable();
                _deviceTx.find_channel("voltage1", true).enable();
                var txBuf = new IOBuffer(_deviceTx, (uint)ChannelSample, cirtular);
                txBuf.fill(iqBytes);
                txBuf.push();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public bool PlutoTxOn(double signalFrequency = 1e5, 
            double dds = 1e5,
            double raw0 = 1,
            double raw2 = 1,
            double scale0 = 1,
            double scale2 = 1,
            double phase0 = 90e3,
            double phase2 = 0)
        {
            if (!isInitialized)
                return false;

            var results = new List<(string, bool)>();
            bool status = false;

            status = SetTxFrequency(TxFrequency);
            results.Add((nameof(SetTxFrequency), status));

            status = SetTxSampleRate(TxSampleRate);
            results.Add((nameof(SetTxSampleRate), status));

            status = SetTxBandwidth(TxBandwidth);
            results.Add((nameof(SetTxBandwidth), status));

            status = SetTxGain(TxGain);
            results.Add((nameof(SetTxGain), status));

            status = SetTxDds(frequency0: dds,
                frequency2: dds,
                raw0,
                raw2,
                scale0,
                scale2,
                phase0,
                phase2
                );
            results.Add((nameof(SetTxDds), status));

            var iqBytes = generate_sine(ChannelSample, TxSampleRate, signalFrequency);

            //status = EnableRx();
            //results.Add((nameof(EnableRx), status));

            status = EnableTx(iqBytes);
            results.Add((nameof(EnableTx), status));

            foreach (var item in results)
            {
                if (item.Item2 is false)
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region Rx

        private Channel SetRx()
        {
            try
            {
                return _device.find_channel("voltage0", false);
            }
            catch (Exception)
            {
                return null!;
            }
        }
        public bool SetRxFrequency(double frequency)
        {
            if (!isInitialized)
                return false;

            RxFrequency = frequency;

            try
            {
                _device.find_channel("RX_LO", true).find_attribute("frequency").write(RxFrequency);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public bool SetRxSampleRate(double sampleRate)
        {
            if (!isInitialized)
                return false;

            RxSampleRate = sampleRate;

            try
            {
                SetRx().find_attribute("sampling_frequency").write(RxSampleRate);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public bool SetRxBandwidth(double bandwidth)
        {
            if (!isInitialized)
                return false;
            RxBandwidth = bandwidth;

            try
            {
                SetRx().find_attribute("rf_bandwidth").write(RxBandwidth);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public bool SetRxGain(double gain, string gainControl = "manual")
        {
            if (!isInitialized)
                return false;

            RxGain = gain;

            try
            {
                SetRx().find_attribute("gain_control_mode").write(gainControl);

                if (string.Equals(gainControl, "manual"))
                {
                    SetRx().find_attribute("hardwaregain").write(RxGain);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public bool EnableRx()
        {
            if (!isInitialized)
                return false;

            _deviceRx.find_channel("voltage0", false).enable();
            _deviceRx.find_channel("voltage1", false).enable();

            return true;
        }
        public (double[] reals, double[] imags) ReadRx(double bufferLength, bool isCircular = false)
        {
            List<double> reals = new();
            List<double> imags = new();
            byte[] data = new byte[Convert.ToInt32(ChannelSample)];

            try
            {
                var rxBuf = new IOBuffer(_deviceRx, (uint)ChannelSample, isCircular);

                rxBuf.refill();
                rxBuf.read(data);
            }
            catch (Exception)
            {
                return (reals.ToArray(), imags.ToArray());
            }

            short[] data_casted = new short[data.Length / 2];
            Buffer.BlockCopy(data, 0, data_casted, 0, data.Length);
            for (int idx = 0; idx < data_casted.Length; idx += 2)
            {
                reals.Add(data_casted[idx]);
                imags.Add(data_casted[idx + 1]);
            }

            return (reals.ToArray(), imags.ToArray());
        }
        public (double[] reals, double[] imags) PlutoRxOn()
        {
            //if (!isInitialized)
            //    return false;

            var results = new List<(string, bool)>();
            bool status = false;

            status = SetRxFrequency(RxFrequency);
            results.Add((nameof(SetRxFrequency), status));

            status = SetRxSampleRate(RxSampleRate);
            results.Add((nameof(SetRxSampleRate), status));

            status = SetRxBandwidth(RxBandwidth);
            results.Add((nameof(SetRxBandwidth), status));

            status = SetRxGain(TxGain);
            results.Add((nameof(SetRxGain), status));

            status = EnableRx();
            results.Add((nameof(EnableRx), status));

            foreach (var item in results)
            {
                if (item.Item2 is false)
                {
                    return (new double[0], new double[0]);
                }
            }

            return ReadRx(ChannelSample);
        }

        #endregion

        #region Common

        public bool SetSampleRate(double sampleRate)
        {
            ChannelSample = sampleRate;

            SetTxSampleRate(ChannelSample);
            SetRxSampleRate(ChannelSample);

            return true;
        }

        #endregion

        #region Helpers

        public byte[] generate_sine(double samples_per_channel, double rx_fs, double fc)
        {
            double ts = 1.0 / rx_fs;
            double[] t = Enumerable.Range(0, Convert.ToInt32(samples_per_channel)).Select(i => i * ts).ToArray();

            double[] i = t.Select(x => (Math.Sin(2 * Math.PI * x * fc) * Math.Pow(2,14))).ToArray();
            double[] q = t.Select(x => (Math.Cos(2 * Math.PI * x * fc) * Math.Pow(2,14))).ToArray();

            int[] iq = new int[i.Length + q.Length];
            for (int j = 0; j < i.Length; j++)
            {
                iq[j * 2] = Convert.ToInt16(i[j]);
                iq[j * 2 + 1] = Convert.ToInt16(q[j]);
            }

            byte[] iqBytes = iq.SelectMany(BitConverter.GetBytes).ToArray();



            return iqBytes;
        }


        #endregion

        #endregion
    }
}
