namespace RTLS.Model.View
{
    public class ReaderDataView
    {
        public string ReaderId { get; set; }
        public string IpAddress { get; set; }
        public ushort HardwareVersion { get; set; }
        public ushort SoftwareVersion { get; set; }
        public ushort ProtocolVersion { get; set; }
    }
}
