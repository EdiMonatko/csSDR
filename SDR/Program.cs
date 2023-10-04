// See https://aka.ms/new-console-template for more information
using SDR;

Console.WriteLine("Hello, World!");

//Context _ctx = new Context("192.168.2.1");

var freq = 10e6;
//create new SDR object
//SDR.SDR sdr = new SDR.SDR("10.100.102.108");
SDR.SDR sdr = new SDR.SDR("192.168.2.1");
sdr.Connect();

sdr.SetAd9361Tx(freq);
var receivedData = sdr.SetAd9361Rx(freq, 20000000);

//convert byte[] receivedData to I and Q data
var I = new double[receivedData.Length / 2];
for (int i = 0; i < receivedData.Length; i += 2)
{
    I[i / 2] = BitConverter.ToInt16(receivedData, i);
}

var Q = new double[receivedData.Length / 2];
for (int i = 1; i < receivedData.Length - 1; i += 2)
{
    Q[i / 2] = BitConverter.ToInt16(receivedData, i);
}

var spectrum = CalculateIQData.CreateSpectrum(I, Q, sampling: sdr._bufferSize);


var plt = new ScottPlot.Plot();

plt.PlotSignalXY(spectrum.Keys.ToArray(), spectrum.Values.ToArray(), label: "Spectrum");
plt.SaveFig("spectrum.png");

sdr.Disconnect();

