using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramVPNBot.Models
{
    public class VpnServerMetrics
    {
        public Dictionary<string, long> BytesTransferredByUserId { get; set; }
    }
}
