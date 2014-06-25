using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;

namespace DrawingBoard.Hubs
{
    public class DrawinghHub : Hub
    {
        private static Random _random;

        static DrawinghHub()
        {
            _random = new Random();
        }

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

        public async Task<int> CreateGame()
        {
            int gameId = _random.Next();
            Clients.Caller.color = colors[Interlocked.Increment(ref _id) % colors.Length];
            await Groups.Add(Context.ConnectionId, gameId.ToString());
            return gameId;
        }

        public async Task Join(int gameId)
        {
            Clients.Caller.color = colors[Interlocked.Increment(ref _id) % colors.Length];
            await Groups.Add(Context.ConnectionId, gameId.ToString());
        }

        public void Draw(int gameId, Line data)
        {
            Clients.OthersInGroup(gameId.ToString()).draw(data);
        }
    }
}
