// CS0139: No enclosing loop out of which to break or continue
// Line: 10
public class Test
{
        public static void Foo (char c)
        {
                switch (char.GetUnicodeCategory (c)) {
                default:
                        if (c == 'a')
                                continue;
                        System.Console.WriteLine ();
                        break;
                }
        }
}

