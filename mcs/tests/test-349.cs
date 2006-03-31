// Compiler options: -optimize+

// TODO: I will have to investigate how to test that instance ctor is really empty
// GetMethodBody in 2.0

using System;
using System.Reflection;

class C {
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
}

class X
{
	public delegate void D();
	public static event D Ev1 = null;
	public static event D Ev2 = null;
	protected static string temp = null, real_temp = null;
}
	
class X2 
{
	static int i = 5;
}
	

class Test
{
	static int a = b = 5;
	static int b = 0;
		
	static int Main ()
	{
		if (a != 5 || b != 0)
			return 1;
			
		if ((typeof (X2).Attributes & TypeAttributes.BeforeFieldInit) == 0)
			return 2;
			
		Console.WriteLine ("OK");
		return 0;
	}
}