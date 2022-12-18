using System;
using System.Collections.Generic;
using System.Linq;
using DevExpress.Xpo;
using RTLS.Dal.Entity;
using RTLS.Model.Manager;
using RTLS.Model.View;

namespace RTLS.Manager
{
    public class SectorManager
    {
        //public static SortedDictionary<string, SectorView> knownSectors;

        public static void SetSector(Sector sector, Session session, ReadDataView value)
        {
            var accessPoint =
                        session.Query<AccessPoint>()
                            .FirstOrDefault(w => w.SectorId.SectorId == sector.SectorId && w.Bssid == value.ReaderId.ToString());
            if (accessPoint != null)
            {
                //accessPoint.Rssi = value.Rssi;
                accessPoint.MaxValue = value.Rssi > accessPoint.MaxValue ? value.Rssi : accessPoint.MaxValue;
                accessPoint.MinValue = value.Rssi < accessPoint.MinValue ? value.Rssi : accessPoint.MinValue;
                accessPoint.TotalValue = accessPoint.TotalValue + value.Rssi;
                accessPoint.Count = accessPoint.Count + 1;
                accessPoint.Rssi = accessPoint.TotalValue / Convert.ToDouble(accessPoint.Count);
                accessPoint.Save();
            }
            else
            {
                new AccessPoint(session)
                {
                    SectorId = sector,
                    Bssid = value.ReaderId,
                    MaxValue = value.Rssi,
                    MinValue = value.Rssi,
                    TotalValue = value.Rssi,
                    Count = 1,
                    Rssi = value.Rssi
                }.Save();
            }            
        }

        public static List<SectorDataView> GetSectors()
        {
            using (Session session = XpoManager.GetNewSession())
            {
               return session.Query<Sector>().Select(s => new SectorDataView()
                {
                    SensorId = s.SectorId,
                    FloorOid = s.MapId.MapId,
                    Point = new PointView()
                    {
                        X = s.X,
                        Y = s.Y
                    }
                }).ToList();
            }
        }
        public static SortedDictionary<string, SectorView> GetKnownSectors()
        {
            using (Session session = XpoManager.GetNewSession())
            {
                var sectors = session.Query<Sector>().ToList();

                var dict = sectors.Select(s => new
                {
                    key = s.SectorId,
                    sector = new SectorView()
                    {
                        SectorId = s.SectorId,
                        X = s.X,
                        Y = s.Y,
                        Accesspoints =
                            session.Query<AccessPoint>()
                                .Where(w => w.SectorId.SectorId == s.SectorId && w.Rssi > Convert.ToDouble(ConfigManager.GetValue("minRssiValue")))
                                .Select(ss => new AccessPointView()
                                {
                                    SectorId = ss.SectorId.SectorId,
                                    Bssid = ss.Bssid,
                                    Rssi = GetRssiList(ss.Rssi)
                                }).ToList()
                    }
                })
                .Where(w=>w.sector.Accesspoints.Count>0)
                .ToDictionary(d => d.key, d => d.sector);

                return new SortedDictionary<string, SectorView>(dict);
            }
        }

        private static List<double> GetRssiList(double rssi)
        {
            return new List<double>()
            {
                rssi
            };
        }
    }
}
