using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScottPlot;
using TelegramVPNBot.Models;

namespace TelegramVPNBot.Helpers
{
    public static class GraphScot
    {
        public static Task<string> CreateGraphAsync(List<Connection> connections)
        {
            DateTime[] connectionDates = connections.Select(c => c.DateTimeUtc).ToArray();
            double[] connectionCounts = connections
                .Select(c => c.IpsList?.Count ?? 0)
                .Select(count => (double)count)
                .ToArray();

            var myPlot = new Plot();
            myPlot.Add.Scatter(
                connectionDates.Select(d => d.ToOADate()).ToArray(),
                connectionCounts);

            myPlot.Axes.DateTimeTicksBottom();
            myPlot.Title("Connections Over Time");
            myPlot.XLabel("Time");
            myPlot.YLabel("Connection Count");

            string fileName = "connections_graph.png";
            myPlot.SavePng(fileName, 800, 600);

            return Task.FromResult(fileName);
        }
    }
}
