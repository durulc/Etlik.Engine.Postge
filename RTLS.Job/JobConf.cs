using System;
using System.Collections.Generic;

namespace RTLS.Job
{
    public class JobConf
    {
        public static bool JobStartSetPositionState { get; set; }
        public static DateTime JobStartSetPositionDateTime { get; set; }

        public static Dictionary<string,DateTime> TagLastReadTime { get; set; }

        public static void SetTagLastReadTime(string key, DateTime value)
        {
            if (TagLastReadTime==null)
                TagLastReadTime = new Dictionary<string, DateTime>();
            if (TagLastReadTime.ContainsKey(key))
            {
                TagLastReadTime[key]=value;
            }
            else
            {
                TagLastReadTime.Add(key, value);
            }            
        }

        public static DateTime GetTagLastReadTime(string key)
        {
            if (TagLastReadTime == null) TagLastReadTime = new Dictionary<string, DateTime>();
            if (TagLastReadTime.ContainsKey(key))
            {
                return TagLastReadTime[key];                
            }
            return DateTime.MinValue;
        }
    }
    
}
