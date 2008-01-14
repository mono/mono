public class Thing
{
    public delegate void Handler ();
    
    static void Foo ()
    {
    }

    public static int Main ()
    {
        Method (delegate { }, 
            "Hello", "How", "Are", "You"); // compiler explodes
        Method (delegate { });             // compiler explodes

        Method (null, null);               // ok
        Method (null);                     // ok
        Method (Foo, "Hi");               // ok
        return 0;
    }

    public static void Method (Handler handler, params string [] args)
    {
    }
}

