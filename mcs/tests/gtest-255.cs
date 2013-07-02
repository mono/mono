using System;

public abstract class A
{
        public abstract T Foo<T> ();
}

public abstract class B : A
{
        public override T Foo<T> ()
        {
                return default (T);
        }
}

public class C : B
{
	public static void Main ()
	{ }
}
