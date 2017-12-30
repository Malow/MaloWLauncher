using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaloWLauncher
{
    class ConfigFile
    {
        public string gameLocation;
        public string launchParameters;

        public ConfigFile()
        {
            // Set default values
            this.gameLocation = @"C:\Program Files (x86)\Steam\steamapps\common\Sid Meier's Civilization V";
            this.launchParameters = @"\dx11";
        }
    }
}
