namespace RTLS.Model
{
    public class BlindMeasurement
    {
        /// <summary>
        /// string - the MAC address of the access point
        /// </summary>
        private string bssid;

        /// <summary>
        /// double - the signal strength of the access point
        /// </summary>
        private double rssi;

        /// <summary>
        /// double - the calculated average fluctuation of the access point
        /// </summary>
        private double averageFluctuation;

        /// <summary>
        /// Standardconstructor creates an instance of this class and initializes all data fields to 0
        /// </summary>
        public BlindMeasurement()
        {
            this.rssi = 0;
            this.bssid = null;
            this.averageFluctuation = 0;
        }

        /// <summary>
        /// Constructor creates an instance of this class and sets the MAC address and the signal strength to a given value
        /// </summary>
        /// <param name="bssid">string</param>
        /// <param name="rssi">double</param>
        public BlindMeasurement(string bssid, double rssi)
        {
            this.rssi = rssi;
            this.bssid = bssid;
        }

        /// <summary>
        /// Constructor creates an instance of this class and sets the MAC address, the signal strength and the fluctuation to a given value
        /// </summary>
        /// <param name="bssid">string</param>
        /// <param name="rssi">double</param>
        /// <param name="averageFluctuation">double</param>
        public BlindMeasurement(string bssid, double rssi, double averageFluctuation)
        {
            this.rssi = rssi;
            this.bssid = bssid;
            this.averageFluctuation = averageFluctuation;
        }

        /// <summary>
        /// returns the MAC address
        /// </summary>
        /// <returns>string</returns>
        public string getBssid()
        {
            return this.bssid;
        }

        /// <summary>
        /// returns the signal strength
        /// </summary>
        /// <returns>double</returns>
        public double getRssi()
        {
            return this.rssi;
        }

        /// <summary>
        /// returns the average fluctuation
        /// </summary>
        /// <returns>double</returns>
        public double getAverageFluctuation()
        {
            return this.averageFluctuation;
        }

        /// <summary>
        /// sets the MAC address to a new value
        /// </summary>
        /// <param name="bssid">string</param>
        public void setBssid(string bssid)
        {
            this.bssid = bssid;
        }

        /// <summary>
        /// sets the signal strength to a new value
        /// </summary>
        /// <param name="rssi">double</param>
        public void setRssi(double rssi)
        {
            this.rssi = rssi;
        }

        /// <summary>
        /// sets the average fluctuation to a new value
        /// </summary>
        /// <param name="averageFluctuation">double</param>
        public void setAverageFluctuation(double averageFluctuation)
        {
            this.averageFluctuation = averageFluctuation;
        }
    }
}
