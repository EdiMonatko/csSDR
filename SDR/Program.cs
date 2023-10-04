// See https://aka.ms/new-console-template for more information
using iio;
using SDR;
using ScottPlot;
using System.Numerics;

Console.WriteLine("Hello, World!");

//Context _ctx = new Context("192.168.2.1");

var freq = 1e9;
//create new SDR object
SDR.SDR sdr = new SDR.SDR("192.168.2.1");
sdr.Connect();

sdr.SetAd9361Tx(freq);
var receivedData = sdr.SetAd9361Rx(freq);

//convert byte[] receivedData to I and Q data
var I = new double[receivedData.Length / 2];
for (int i = 0; i < receivedData.Length; i += 2)
{
    I[i / 2] = BitConverter.ToInt16(receivedData, i);
}

var Q = new double[receivedData.Length / 2];
for (int i = 1; i < receivedData.Length; i += 2)
{
    Q[i / 2] = BitConverter.ToInt16(receivedData, i);
}

var spectrum = CalculateIQData.CreateSpectrum(I, Q, sampling: sdr._bufferSize);


var plt = new ScottPlot.Plot();

plt.PlotSignalXY(spectrum.Keys.ToArray(), spectrum.Values.ToArray(), label: "Spectrum");
plt.SaveFig("spectrum.png");

sdr.Disconnect();

