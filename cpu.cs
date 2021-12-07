using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;

namespace emofunge
{
    class Stack0 : Stack<int>
    {
        public int Pop0()
        {
            if(this.Count == 0) return 0;
            return this.Pop();
        }
        public int Peek0()
        {
            if(this.Count == 0) return 0;
            return this.Peek();
        }
        public Stack0() : base(){}
        public Stack0(int capacity) : base(capacity){}
    }
    enum Direction
    {
        // 701
        // 6 2
        // 543
        N = 0, NE, E, SE, S, SW, W, NW,
        North = 0, Northeast, East, Southeast, South, Southwest, West, Northwest,
        None = -1
    }
    struct Cell : IEquatable<Cell>
    {
        public int x;
        public int y;
        public Cell(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
        public bool Equals(Cell other)
        {
            //if(other is null)
            //    return false;
            return this.x == other.x && this.y == other.y;
        }
        public override bool Equals(object? obj) => Equals(obj as Cell?);
        public override int GetHashCode() => (x, y).GetHashCode();
        public override string ToString() => $"@{x},{y}";
    }
    class Cpu
    {
        Dictionary<Cell, string> Prg;
        //string[][] Prg;
        Stack0 MainStack;
        Stack<Tuple<Cell, Direction>> MacroStack;
        Dictionary<int, Cell> Macros;
        int _Width = 0;
        int _Height = 0;
        public int Width
        {
            get
            {
                return _Width<=0?Prg.Aggregate(0, (acc, x) => Math.Max(acc, x.Key.x))+1:_Width;
            }
            set
            {
                _Width = value;
            }
        }
        public int Height
        {
            get
            {
                return _Height<=0?Prg.Aggregate(0, (acc, x) => Math.Max(acc, x.Key.y))+1:_Height;
            }
            set
            {
                _Height = value;
            }
        }
        private Cell _pc;
        public Cell pc
        {
            get
            {
                return new Cell(_pc.x%Width, _pc.y%Height);
            }
            set
            {
                // TODO: allow negative values
                _pc.x = value.x%Width;
                if(_pc.x<0)_pc.x+=Width;
                _pc.y = value.y%Height;
                if(_pc.y<0)_pc.y+=Height;
            }
        }
        private int _dirX = 1;
        private int _dirY = 0;
        public Direction CurrentDirection
        {
            get
            {
                return (new Direction[3]{
                    (new Direction[3]{Direction.NW, Direction.N,    Direction.NE})[_dirX+1],
                    (new Direction[3]{Direction.W,  Direction.None, Direction.E })[_dirX+1],
                    (new Direction[3]{Direction.SW, Direction.S,    Direction.SE})[_dirX+1]
                })[_dirY+1];
            }
            set
            {
                int dir = (int)value+1;
                _dirX = (new int[9]{0, 0, 1,1,1,0,-1,-1,-1})[dir];
                _dirY = (new int[9]{0,-1,-1,0,1,1, 1, 0,-1})[dir];
            }
        }
        bool StringMode = false;
        static public bool Debug { get; set; }
        public CommandSet Commands;
        public Cpu(string[] prg, bool debug, int capacity)
        {
            Commands = new CommandSet();
            Debug = debug;
            MainStack = new Stack0(capacity);
            MacroStack = new Stack<Tuple<Cell, Direction>>(capacity);
            Macros = new Dictionary<int, Cell>(capacity);
            Prg = Parse(prg);
            if(Debug) Console.Error.WriteLine("size {0}x{1}", Width, Height);
            pc = new Cell(0,0);
            CurrentDirection = Direction.East;
        }
        public Dictionary<Cell, string> Parse(string[] str)
        {
            Dictionary<Cell, string> ret = new Dictionary<Cell, string>{};
            int x = 0, y = 0;
            foreach (string line in str)
            {
                TextElementEnumerator teEnum = StringInfo.GetTextElementEnumerator(line);
                x=0;
                while(teEnum.MoveNext())
                {
                    string cur = (string)teEnum.Current;
                    if(cur != " ")
                        ret[new Cell(x, y)] = cur;
                    Rune[] runes = cur.EnumerateRunes().ToArray();
                    if(runes.Skip(1).Contains(new Rune(Commands.MacroDef)))
                    {
                        Macros[runes[0].Value] = new Cell(x, y);
                    }
                    if(Debug)
                    {
                        Console.Error.WriteLine("char @{0},{1}: {2}", x, y, cur);
                        foreach (Rune r in cur.EnumerateRunes())
                        {
                            Console.Error.WriteLine("  value U+{0:x}", r.Value);
                        }
                    }
                    x++;
                }
                y++;
            }
            return ret;
        }

