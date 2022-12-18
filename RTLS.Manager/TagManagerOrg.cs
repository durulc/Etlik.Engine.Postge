//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using DevExpress.Data.Filtering;
//using DevExpress.Xpo;
//using DevExpress.Xpo.DB;
//using log4net;
//using RTLS.Dal.Entity;
//using RTLS.Model;
//using RTLS.Model.Manager;
//using RTLS.Model.View;

//namespace RTLS.Manager
//{
//    public class TagManager
//    {
//        static ILog log = LogManager.GetLogger(typeof(TagManager));

//        public static void SetPosition(PositionItem tagPosition)
//        {
//            using (Session session = XpoManager.GetNewSession())
//            {
//                SelectedData data = session.ExecuteSproc("dbo.SetTagPositionSp", new OperandValue(tagPosition.TagId), new OperandValue(tagPosition.FloorOid), new OperandValue(tagPosition.Position.X), new OperandValue(tagPosition.Position.Y));
//            }
//        }

//        public static void WriteTag(Session session, ReadDataView data)
//        {
//            new TagSta(session)
//            {
//                Rssi = data.Rssi,
//                TagId = data.TagId,
//                Bssid = data.ReaderId,
//                CreateDate = DateTime.Now
//            }.Save();
//        }

//        public static Dictionary<string, TagDataView>  CollectTagData;
//        public static void CollectData(ReadDataView data)
//        {

//                if (CollectTagData == null) { CollectTagData = new Dictionary<string, TagDataView>(); }

//                var tag = CollectTagData.FirstOrDefault(w => w.Key == data.TagId);
//                var nowDateTime = DateTime.Now;
//                if (tag.Key == null)
//                {
//                    CollectTagData.Add(data.TagId, new TagDataView()
//                    {
//                        LastReadTime = nowDateTime,
//                        ReaderData = new List<TagReaderView>()
//                    {
//                        new TagReaderView()
//                        {
//                            ReaderId = data.ReaderId,
//                            Rssi = data.Rssi,
//                            LastReadTime = nowDateTime
//                        }
//                    }
//                    });
//                    return;
//                }

//                tag.Value.LastReadTime = nowDateTime;
//                tag.Value.ReaderData.Add(new TagReaderView()
//                {
//                    LastReadTime = nowDateTime,
//                    Rssi = data.Rssi,
//                    ReaderId = data.ReaderId
//                });

//            RemoveTagData(tag, nowDateTime);
//        }

//        private static void RemoveTagData(KeyValuePair<string, TagDataView> tag, DateTime nowDateTime)
//        {
//            try
//            {
//                while (nowDateTime.Subtract(tag.Value.ReaderData.First().LastReadTime).TotalSeconds > Convert.ToInt32(ConfigManager.GetValue("TagFirstCalculateTotalSeconds")))
//                {
//                    tag.Value.ReaderData.Remove(tag.Value.ReaderData.First());
//                    if (tag.Value.ReaderData.Count == 0)
//                    {
//                        CollectTagData.Remove(tag.Key);
//                    }
//                }
//            }
//            catch (Exception ex)
//            {
//                log.Error("Tag Id: " + tag.Key + " Error:" + ex.Message);
//                Thread.Sleep(200);
//                RemoveTagData(tag,nowDateTime);                     
//            }
//        }
//    }

//    //public class RemoveItem
//    //{
//    //    private object LOCK = new object();
//    //    public void EnsureLimitConstraint(ICollection<TagReaderView> items, DateTime nowDateTime)
//    //    {           
//    //        lock (LOCK)
//    //        {
//    //            foreach (var item in items.Where(w=>nowDateTime.Subtract(w.LastReadTime).TotalSeconds > Convert.ToInt32(ConfigManager.GetValue("TagFirstCalculateTotalSeconds"))))
//    //            {
//    //                items.Remove(item);
//    //            }
//    //            //while (items.Count > Convert.ToInt32(ConfigManager.GetValue("maxTagReaderCount")))
//    //            //{                   
//    //            //    items.Remove(items.First());
//    //            //}               
//    //        }
//    //    }

//    //}
//}
