using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using DevExpress.Utils.Filtering.Internal;
using log4net;
using Newtonsoft.Json;
using RTLS.Manager.Helper;
using RTLS.Model;
using RTLS.Model.Enums;
using RTLS.Model.Response;
using RTLS.Model.View;

namespace RTLS.Manager
{
    public class PositionManager
    {
        static ILog log = LogManager.GetLogger(typeof(PositionManager));
        private PointView GetReaderPoint(string readerId)
        {
            var readerPoint = ReaderManager.RtlsReaders.FirstOrDefault(w => w.ReaderId == readerId);
            if (readerPoint != null)
                return readerPoint.Point;
            else
            {
                return new PointView();
            }
            //return new PointView();
        }
        public PositionResponse FindReader(ICollection<TagReaderView> data)
        {           
            if (data.Count == 1)
            {
                var reader=ReaderManager.RtlsReaders.FirstOrDefault(w => w.ReaderId == data.First().ReaderId);                
                return new PositionResponse()
                {
                    PositionType = EPositionType.Reader,
                    SectorDifference = new List<SectorDif>()
                    {
                        new SectorDif()
                        {
                            DRssi = data.First().Rssi,
                            X = reader.Point.X,
                            Y = reader.Point.Y,
                            SectorId = reader.ReaderId                            
                        }
                    },
                    Point = reader.Point
                };
            }

            var power = Convert.ToInt16(ConfigManager.GetValue("AntennePower"));
            var readerRssiValues = (from p in data //.Where(w => w.Rssi > Convert.ToDouble(ConfigManager.GetValue("minRssiValue")))
                                    group p.Rssi by p.ReaderId into g
                                    select new SectorDifference(g.Key, GetReaderPoint(g.Key).X, GetReaderPoint(g.Key).Y, g.Max())).ToList().OrderBy(o => o.getRssi()).ToList();
            
            var readerDistanceValues =
                readerRssiValues.Where(
                        w =>
                            RssiManagement.BeaconCalculateDistance(power, w.getRssi()) <
                            Convert.ToDouble(ConfigManager.GetValue("maxDistanceValueCentimeter")))                            
                    .Select(
                        s =>
                            new SectorDifference(s.getSectorId(), s.getX(), s.getY(),
                                RssiManagement.BeaconCalculateDistance(power, s.getRssi()))).ToList()
                                .Where(ww=>ww.getRssi() > 0).ToList();

            //log
            //log.Info("Reader Intersecting; Time:" + DateTime.Now + " count: " + readerDistanceValues.Count);

            var geometry = GetCoordinatIntersect(readerDistanceValues);
            //log
            //log.Info("Reader Intersected; Time:" + DateTime.Now);

            return new PositionResponse()
            {
                PositionType = EPositionType.Reader,
                SectorDifference = readerDistanceValues.Select(s => new SectorDif()
                {
                    SectorId = s.getSectorId(),
                    X = s.getX(),
                    Y = s.getY(),
                    DRssi = s.getRssi()
                }).OrderBy(o => o.DRssi).ToList(),
                Point = geometry
            };
        }