        public Rune[] GetRunes(Cell c)
        {
            try
            {
                return Prg[c].EnumerateRunes().ToArray();
            }
            catch (KeyNotFoundException)
            {
                return " ".EnumerateRunes().ToArray();
            }
        }
        public Rune[] GetRunes(int x, int y)
        {
            return GetRunes(new Cell(x, y));
        }

        public Rune[] CurrentRunes
        {
            get
            {
                return GetRunes(pc);
            }
        }

        public void Move(int x, int y)
        {
            Cell c = pc;
            c.x += x;
            c.y += y;
            pc = c;
        }

        public void Move()
        {
            Move(_dirX, _dirY);
        }

        public void Step()
        {
            Rune[] runes = CurrentRunes;
            Rune cmd = runes[0];
            Rune[] mods = runes.Skip(1).ToArray();
            if(Debug) Console.Error.WriteLine("U+{0:X} {1} @{2},{3}", cmd.Value, cmd.ToString(), pc.x, pc.y);
            if(cmd.Value == Commands.StringMode)
            {
                StringMode = !StringMode;
                Move();
            }
            else if(StringMode)
            {
                foreach (Rune item in runes)
                {
                    MainStack.Push(item.Value);
                }
                Move();
            }
            else
            {
                switch(cmd.Value)
                {
                #region Values
                    case var v when Commands.IsValue(v):
                    { // push num to stack
                        int val = Commands.GetValue(cmd.Value);
                        // TODO: Combining characters
                        //CurrentRunes.Count(x => x.Value == )
                        MainStack.Push(val);
                        Move();
                    } break;
                #endregion
                #region Space
                    case var v when Commands.IsSpace(v):
                    { // explicitly defined space
                        Move();
                    } break;
                #endregion
                #region Output
                    case var v when v == Commands.PrintInt:
                    { // pop value and print integer
                        Console.Write("{0} ", MainStack.Pop0());
                        Move();
                    } break;
                    case var v when v == Commands.PrintChar:
                    { // pop value and print Unicode character
                        Console.Write(new Rune(MainStack.Pop0()).ToString());
                        Move();
                    } break;
                #endregion
                #region Input
                    case var v when v == Commands.InputChar:
                    {
                        MainStack.Push(Console.ReadKey(true).KeyChar);
                        Move();
                    } break;
                    case var v when v == Commands.InputInt:
                    {
                        int result = 0;
                        while(!int.TryParse(Console.ReadLine(), NumberStyles.Integer, null, out result)){}
                        MainStack.Push(result);
                        Move();
                    } break;
                #endregion
                #region Movement and control
                    case var v when v == Commands.End:
                    {
                        CurrentDirection = Direction.None;
                    } break;
                    case var v when v == Commands.East:
                    {
                        CurrentDirection = Direction.East;
                        Move();
                    } break;
                    case var v when v == Commands.West:
                    {
                        CurrentDirection = Direction.West;
                        Move();
                    } break;
                    case var v when v == Commands.North:
                    {
                        CurrentDirection = Direction.North;
                        Move();
                    } break;
                    case var v when v == Commands.South:
                    {
                        CurrentDirection = Direction.South;
                        Move();
                    } break;
                    case var v when v == Commands.Northwest:
                    {
                        CurrentDirection = Direction.Northwest;
                        Move();
                    } break;
                    case var v when v == Commands.Northeast:
                    {
                        CurrentDirection = Direction.Northeast;
                        Move();
                    } break;
                    case var v when v == Commands.Southeast:
                    {
                        CurrentDirection = Direction.Southeast;
                        Move();
                    } break;
                    case var v when v == Commands.Southwest:
                    {
                        CurrentDirection = Direction.Southwest;
                        Move();
                    } break;
                    case var v when v == Commands.WestEast:
                    {
                        int a = MainStack.Pop0();
                        CurrentDirection = a==0?Direction.E:Direction.W;
                        Move();
                    } break;
                    case var v when v == Commands.NorthSouth:
                    {
                        int a = MainStack.Pop0();
                        CurrentDirection = a==0?Direction.S:Direction.N;
                        Move();
                    } break;
                    case var v when v == Commands.NorthwestSoutheast:
                    {
                        int a = MainStack.Pop0();
                        CurrentDirection = a==0?Direction.SE:Direction.NW;
                        Move();
                    } break;
                    case var v when v == Commands.NortheastSouthwest:
                    {
                        int a = MainStack.Pop0();
                        CurrentDirection = a==0?Direction.SW:Direction.NE;
                        Move();
                    } break;
                    case var v when v == Commands.Anticlockwise:
                    {
                        int dir = ((int)CurrentDirection-2)&7;
                        CurrentDirection = (Direction)dir;
                        Move();
                    } break;
                    case var v when v == Commands.Clockwise:
                    {
                        int dir = ((int)CurrentDirection+2)&7;
                        CurrentDirection = (Direction)dir;
                        Move();
                    } break;
                    case var v when v == Commands.Skip: 
                    {// aka bridge
                        Move(); Move();
                    } break;
                    case var v when v == Commands.Random:
                    {
                        Random r = new Random();
                        CurrentDirection = (Direction)r.Next(8);
                        Move();
                    } break;
                #endregion
                #region Math
                    case var v when v == Commands.Multiply:
                    {
                        int a = MainStack.Pop0();
                        int b = MainStack.Pop0();
                        MainStack.Push(a*b);
                        Move();
                    } break;
                    case var v when v == Commands.Add:
                    {
                        int a = MainStack.Pop0();
                        int b = MainStack.Pop0();
                        MainStack.Push(a+b);
                        Move();
                    } break;
                    case var v when v == Commands.Substract:
                    {
                        int a = MainStack.Pop0();
                        int b = MainStack.Pop0();
                        MainStack.Push(b-a);
                        Move();
                    } break;
                    case var v when v == Commands.Divide:
                    {
                        int a = MainStack.Pop0();
                        int b = MainStack.Pop0();
                        MainStack.Push(b/a);
                        Move();
                    } break;
                    case var v when v == Commands.Modulo:
                    {
                        int a = MainStack.Pop0();
                        int b = MainStack.Pop0();
                        MainStack.Push(b%a);
                        Move();
                    } break;
                    case var v when v == Commands.Not:
                    {
                        int a = MainStack.Pop0();
                        MainStack.Push(a==0?1:0);
                        Move();
                    } break;
                    case var v when v == Commands.GreaterThan:
                    {
                        int a = MainStack.Pop0();
                        int b = MainStack.Pop0();
                        MainStack.Push(b>a?1:0);
                        Move();
                    } break;
                    case var v when v == Commands.Duplicate:
                    {
                        int a = MainStack.Peek0();
                        MainStack.Push(a);
                        Move();
                    } break;
                    case var v when v == Commands.Swap:
                    {
                        int a = MainStack.Pop0();
                        int b = MainStack.Pop0();
                        MainStack.Push(a);
                        MainStack.Push(b);
                        Move();
                    } break;
                    case var v when v == Commands.Discard:
                    {
                        MainStack.Pop0();
                        Move();
                    } break;
                #endregion
                #region Get/Put
                    case var v when v == Commands.Get:
                    {
                        int y = MainStack.Pop0();
                        int x = MainStack.Pop0();
                        Rune[] r = GetRunes(x, y);
                        MainStack.Push(r[0].Value);
                        Move();
                    } break;
                    case var v when v == Commands.Put:
                    {
                        int y = MainStack.Pop0();
                        int x = MainStack.Pop0();
                        int u = MainStack.Pop0();
                        Prg[new Cell(x, y)] = new Rune(u).ToString();
                        Move();
                    } break;
                #endregion
                #region Misc
                    case var v when v == Commands.Time:
                    {
                        MainStack.Push((int)DateTimeOffset.Now.ToUnixTimeSeconds());
                        Move();
                    } break;
                #endregion
                #region Macros
                    case var v when v == Commands.Return:
                    {
                        if(MacroStack.Count > 0)
                        {
                            Tuple<Cell, Direction> ret = MacroStack.Pop();
                            pc = ret.Item1;
                            CurrentDirection = ret.Item2;
                        }
                        Move();
                    } break;
                    default: // Undefined
                    {
                        if(Macros.ContainsKey(cmd.Value) && !mods.Contains(new Rune(Commands.MacroDef)))
                        {
                            Cell coords = Macros[cmd.Value];
                            MacroStack.Push(Tuple.Create(pc, CurrentDirection));
                            foreach (Rune mod in mods)
                            {
                                MainStack.Push(mod.Value);                                    
                            }
                            pc = new Cell(coords.x, coords.y);
                            CurrentDirection = Direction.East;
                        }
                        Move();
                    } break;
                #endregion
                }
            }
        }
        public void Run()
        {
            while(CurrentDirection != Direction.None)
            {
                Step();
            }
        }
    }
}