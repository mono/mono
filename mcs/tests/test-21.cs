using System;

public class Blah {

	public class Foo {

		public Foo ()
		{
			Console.WriteLine ("Inside the Foo constructor now");
		}
		
		public void Bar ()
		{
			Console.WriteLine ("The Bar method");
		}
		
		
	}

	public static void Main ()
	{
		Blah i = new Blah ();

		Foo f = new Foo ();

		f.Bar ();

	}

}