        public PositionResponse FindSector(string tagId, SortedDictionary<string, SectorView> knownSectors, ICollection<AccessPointView> accesspoints)
        {
            var tags = ConfigManager.GetValue("tagFilter").Split(',');
            var message = "";
            var readerIds = ReaderManager.RtlsReaders.Select(s=>s.ReaderId).ToList();
            var currentReaders = accesspoints.Where(w => readerIds.Contains(w.Bssid) && w.Rssi.Count > 0).Select(s => new TagReaderView()
            {
                Rssi = s.Rssi.Average(),
                ReaderId = s.Bssid,
            }).ToList().Where(w=> !double.IsNaN(w.Rssi) && w.Rssi < 0).ToList();

            //log.Info("Finding Reader:" + DateTime.Now);
            var readerPosition = FindReader(currentReaders);
            //log.Info("Fined Reader:" + DateTime.Now);
            if (knownSectors.Count == 0)
            {
                return readerPosition;
            }

            LinkedList<BlindMeasurement> measured = new LinkedList<BlindMeasurement>();

            Dictionary<string, RssiCalcHelper> calcList = accesspoints.Select(s => s).ToDictionary(d => d.Bssid, d => GetRssiCalcHelper(d.Rssi));

            foreach (KeyValuePair<string, RssiCalcHelper> keyValPair in calcList)
            {
                string bssid = keyValPair.Key;
                RssiCalcHelper tempRssiHelper = keyValPair.Value;
                double averageRssi = tempRssiHelper.getAverageRssi();
                double averageFluctuation = tempRssiHelper.getAverageFluctuation();
                measured.AddLast(new BlindMeasurement(bssid, averageRssi, averageFluctuation));
            }
            List<SectorDifference> results = new List<SectorDifference>();

            int counterMap = 0;

            foreach (KeyValuePair<string, SectorView> keyValPair in knownSectors)
            {
                string sector = keyValPair.Key;
                SectorView tempSector = keyValPair.Value;


                int counter = 0;

                Dictionary<string, AccessPointView> tempApMap = tempSector.Accesspoints.Select(s => s).ToDictionary(d => d.Bssid, d => d);

                double result = 0;

                foreach (BlindMeasurement apData in measured)
                {
                    char[] delimiter = new char[] { '|' };
                    string[] splitted = apData.getBssid().Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
                    apData.setBssid(splitted[0]);

                    if (tempApMap.ContainsKey(apData.getBssid()))
                    {
                        AccessPointView apTemp = tempApMap[apData.getBssid()];

                        double tempResult = Math.Abs(apTemp.Rssi.Average() - apData.getRssi());

                        //if (
                        //    (standardError == false && tempResult <= apData.getAverageFluctuation()) ||
                        //    (standardError == true && tempResult <= this.errorBuffer)
                        //   )
                        //{
                        //    tempResult = 0;
                        //}

                        result += tempResult;
                        counter++;
                    }
                }
                counterMap++;

                double average = 0;

                if (counter > 0)
                {
                    average = result / counter;
                    results.Add(new SectorDifference(tempSector.SectorId, tempSector.X, tempSector.Y, average));
                }
            }

            results.Sort(delegate (SectorDifference s1, SectorDifference s2)
            {
                return s1.getRssi().CompareTo(s2.getRssi());
            });

            if (Convert.ToBoolean(ConfigManager.GetValue("tagStaLog")) && tags.Any(w => w == tagId))
            {
                message = "Tag Id:" + tagId + " results:" + JsonConvert.SerializeObject(results.Select(s=> new
                {
                    rssi=s.getRssi(),                    
                    sectorId=s.getSectorId(),
                    X = s.getX(),
                    Y=s.getY()
                }).ToList());
                log.Info(message);
            }

            //start filtering
            var sectorCount = 0;
            Int64 distanceValue = 1;
            List<SectorDifference> filterResult = new List<SectorDifference>();
            while (sectorCount < 4 || sectorCount==results.Count)
            {
                distanceValue++;
                filterResult =
                    results.Where(
                        w =>
                            GetDistance(w.getX(), w.getY(), readerPosition.Point.X, readerPosition.Point.Y) <
                            distanceValue).ToList();

                sectorCount = filterResult.Count;
                if (distanceValue > Convert.ToDouble(ConfigManager.GetValue("readerSectorMaxDistanceCentimeter"))) break;
            }

            results = filterResult.OrderBy(o => o.getRssi()).ToList();            
            //end filtering             

            if (Convert.ToBoolean(ConfigManager.GetValue("tagStaLog")) && tags.Any(w => w == tagId))
            {
                message = "Tag Id:" + tagId + " filter results:" + JsonConvert.SerializeObject(results.Select(s => new
                {
                    rssi = s.getRssi(),
                    sectorId = s.getSectorId(),
                    X = s.getX(),
                    Y = s.getY()
                }).ToList());
                log.Info(message);
            }

            if (results.Count == 0)
            {
                return readerPosition;
            }

            //results = results.Where(w => !double.IsNaN(w.getRssi())).ToList();
            //if (results.Count == 0) throw new Exception("Pozisyonlama için geçerli sonuç bulunamadı.");

            //log
            //log.Info("Position Filtering ; Time:" + DateTime.Now + " count: " + results.Count);

            int takeValue = 0;
            foreach (var item in results)
            {
                if ((item.getRssi() / results[0].getRssi()) < (Convert.ToDouble(ConfigManager.GetValue("maxRssiDifRatio"))/100.0))
                {
                    takeValue++;
                }
                else break;
            }
            
            if (Convert.ToBoolean(ConfigManager.GetValue("tagStaLog")) && tags.Any(w => w == tagId))
            {
                message = "Tag Id:" + tagId + " takeValue:" + takeValue;
                log.Info(message);
            }

            if (takeValue == 1)
            {
                return new PositionResponse()
                {
                    PositionType = EPositionType.Sector,
                    Point = new PointView()
                    {
                        X = results[0].getX(),
                        Y = results[0].getY()
                    },
                    SectorDifference = results.ToList()
                        .Select(s => new SectorDif()
                        {
                            SectorId = s.getSectorId(),
                            X = s.getX(),
                            Y = s.getY(),
                            DRssi = s.getRssi(),
                        }).ToList()
                };
            }
            //get Rssi
            var readerRssiValues =
                      results.Take(takeValue).ToList()
                           .Select(
                               s =>
                                   new SectorDifference(s.getSectorId(), s.getX(), s.getY(),
                                       s.getRssi()*(1))).ToList();
            //log
            //log.Info("Returning Position Time:" + DateTime.Now);
            readerRssiValues = readerRssiValues.ToList();
            var finalPosition= new PositionResponse()
            {
                PositionType = EPositionType.Sector,
                Point = GetCoordinatIntersect(readerRssiValues),
                SectorDifference = readerRssiValues
                .Select(s => new SectorDif()
                {
                    SectorId = s.getSectorId(),
                    X = s.getX(),
                    Y = s.getY(),
                    DRssi = s.getRssi(),
                }).ToList()
            };
            //log
            //log.Info("Returned Position Time:" + DateTime.Now);
            if (finalPosition.SectorDifference.Count==0)
            {
                return readerPosition;
            }

            return finalPosition;
        }

