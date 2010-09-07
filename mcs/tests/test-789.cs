using System;

class Program {
    static void Main ()
    {
        Action action = () => Console.WriteLine (1);
        action += null;
        action ();
    }
}
