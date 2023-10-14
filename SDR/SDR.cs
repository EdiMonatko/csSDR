using iio;
using IIOCSharp;
using System.Numerics;

namespace SDR

{
    public class SDR
    {
        public Context _ctx;
        public Device _sdr;

        public uint _bufferSize = 2048;

        private Device _txDevice;
        private Device _rxDevice;

        private const bool _tx = true;
        private const bool _rx = false;

        private Channel _ch;
        private Dictionary<string, Attr> _attrs;

        private string _ipAddress;
        public SDR(string ipAddress)
        {
            Console.WriteLine("Hello, World from sdr");

            _ipAddress = ipAddress;

            _attrs = new Dictionary<string, Attr>();
        }

        public void Connect()
        {
            //call dll libiio-csharp

            _ctx = new Context($"{_ipAddress}");
            SetUutAd9361();


            //ExampleProgram.Main(_ctx);
        }

        public void Disconnect()
        {
            //call dll libiio-csharp
            _ctx.Dispose();
        }

        public void SetUutAd9361()
        {
            //call dll libiio-csharp
            _sdr = _ctx.get_device("ad9361-phy");

            _txDevice = _ctx.find_device("cf-ad9361-dds-core-lpc");
            _txDevice.find_channel("voltage0", true).enable();
            _txDevice.find_channel("voltage1", true).enable();


            _rxDevice = _ctx.find_device("cf-ad9361-lpc");
            _rxDevice.find_channel("voltage0", false).enable();
            _rxDevice.find_channel("voltage1", false).enable();
        }


        //config method for ad9361 rx
        public byte[] SetAd9361Rx(double freq, double bandWidth = 1e6)
        {

            var numberOfSample = 1_000_000;
            var hardwareGain = 20;

            //_sdr = _ctx.get_device("cf-ad9361-lpc");

            ConvertAttrToDictionary("altvoltage0", true);
            Console.WriteLine();
            PrintAttrs();

            WriteToAttr("frequency", freq);
            //WriteToAttr("sampling_frequency", 61440000);
            //WriteToAttr("hardwaregain", hardwareGain);

            PrintAttrs();
            Console.WriteLine();
            ConvertAttrToDictionary("voltage0", _rx);

            Console.WriteLine();
            PrintAttrs();
            Console.WriteLine(ReadFromAttr("rf_port_select"));

            WriteToAttr("rf_port_select", "A_BALANCED");

            WriteToAttr("rf_bandwidth", bandWidth);

            WriteToAttr("gain_control_mode", "manual");

            //ConvertAttrToDictionary("voltage0", true);

            WriteToAttr("hardwaregain", hardwareGain);


            //WriteToAttr("quadrature_tracking_en", 1);


            //WriteToAttr("rf_dc_offset_tracking_en", 1);

            Console.WriteLine();

            PrintAttrs();

            var a = _rxDevice.find_channel("voltage0", false);
            //var b = _rxDevice.find_channel("altvoltage0", false);

            var c = _rxDevice.find_channel("voltage1", false);
            //var d = _rxDevice.find_channel("altvoltage1", false);

            a.enable();
            //b.enable();
            c.enable();
            //d.enable();

            //recieve data
            var buf = new IOBuffer(_rxDevice, _bufferSize, true);

            if (buf.buf is 0)
            {
                Disconnect();
            }
            else
            {
                buf.refill();
            }

            byte[] test = new byte[_bufferSize];

            buf.read(test);
            return test;
        }

        //config method for ad9361 tx
        public void SetAd9361Tx(double freq)
        {
            var numberOfSample = 1_000_000;
            var hardwareGain = -60;

            //var bw = 1.5e6;
            //var fs = 2.5e6;
            var rfPort = "A";

            // Configure the AD9361 streaming channels

            ConvertAttrToDictionary("altvoltage0", _tx);

            WriteToAttr("frequency", freq);

            PrintAttrs();
            Console.WriteLine();
            ConvertAttrToDictionary("voltage0", _tx);

            PrintAttrs();

            WriteToAttr("hardwaregain", hardwareGain);
            WriteToAttr("sampling_frequency", 2083333);
            WriteToAttr("rf_bandwidth", numberOfSample);
            WriteToAttr("rf_port_select", rfPort);
            //Initializing AD9361 IIO streaming channels
            _txDevice.find_channel("voltage0", _tx).enable();
            _txDevice.find_channel("voltage1", _tx).enable();
           
            QPSKModulator modulator = new QPSKModulator();

            // Generate random data bits (0s and 1s)
            int numBits = 1000; // Change this to the desired number of bits
            Random rand = new Random();
            int[] dataBits = new int[numBits];
            for (int index = 0; index < numBits; index++)
            {
                dataBits[index] = rand.Next(2); // Generate 0 or 1
            }

            // Modulate the data using QPSK
            Complex[] modulatedSignal = modulator.Modulate(dataBits);

            //convert complex to byte[]
            var complexData = modulator.ComplexToBytes(modulatedSignal);

            Console.WriteLine();
            PrintAttrs();

            var buf = new IOBuffer(_txDevice, _bufferSize, true);
            if (buf.buf is 0)
            {
                Disconnect();
            }
            else
            {
                buf.fill(complexData);


                buf.push(_bufferSize);
            }
        }

