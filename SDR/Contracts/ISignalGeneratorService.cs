namespace SDR.Contracts
{
    public interface ISignalGeneratorService : IEquipmentService
    {
        #region Propreties

        double MinimumFrequency { get; private protected set; }
        double MaximumFrequency { get; private protected set; }
        double MinimumAmplitude { get; private protected set; }
        double MaximumAmplitude { get; private protected set; }

        #endregion

        #region Methods

        bool Preset();
        bool SetRfOn();
        bool SetRfOff();
        bool SetModulationOn();
        bool SetModulationOff();
        bool SetFrequency(double frequency);
        bool SetPower(double power);
        public (bool, double) GetPower();
        (bool, double) GetFrequency();
        (bool, double) GetAmplitureCw();

        bool RecallState(string register, string sequnce);
        public (bool, bool) Error();

        #endregion
    }
}
