using System.Threading;
using Microsoft.AspNet.SignalR.Hubs;

namespace Microsoft.AspNet.SignalR.Hosting.AspNet.Samples.Hubs.DrawingPad
{
    public class DrawinghHub : Hub
    {
        #region Data structures

        public class Point
        {
            public double X { get; set; }
            public double Y { get; set; }
        }

        public class Line
        {
            public Point From { get; set; }
            public Point To { get; set; }
            public string Color { get; set; }
        }
        #endregion

        private static long _id;
        // defines some colors
        private readonly static string[] colors = new string[]{
            "red", "green", "blue", "orange", "navy", "silver", "black", "lime"
        };

        public void Join()
        {
            Clients.Caller.color = colors[Interlocked.Increment(ref _id) % colors.Length];
        }

        public void Draw(Line data)
        {
            Clients.Others.draw(data);
        }
    }
}
