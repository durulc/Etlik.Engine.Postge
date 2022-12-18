using System.Collections.Generic;

namespace RTLS.Model.View
{
    public class SectorView
    {
        public string SectorId { get; set; }
        public long X { get; set; }
        public long Y { get; set; }
        public virtual ICollection<AccessPointView> Accesspoints { get; set; }
    }
}
