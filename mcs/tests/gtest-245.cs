using System;

class DerivedGenericClass<T> : BaseClass
{
        public override void Foo () {}

        public void Baz ()
        {
                Foo ();
        }
}

abstract class BaseClass
{
        public abstract void Foo ();
}

class X
{
	public static void Main ()
	{
	}
}
