namespace SDR
{
    public interface IAdalmPluto936X
    {
        double ChannelSample { get; protected set; }
        string GainControlMode { get; protected set; }
        double RxBandwidth { get; protected set; }
        double RxFrequency { get; protected set; }
        double RxGain { get; protected set; }
        double RxSampleRate { get; protected set; }
        double TxBandwidth { get; protected set; }
        double TxFrequency { get; protected set; }
        double TxGain { get; protected set; }
        double TxSampleRate { get; protected set; }

        bool Dispose();
        bool EnableRx();
        bool EnableTx(byte[] iqBytes, bool cirtular = true);
        bool Init();
        (List<double> reals, List<double> imags) PlutoRxOn();
        bool PlutoTxOn(double signalFrequency, double dds);
        (List<double> reals, List<double> imags) ReadRx(double bufferLength, bool isCircular = false);
        bool SetRxBandwidth(double bandwidth);
        bool SetRxFrequency(double frequency);
        bool SetRxGain(double gain, string gainControl = "manual");
        bool SetRxSampleRate(double sampleRate);
        bool SetSampleRate(double sampleRate);
        bool SetTxBandwidth(double bandwidth);
        bool SetTxFrequency(double frequency);
        bool SetTxGain(double gain);
        bool SetTxSampleRate(double sampleRate);
    }
}