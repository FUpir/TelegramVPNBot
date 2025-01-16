using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramVPNBot.Models
{
    public class Purchase
    {
        public int Days { get; set; }

        public decimal Price { get; set; }

        public DateTime StartDateUtc { get; set; }
    }
}