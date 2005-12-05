// Compiler options: -optimize+

// TODO: I will have to investigate how to test that ctor is really empty
using System;

class C
{
    public C () {}

    int i = new int ();
	int i2 = 1 - 1;
    double d = new double ();
    char c = new char ();
    bool b = new bool ();
    decimal dec2 = new decimal ();
	object o = null;
	ValueType BoolVal = (ValueType)null;
		
	int[] a_i = null;
	object[] a_o = null;
	ValueType[] a_v = null;
	
    public static void Main ()
    {
    }
}
