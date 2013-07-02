//
// This tests checks out field access to arrays
//
using System;

struct A {
	public int a;
}

class Y {
	public object a;
}

class X {
	static A [] a_single = new A [10];
	static A [,] a_double = new A [10,10];
	static Y [] o_single = new Y [10];
	static Y [,] o_double = new Y [10,10];

	static void FillOne ()
	{
		a_single [0].a = 1;
	}
	
	static void FillSingle ()
	{
		int i;
		
		for (i = 0; i < 10; i++){
			a_single [i].a = i + 1;
		}
	}

	static void FillDouble ()
	{
		int i, j;
		
		for (i = 0; i < 10; i++)
			for (j = 0; j < 10; j++)
				a_double [i,j].a = i * j;
	}

	static void FillObject ()
	{
		int i;
		
		for (i = 0; i < 10; i++){
			o_single [i] = new Y ();
			o_single [i].a = (i + 1);
		}
	}

	static void FillDoubleObject ()
	{
		int i, j;
		
		for (i = 0; i < 10; i++)
			for (j = 0; j < 10; j++){
				o_double [i,j] = new Y ();
				o_double [i,j].a = i * j;
			}
	}
	
	static int TestSingle ()
	{
		int i;
		
		for (i = 0; i < 10; i++){
			if (a_single [i].a != i + 1)
				return 1;
		}
		return 0;
	}

	static int TestDouble ()
	{
		int i, j;

		for (i = 0; i < 10; i++){		
			for (j = 0; j < 10; j++)
				if (a_double [i,j].a != (i *j))
					return 2;
		}

		return 0;
	}

	static int TestObjectSingle ()
	{
		int i;
		
		for (i = 0; i < 10; i++){
			if ((int)(o_single [i].a) != i + 1)
				return 1;
		}
		return 0;
	}

	static int TestObjectDouble ()
	{
		int i, j;

		for (i = 0; i < 10; i++){		
			for (j = 0; j < 10; j++)
				if (((int)o_double [i,j].a) != (i *j))
					return 2;
		}

		return 0;
	}
	
	public static int Main ()
	{
		FillSingle ();
		FillDouble ();
		FillObject ();
		FillDoubleObject ();
		
		if (TestSingle () != 0)
			return 1;
		
		if (TestDouble () != 0)
			return 2;

		if (TestObjectSingle () != 0)
			return 3;

		if (TestObjectDouble () != 0)
			return 4;

		Console.WriteLine ("test passes");
		return 0;
	}
}
