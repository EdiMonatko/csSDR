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
        private const bool _rx = true;

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
        public byte[] SetAd9361Rx(double freq)
        {
            //_sdr = _ctx.get_device("cf-ad9361-lpc");

            ConvertAttrToDictionary("altvoltage0", _rx);

            WriteToAttr("frequency", freq); 

            PrintAttrs();
            Console.WriteLine(  );
            ConvertAttrToDictionary("voltage0", _rx);

            PrintAttrs();
            Console.WriteLine(  );
            WriteToAttr("sampling_frequency", 1e6);

            WriteToAttr("rf_bandwidth", 1e6);

            WriteToAttr("gain_control_mode", "slow_attack");

            WriteToAttr("hardwaregain", 30);

            WriteToAttr("quadrature_tracking_en", 1);

            WriteToAttr("rf_port_select", "A_BALANCED");

            WriteToAttr("rf_dc_offset_tracking_en", 1);

            Console.WriteLine();

            PrintAttrs();

            var a = _rxDevice.find_channel("voltage0", false);
            //var b = _rxDevice.find_channel("altvoltage0", true);

            var c = _rxDevice.find_channel("voltage1", false);
            //var d = _rxDevice.find_channel("altvoltage1", true);

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
            var bw = 1.5e6;
            var fs = 2.5e6;
            var rfPort = "A";

            // Configure the AD9361 streaming channels

            ConvertAttrToDictionary("altvoltage0", _tx);

            WriteToAttr("frequency", freq);

            PrintAttrs();
            Console.WriteLine(  );
            ConvertAttrToDictionary("voltage0", _tx);

            PrintAttrs();

            WriteToAttr("rf_bandwidth", bw);
            WriteToAttr("sampling_frequency", fs);
            WriteToAttr("rf_port_select", rfPort);
            WriteToAttr("hardwaregain", -20);
            //Initializing AD9361 IIO streaming channels
            var a = _txDevice.find_channel("voltage0", _tx);
            var b = _txDevice.find_channel("altvoltage0", _tx);

            var c = _txDevice.find_channel("voltage1", _tx);
            var d = _txDevice.find_channel("altvoltage1", _tx);

            a.enable();
            b.enable();
            c.enable();
            d.enable();

            Console.WriteLine();
            PrintAttrs();
            var buf = new IOBuffer(_txDevice, _bufferSize);
            if (buf.buf is 0)
            {
                Disconnect();
            }
            else
            {
                buf.push(_bufferSize);
            }
            //SetUutAd9361();
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

        private void FindChannel(string channelName, bool isTx = true)
        {
            _ch = _sdr.find_channel(channelName, isTx);
            //
        }
    }
}
