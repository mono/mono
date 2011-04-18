// CS1023: An embedded statement may not be a declaration or labeled statement
// Line: 9

class Test
{
        static void Main ()
        {
                for (int i = 0; i < 1000000; i++)
                        int k = i;
        }
}