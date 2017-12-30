using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaloWLauncher
{
    public class ModList
    {
        public class Mod
        {
            public class Version
            {
                public string version { get; set; }
                public DateTime released { get; set; }
                public string downloadURL { get; set; }
            }

            public string name { get; set; }
            public List<Version> versions { get; set; }
        }

        public string latestClientVersion { get; set; }
        public string latestClientDownloadUrl { get; set; }
        public List<Mod> mods { get; set; }
    }
}
