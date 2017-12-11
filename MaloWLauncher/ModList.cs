using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaloWLauncher
{
    public class ModList
    {
        public class ModInfo
        {
            public string Name { get; set; }
            public DateTime Released { get; set; }
            public string DownloadURL { get; set; }
        }

        public string latestClientVersion { get; set; }
        public string latestClientDownloadUrl { get; set; }
        public List<ModInfo> mods { get; set; }
    }
}
