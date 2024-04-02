namespace SDR.Setings
{
    public class SpectrumAnalyzerSettings : EquipmentSettings
    {
        public double MinimumFrequency { get; set; }
        public double MaximumFrequency { get; set; }
        public double MaximumAttenuation { get; set; }
        public double MinimumAttenuation { get; set; }
    }
}