        public void SetAd936xTx()
        {

            //configure sample rate
            var sampleRate = 1e6; //Hz
            //configure center frequency
            var centerFreq = 915e6; //Hz
            //configure gain
            var hardwareGain = -50;
            //configure bandwidth
            var numberOfSample = 1_000_000;
            //configure rf port
            var rfPort = "A";

            // Configure the AD9361 streaming channels

            ConvertAttrToDictionary("altvoltage0", _tx);

            WriteToAttr("frequency", centerFreq);

            PrintAttrs();

            ConvertAttrToDictionary("voltage0", _tx);

            PrintAttrs();

            WriteToAttr("hardwaregain", hardwareGain);
            WriteToAttr("sampling_frequency", sampleRate);
            WriteToAttr("rf_bandwidth", numberOfSample);
            WriteToAttr("rf_port_select", rfPort);
            //Initializing AD9361 IIO streaming channels
            var txI1 = _txDevice.find_channel("voltage0", _tx);
            var txI = _txDevice.find_channel("altvoltage0", _tx);

            var txQ1 = _txDevice.find_channel("voltage1", _tx);
            var txQ = _txDevice.find_channel("altvoltage1", _tx);

            txI1.enable();
            txI.enable();
            txQ1.enable();
            txQ.enable();



        }

        public void TransmitData()
        {
            int num_symbols = 1000;
            Random random = new Random();
            int[] x_int = Enumerable.Repeat(0, num_symbols)
                                        .Select(i => random.Next(0, 4))
                                        .ToArray();
            double[] x_degrees = x_int.Select(i => i * 360 / 4.0 + 45).ToArray();
            double[] x_radians = x_degrees.Select(d => d * Math.PI / 180.0).ToArray();
            double[] x_symbols = x_radians.Select(r => Math.Cos(r)).ToArray();
            double[] samples = Enumerable.Repeat(x_symbols, 16)
                                        .SelectMany(arr => arr)
                                        .ToArray();
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] *= Math.Pow(2, 14);
            }

        }

        public void PrintAttrs()
        {
            foreach (var attr in _attrs)
            {
                var key = attr.Key;
                var value = attr.Value;

                try
                {
                    Console.WriteLine($"{key} {value.read()}");

                }
                catch (Exception)
                {

                    Console.WriteLine($"Failed to read {key}");
                }
            }
        }
        private void ConvertAttrToDictionary(string channelName, bool isTx)
        {
            //_ch = _sdr.get_channel(channelName);
            _ch = _sdr.find_channel(channelName, isTx);
            _ch.enable();
            _attrs = _ch.attrs.ToDictionary(x => x.name, x => x);
        }

        private void WriteToAttr(string attrName, string value)
        {
            try
            {
                _attrs[attrName].write(value);
            }
            catch (Exception)
            {
                _attrs.TryGetValue(attrName, out Attr attr);

                if (attr is null)
                {
                    Console.WriteLine($"No such attr in current channel: {attrName}");

                }
                else
                {
                    Console.WriteLine($"Failed to write from attrs: {attr.name}");

                }
            }
        }
        private void WriteToAttr(string attrName, double value)
        {
            WriteToAttr(attrName, value.ToString());
        }
        private string ReadFromAttr(string attrName)
        {
            try
            {
                return _attrs[attrName].read().ToString();
            }
            catch (Exception)
            {
                _attrs.TryGetValue(attrName, out Attr attr);

                if (attr is null)
                {
                    Console.WriteLine($"No such attr in current channel: {attrName}");

                }
                else
                {
                    Console.WriteLine($"Failed to write from attrs: {attr.name}");

                }
                return string.Empty;
            }
        }
        private void FindChannel(string channelName, bool isTx = true)
        {
            _ch = _sdr.find_channel(channelName, isTx);
            //
        }



        void rx_config(Device device, string rx_lo, string rx_bw, string rx_fs, string gain_ctrl_mode)
        {
            var rx_chn = device.find_channel("voltage0", false);

            device.find_channel("RX_LO", true).find_attribute("frequency").write(rx_lo);
            rx_chn.find_attribute("rf_bandwidth").write(rx_bw);
            rx_chn.find_attribute("sampling_frequency").write(rx_fs);
            rx_chn.find_attribute("gain_control_mode").write(gain_ctrl_mode);
        }

        void tx_config(Device device, string tx_lo, string tx_bw, string tx_fs, string hw_gain)
        {
            var tx_chn = device.find_channel("voltage0", true);

            device.find_channel("TX_LO", true).find_attribute("frequency").write(tx_lo);
            tx_chn.find_attribute("rf_bandwidth").write(tx_bw);
            tx_chn.find_attribute("sampling_frequency").write(tx_fs);
            tx_chn.find_attribute("hardwaregain").write(hw_gain);
        }

        void enable_tx(Device tx_adc)
        {
            tx_adc.find_channel("voltage0", true).enable();
            tx_adc.find_channel("voltage1", true).enable();
        }

        void enable_rx(Device rx_dac)
        {
            rx_dac.find_channel("voltage0", false).enable();
            rx_dac.find_channel("voltage1", false).enable();
        }

        byte[] generate_sine(int samples_per_channel, string rx_fs)
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

        (List<int> reals, List<int> imags) read_rx_data(IOBuffer rx_buff, int num_readings, int samples_per_channel)
        {
            List<int> reals = new List<int>();
            List<int> imags = new List<int>();

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
    }
}
