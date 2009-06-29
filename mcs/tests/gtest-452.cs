// Compiler options: -langversion:future

using System;

struct S
{
}

public class C
{
	static void Foo<T> (T t, T u = default (T))
	{
	}

	static void TestParams (params int[] i)
	{
		throw new ApplicationException ();
	}

	static void TestParams (int i = 4)
	{
	}

	static void TestStruct (S? s = new S ())
	{
	}
	
	public string this [int i, string s = "test"] {
		get { return s; }
		set { value = s; }
	}

	public static int Main ()
	{
		Foo ("f");
		Foo (2);
		Foo (2, 4);
		Foo<long> (2);
		Foo<string> ("2", "3");
		
		TestParams ();
		
		TestStruct ();
		
		C c = new C ();
		if (c [1] != "test")
			return 1;
		
		c [3] = "value";
		
		return 0;
	}
}
