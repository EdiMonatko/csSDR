// See https://aka.ms/new-console-template for more information
using SDR;
using ScottPlot;
using ScottPlot.WinForms;
using System.Windows.Forms;

Console.WriteLine("Hello, World!");
var status = false;
var pluto = new AdalmPluto936X("192.168.2.1");
status = pluto.Init();
pluto.SetTxFrequency(2e9);
//pluto.SetRxFrequency(2e9);
//pluto.SetTxFrequency(2e9);
//pluto.SetTxFrequency(3e9);
//pluto.SetTxGain(0);
//pluto.SetTxGain(-10);
//pluto.SetTxGain(-20);
pluto.SetTxGain(-20);
//pluto.SetTxGain(-20);
pluto.ChannelSample = 1e6;
//pluto.TxGain = -20;
pluto.GainControlMode = "manual";
pluto.RxGain = 30;
status = pluto.PlutoTxOn(dds: 3e5);

//for (int i = 0; i < 1000; i+=10)
//{
//    //pluto.SetTxDds(frequency0: 3e5, frequency2:3e5, phase2: i, phase0:i);
//    Console.WriteLine( $"phase0:{i}");
//    Thread.Sleep(1 * 1000);
//}

//Create Rx Thread
var b = pluto.SetRxGain(20);
var (I, Q) = pluto.PlutoRxOn();


//Call PlutoRx as new thread

Task t = new Task(() => PlutoRx.PlutoRxClass(pluto));
t.Start();

status = pluto.PlutoTxOn(dds: 3e5);
var iqBytes = pluto.generate_sine(pluto.ChannelSample,
    pluto.TxSampleRate, 
    3e6);

//Thread.Sleep(60 * 1000);

pluto.Dispose();

var a = 0;
//Context _ctx = new Context("192.168.2.1");

//QPSKModulator.Main();

var freq = 1e9;
//create new SDR object
//SDR.SDR sdr = new SDR.SDR("10.100.102.108");

//SDR.SDR sdr = new SDR.SDR("192.168.2.1");

//ExampleFromAnalog.Main();


//SDR.SDR sdr = new SDR.SDR("10.100.102.108");
//sdr.Connect();

//var spectrum = new Dictionary<double, double>();
//var plt = new Plot();


//sdr.SetAd9361Tx(freq);

//for (int j = 0; j < 1000; j++)
//{
//    var receivedData = sdr.SetAd9361Rx(freq, 20000000);

//    //convert byte[] receivedData to I and Q data
//    var I = new double[receivedData.Length / 2];
//    var Q = new double[receivedData.Length / 2];
//    for (int i = 0; i < receivedData.Length; i += 2)
//    {
//        if (i == 2046)
//        {
//            break;
//        }
//        I[i / 2] = BitConverter.ToInt16(receivedData, i);
//        Q[i / 2] = BitConverter.ToInt16(receivedData, i + 1);

//    }

//    plt.Clear();
//    spectrum = CalculateIQData.CreateSpectrum(I, Q, sampling: sdr._bufferSize);



//    plt.PlotSignalXY(spectrum.Keys.ToArray(), spectrum.Values.ToArray(), label: "Spectrum");
//    plt.SaveFig("spectrum.png");

//    var showPlt = new FormsPlotViewer(plt);


//    showPlt.Refresh();
//    showPlt.ShowDialog();
//    Thread.Sleep((int)2 * 1000);
//}



//sdr.Disconnect();

