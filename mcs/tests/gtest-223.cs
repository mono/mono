// Bug #76551
using System;
using System.Reflection;

public abstract class Foo<T> where T : class
{
}

public class Test
{
        public Foo<K> Hoge<K> () where K : class { return null; }

        public static void Main ()
        {
                MethodInfo mi = typeof (Test).GetMethod ("Hoge");
                foreach (Type t in mi.GetGenericArguments ())
			if ((t.GenericParameterAttributes & GenericParameterAttributes.ReferenceTypeConstraint) == 0)
				throw new Exception ();
        }
}

