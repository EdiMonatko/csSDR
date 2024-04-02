namespace SDR.Setings
{
    public class EquipmentSettings
    {
        public string Alias { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public string Address { get; set; }
        public ushort Port { get; set; }
        public string SerialNumber { get; set; }
        public int ReadBufferSize { get; set; } = 1024;
        public int WriteBufferSize { get; set; } = 1024;
        public bool IsAsync { get; set; } = false;
    }
}
