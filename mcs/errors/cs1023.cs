// cs1023.cs: an embedded statement cannot be a declaration or a labeled statement.
// line: 9

class Test
{
        static void Main ()
        {
                for (int i = 0; i < 1000000; i++)
                        int k = i;
        }
}