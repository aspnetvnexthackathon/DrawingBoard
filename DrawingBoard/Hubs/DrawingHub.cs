using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;

namespace DrawingBoard.Hubs
{
    public class DrawingHub : Hub
    {
        private static readonly Random _random;

        private static readonly ConcurrentDictionary<string, GameState> _games;
        private static readonly GameState _newGame;

        private static readonly Dictionary<string, string[]> _animalSynonyms;
        private static readonly string[] _animals;

        static DrawingHub()
        {
            _random = new Random();
            _games = new ConcurrentDictionary<string, GameState>();
            _newGame = new GameState();

            // Generated from http://en.wikipedia.org/wiki/List_of_English_animal_nouns
            var animalsJson = File.ReadAllText(Startup.AppBasePath + @"\Hubs\animals.json");
            _animalSynonyms = JsonConvert.DeserializeObject<Dictionary<string, string[]>>(animalsJson);
            _animals = _animalSynonyms.Keys.ToArray();
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

        public async Task<string> CreateGame()
        {
            var gameId = _random.Next().ToString();

            if (!_games.TryAdd(gameId, _newGame))
            {
                throw new InvalidOperationException("Insufficient Entropy");
            }

            Clients.Caller.color = colors[Interlocked.Increment(ref _id) % colors.Length];
            await Groups.Add(Context.ConnectionId, gameId);
            await Groups.Add(Context.ConnectionId, "guess-" + gameId);
            return gameId;
        }

        public async Task Join(string gameId)
        {
            GameState ignore;
            if (!_games.TryGetValue(gameId, out ignore))
            {
                throw new InvalidOperationException("A game with the specified id has not yet been created.");
            }

            Clients.Caller.color = colors[Interlocked.Increment(ref _id) % colors.Length];
            await Groups.Add(Context.ConnectionId, gameId);
            await Groups.Add(Context.ConnectionId, "draw-" + gameId);
            await Clients.Group("draw-" + gameId).playerJoined(gameId);
        }

        public string Start(string gameId)
        {
            var animal = _animals[_random.Next(_animals.Length)];
            Timer gameTimer = null;
            var gameState = new GameState
            {
                Animal = animal,
                Timer = gameTimer
            };

            if (!_games.TryUpdate(gameId, gameState , _newGame))
            {
                throw new InvalidOperationException("A game with the specified id has alread started.");
            }

            gameTimer = new Timer(_ =>
            {
                Clients.Group(gameId).gameTimedOut(gameId);

                gameTimer.Dispose();

                GameState ignore;
                _games.TryRemove(gameId, out ignore);
            }, null, 60 * 1000, -1);

            Clients.Group(gameId).gameStarted(gameId);

            return animal;
        }

        public bool Guess(string gameId, string animalGuess)
        {
            Clients.Group("draw-" + gameId).guess(gameId, animalGuess);

            GameState game;
            return _games.TryGetValue(gameId, out game) &&
                (animalGuess == game.Animal || _animalSynonyms[game.Animal].Contains(animalGuess));
        }

        public void Draw(int gameId, Line data)
        {
            Clients.OthersInGroup(gameId.ToString()).draw(gameId, data);
        }
    }
}
