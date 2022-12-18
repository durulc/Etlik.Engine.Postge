using System;
using System.Collections.Generic;
using System.Linq;
using MediRef.Business.Manager;
using MediRef.Common.Models.Entity;
using MediRef.Common.Models.View;
using RTLS.Model;

namespace DAL.Manager
{
    public static class DalManager
    {
        public static void SendTagPosition(List<PositionItem> data)
        {
            var sendData = data.Select(s => new PositionView()
            {
                FloorOid =  (long)s.FloorOid,
                Position =  new Position()
                {
                    X = s.Position.X,
                    Y = s.Position.Y
                },
                TagId = s.TagId
            
            }).ToList();
            TagManager.Instance.SetPosition(sendData);
        }

        public static void SendReaderState(List<string> data)
        {
            var sendData = data.Select(s => new ReaderView()
            {
                ReaderId=s

            }).ToList();
            ReaderManager.Instance.ReaderReadTime(sendData);
        }

        public static void Init()
        {
            //SqlServerTypes.Utilities.LoadNativeAssemblies(AppDomain.CurrentDomain.BaseDirectory);
            //Database
            MediRef.Model.Manager.XpoManager.Instance.InitXpo(System.Configuration.ConfigurationManager.ConnectionStrings["ConnectionStringMediref"].ConnectionString);            
        }

    }
}
