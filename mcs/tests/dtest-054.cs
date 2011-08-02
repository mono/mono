using System;

// dynamic with anonymous method mutator

class C
{
	static Action<T> Test<T> (T t)
	{
		return l => {
			dynamic d = l;
			d.Method (l);
		};
	}

	static Action Test2<T> (T t)
	{
		T l = t;
		return () => {
			T l2 = l;
			Action a = () => {
				dynamic d = l2;
				d.Method (l);
			};

			a ();
		};
	}

	static Action<T> Test3<T> (T t)
	{
		return l => {
			dynamic d = l;
			d.MethodRef (ref l);
		};
	}

	static Action Test4<T> (T t)
	{
		T l = t;
		return () => {
			dynamic d = l;
			d.MethodRef (ref l);
		};
	}

	void Method (object arg)
	{
	}

	void MethodRef (ref C arg)
	{
		arg = new C ();
	}

	public static int Main ()
	{
		Test<C> (null) (new C ());
		Test2 (new C ()) ();
		Test<C> (null) (new C ());
		Test4 (new C ()) ();

		return 0;
	}
}