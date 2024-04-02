namespace SDR.Contracts
{
    public interface IEquipmentService : IInitializable, IDisposable, IResetable
    {
        #region Propreties

        string Alias { get; private protected set; }
        string Manufacturer { get; private protected set; }
        string Model { get; private protected set; }

        #endregion

        #region Methods

        public bool ResetEquipment();
        public bool ClearStatus();
        public bool Wait();

        public (bool, bool) Ready(int actualRetry = 0, int retryNumbers = 3);
        public (bool, (int, string)) Error(string command = "ERR?");
        public (bool, string) Identify();

        #endregion
    }

}
