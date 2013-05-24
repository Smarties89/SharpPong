using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpPong
{
    interface GameElement
    {
        void Update();
        void Draw();
        bool CollidedWith(int x, int y);
    }

    class Ball : GameElement
    {
        public int x; // from left
        public int y; // From top

        int mx;
        int my;
        int tick = 15;
        int nowTick = 0;

        class CollisionClass
        {
            public Func<int, int, bool> hit;
            public BouncingWay way;
            public Action callback;

            public CollisionClass(Func<int, int, bool> _hit, BouncingWay _way, Action _callback)
            {
                hit = _hit;
                way = _way;
                callback = _callback;
            }


        }

        public void ResetBall()
        {
            x = (int)Console.WindowWidth / 2;
            y = (int)Console.WindowHeight / 2;
        }

        private List<CollisionClass> _collisions = new List<CollisionClass>();

        public Ball()
        {
            x = 30;
            y = 20;
            mx = 1;
            my = -1;
        }

        public bool CollidedWith(int x, int y)
        {
            throw new NotImplementedException();
        }

        public void AddCollision(Func<int, int, bool> hit, BouncingWay way)
        {
            _collisions.Add(new CollisionClass(hit, way, null));
        }

        public void AddCollision(Func<int, int, bool> hit, BouncingWay way, Action callback)
        {
            _collisions.Add(new CollisionClass(hit, way, callback));
        }

        public void Draw()
        {
            Console.SetCursorPosition(x, y);
            Console.Write("o");
        }


        public void Update()
        {
            if (nowTick++ < tick)
                return;
            nowTick = 0;
            Console.SetCursorPosition(x, y);
            Console.Write(" ");

            foreach (var c in _collisions)
                if (c.hit(x + mx, y + my))
                {
                    switch (c.way)
                    {
                        case BouncingWay.North:
                        case BouncingWay.South:
                            my = -my;
                            break;

                        case BouncingWay.East:
                        case BouncingWay.West:
                            mx = -mx;
                            break;
                    }
                    if (c.callback != null)
                        c.callback();

                }
            x += mx;
            y += my;
        }
    }


    class Board : GameElement
    {
        protected int xstart;
        protected int xend;
        protected int ystart;
        protected int yend;
        string symbol;

        public Board(int _xstart, int _xend, int _ystart, int _yend, string _symbol)
        {
            xstart = _xstart;
            xend = _xend;
            ystart = _ystart;
            yend = _yend;
            symbol = _symbol;
        }

        public bool CollidedWith(int x, int y)
        {
           // if ((x >= xstart & x <= xend) & (y >= ystart & y <= yend))
            //    throw new Exception();
            return (x >= xstart & x <= xend) & (y >= ystart & y <= yend);
        }


        public void Move(int toside)
        {

            if (xstart + toside < 1 | xend + toside > Console.WindowWidth-2)
                return;
            // Clears the screen
            string _tempSymbol = symbol;
            symbol = " ";
            Draw();
            symbol = _tempSymbol;


            xstart += toside;
            xend += toside;
            Draw();
            
        }

        public void Draw()
        {
            if (symbol == "")
                return;
            for (int i = 0; i <= (yend - ystart); i++)
            {
                Console.SetCursorPosition(xstart, ystart + i);
                for (int y = 0; y <= (xend - xstart); y++)
                    Console.Write(symbol);
            }
        }
        public void Update()
        {

        }

    }

    class ControllableBoard : Board
    {
        ConsoleKey left, right;

        public ControllableBoard(int _xstart, int _xend, int _ystart, int _yend, string _symbol, ConsoleKey _left, ConsoleKey _right)
            : base( _xstart,  _xend,  _ystart,  _yend,  _symbol)
        {
            left = _left;
            right = _right;
        }

        public void KeyPressed(System.ConsoleKeyInfo key)
        {
            if (key.Key==right)
                    Move(1);
            else if (key.Key == left)
                    Move(-1);

        }
    }

    enum BouncingWay
    {
        North,
        South,
        East,
        West

    }

    class ScoreBoard : GameElement
    {
        public int topscore = 0;
        public int botscore = 0;

        public void Update()
        {

        }

        public void Draw()
        {
            Console.Title = String.Format("Top player {0}, bottom player {1}", topscore, botscore);
        }

        public bool CollidedWith(int x, int y)
        {
            return false;
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            var gameelements = new List<GameElement>();

            Board topplayerboard = new ControllableBoard(5, 15, 3, 3, "*", ConsoleKey.LeftArrow, ConsoleKey.RightArrow);
            gameelements.Add(topplayerboard);

            Board bottomplayerboard = new ControllableBoard(5, 15, Console.WindowHeight - 2, Console.WindowHeight - 2, "*", ConsoleKey.A, ConsoleKey.D);
            gameelements.Add(bottomplayerboard);


            Board leftsidedummy = new Board(0, 0, 2, Console.WindowHeight, "*");
            gameelements.Add(leftsidedummy);

            Board rigtsidedummy = new Board(Console.WindowWidth - 1, Console.WindowWidth - 1, 2, Console.WindowHeight, "*");
            gameelements.Add(rigtsidedummy);


            Board toplosingdummy = new Board(0, Console.WindowWidth - 1, 1, 1, "");
            gameelements.Add(toplosingdummy);

            Board bottomlosingdummy = new Board(0, Console.WindowWidth - 2, Console.WindowHeight, Console.WindowHeight, "");
            gameelements.Add(bottomlosingdummy);


            ScoreBoard b = new ScoreBoard();
            gameelements.Add(b);


            Ball theball = new Ball();
            gameelements.Add(theball);
            theball.AddCollision(topplayerboard.CollidedWith, BouncingWay.South);
            theball.AddCollision(bottomplayerboard.CollidedWith, BouncingWay.North);
            theball.AddCollision(leftsidedummy.CollidedWith, BouncingWay.East);
            theball.AddCollision(rigtsidedummy.CollidedWith, BouncingWay.East);
            theball.AddCollision(toplosingdummy.CollidedWith, BouncingWay.East, delegate() { theball.ResetBall(); b.botscore++; });
            theball.AddCollision(bottomlosingdummy.CollidedWith, BouncingWay.East, delegate() { theball.ResetBall(); b.topscore++; });


            while (true)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);
                    foreach (var g in gameelements)
                    {
                        
                        var board = g as ControllableBoard;

                        if (board != null)
                        {
                            board.KeyPressed(key);
                        }
                    }
                }

                foreach (var g in gameelements)
                {
                    g.Update();
                    g.Draw();
                }

                System.Threading.Thread.Sleep(10);
            }

        }
    }
}