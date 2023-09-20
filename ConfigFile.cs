using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PUBG_Volume_Control
{
    public class ConfigFile
    {
        public List<string> Games { get; set; }
        public string VolumeState1 { get; set; }
        public string VolumeState2 { get; set; }
    }
}
