using ScottPlot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SDR
{
    public static class PlutoRx
    {
        public static void PlutoRxClass(AdalmPluto936X pluto)
        {
            var plt = new Plot();
            var showPlt = new FormsPlotViewer(plt);
            pluto.PlutoRxOn();
            // Create a background worker thread for updating the plot
            //var plotThread = new Thread(() =>
            //{

            //});

            while (pluto.isInitialized)
            {
                var I = new double[250000];
                var Q = new double[250000];
                for (int i = 0; i < 10; i++)
                {
                    (I, Q) = pluto.ReadRx(pluto.ChannelSample, isCircular: true);


                }

                var spectrum = CalculateIQData.CreateSpectrum(I.ToArray(), Q.ToArray(), sampling: pluto.ChannelSample);

                var (peakFreq, peakAmpltitude) = CalculateIQData.PeakSearch(spectrum);


                // Plot the new data on the UI thread
                if (spectrum.Count > 0)
                {
                    //write peakFreq and peakAmpltitude to console with 3 decimal places
                    Console.WriteLine("Peak Frequency: {0:F3}Hz", peakFreq);
                    Console.WriteLine("Peak Amplitude: {0:F3}dBm", peakAmpltitude);

                    showPlt.Invoke((MethodInvoker)delegate
                    {
                        plt.Clear();

                        plt.PlotSignalXY(spectrum.Keys.ToArray(), spectrum.Values.ToArray(), label: "Spectrum");
                        plt.SaveFig("spectrum.png");
                        showPlt.Refresh();
                        showPlt.Update();
                    });
                }


                Thread.Sleep((int)(0.5 * 1000));
            }

            // Start the background thread
            //plotThread.Start();

            // The main UI thread to display the plot
            //Application.Run(showPlt);
        }
    }
}
