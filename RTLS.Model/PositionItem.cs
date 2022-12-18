using RTLS.Model.View;

namespace RTLS.Model
{
    public class PositionItem
    {
        public string TagId { get; set; }
        public double FloorOid { get; set; }
        public PointView Position { get; set; }
        public float Radius { get; set; }
    }
}
