using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading.Tasks;
using DevExpress.Xpo;
using Newtonsoft.Json;
using RestSharp;
using RTLS.Dal.Entity;
using RTLS.Model.Manager;
using RTLS.Model.View;

namespace RTLS.Manager
{
    public class ReaderManager
    {
        public static List<RtlsReader> RtlsReaders= new List<RtlsReader>();

        public static void SetReaderInfo(ReaderDataView item)
        {
            using (Session session = XpoManager.GetNewSession())
            {
                var reader = session.GetObjectByKey<ReaderState>(item.ReaderId);
                if (reader == null)
                {
                    new ReaderState(session)
                    {
                        ReaderId = item.ReaderId,
                        IpAddress = item.IpAddress,
                        HardwareVersion = item.HardwareVersion,
                        ProtocolVersion = item.ProtocolVersion,
                        SoftwareVersion = item.SoftwareVersion,
                        LastConnectedDateTime = DateTime.Now
                    }.Save();
                }
                else
                {
                    reader.IpAddress = item.IpAddress;
                    reader.HardwareVersion = item.HardwareVersion;
                    reader.SoftwareVersion = item.SoftwareVersion;
                    reader.ProtocolVersion = item.ProtocolVersion;
                    reader.LastConnectedDateTime = DateTime.Now;
                    reader.Save();
                }
            }
        }

        private static void SetReaderReadTime(ReadDataView item)
        {
            using (Session session = XpoManager.GetNewSession())
            {
                var reader = session.GetObjectByKey<ReaderState>(item.ReaderId);
                if (reader == null)
                {
                 new ReaderState(session)
                 {
                     ReaderId = item.ReaderId,
                     LastReadDateTime = DateTime.Now
                 }.Save();    
                }
                else
                {
                    reader.LastReadDateTime = DateTime.Now;
                    reader.Save();
                }
            }
        }
        public static void SetReadReaders(List<ReadDataView> readerReadList)
        {
            Parallel.ForEach(readerReadList, item =>
            {
                SetReaderReadTime(item);
            });
        }
        public static void GetUdeaReaders()
        {
            var readerList = new RestClient(ConfigManager.GetValue("readerListUrl"));
            var requestReaderList = new RestRequest(Method.GET);
            var responseReader = readerList.Execute(requestReaderList);
            RtlsReaders = JsonConvert.DeserializeObject<List<RtlsReader>>(responseReader.Content);
        }
    }
    public class RtlsReader
    {
        public string ReaderId { get; set; }
        public int RunningGroup { get; set; }
        public long FloorOid { get; set; }
        public PointView Point { get; set; }
    }
}
