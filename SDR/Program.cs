// See https://aka.ms/new-console-template for more information
using iio;
using SDR;

Console.WriteLine("Hello, World!");

//Context _ctx = new Context("192.168.2.1");

var freq = 1e9;
//create new SDR object
SDR.SDR sdr = new SDR.SDR("192.168.2.1");
sdr.Connect();

sdr.SetAd9361Tx(freq);
sdr.SetAd9361Rx(freq);

sdr.Disconnect();

