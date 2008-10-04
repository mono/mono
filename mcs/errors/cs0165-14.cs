// CS0165: Use of unassigned local variable `y'
// Line: 12

class test
{
        static void Main(string[] args)
        {
                {
                        int x = 8;
                }
                string y;
                args[0] = y;    // use of unassigned variable y
        }
}
