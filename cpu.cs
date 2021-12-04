using System.Collections;
using System.Globalization;
using System.Text;

namespace emofunge
{
    enum Direction
    {
        // 701
        // 6 2
        // 543
        N = 0, NE, E, SE, S, SW, W, NW,
        North = 0, Northeast, East, Southeast, South, Southwest, West, Northwest,
        None = -1
    }
    struct Cell
    {
        public int x;
        public int y;
        public Cell(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
    class Cpu
    {
        string[][] Prg;
        Stack<int> MainStack;
        Stack<Tuple<Cell, Direction>> MacroStack;
        Dictionary<int, Cell> Macros;
        public int Width
        {
            get
            {
                return Prg.Aggregate(0, (acc, x) => Math.Max(acc, x.Length));
            }
        }
        public int Height
        {
            get
            {
                return Prg.Length;
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
                _pc.x = value.x%Width;
                if(_pc.x<0)_pc.x+=Width;
                _pc.y = value.y%Height;
                if(_pc.y<0)_pc.y+=Height;
            }
        }
        private int _dirX = 1;
        private int _dirY = 0;
        Direction CurrentDirection
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
        public Cpu(string[] prg)
        {
            Debug = false;
            MainStack = new Stack<int>(256);
            MacroStack = new Stack<Tuple<Cell, Direction>>(256);
            Macros = new Dictionary<int, Cell>(256);
            Prg = Parse(prg);
            if(Debug) Console.WriteLine("size {0}x{1}", Width, Height);
            pc = new Cell(0,0);
            CurrentDirection = Direction.East;
        }
        public string[][] Parse(string[] str)
        {
            string[][] ret = {};
            int x = 0, y = 0;
            foreach (string line in str)
            {
                string[] parsed = {};
                TextElementEnumerator teEnum = StringInfo.GetTextElementEnumerator(line);
                x=0;
                while(teEnum.MoveNext())
                {
                    string cur = (string)teEnum.Current;
                    parsed = parsed.Append(cur).ToArray();
                    Rune[] runes = cur.EnumerateRunes().ToArray();
                    foreach(Rune r in runes)
                    {
                        if(r.Value == 0x20DE)
                        {
                            Macros[runes[0].Value] = new Cell(x, y);
                        }
                    }
                    if(Debug)
                    {
                        Console.WriteLine("char @{0},{1}: {2}", x, y, cur);
                        foreach (Rune r in cur.EnumerateRunes())
                        {
                            Console.WriteLine("  value U+{0:x}", r.Value);
                        }
                    }
                    x++;
                }
                ret = ret.Append(parsed).ToArray();
                y++;
            }
            return ret;
        }

        public Rune[] GetRunes(Cell c)
        {
            try
            {
                return Prg[c.y][c.x].EnumerateRunes().ToArray();
            }
            catch (Exception)
            {
                return " ".EnumerateRunes().ToArray();
            }
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
            if(Debug) Console.WriteLine("U+{0:X} {1} @{2},{3}", cmd.Value, cmd.ToString(), pc.x, pc.y);
            if(cmd.Value == 0x1f9f5)
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
                try
                {
                    #region Values
                    if(cmd.Value >= 0x2800 && cmd.Value <= 0x28FF)
                    { // push num to stack
                        int val = cmd.Value - 0x2800;
                        // TODO: Combining characters
                        //CurrentRunes.Count(x => x.Value == )
                        MainStack.Push(val);
                        Move();
                    }
                    #endregion
                    else if(CharUnicodeInfo.GetUnicodeCategory(cmd.Value) == UnicodeCategory.SpaceSeparator)
                    { // explicitly defined space
                        Move();
                    }
                    else switch (cmd.Value)
                    {
                    #region Output
                        case 0x1f4df: // pop value and print integer
                        {
                            Console.Write("{0} ", MainStack.Pop());
                            Move();
                        } break;
                        case 0x1f5a8: // pop value and print Unicode character
                        {
                            Console.Write(new Rune(MainStack.Pop()).ToString());
                            Move();
                        } break;
                    #endregion
                    #region Input
                        case 0x2328: // input character
                        {
                            MainStack.Push(Console.Read());
                        } break;
                        case 0x1f9ee: // input number
                        {
                            int result;
                            while(!int.TryParse(Console.ReadLine(), NumberStyles.Integer, null, out result)){}
                            MainStack.Push(result);
                        } break;
                    #endregion
                    #region Movement and control
                        case 0x1f6d1: // stop
                        {
                            CurrentDirection = Direction.None;
                        } break;
                        case 0x27a1: // move east
                        {
                            CurrentDirection = Direction.East;
                            Move();
                        } break;
                        case 0x2b05: // move west
                        {
                            CurrentDirection = Direction.West;
                            Move();
                        } break;
                        case 0x2b06: // move north
                        {
                            CurrentDirection = Direction.North;
                            Move();
                        } break;
                        case 0x2b07: // move south
                        {
                            CurrentDirection = Direction.South;
                            Move();
                        } break;
                        case 0x2196: // move northwest
                        {
                            CurrentDirection = Direction.Northwest;
                            Move();
                        } break;
                        case 0x2197: // move northeast
                        {
                            CurrentDirection = Direction.Northeast;
                            Move();
                        } break;
                        case 0x2198: // move southeast
                        {
                            CurrentDirection = Direction.Southeast;
                            Move();
                        } break;
                        case 0x2199: // move southwest
                        {
                            CurrentDirection = Direction.Southwest;
                            Move();
                        } break;
                        case 0x2194: // move west-east
                        {
                            int a = MainStack.Pop();
                            CurrentDirection = a==0?Direction.E:Direction.W;
                            Move();
                        } break;
                        case 0x2195: // move west-east
                        {
                            int a = MainStack.Pop();
                            CurrentDirection = a==0?Direction.S:Direction.N;
                            Move();
                        } break;
                        case 0x2921: // move west-east
                        {
                            int a = MainStack.Pop();
                            CurrentDirection = a==0?Direction.SE:Direction.NW;
                            Move();
                        } break;
                        case 0x2922: // move west-east
                        {
                            int a = MainStack.Pop();
                            CurrentDirection = a==0?Direction.SW:Direction.NE;
                            Move();
                        } break;
                        case 0x21ba: // turn anticlockwise
                        {
                            int dir = ((int)CurrentDirection-2)&7;
                            CurrentDirection = (Direction)dir;
                            Move();
                        } break;
                        case 0x21bb: // turn clockwise
                        {
                            int dir = ((int)CurrentDirection+2)&7;
                            CurrentDirection = (Direction)dir;
                            Move();
                        } break;
                        case 0x1f309: // bridge
                        {
                            Move(); Move();
                        } break;
                        case 0x1f9ed: // random
                        {
                            Random r = new Random();
                            CurrentDirection = (Direction)r.Next(8);
                            Move();
                        } break;
                    #endregion
                    #region Math
                        case 0x2716: // Multiply
                        {
                            int a = MainStack.Pop();
                            int b = MainStack.Pop();
                            MainStack.Push(a*b);
                            Move();
                        } break;
                        case 0x2795: // Add
                        {
                            int a = MainStack.Pop();
                            int b = MainStack.Pop();
                            MainStack.Push(a+b);
                            Move();
                        } break;
                        case 0x2796: // Substract
                        {
                            int a = MainStack.Pop();
                            int b = MainStack.Pop();
                            MainStack.Push(b-a);
                            Move();
                        } break;
                        case 0x2797: // Divide
                        {
                            int a = MainStack.Pop();
                            int b = MainStack.Pop();
                            MainStack.Push(b/a);
                            Move();
                        } break;
                        case 0x1f4a0: // Modulo
                        {
                            int a = MainStack.Pop();
                            int b = MainStack.Pop();
                            MainStack.Push(b%a);
                            Move();
                        } break;
                        case 0x1f6ab: // Not
                        {
                            int a = MainStack.Pop();
                            MainStack.Push(a==0?1:0);
                            Move();
                        } break;
                        case 0x1f50d: // Greater than
                        {
                            int a = MainStack.Pop();
                            int b = MainStack.Pop();
                            MainStack.Push(b>a?1:0);
                            Move();
                        } break;
                        case 0x1fa9e: // Duplicate
                        {
                            int a = MainStack.Peek();
                            MainStack.Push(a);
                            Move();
                        } break;
                        case 0x1f500: // Duplicate
                        {
                            int a = MainStack.Pop();
                            int b = MainStack.Pop();
                            MainStack.Push(a);
                            MainStack.Push(b);
                            Move();
                        } break;
                        case 0x1f5D1: // Discard
                        {
                            MainStack.Pop();
                            Move();
                        } break;
                    #endregion
                    #region Misc
                        case 0x231A: // Timestamp
                        {
                            MainStack.Push((int)DateTimeOffset.Now.ToUnixTimeSeconds());
                            Move();
                        } break;
                    #endregion
                    #region Macros
                        case 0x21a9: // Return
                        {
                            Tuple<Cell, Direction> ret = MacroStack.Pop();
                            pc = ret.Item1;
                            CurrentDirection = ret.Item2;
                            Move();
                        } break;
                        default: // Undefined
                        {
                            if(Macros.ContainsKey(cmd.Value) && !runes.Contains(new Rune(0x20DE)))
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
                catch(InvalidOperationException)
                {
                    // TODO: do something when stack is empty
                    Move();
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