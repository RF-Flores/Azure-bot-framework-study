using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pluralsight_bot.Services
{
    public class Common
    {
        public static readonly List<String> BugTypes = new List<string>
        {
            "Security",
            "Crash",
            "Power",
            "Performance",
            "Usability",
            "Serious Bug",
            "Other"
        };
    }
}
