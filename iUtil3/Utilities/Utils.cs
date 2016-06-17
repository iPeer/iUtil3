using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iUtil3.Utilities
{
    public class Utils
    {

        public static String getApplicationEXEPath()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        public static string FormatUptime(Int32 secs)
        {
            return FormatUptime(TimeSpan.FromSeconds(secs));
        }

        public static string FormatUptime(TimeSpan t)
        {
            return string.Format("{0:00}:{1:00}:{2:00}:{3:00}", t.TotalDays, t.Hours, t.Minutes, t.Seconds);
        }

    }
}
