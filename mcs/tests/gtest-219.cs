using System;
using System.Reflection;

public interface IFoo : ICloneable {}

public class Test
{
        public void Foo<T> () where T : IFoo {}

        public static int Main ()
        {
		MethodInfo mi = typeof (Test).GetMethod ("Foo");
		Type t = mi.GetGenericArguments () [0];
		Type[] ifaces = t.GetGenericParameterConstraints ();
		if (ifaces.Length != 1)
			return 1;
		if (ifaces [0] != typeof (IFoo))
			return 2;
		return 0;
        }
}

