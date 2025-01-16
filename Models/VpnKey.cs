using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramVPNBot.Models
{
    public class VpnKey
    {
        public string id { get; set; }
        public string name { get; set; }
        public string password { get; set; }
        public int port { get; set; }
        public string method { get; set; }
        public string accessUrl { get; set; }
    }
}
