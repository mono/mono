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
		int i = 2;
		yield return 3;
        }
}
