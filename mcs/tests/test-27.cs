using System;

public interface Hello {

	bool MyMethod (int i);
}

public interface Another : Hello {

	int AnotherMethod (int i);
}

public class Foo : Hello, Another {

	public bool MyMethod (int i)
	{
		if (i == 22)
			return true;
		else
			return false;
	}

	public int AnotherMethod (int i)
	{
		return i * 10;
	}
	
}

public interface ITest {

	bool TestMethod (int i, float j);
}

public class Blah : Foo {

	public delegate void MyDelegate (int i, int j);

	void Bar (int i, int j)
	{
		Console.WriteLine (i+j);
	}
	
	public static int Main ()
	{
		Blah k = new Blah ();

		Foo f = k;

		object o = k;

		if (f is Foo)
			Console.WriteLine ("I am a Foo!");

		Hello ihello = f;

		Another ianother = f;

		ihello = ianother; 

		bool b = f.MyMethod (22);

		MyDelegate del = new MyDelegate (k.Bar);

		del (2, 3);
		
		Delegate tmp = del;

		// Explicit reference conversions
		
		MyDelegate adel = (MyDelegate) tmp;

		adel (4, 7);

		Blah l = (Blah) o;

		l.Bar (20, 30);

		l = (Blah) f;

		l.Bar (2, 5);

		f = (Foo) ihello;

		// The following cause exceptions even though they are supposed to work
		// according to the spec

		// This one sounds ridiculous !
		// ITest t = (ITest) l;
		
		// ITest u = (ITest) ihello;

		return 0;

	}
}

