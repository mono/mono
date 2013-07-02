using System;
using System.Reflection;

class Program
{
	public static int Main ()
	{
		Type type = typeof (Foo<>);
		Type [] gargs = type.GetGenericArguments ();
		if (gargs == null || gargs.Length != 1) {
			Console.WriteLine ("#1");
			return 1;
		}

		Type garg = gargs [0];
		Type [] csts = garg.GetGenericParameterConstraints ();

		if (garg.Name != "T") {
			Console.WriteLine ("#2: " + garg.Name);
			return 2;
		}
		if (garg.GenericParameterAttributes !=
			(GenericParameterAttributes.DefaultConstructorConstraint | GenericParameterAttributes.NotNullableValueTypeConstraint)) {
			Console.WriteLine ("#3: " + garg.GenericParameterAttributes);
			return 3;
		}
		if (csts == null || csts.Length != 1) {
			Console.WriteLine ("#4");
			return 4;
		}
		if (csts [0] != typeof (ValueType)) {
			Console.WriteLine ("#5: " + csts [0].FullName);
			return 5;
		}

		return 0;
	}
}

struct Foo<T> where T : struct
{
}
