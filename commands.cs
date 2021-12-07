using System.Globalization;

namespace emofunge
{
    enum CommandSets
    {
        Emofunge, Befunge93
    }
    class CommandSet
    {
        public int MacroDef=0, ValueLow=0, ValueHigh=0, PrintInt=0, PrintChar=0, InputChar=0, InputInt=0, StringMode=0,
        Add=0, Substract=0, Divide=0, Multiply=0, Modulo=0, Not=0, GreaterThan=0,
        East=0, West=0, North=0, South=0,
        Northeast=0, Northwest=0, Southeast=0, Southwest=0,
        Anticlockwise=0, Clockwise=0, Random=0,
        WestEast=0, NorthSouth=0, NorthwestSoutheast=0, NortheastSouthwest=0,
        Duplicate=0, Swap=0, Discard=0, Skip=0, Return=0, End=0,
        Get=0, Put=0,
        Time=0;
        CommandSets _set;
        public CommandSets Set 
        {
            get
            {
                return _set;
            }
            set
            {
                _set = value;
                switch(_set)
                {
                    case CommandSets.Emofunge:
                        MacroDef = 0x20de;
                        ValueLow = 0x2800;
                        ValueHigh = 0x28ff;
                        PrintInt = 0x1f4df;
                        PrintChar = 0x1f5a8;
                        InputInt = 0x1f9ee;
                        InputChar = 0x2328;
                        StringMode = 0x1f9f5;
                        Add = 0x2795;
                        Substract = 0x2796;
                        Divide = 0x2797;
                        Multiply = 0x2716;
                        Modulo = 0x1f4a0;
                        Not = 0x1f6ab;
                        GreaterThan = 0x1f50d;
                        East = 0x27a1;
                        West = 0x2b05;
                        North = 0x2b06;
                        South = 0x2b07;
                        Northwest = 0x2196;
                        Northeast = 0x2197;
                        Southeast = 0x2198;
                        Southwest = 0x2199;
                        WestEast = 0x2194;
                        NorthSouth = 0x2195;
                        NorthwestSoutheast = 0x2921;
                        NortheastSouthwest = 0x2922;
                        Anticlockwise = 0x21ba;
                        Clockwise = 0x21bb;
                        Skip = 0x1f309;
                        Random = 0x1f9ed;
                        End = 0x1f6d1;
                        Duplicate = 0x1fa9e;
                        Swap = 0x1f500;
                        Discard = 0x1f5d1;
                        Get = 0;
                        Put = 0;
                        Time = 0x231a;
                        Return = 0x21a9;
                        break;
                    case CommandSets.Befunge93:
                        // setting commands and modifiers to 0 ensures they're assigned to nothing
                        // the actual NUL character gets caught as a space before everything,
                        // and can't be a combining character
                        MacroDef = 0;
                        ValueLow = 0x30;
                        ValueHigh = 0x39;
                        PrintInt = 0x2e;
                        PrintChar = 0x2c;
                        InputInt = 0x26;
                        InputChar = 0x7e;
                        StringMode = 0x22;
                        Add = 0x2b;
                        Substract = 0x2d;
                        Divide = 0x2f;
                        Multiply = 0x2a;
                        Modulo = 0x25;
                        Not = 0x21;
                        GreaterThan = 0x60;
                        East = 0x3e;
                        West = 0x3c;
                        North = 0x5e;
                        South = 0x76;
                        Northwest = 0;
                        Northeast = 0;
                        Southeast = 0;
                        Southwest = 0;
                        WestEast = 0x5f;
                        NorthSouth = 0x7c;
                        NorthwestSoutheast = 0;
                        NortheastSouthwest = 0;
                        Anticlockwise = 0x5b; // those two were actually introduced in Befunge98
                        Clockwise = 0x5d;     // but who cares
                        Skip = 0x23;
                        Random = 0x3f;
                        End = 0x40;
                        Duplicate = 0x3a;
                        Swap = 0x5c;
                        Discard = 0x24;
                        Get = 0x67;
                        Put = 0x70;
                        Time = 0;
                        Return = 0;
                        break;
                }
            }
        }
        public CommandSet(CommandSets set)
        {
            Set = set;
        }
        public CommandSet()
        {
            Set = CommandSets.Emofunge;
        }
        public bool IsSpace(int value)
        {
            return CharUnicodeInfo.GetUnicodeCategory(value) == UnicodeCategory.SpaceSeparator || value <= 0x20;
        }
        public bool IsValue(int value)
        {
            return value >= ValueLow && value <= ValueHigh;
        }
        public int GetValue(int value)
        {
            if(IsValue(value))
                return value - ValueLow;
            else return 0;
        }
    }
}