using System;
using System.Collections;

class Program
{
    static public IEnumerable Empty {
        get {
            object [] os = new object [] { };
            foreach (object o in os) {
                yield return o;
            }
        }
    }

    static void Main()
    {
        IEnumerator enumerator = Empty.GetEnumerator();
        if (enumerator.Current == null)
            Console.WriteLine("Successful");
        enumerator.MoveNext();
        if (enumerator.Current == null)
            Console.WriteLine("Successful");
    }
}
