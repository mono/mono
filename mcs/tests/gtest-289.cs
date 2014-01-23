// Compiler options: -optimize+

using System;
using System.Reflection;

enum E
{
}

delegate void D();

class C {
	public C () {}

	int i = default (int);
	double d = default (double);
	char c = default (char);
	bool b = default (bool);
	decimal dec2 = default (decimal);
	object o = default (object);
	ValueType BoolVal = default (ValueType);
	E e = default (E);
		
	int[] a_i = default(int[]);
	object[] a_o = default(object[]);
	ValueType[] a_v = default(ValueType[]);

	event D Ev1 = default(D);
}

class Consts
{
	const int i = default (int);
	const double d = default (double);
	const char c = default (char);
	const bool b = default (bool);
	const decimal dec2 = default (decimal);
	const object o = default (object);
	const ValueType BoolVal = default (ValueType);
	const E e = default (E);
		
	const int[] a_i = default(int[]);
	const object[] a_o = default(object[]);
	const ValueType[] a_v = default(ValueType[]);
}

class Test
{
	public static int Main ()
	{
		ConstructorInfo mi = typeof(C).GetConstructors ()[0];
        MethodBody mb = mi.GetMethodBody();
		
		if (mb.GetILAsByteArray ().Length != 7) {
			Console.WriteLine("Optimization failed");
			return 3;
		}

		bool b = default (DateTime?) == default (DateTime?);
		if (!b)
			return 19;
		
		Console.WriteLine ("OK");
		return 0;
	}
}