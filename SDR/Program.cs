// See https://aka.ms/new-console-template for more information
using SDR;
using ScottPlot;
using ScottPlot.WinForms;

Console.WriteLine("Hello, World!");

//Context _ctx = new Context("192.168.2.1");

//QPSKModulator.Main();

var freq = 1e9;
//create new SDR object
//SDR.SDR sdr = new SDR.SDR("10.100.102.108");

//SDR.SDR sdr = new SDR.SDR("192.168.2.1");

ExampleFromAnalog.Main();


SDR.SDR sdr = new SDR.SDR("10.100.102.108");
sdr.Connect();

var spectrum = new Dictionary<double, double>();
var plt = new Plot();


sdr.SetAd9361Tx(freq);

for (int j = 0; j < 1000; j++)
{
    var receivedData = sdr.SetAd9361Rx(freq, 20000000);

    //convert byte[] receivedData to I and Q data
    var I = new double[receivedData.Length / 2];
    var Q = new double[receivedData.Length / 2];
    for (int i = 0; i < receivedData.Length; i += 2)
    {
        if (i == 2046)
        {
            break;
        }
        I[i / 2] = BitConverter.ToInt16(receivedData, i);
        Q[i / 2] = BitConverter.ToInt16(receivedData, i + 1);

    }

    plt.Clear();
    spectrum = CalculateIQData.CreateSpectrum(I, Q, sampling: sdr._bufferSize);



    plt.PlotSignalXY(spectrum.Keys.ToArray(), spectrum.Values.ToArray(), label: "Spectrum");
    plt.SaveFig("spectrum.png");

    var showPlt = new FormsPlotViewer(plt);


    showPlt.Refresh();
    showPlt.ShowDialog();
    Thread.Sleep((int)2 * 1000);
}



sdr.Disconnect();