        private static double GetDistance(double x1, double y1, double x2, double y2)
        {
            return Math.Abs(Math.Sqrt(Math.Pow((x2 - x1), 2) + Math.Pow((y2 - y1), 2)));
        }

        private RssiCalcHelper GetRssiCalcHelper(List<Double> rssis)
        {            
            RssiCalcHelper rssiCalcHelper = new RssiCalcHelper(rssis[0]);
            rssis.RemoveAt(0);
            foreach (var item in rssis)
            {
                rssiCalcHelper.addRssi(item);
            }
            return rssiCalcHelper;
        }

        public static double Hypotenuse(long a, long b)
            {
                return Math.Sqrt(Math.Pow(a, 2) + Math.Pow(b, 2));
            }

        public PointView GetCoordinatIntersect(List<SectorDifference> data)
            {
                data = data.Where(w => !double.IsNaN(w.getRssi()) && w.getRssi() > 0).ToList();

                if (data.Count == 0)
                {
                    log.Error("Coordinat Intersect datası bulunamadı.");
                    return new PointView();
                }

                if (data.Count == 1)
                {
                    return new PointView()
                    {
                        X = data[0].getX(),
                        Y = data[0].getY()
                    };
                }

                //bool noIntersect = true;
                double value = 0.0;
            //log
            //log.Info("Iteration Start Time:" + DateTime.Now);

            //En büyük yarıçap çarpanını bul                
                    for (int i=0; i < data.Count-1; i++)
                    {
                        for (int ii = i+1; ii < data.Count; ii++)
                        {                            
                            var item1 = data[i];
                            var item2 = data[ii];                            
                            double r = ((Hypotenuse(item1.getX() - item2.getX(), item1.getY() - item2.getY()))/(item1.getRssi() + item2.getRssi()));
                            if (r > value) value = r;
                        }
                    }
            //log
            //log.Info("Iteration End Time:" + DateTime.Now + " count:" + (value / 0.001));

            //Dairelerin kesişim alanını bul ve ortalama X,Y değerlerini çıkart
            PointF centroid= PointF.Empty;
            List<PointF> centers = data.Select(s => new PointF()
            {
                X = s.getX(),
                Y = s.getY()
            }).ToList();
            List<float> radii = data.Select(s => Convert.ToSingle(s.getRssi() * value)).ToList();
                double oldValue = value;
            while (centroid.IsEmpty || float.IsNaN(centroid.X)  || float.IsNaN(centroid.Y))
                {               
                using (Bitmap img = new Bitmap(1, 1))
                    {
                        using (Graphics graphics = Graphics.FromImage(img))
                        {
                            using (Region intersection = FindCircleIntersections(centers, radii))
                            {
                                if (intersection != null)
                                {
                                    graphics.FillRegion(Brushes.LightGreen, intersection);
                                    centroid = RegionCentroid(intersection, graphics.Transform);
                                }
                            }
                        }
                    }

                value = value + (Convert.ToDouble(ConfigManager.GetValue("IntersectionIterationRation")) / 100.0);
                radii = data.Select(s => Convert.ToSingle(s.getRssi() * value)).ToList();

                    var maxRadii = Convert.ToInt64(ConfigManager.GetValue("circleMaxDistanceValueCentimeter"));

                    if (radii.Max() > maxRadii)
                    {
                        var oldData = data;

                        data = data.Where(w => w.getRssi() * value < maxRadii).ToList();
                        if (data.Count == 0)
                        {
                        
                            log.Warn("Koordinat bulunamadi. Error: " + JsonConvert.SerializeObject(oldData.Select(s=>new
                            {
                                rssi=s.getRssi(),
                                sector=s.getSectorId(),
                                X=s.getX(),
                                YearInterval=s.getY()
                            }).ToList()) );
                            return new PointView(); // GetCoordinat(oldData);                            
                        }
                        //log.Error("Yarı çap aşırı büyük: " + radii.Max());
                        centers = data.Select(s => new PointF()
                            {
                                X = s.getX(),
                                Y = s.getY()
                            }).ToList();

                        radii = data.Select(s => Convert.ToSingle(s.getRssi() * value)).ToList();
                        if (centers.Count == 0 || radii.Count == 0)
                        {
                            log.Error("Center ve radii küçültülemedi");
                            return new PointView();
                        }
                }

                    if ((value / oldValue) > (Convert.ToDouble(ConfigManager.GetValue("iterationMaxValue"))/100.0))
                    {
                        var tempData = data.Select(s => new
                        {
                            Rssi = s.getRssi() * value,
                            Sector = s.getSectorId(),
                            X = s.getX(),
                            Y = s.getY()
                        }).ToList();

                        log.Error("Value: " + value + " Max Value: " + radii.Max() + " Data: " +  JsonConvert.SerializeObject(tempData));
                        return new PointView(); // GetCoordinat(data);
                    }
                //radii = radii.Select(s => s + Convert.ToInt16(ConfigManager.GetValue("IntersectionIterationRation"))).ToList();
                //value = value + 0.01;
            }

            //log
            //log.Info("Kesişim End Time:" + DateTime.Now + " count:" + (value / 0.001));

            return new PointView()
                {
                    X = Convert.ToInt64(centroid.X),
                    Y = Convert.ToInt64(centroid.Y)
            };
            }

