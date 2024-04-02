namespace SDR.Contracts
{
    public interface IUutDigitalService : IInitializable, IDisposable, IResetable
    {
        #region Methods

        (bool, Dictionary<string, IEnumerable<double>>) RecordIQData(byte blockNum,
                                                                    (int, int, Predicate<int>) q2Params);
        public (bool, Dictionary<double, double>) GetSpectrum(int numberOfSamples, int testedChannel, byte blockNum);

        public (bool, Dictionary<double, double>) GetPsdSpectrum(int numberOfSamples, int testedChannel, byte blockNum);

        #endregion
    }
}
