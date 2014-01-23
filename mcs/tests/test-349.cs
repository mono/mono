// Compiler options: -optimize+

using System;
using System.Reflection;

enum E
{
}

delegate void D();
		
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
	E e = new E ();
	event D Ev1 = null;
		
	int[] a_i = null;
	object[] a_o = null;
	ValueType[] a_v = null;
}

class X
{
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
		
	public static int Main ()
	{
		if (a != 5 || b != 0)
			return 1;
			
		if ((typeof (X2).Attributes & TypeAttributes.BeforeFieldInit) == 0)
			return 2;
		
#if NET_2_0
		ConstructorInfo mi = typeof(C).GetConstructors ()[0];
        MethodBody mb = mi.GetMethodBody();
		
		if (mb.GetILAsByteArray ().Length != 7) {
			Console.WriteLine("Optimization failed");
			return 3;
		}
#endif
			
		Console.WriteLine ("OK");
		return 0;
	}
}