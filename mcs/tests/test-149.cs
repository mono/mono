using System;

public delegate long MyDelegate (int a);

public interface X
{
	event EventHandler Foo;

	event MyDelegate TestEvent;
}

public class Y : X
{
	static int a = 0;

	event EventHandler X.Foo {
		add {
		}

		remove {
		}
	}

	public event EventHandler Foo;

	public event MyDelegate TestEvent;

	public int Test ()
	{
		X x = this;

		Foo += new EventHandler (callback1);
		TestEvent += new MyDelegate (callback2);

		x.Foo += new EventHandler (callback3);

		if (a != 0)
			return 1;

		Foo (this, new EventArgs ());
		if (a != 1)
			return 2;

		if (TestEvent (2) != 4)
			return 3;

		if (a != 2)
			return 4;

		return 0;
	}


	private static void callback1 (object sender, EventArgs e)
	{
		a = 1;
	}

	private static long callback2 (int b)
	{
		a = b;
		return a * a;
	}

	private static void callback3 (object sender, EventArgs e)
	{
		a = 3;
	}
}

public class Z : Y
{
	public delegate int SomeEventHandler();
        public static event SomeEventHandler BuildStarted;

	static int a ()
	{
		return 1;
	}
	public static int Main ()
	{
		Z z = new Z ();

		int result = z.Test ();

		if (result != 0)
			return result;

         	if (BuildStarted != null) {
             		BuildStarted();
         	}
		BuildStarted = new SomeEventHandler (a);
		if (BuildStarted () != 1)
			return 50; 

		return 0;
	}
}

//
// This class is just to test a bug in mcs; where we used to fail
// when accessing a static event, from an instance method.
//
public class Static {
	public static event EventHandler Test;
			
	public void Fire()
	{
		if ( Test != null )
			Test (null, null);
	}
}
