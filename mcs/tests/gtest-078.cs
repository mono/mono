using System;
using System.Collections;

public class Test
{
        public static void Main ()
        {
                foreach (object o in new Test ())
                        Console.WriteLine (o);
        }

        public IEnumerator GetEnumerator ()
        {
                foreach (int i in new ArrayList ())
                        yield return i;
        }
}
