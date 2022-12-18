using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using DevExpress.Xpo;
using log4net;
using Newtonsoft.Json;
using RestSharp;
using RTLS.Dal.Entity;
using RTLS.Job;
using RTLS.Manager;
using RTLS.Model.Manager;
using RTLS.Model.View;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace RTLS.Engine
{
    partial class EngineService : ServiceBase
    {
        static ILog log = LogManager.GetLogger(typeof(EngineService));        
        public EngineService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                Start();
            }
            catch
            {
              Environment.Exit(0);  
            }
            
        }

        protected override void OnStop()
        {
            MqClient.Disconnect();            
            StartJobs.Stop();
            Environment.Exit(0);
        }        

        public void Start()
        {
            //SqlServerTypes.Utilities.LoadNativeAssemblies(AppDomain.CurrentDomain.BaseDirectory);
            try
            {
                ConfigManager.Init();
                log.Info("Config yüklendi.");

                string _connectionString = "";

                _connectionString = "Server=" + ConfigManager.GetValue("_BaglantiSunucuIp") + ";User ID=" + ConfigManager.GetValue("_BaglantiKullaniciAdi") + ";password=" + ConfigManager.GetValue("_BaglantiSifre") + ";Database=" + ConfigManager.GetValue("_BaglantiDatabase") + "";

                //XpoManager.InitXpo(ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString);
                XpoManager.InitXpo(_connectionString);
                log.Info("Kayit:Veri tabanı bağlantısı gerçekleştirildi.");
                
                //DalManager.Init();

                /*
                GetSectors();
                log.Info("Sectorler okundu.");
                */

                ReaderManager.GetUdeaReaders();
                log.Info("Readerler okundu.");

                MqttClientStart();
                log.Info("MQTT client çalıştı.");

                StartJobs.Start(_sectors);
                log.Info("Job çalıştı.");
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                //throw  new Exception("Hata oluştu.");
            }            
        }

        private static List<SectorDataView> _sectors;
        public static void GetSectors()
        {
            var sectorList = new RestClient(ConfigManager.GetValue("sectorListUrl"));
            var requestSectorList = new RestRequest(Method.GET);
            var responseSector = sectorList.Execute(requestSectorList);

            _sectors = JsonConvert.DeserializeObject<List<SectorDataView>>(responseSector.Content);
            //_sectors = sectors;
            log.Info("Toplam Socket Sayısı:" + _sectors.Count);
            //return;
            using (Session session = XpoManager.GetNewSession())
            {
              foreach (var sector in _sectors)
                {
                    var map = session.Query<Map>().FirstOrDefault(w => w.MapId == sector.FloorOid);
                    if (map == null)
                    {
                        map = new Map(session)
                        {
                            MapId = sector.FloorOid,
                            Name = sector.FloorOid.ToString() + " .Kat"
                        };
                        map.Save();
                    }

                    var sectorUpdate = session.Query<Sector>().FirstOrDefault(w => w.SectorId == sector.SensorId);
                    if (sectorUpdate != null && map != null)
                    {
                        sectorUpdate.MapId = map;
                        sectorUpdate.X = sector.Point.X;
                        sectorUpdate.Y = sector.Point.Y;
                        sectorUpdate.Save();
                    }
                    else
                    {
                        new Sector(session)
                        {
                            SectorId = sector.SensorId,
                            MapId = map,
                            X = sector.Point.X,
                            Y = sector.Point.Y
                        }.Save();
                    }                   
                }
                log.Info("Sector eklendi / güncellendi.");
                var currentSectors = _sectors.Select(s => s.SensorId).ToList();
                foreach (var deleteAccessPoint in session.Query<AccessPoint>().Where(w => !currentSectors.Contains(w.SectorId.SectorId)).ToList())
                {
                    deleteAccessPoint.Delete();
                }
                log.Info("Sector accesspoint silme işlemi tamamlandı.");
                foreach (var deleteSector in session.Query<Sector>().Where(w => !currentSectors.Contains(w.SectorId)).ToList())
                {
                    deleteSector.Delete();
                }
                log.Info("Socket silme işlemi tamamlandı.");
            }
        }

        public static MqttClient MqClient = new MqttClient(ConfigManager.GetValue("mqttBrokerUrl"));
        private void MqttClientStart()
        {            
            try
            {
                //MqttClient mqClient = new MqttClient(ConfigManager.GetValue("mqttBrokerUrl"));
             
                MqClient.MqttMsgPublishReceived += MqClient_MqttMsgPublishReceived;
                string clientId = "RTLSEnginePostgre";
                MqClient.Connect(clientId);
                MqClient.Subscribe(new string[] { "/rtls", "/readerStatus" }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
                log.Info("Mqtt Client Subscribe");
            }
            catch (Exception ex)
            {
                log.Error("MQTT Broker bulunamadı - " + ex.Message);
                Thread.Sleep(1000);
                MqttClientStart();
            }
        }

        private static void MqClient_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
           

            if (e.Topic == "/rtls")
            {
                SetRtls(e);
            }
            else
            {
                if (e.Topic == "/readerStatus")
                {
                    //SetReaderInfo(e);
                }
               
                    
            }
        }

        private static void SetRtls(MqttMsgPublishEventArgs e)
        {
            

            var data = JsonConvert.DeserializeObject<ReadDataView>(Encoding.UTF8.GetString(e.Message));

          

            /*
            try
            {
                if (data.TagType == 2)
                {
                    TagManager.SetTagAlarm(data.TagId);
                }
                if (data.Body != null && data.Body.Length >= 24 && (decimal) Int64.Parse(data.Body.Substring(22, 2), System.Globalization.NumberStyles.HexNumber) == 1)
                {
                    TagManager.SetTagAlarm(data.TagId);
                }
                //if (Convert.ToInt16(data.Body.Substring(22, 2)) == 1)
                //{
                //    TagManager.SetTagAlarm(data.TagId);
                //}
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }            
            */

            if (Double.IsNaN(data.Rssi) || data.Rssi < Convert.ToDouble(ConfigManager.GetValue("minRssiValue")))
            {
                return;
            }

            try
            {
                RssiManagement.SetRssi(data);
            }
            catch (Exception ex)
            {
                log.Error("Tag datası eklenirken hata olustu Tag Id: " + data.TagId + " Error: " + ex.Message);
            }
        }

        private static void SetReaderInfo(MqttMsgPublishEventArgs e)
        {           

            var data = JsonConvert.DeserializeObject<ReaderDataView>(Encoding.UTF8.GetString(e.Message));
            log.Info("Connect Reader ReaderId: " + data.ReaderId);
            ReaderManager.SetReaderInfo(data);
        }
    }

}
