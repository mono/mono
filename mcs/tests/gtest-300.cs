// Compiler options: -warnaserror -warn:4

using System;
using System.Collections.Generic;

public class Test
{
        public static void Main ()
        {
                IDictionary<string,object> c =
                        new Dictionary<string,object> ();
                foreach (string s in c.Keys)
                        Console.WriteLine (s);
        }
}