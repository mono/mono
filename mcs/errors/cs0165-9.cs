using System;

public class Test
{
        public int i;

        public void Hoge ()
        {
                if (i > 0)
                        goto Fuga;
                string s = "defined later";
        Fuga:
                Console.WriteLine (s);
        }
}
