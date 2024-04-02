namespace SDR.Contracts
{
    public interface IInitializable
    {
        #region Properties

        bool IsInitialized { get; protected set; }

        #endregion

        #region Methods

        void Init();

        #endregion
    }
}
