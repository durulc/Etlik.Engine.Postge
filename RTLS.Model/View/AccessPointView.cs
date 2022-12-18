using System.Collections.Generic;

namespace RTLS.Model.View
{
    public class AccessPointView
    {        
        public string Bssid { get; set; }
        public List<double> Rssi { get; set; }
        public string SectorId { get; set; }
    }

    public class AccessPointAverageRssiView
    {
        public string Bssid { get; set; }
        public double Rssi { get; set; }
    }
}