        private Region FindCircleIntersections(List<PointF> centers, List<float> radii)
        {
            if (centers.Count < 1) return null;

            // Make a region.
            Region result_region = new Region();

            // Intersect the region with the circles.
            for (int i = 0; i < centers.Count; i++)
            {
                using (GraphicsPath circle_path = new GraphicsPath())
                {
                    circle_path.AddEllipse(
                        centers[i].X - radii[i], centers[i].Y - radii[i],
                        2 * radii[i], 2 * radii[i]);
                    result_region.Intersect(circle_path);
                }
            }

            return result_region;
        }

        private PointF RegionCentroid(Region region, Matrix transform)
        {
            float mx = 0;
            float my = 0;
            float total_weight = 0;
            foreach (RectangleF rect in region.GetRegionScans(transform))
            {
                float rect_weight = rect.Width * rect.Height;
                mx += rect_weight * (rect.Left + rect.Width / 2f);
                my += rect_weight * (rect.Top + rect.Height / 2f);
                total_weight += rect_weight;
            }

            return new PointF(mx / total_weight, my / total_weight);
        }

        public PointView GetCoordinat(List<SectorDifference> data)
        {
            if (data.Count == 0) { return new PointView(); }

            double maxValue = data.Max(m => m.getRssi());
            double totalVariable = 0.0;
            double totalX = 0.0;
            double totalY = 0.0;
            foreach (var item in data)
            {
                double weight = (maxValue - item.getRssi());
                totalX = totalX + item.getX()* weight;
                totalY = totalY + item.getY()* weight;
                totalVariable = totalVariable + weight;
            }

            if (totalVariable < 0.001)
            {
                return new PointView()
                {
                    X = Convert.ToInt64(data.Average(a=>a.getX())),
                    Y = Convert.ToInt64(data.Average(a => a.getY()))
                };
            }

            return new PointView()
            {
                X = Convert.ToInt64(totalX / totalVariable),
                Y = Convert.ToInt64(totalY / totalVariable)
            };
        }

