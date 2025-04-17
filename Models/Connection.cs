using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramVPNBot.Models
{
    public class Connection
    {
        public List<string>? IpsList { get; set; }

        public required DateTime DateTimeUtc { get; set; }
    }
}
