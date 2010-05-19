using System;

public class MyClass
{
	public class A
	{
		public event EventHandler MouseClick;
	}

	public class B : A
	{
		public new event EventHandler MouseClick;
	}

	public class C : B
	{
		public new void MouseClick ()
		{
			Console.WriteLine ("This should be printed");
		}
	}

	static public void Main ()
	{
		C myclass = new C ();
		myclass.MouseClick ();
	}
}

