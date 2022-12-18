using System;
using System.Collections.Generic;

namespace RTLS.Manager.Helper
{
    public class RssiCalcHelper
    {
        /// <summary>
        /// double - stores the summed up signal strengths
        /// </summary>
        private double rssi;

        /// <summary>
        /// int - stores the counter for all saved signal strength
        /// </summary>
        private int count = 0;

        /// <summary>
        /// List - stores a list of all signal strength of one access points
        /// </summary>
        private List<Double> rssiValues;

        /// <summary>
        /// double - the maximum rssi value of an access point measured
        /// </summary>
        private double maxRssiValue;

        /// <summary>
        /// double - the minimum rssi value of an access point measured
        /// </summary>
        private double minRssiValue;

        /// <summary>
        /// Constructor creates an instance of this class using the first signal strength entry which has to be overgiven
        /// by a parameter.
        /// </summary>
        /// <param name="rssi">double</param>
        public RssiCalcHelper(double rssi)
        {
            this.rssi = rssi;
            this.count++;

            this.rssiValues = new List<Double>();
            this.rssiValues.Add(rssi);
        }

        /// <summary>
        /// method adds a new signal strength to the list and adds up the counter
        /// </summary>
        /// <param name="rssi">double</param>
        public void addRssi(double rssi)
        {
            this.rssi += rssi;
            this.count++;

            this.rssiValues.Add(rssi);
        }

        /// <summary>
        /// returns the average signal strength: summed signal strength divided by the counter
        /// </summary>
        /// <returns>double</returns>
        public double getAverageRssi()
        {
            return (this.rssi) / (this.count);
        }

        /// <summary>
        /// returns the summed up signal strength
        /// </summary>
        /// <returns>double</returns>
        public double getRssi()
        {
            return this.rssi;
        }

        /// <summary>
        /// returns the counter
        /// </summary>
        /// <returns>int</returns>
        public int getCount()
        {
            return this.count;
        }

        /// <summary>
        /// returns the average fluctuation of the signal strength
        /// </summary>
        /// <returns>Double</returns>
        public Double getAverageFluctuation()
        {
            this.rssiValues.Sort();

            return this.printFinalResult();
        }

        /// <summary>
        /// returns the intervall in which one access point sends it's signal (e.g. -56 to -46 would be the returned result: 10)
        /// </summary>
        /// <returns>Double</returns>
        public Double printFinalResult()
        {
            Double theMinDifference = -1;
            Double theMaxDifference = -1;
            bool reversed = false;

            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 1; j++)
                {
                    if (reversed == false)
                    {
                        theMinDifference = this.rssiValues[j];
                        this.minRssiValue = theMinDifference;
                    }

                    if (reversed == true)
                    {
                        theMaxDifference = this.rssiValues[j];
                        this.maxRssiValue = theMaxDifference;
                    }

                    reversed = true;
                }
                this.rssiValues.Reverse();
            }

            return theMaxDifference - theMinDifference;
        }

        /// <summary>
        /// method shows the entire liste
        /// </summary>
        public void showList()
        {
            Console.WriteLine(this.rssiValues.ToString());
        }

        /// <summary>
        /// returns the maximum signal strength
        /// </summary>
        /// <returns>double</returns>
        public double getMaxRssi()
        {
            return this.maxRssiValue;
        }

        /// <summary>
        /// retruns the minimum signal strength
        /// </summary>
        /// <returns>double</returns>
        public double getMinRssi()
        {
            return this.minRssiValue;
        }
    }
}