        public PointView GetCoordinatOld(List<SectorDifference> data)
        {
            if (data.Count == 0) { return new PointView(); }

            if (data.Count == 1)
            {
                return new PointView()
                {
                    X = Convert.ToInt64(data.First().getX()),
                    Y = Convert.ToInt64(data.First().getY())
                };
            }

            List<SectorDifference> result = new List<SectorDifference>();
            double maxValue = data.Max(m => m.getRssi());
            double minValue = data.Min(m => m.getRssi());
            double referenceValue = maxValue + minValue;
            double different = (maxValue - minValue);

            if (different < 0.01)
            {
                return new PointView()
                {
                    X = Convert.ToInt64(data.Select(s => s.getX()).Average()),
                    Y = Convert.ToInt64(data.Select(s => s.getY()).Average())
                };
            }

            double a = 1;
            double totalVariable = 0;
            foreach (var item in data)
            {
                //totalVariable = totalVariable + Math.Pow((1 - item.getRssi() / maxValue), a); 
                totalVariable = totalVariable + (1 - item.getRssi() / referenceValue);
            }
            if (totalVariable < 0.01)
            {
                return new PointView()
                {
                    X = Convert.ToInt64(data.Select(s => s.getX()).Average()),
                    Y = Convert.ToInt64(data.Select(s => s.getY()).Average())
                };
            }
            totalVariable = 1 / totalVariable;
            double tx = 0;
            double ty = 0;
            foreach (var itemData in data)
            {
                tx = tx + totalVariable * (itemData.getX() * (1 - itemData.getRssi() / referenceValue));
                ty = ty + totalVariable * (itemData.getY() * (1 - itemData.getRssi() / referenceValue));
            }
            return new PointView()
            {
                X = Convert.ToInt64(tx),
                Y = Convert.ToInt64(ty)
            };
        }
    }
}
