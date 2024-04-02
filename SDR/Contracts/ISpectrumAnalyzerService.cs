namespace SDR.Contracts
{
    public interface ISpectrumAnalyzerService : IEquipmentService
    {
        #region Propreties

        public double MaximumFrequency { get; private protected set; }
        public double MaximumAttenuation { get; private protected set; }
        public double MinimumAttenuation { get; private protected set; }

        #endregion

        #region Methods

        (bool, double) GetMarkerPowerMeas(int marker_ID);
        (bool, double) GetMarkerFreq(int marker_ID);
        (bool, double) GetCurrentAtten();
        (bool, Dictionary<double, double>?) GetTrace(int traceNum);
        (bool, double) GetSweepTime();
        (bool, double) GetRBW();
        (bool, double) GetVBW();
        (bool, double) GetStartFreq();
        (bool, double) GetStopFreq();
        (bool, double) GetAmplitudeReferenceLevel();
        (bool, double) GetMarkerCounter(int marker_ID);
        bool SetNoiseMarker(int marker_ID);
        bool SetDefaultMarker(int marker_ID);
        bool SetBandPowerMarker(int marker_ID);
        bool SetTraceAvg(int traceNum);
        bool SetSpan(double frequency);
        bool SetMarkerOnPeakSearch(int marker_ID, double peakLowerLimit = -120);
        bool SetMarkerNextPeak(int marker_ID, double peakLowerLimit = -120);
        bool SetMarkerAbsulutMax(int marker_ID);
        bool SetAllMarkersOff();
        bool SetTraceMin();
        bool SetMarkerOn(int marker_ID);
        bool SetMarkerToFreq(int marker_ID, double freq_Hz);
        bool SetNextPeakExcursion(double value, double peakLowerLimit = -120);
        bool SetMarkerThreshold(double peakLowerLimit, int markerId);
        bool SetAmplitudeReferenceLevel(double value);
        bool SetCwMode(double freq);
        bool SetFreq(double freq);
        bool SetStartFreq(double freq);
        bool SetStopFreq(double freq);
        bool SetRBW(double frequency);
        bool SetVBW(double frequency);
        bool SendAlignment();
        bool SendAlignmentWithOperationComplete();
        bool SetAutomaticAttenuator();
        bool SetPhaseNoise();
        bool SetSweepMode();
        bool SetLogPlot();
        bool SetPhaseNoiseMarkerOn(int markerID);
        bool SetPhaseNoiseMarkerToXValue(int markerID, double freqOffset);
        bool SetPhaseNoiseFrequency(double frequency);
        bool SetPhaseNoiseStopFrequency(double frequency);
        bool SetPhaseNoiseStartFrequency(double frequency);
        bool SetPreAmplifierOn();
        bool SetPreAmplifierOff();
        bool SetMarkerCounterOn(int marker_ID);
        bool SetMarkerCounterOff(int marker_ID);
        (bool, double) GetPhaseNoiseMarkerXPosition(int markerID);
        (bool, double) GetPhaseNoiseMarkerYPosition(int markerID);
        bool AdjustToMinClip();
        bool SetManualAtten(double attenuation);
        bool RecallFile(string stateFilePath);
        bool Preset();
        bool PresetWithOperationComplete();

        #endregion
    }
}
