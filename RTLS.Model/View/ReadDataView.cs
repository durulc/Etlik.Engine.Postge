using System;

namespace RTLS.Model.View
{
    public class ReadDataView
    {
        public string ReaderId { get; set; }
        public string TagId { get; set; }
        public byte TagType { get; set; }
        public double Rssi { get; set; }
        public string Body { get; set; }
        public DateTime ReadDateTime { get; set; }
    }
}

