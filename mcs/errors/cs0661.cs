// CS0661: `T' defines operator == or operator != but does not override Object.GetHashCode()
// Line: 5
// Compiler options: -warnaserror -warn:3

class T
{
        public static bool operator == (object o, T t)
        {
            return false;
        }

        public static bool operator != (object o, T t)
        {
            return true;
        }
        
        public override bool Equals(object o)
        {
            return true;
        }
}
