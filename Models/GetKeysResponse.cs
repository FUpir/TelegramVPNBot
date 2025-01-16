using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramVPNBot.Models
{
    public class GetKeysResponse
    {
        public List<VpnKey>? accessKeys { get; set; }
    }
}
