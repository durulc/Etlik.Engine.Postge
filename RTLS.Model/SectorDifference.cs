namespace RTLS.Model
{
    public class SectorDifference
    {
        private string sectorId;
        /// <summary>
        /// int - stores the x coordinate of the sector
        /// </summary>
        private long x;

        /// <summary>
        /// int - stores the y coordinate of the sector
        /// </summary>
        private long y;

        /// <summary>
        /// double - stores the difference between the access points
        /// </summary>
        private double dRssi;

        /// <summary>
        /// Constructor creates an instance of this class and initializes the private data fields by a list of parameters.
        /// </summary>
        /// <param name="x">int</param>
        /// <param name="y">int</param>
        /// <param name="dRssi">double</param>
        public SectorDifference(string sectorId, long x, long y, double dRssi)
        {
            this.x = x;
            this.y = y;
            this.dRssi = dRssi;
            this.sectorId = sectorId;
        }

        public string getSectorId()
        {
            return this.sectorId;
        }

        /// <summary>
        /// returns the x coordinate
        /// </summary>
        /// <returns>int</returns>
        public long getX()
        {
            return this.x;
        }

        /// <summary>
        /// returns the y coordinate
        /// </summary>
        /// <returns>int</returns>
        public long getY()
        {
            return this.y;
        }

        /// <summary>
        /// returns the difference of the signal strengths
        /// </summary>
        /// <returns></returns>
        public double getRssi()
        {
            return this.dRssi;
        }

        /// <summary>
        /// sets the x coordinate to a new value
        /// </summary>
        /// <param name="x">int</param>
        public void setX(int x)
        {
            this.x = x;
        }

        /// <summary>
        /// sets the y coordinate to a new value
        /// </summary>
        /// <param name="y">int</param>
        public void setY(int y)
        {
            this.y = y;
        }


        public void setSectorId(string sectorId)
        {
            this.sectorId = sectorId;
        }
        /// <summary>
        /// sets the signal strength to a new value
        /// </summary>
        /// <param name="dRssi">double</param>
        public void setdRssi(double dRssi)
        {
            this.dRssi = dRssi;
        }
    }
}
