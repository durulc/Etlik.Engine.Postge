using System;
using System.Linq;
using DevExpress.Xpo;
using RTLS.Dal.Entity;
using RTLS.Model.Manager;
using RTLS.Model.View;

namespace RTLS.Manager
{
    public class RssiManagement
    {
        public static void SetRssi(ReadDataView data)
        {
            //tag pozisyonla
            if (Convert.ToBoolean(ConfigManager.GetValue("tagPosition")))
            {
                TagManager.CollectData(data);
            }
                           
            //sector verisi biriktir
            if (Convert.ToBoolean(ConfigManager.GetValue("collectedSectorData")))
            {
                using (Session session = XpoManager.GetNewSession())
                {
                    var sector = session.GetObjectByKey<Sector>(data.TagId);
                    if (sector != null && sector.Lock==false)
                    {
                        //aynı map ise data biriktir
                        var reader = ReaderManager.RtlsReaders.FirstOrDefault(w => w.ReaderId == data.ReaderId);
                        if (reader == null) return;
                        if (reader.FloorOid == sector.MapId.MapId)
                        {
                            SectorManager.SetSector(sector, session, data);
                        }                        
                    }
                }                
                
            }
        }

        public static long CalculateDistance(double signalLevelInDb)
        {
            double freqInMHz = 433.0; // 2412.0;
            double exp = (27.55 - (20 * Math.Log10(freqInMHz)) + Math.Abs(signalLevelInDb)) / 20.0;
            double accuracy = Math.Pow(10.0, exp);
            return Convert.ToInt64(accuracy);
        }

        public static long BeaconCalculateDistance(float txPower, double rssi)
        {
            var rssiAtOneMeter = txPower - 62;
            return Convert.ToInt64(Math.Pow(10, (rssiAtOneMeter - rssi) / 20)*100);            
        }

        public static long CalculateDistance(float txPower, double rssi)
        {

            if (rssi == 0)
            {
                return -1; // if we cannot determine distance, return -1.
            }
            double ratio = rssi * 1.0 / txPower;

            if (ratio < 1.0)
            {
                return Convert.ToInt64(Math.Pow(ratio, 10));
            }
            else
            {
                double accuracy = (0.89976) * Math.Pow(ratio, 7.7095) + 0.111;
                return Convert.ToInt64(accuracy);
            }
        }
    }
}
