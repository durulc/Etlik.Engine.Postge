using System;
using System.Collections.Generic;
using System.Linq;
using DevExpress.Data.Filtering;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using log4net;
using RTLS.Dal.Entity;
using RTLS.Model;
using RTLS.Model.Manager;
using RTLS.Model.View;

namespace RTLS.Manager
{
    public class TagManager
    {
        static ILog log = LogManager.GetLogger(typeof(TagManager));

        public static ICollection<ReadDataView> ReadDataList = new List<ReadDataView>();

        public static void CollectData(ReadDataView data)
        {
            var readDateTime = DateTime.Now;            

            ReadDataList.Add(new ReadDataView()
            {
                Rssi = data.Rssi,
                ReaderId = data.ReaderId,
                TagId = data.TagId,
                TagType = data.TagType,
                ReadDateTime = readDateTime
            });
            try
            {
                RemoveTagData(data.TagId, readDateTime);
            }
            catch (Exception ex)
            {
                log.Error("Tag datası uzaklaştırılırken hata oluştu. Error: " + ex.Message);
            }            
        }

        private static void RemoveTagData(string tagId, DateTime nowDateTime)
        {
            foreach (var item in ReadDataList.Where(w => w.TagId == tagId && nowDateTime.Subtract(w.ReadDateTime).TotalSeconds >= Convert.ToInt32(ConfigManager.GetValue("TagFirstCalculateTotalSeconds"))).ToList())
            {
                ReadDataList.Remove(item);
            }            
        }

        public static void WriteTag(Session session, ReadDataView data)
        {
            new TagSta(session)
            {
                Rssi = data.Rssi,
                TagId = data.TagId,
                Bssid = data.ReaderId,
                CreateDate = DateTime.Now
            }.Save();
        }

        public static void SetPosition(PositionItem tagPosition)
        {
            using (Session session = XpoManager.GetNewSession())
            {
                /*
                log.Info("tagPosition.TagId = " + tagPosition.TagId);
                log.Info("tagPosition.FloorOid = " + tagPosition.FloorOid);
                log.Info("tagPosition.Position.X = " + tagPosition.Position.X);
                log.Info("tagPosition.Position.Y = " + tagPosition.Position.Y);
                */

                string _strPosizyon = "'POINT("+ tagPosition.Position.X.ToString() + " "+ tagPosition.Position.Y.ToString() + ")'";


                /*
                                log.Info("TagId = " + tagPosition.TagId);
                                log.Info("FloorOid = " + tagPosition.FloorOid);
                                log.Info("_strPosizyon = " + _strPosizyon);
                                */
                /*
               SelectedData data = session.ExecuteSproc(
                   "__settagpositionsp",
                   new OperandValue(tagPosition.TagId),
                   new OperandValue(tagPosition.FloorOid),
                   new OperandValue(tagPosition.Position.X.ToString()),
                   new OperandValue(tagPosition.Position.Y.ToString())
                   );
                   */


                SelectedData data = session.ExecuteSproc(
                    "__settagpositionsp",
                    new OperandValue(tagPosition.TagId),
                    new OperandValue(tagPosition.FloorOid),
                    new OperandValue(tagPosition.Position.X.ToString()),
                    new OperandValue(tagPosition.Position.Y.ToString())
                    );

            }
        }

        public static void SetTagAlarm(String tagId)
        {
            using (Session session = XpoManager.GetNewSession())
            {
              //  SelectedData data = session.ExecuteSproc("dbo.SetTagAlarmSp", new OperandValue(tagId));
            }
        }

    }
}
