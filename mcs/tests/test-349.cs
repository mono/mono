// Compiler options: -optimize+

// TODO: I will have to investigate how to test that ctor is really empty

class C
{
    public C () {}

    int i = new int ();
    double d = new double ();
    char c = new char ();
    bool b = new bool ();
    decimal dec2 = new decimal ();
    object o = new object ();
	    
    public static void Main ()
    {
    }
}
