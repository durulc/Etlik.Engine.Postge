using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using log4net;
using Quartz;
using Quartz.Impl;
using RTLS.Job.Jobs;
using RTLS.Manager;
using RTLS.Model.View;

namespace RTLS.Job
{    
    public class StartJobs
    {
        static ILog log = LogManager.GetLogger(typeof(StartJobs));
        ////Start Job
        //private static IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();

        public static List<SectorDataView> Sectors;
        //Start Job
        private static IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();

        public static void Clear()
        {
            scheduler.Clear();
        }

        public static void Stop()
        {
            //SetPosition.MqJClient.Disconnect();
            scheduler.Shutdown();
        }

        //public static string[] Tags;
        public static void Start(List<SectorDataView> sectors)
        {
            JobConf.JobStartSetPositionDateTime = DateTime.Now;
            Sectors = sectors;
            log.Info("Started Jobs.");

            //Tags = ConfigManager.GetValue("tagFilter").Split(',');

            //Start Job
            //scheduler = StdSchedulerFactory.GetDefaultScheduler();
            //Jobs
            IJobDetail positionJob = JobBuilder.Create<SetPosition>().WithIdentity("SetPositionJob").Build();

                //Define Jobs
                var dictionary = new Dictionary<IJobDetail, Quartz.Collection.ISet<ITrigger>>();
                dictionary.Add(positionJob, new Quartz.Collection.HashSet<ITrigger>()
                    {
                        TriggerBuilder.Create()
                        .WithDailyTimeIntervalSchedule
                            (s =>
                                s.WithIntervalInSeconds(1)
                            )
                        .Build()
                    });

                scheduler.ScheduleJobs(dictionary, true);
                //Start Job
                scheduler.Start();
        }
    }
}
