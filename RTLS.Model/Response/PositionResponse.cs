using System.Collections.Generic;
using RTLS.Model.Enums;
using RTLS.Model.View;

namespace RTLS.Model.Response
{
    public class PositionResponse
    {
        public PointView Point { get; set; }
        public EPositionType PositionType { get; set; }
        public List<SectorDif> SectorDifference { get; set; }
    }

    public class SectorDif
    {
        public double DRssi { get; set; }
        public string SectorId { get; set; }
        public long X { get; set; }
        public long Y { get; set; }
    }
}
