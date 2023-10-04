using iio;
using IIOCSharp;

namespace SDR

{
    public class SDR
    {
        public Context _ctx;
        public Device _sdr;

        public uint _bufferSize = 1024 * 1024;

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

            _txDevice = _ctx.find_device("cf-ad9361-dds-core-lpc");
            _rxDevice = _ctx.find_device("cf-ad9361-lpc");
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
        }



        //config method for ad9361 rx
        public byte[] SetAd9361Rx(double freq, double bandWidth = 1e6)
        {

            var numberOfSample = 2048;
            var hardwareGain = 0;

            //_sdr = _ctx.get_device("cf-ad9361-lpc");

            ConvertAttrToDictionary("altvoltage0", true);
            Console.WriteLine(  );
            PrintAttrs();

            WriteToAttr("frequency", freq); 
            WriteToAttr("sampling_frequency", 61440000);
            WriteToAttr("hardwaregain", hardwareGain);

            PrintAttrs();
            Console.WriteLine(  );
            ConvertAttrToDictionary("voltage0", _rx);

            PrintAttrs();
            Console.WriteLine(  );
            Console.WriteLine(ReadFromAttr("rf_port_select"));

            WriteToAttr("rf_port_select", "A_BALANCED");

            WriteToAttr("rf_bandwidth", bandWidth);

            WriteToAttr("gain_control_mode", "slow_attack");


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
            var sampleRate = 1e6; //Hz
            var centerFreq = 915e6; //Hz
            var numberOfSample = 1_000_000;
            var hardwareGain = -70;

            //var bw = 1.5e6;
            //var fs = 2.5e6;
            var rfPort = "A";

            // Configure the AD9361 streaming channels

            ConvertAttrToDictionary("altvoltage0", _tx);

            WriteToAttr("frequency", centerFreq);

            PrintAttrs();
            Console.WriteLine(  );
            ConvertAttrToDictionary("voltage0", _tx);

            PrintAttrs();

            WriteToAttr("hardwaregain", hardwareGain);
            WriteToAttr("sampling_frequency", 61440000);
            WriteToAttr("rf_bandwidth", numberOfSample);
            WriteToAttr("rf_port_select", rfPort);
            //Initializing AD9361 IIO streaming channels
            var a = _txDevice.find_channel("voltage0", _tx);
            var txI = _txDevice.find_channel("altvoltage0", _tx);

            var c = _txDevice.find_channel("voltage1", _tx);
            var txQ = _txDevice.find_channel("altvoltage1", _tx);

            a.enable();
            txI.enable();
            c.enable();
            txQ.enable();

            Console.WriteLine();
            PrintAttrs();
            var buf = new IOBuffer(_txDevice, _bufferSize, false);
            if (buf.buf is 0)
            {
                Disconnect();
            }
            else
            {
                buf.push(1024);
            }
            //SetUutAd9361();
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

                    Console.WriteLine(  $"Failed to read {key}");
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
    }
}
