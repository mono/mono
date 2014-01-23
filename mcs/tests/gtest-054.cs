//
// Conversions involving type parameters (26.7.4).
// This is a compilation-only test since some of the explict
// conversions would trigger an InvalidCastException.
//

using System;

interface Foo
{
	void Hello ();
}

class A
{ }

class B : A, Foo
{
	public void Hello ()
	{ }

	public static implicit operator C (B b)
	{
		return new C ();
	}
}

class C
{
	public static explicit operator B (C c)
	{
		return new B ();
	}
}

class Test
{
	static void Simple<T> (T t)
	{
		object o = t;
		t = (T) o;
		Foo foo = (Foo) t;
		t = (T) foo;
	}

	static void Interface<T> (T t)
		where T : Foo
	{
		Foo foo = t;
	}

	static void Class<T> (T t)
		where T : B
	{
		B b = t;
		A a = t;
		Foo foo = t;
		t = (T) b;
		t = (T) a;
		t = (T) foo;
		C c = t;
		t = (T) c;
	}

	static void Array<T> (T[] t)
	{
		object o = t;
		Array a = t;
		t = (T []) o;
		t = (T []) a;
	}

	public static void Main ()
	{ }
}
