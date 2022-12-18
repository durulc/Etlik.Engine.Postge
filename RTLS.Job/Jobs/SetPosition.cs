using log4net;
using MoreLinq;
using Newtonsoft.Json;
using Quartz;
using RTLS.Manager;
using RTLS.Model;
using RTLS.Model.Response;
using RTLS.Model.View;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RTLS.Job.Jobs
{
    public class SetPosition : IJob
    {
        static ILog log = LogManager.GetLogger(typeof(SetPosition));

        public static SortedDictionary<string, SectorView> KnownSectors = SectorManager.GetKnownSectors();

        PositionManager positionManager = new PositionManager();
        public void Execute(IJobExecutionContext context)
        {            
            try
            {

                if (JobConf.JobStartSetPositionState)
                {
                    //log.Warn("Job running");
                    return;
                }
                JobConf.JobStartSetPositionState = true;
                JobConf.JobStartSetPositionDateTime=DateTime.Now;
                //log
                //log.Info("Start Job");

                Start();
                //log
                //log.Info("Stop Job");

                JobConf.JobStartSetPositionState = false;
            }
            catch (Exception ex)
            {
                JobConf.JobStartSetPositionState = false;
                log.Error("Genel Hata " + ex.Message);                
            }
        }

        //public static Dictionary<string, TagDataView> CollectTagsData; //= new Dictionary<string, TagDataView>();

        private TagDataView GetTagDataView(IEnumerable<ReadDataView> data)
        {
            return new TagDataView()
            {
                _dateIlkOkumaSaati = data.Min(m => m.ReadDateTime),
                _dateSonOkumaSaati = data.Max(m=>m.ReadDateTime),
                ReaderData = data.Select(s=> new TagReaderView()
                {
                    Rssi = s.Rssi,
                    ReaderId = s.ReaderId,
                    LastReadTime = s.ReadDateTime
                }).ToList()
            };
        }

        private Dictionary<string, TagDataView> GetCollectTagsData()
        {
            Dictionary < string, TagDataView > collectData= new Dictionary<string, TagDataView>();

            try
            {
                collectData= TagManager.ReadDataList.ToList().GroupBy(g => g.TagId).ToDictionary(d => d.Key,
                    d => GetTagDataView(d.Select(s => s)));
            }
            catch (Exception ex)
            {
                Thread.Sleep(100);
                
                //log.Error("Collect Data oluşturuluken hata oluştu. TagId = "+ g.TagId + " ex = " + ex.ToString());
                return GetCollectTagsData();
            }
            return collectData;
        }
        private List<KeyValuePair<string, TagDataView>> GetCollectTagData()
        {
            //ICollection<ReadDataView> data;
            //lock (TagManager.ReadDataList)
            //{
            //    data = TagManager.ReadDataList.ToList();
            //}            

            Dictionary<string, TagDataView> collectTagsData = GetCollectTagsData();


            // int intToplam = collectTagsData.ToList().Count;


            var itemTemp = collectTagsData.ToList();

            bool boolPosizyonlama = false;

            string strPosizyonla = "";

            //foreach (var item in itemTemp)
            //{
            //    if (item.Key == "2005")
            //    {
            //       boolPosizyonlama = DateTime.Now.Subtract(item.Value.LastReadTime).TotalSeconds >= Convert.ToDouble(ConfigManager.GetValue("PositionMaxWaitSeconds"));
            //        strPosizyonla = "";
            //        //if (boolPosizyonlama)
            //        //{
            //        //    //JobConf.SetTagLastReadTime(item.Key, item.Value.LastReadTime);
            //        //    strPosizyonla = "Posizla";
            //        //}
            //        log.Info("Son Sinyal = "+item.Value.LastReadTime.ToString("HH:mm:ss:ff") + " Fark = " + DateTime.Now.Subtract(item.Value.LastReadTime).TotalSeconds);
            //    }
            //}




            //log.Info("All Tag Data:" + collectTagsData.Count);
            List <KeyValuePair<string, TagDataView>> collectTagData = new List<KeyValuePair<string, TagDataView>>();

            var items = collectTagsData.ToList().Where(w => w.Value.ReaderData.Count > 0).ToList();

            foreach (var item in items)
            {
               if (Convert.ToBoolean(ConfigManager.GetValue("YeniAlgoritma")))
                {
                    //if (JobConf.GetTagLastReadTime(item.Key).Subtract(item.Value.LastReadTime).TotalSeconds >= Convert.ToInt32(ConfigManager.GetValue("TagFirstCalculateTotalSeconds")))
                    // if (DateTime.Now.Subtract(JobConf.GetTagLastReadTime(item.Key)).TotalSeconds >= Convert.ToInt32(ConfigManager.GetValue("TagFirstCalculateTotalSeconds")))
                    //if (DateTime.Now.Subtract(item.Value._dateSonOkumaSaati).TotalSeconds >= Convert.ToDouble(ConfigManager.GetValue("PositionMaxWaitSeconds")))
                     double _zamanFarki = DateTime.Now.Subtract(item.Value._dateSonOkumaSaati).TotalSeconds;
                    //double _zamanFarki = DateTime.Now.Subtract(JobConf.GetTagLastReadTime(item.Key)).TotalSeconds;
                    if (_zamanFarki >=2 && _zamanFarki <3)
                    {
                       
                       JobConf.SetTagLastReadTime(item.Key, item.Value._dateSonOkumaSaati);
                        collectTagData.Add(new KeyValuePair<string, TagDataView>(item.Key, new TagDataView()
                        {
                            ReaderData = item.Value.ReaderData.Where(w => !double.IsNaN(w.Rssi) && w.Rssi < 0).Select(s => new TagReaderView()
                            {
                                ReaderId = s.ReaderId,
                                Rssi = s.Rssi,
                                LastReadTime = s.LastReadTime
                            }).ToList(),
                            _dateSonOkumaSaati = item.Value._dateSonOkumaSaati,
                            _datePosizyonlamaZamani=DateTime.Now,
                            _dateIlkOkumaSaati=item.Value._dateIlkOkumaSaati
                        }));
                    }
                }
                else
                {

                    if (JobConf.GetTagLastReadTime(item.Key) != item.Value._dateSonOkumaSaati)
                    {
                        JobConf.SetTagLastReadTime(item.Key, item.Value._dateSonOkumaSaati);
                        collectTagData.Add(new KeyValuePair<string, TagDataView>(item.Key, new TagDataView()
                        {
                            ReaderData = item.Value.ReaderData.Where(w => !double.IsNaN(w.Rssi) && w.Rssi < 0).Select(s => new TagReaderView()
                            {
                                ReaderId = s.ReaderId,
                                Rssi = s.Rssi,
                                LastReadTime = s.LastReadTime
                            }).ToList(),
                            _dateSonOkumaSaati = item.Value._dateSonOkumaSaati,
                            _dateIlkOkumaSaati = item.Value._dateIlkOkumaSaati
                        }));
                    }
                }
            }
            collectTagData = collectTagData.Where(w => w.Value.ReaderData.Count > 0).ToList();

            //collectTagData = collectTagData.Where(w => DateTime.Now.Subtract(w.Value.LastReadTime).TotalSeconds >= Convert.ToInt32(ConfigManager.GetValue("TagFirstCalculateTotalSeconds"))).ToList();


            if (Convert.ToBoolean(ConfigManager.GetValue("sendReaderState")))
            {
                //SendReaderState(collectTagData.ToList());
            }

            //log.Info("Filter Tag Data:" + collectTagData.Count);
            return collectTagData;
        }
        private void Start()
        {                                
            if (TagManager.ReadDataList == null || TagManager.ReadDataList.Count == 0)
            {
                log.Warn("No Collected data");
                return;
            }

           

            Parallel.ForEach(GetCollectTagData(), data =>
            {
                try
                {
                   FindTag(data);              


                }
                catch (Exception ex)
                {
                    var message = "Tag Id:" + data.Key + " Error:" + ex.Message;
                    log.Error(message);
                    //return;
                }
            });
        }

        private static ICollection<AccessPointView> GetAccessPointValues(KeyValuePair<string, TagDataView> data)
        {
            ICollection<AccessPointView> accesspoints= new List<AccessPointView>();
            try
            {
                accesspoints=(from p in data.Value.ReaderData //.Where(w => w.Rssi > Convert.ToDouble(ConfigManager.GetValue("minRssiValue")))
                        group p.Rssi by p.ReaderId into g
                    select new AccessPointView() { Bssid = g.Key, Rssi = g.ToList() }).ToList().Where(w => w.Rssi.Count > 0).ToList();
            }
            catch (Exception ex)
            {
                log.Error("Error: " + ex.Message);
                GetAccessPointValues(data);
            }
            return accesspoints;
        }

        private void FindTag(KeyValuePair<string,TagDataView> data )
        {
            /*
             
            ConfigurationManager.RefreshSection("appSettings");

            TimeSpan zamanFarki = DateTime.Now - data.Value.LastReadTime;

            if (data.Key == "2005")
            {

                var mesaj = "Son Okuma = " + data.Value.LastReadTime.ToString("HH:mm:ss") + " Zaman Farkı = " + zamanFarki.TotalSeconds.ToString();
                log.Info(mesaj);
            }


            return;
           
            if (zamanFarki.TotalSeconds < Convert.ToInt16(ConfigManager.GetValue("PositionMaxWaitSeconds")))
                return;
            
            */
            if (data.Value.ReaderData == null ||data.Value.ReaderData.Count < Convert.ToInt16(ConfigManager.GetValue("minReaderNumber")) || data.Value.ReaderData.Any(a=>Double.IsNaN(a.Rssi)))
            {
                return;
            }

            ICollection<AccessPointView> accesspoints = GetAccessPointValues(data).ToList();
            if (accesspoints.Count < 1)
            {
                log.Error("Access Points not found.");
                return;
            }

            long floorOid = 0;
            var accesspointsOrg = accesspoints.Select(s => new AccessPointAverageRssiView()
            {
                Bssid = s.Bssid,
                Rssi = s.Rssi.Average()
            }).ToList();

            var bssids = accesspointsOrg.Select(s => s.Bssid).ToList();
            var readerFloorOids = ReaderManager.RtlsReaders.Where(w => bssids.Contains(w.ReaderId)).GroupBy(g => g.FloorOid).Select(s => new {
                kat = s.Key,
                readerNumber = s.Select(sf => sf.ReaderId).Count(),
                readers = s.Select(ss => ss.ReaderId).ToList()
            }).ToList();

            if (readerFloorOids.Count == 0) return;

            var maxReaderNumber = readerFloorOids.Max(w => w.readerNumber);
            var readers = readerFloorOids.Where(w => w.readerNumber == maxReaderNumber).SelectMany(s => s.readers).ToList();
            var maxRssiReader = accesspointsOrg.Where(w=>readers.Contains(w.Bssid)).OrderByDescending(o => o.Rssi).First();

            var tags = ConfigManager.GetValue("tagFilter").Split(',');

            if (Convert.ToBoolean(ConfigManager.GetValue("tagStaLog")) && tags.Any(w => w == data.Key))
            {
                var message = "";

                /*
                    TimeSpan zamanFarki = DateTime.Now - data.Value.LastReadTime;                

                    message = "Son Okuma Zamanı =" + data.Value.LastReadTime + " Zaman Farkı = "+ zamanFarki.TotalSeconds;
                    log.Info(message);
                */                


                message = "Tag Id:" + data.Key + " Ilk Sinyal Zamanı "+ data.Value._dateIlkOkumaSaati.ToString("HH:mm:ss:ff") + " Son Sinyal Zamanı " + data.Value._dateSonOkumaSaati.ToString("HH:mm:ss:ff") + " KnownSectorcount:" + KnownSectors.Count +   " Accesspoints:" + JsonConvert.SerializeObject(data.Value.ReaderData);
                log.Info(message);

                message = "Tag Id:" + data.Key + " FloorGroups:" + JsonConvert.SerializeObject(readerFloorOids);
                log.Info(message);
            }

            //Start Floor                        
            var alternateMaxRssiReader = accesspointsOrg.OrderByDescending(o => o.Rssi).First();

            if ((alternateMaxRssiReader.Rssi - maxRssiReader.Rssi) >= Convert.ToInt32(ConfigManager.GetValue("floorRssiDifference")))
            {
                maxRssiReader = alternateMaxRssiReader;
            }

            var readerDetail = ReaderManager.RtlsReaders.FirstOrDefault(w => w.ReaderId == maxRssiReader.Bssid);
            if (readerDetail == null) return;
            floorOid = readerDetail.FloorOid;
            //End Floor 


            //Reader filter - aynı kattakileri dikkate al
            var filterReaders = ReaderManager.RtlsReaders.Where(w => w.FloorOid == floorOid).Select(s => s.ReaderId).ToList();
            //accesspoints = accesspoints.Where(w => filterReaders.Contains(w.Bssid) && w.Rssi.Count > 0).ToList();
            //end reader Filter

            //var positionStart = DateTime.Now;
            var positionData = positionManager.FindSector(data.Key, KnownSectors, accesspoints.Where(w => filterReaders.Contains(w.Bssid) && w.Rssi.Count > 0).ToList());
            if (positionData.Point==null ||float.IsNaN(positionData.Point.X) || float.IsNaN(positionData.Point.Y))
            {
                log.Error("Tag pozisyonlanamadi TagId: " + data.Key + " Accesspoints: " + JsonConvert.SerializeObject(accesspoints));              
            }

            SendPosition(data.Key, positionData, floorOid);
            
            //new Thread(() =>
            //{
            //    SendPosition(data.Key, positionManager.FindSector(KnownSectors, accesspoints), floorOid);
            //}).Start();
        }       

        private List<ReadDataView> GetReadReaders(List<KeyValuePair<string, TagDataView>> collectTagData)
        {
            List<ReadDataView> readReaderData = new List<ReadDataView>();
            try
            {

                readReaderData = collectTagData.SelectMany(s => s.Value.ReaderData).ToList().Select(s => new ReadDataView()
                {
                    ReaderId = s.ReaderId
                }).ToList().DistinctBy(d=>d.ReaderId).ToList();
            }
            catch
            {
                log.Error("Readerlar okuma distinct hatasi.");
               Thread.Sleep(1000);
                GetReadReaders(collectTagData);
            }
            return readReaderData;
        }

        private void SendReaderState(List<KeyValuePair<string, TagDataView>> collectTagData)
        {
            var readers = GetReadReaders(collectTagData);
            log.Info("Reader Bilgisi yazildi. Reader sayisi: " + readers.Count);
            try
            {
                ReaderManager.SetReadReaders(readers);
            }
            catch (Exception ex)
            {
                log.Error("Reader State Yazma Hatasi: " + ex.Message );
            }                      
        }

        private void SendPosition(string tagId, PositionResponse position, double floorOid)
        {
            if (position.Point == null || float.IsNaN(position.Point.X) || float.IsNaN(position.Point.Y) || position.SectorDifference.Count == 0)
            {
                log.Error("Tag Pozisyonlanamadi Tag Id: " + tagId );
                return;
            }

            float radius = 0;
            var positionItem = new PositionItem()
            {
                TagId = tagId,
                FloorOid = floorOid,
                Radius = radius,
                Position = GetPosition(position)
            };


            //try
            //{
            //    TagManager.SetPosition(positionItem);
            //}
            //catch (Exception ex)
            //{
            //    log.Error("Tag PositionYazma Hatasi: " + ex.Message);
            //}

            new Thread(() =>
            {
                try
                {
                    TagManager.SetPosition(positionItem);
                }
                catch (Exception ex)
                {
                    log.Error("Tag PositionYazma Hatasi: " + ex.Message);
                }
            }).Start();

            //TagManager.SetPosition(positionItem);

            //var items = new List<PositionItem>
            //{
            //    positionItem
            //    //new PositionItem()
            //    //{
            //    //    TagId= tagId,
            //    //    FloorOid=floorOid,
            //    //    Radius=radius,
            //    //    Position = GetPosition(position)
            //    //}
            //};

            //var data = JsonConvert.SerializeObject(items);


            //var setPositionUrl = new RestClient(ConfigManager.GetValue("setPositionUrl"));
            //var requestSetPosition = new RestRequest(Method.POST);
            //requestSetPosition.RequestFormat = DataFormat.Json;
            //requestSetPosition.AddBody(items);

            //setPositionUrl.ExecuteAsync(requestSetPosition, response =>
            //{
            //    if (response.StatusCode != HttpStatusCode.NoContent)
            //    {
            //        log.Error("Pozisyon bilgisi sunucuya gönderilemedi");
            //    }
            //});

            //var cancellationTokenSource = new CancellationTokenSource();
            //setPositionUrl.ExecuteAsync(requestSetPosition, cancellationTokenSource.Token);
            //if (responsePosition.StatusCode != HttpStatusCode.NoContent)
            //{
            //    log.Error("Pozisyon bilgisi sunucuya gönderilemedi");
            //}

            //try
            //{
            //    if (data!= null)
            //        MqJClient.Publish("/setTagPosition", Encoding.UTF8.GetBytes(data), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
            //}
            //catch (Exception ex)
            //{
            //    log.Error("MQTT /setTagPosition" + ex.Message);
            //}


            var tags = ConfigManager.GetValue("tagFilter").Split(',');
            //if (Convert.ToBoolean(ConfigManager.GetValue("tagStaLog")) && StartJobs.Tags.Any(w => w == tagId))
            if (Convert.ToBoolean(ConfigManager.GetValue("tagStaLog")) && tags.Any(w => w == tagId))
            {
                var message = "Tag Id:" + tagId + " Kat: " + floorOid + " Position:" + JsonConvert.SerializeObject(position);
                log.Info(message);

                message = "   ";
                log.Info(message);
            }
        }

        private PointView GetPosition(PositionResponse position)
        {
            if (Convert.ToBoolean(ConfigManager.GetValue("tagPositionedNearSector")))
            {
                return new PointView()
                {
                    X = position.SectorDifference[0].X,
                    Y = position.SectorDifference[0].Y
                };
            }
            return new PointView()
            {
                X = position.Point.X,
                Y = position.Point.Y
            };
        }
    }    
}
