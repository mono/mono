using System;
using System.Reflection;

public class SimpleAttribute : Attribute {

	string n;
	
	public SimpleAttribute (string name)
	{
		n = name;
	}
}

[AttributeUsage (AttributeTargets.All)]
public class MineAttribute : Attribute {
        public MineAttribute (params Type [] t)
        {
                types = t;
        }
        public Type[] types;
}

[Mine(typeof(int), typeof (string), typeof(object[]))]
public class Foo {
        public static int MM ()
        {
                object[] attrs = typeof (Foo).GetCustomAttributes (typeof(MineAttribute), true);
                MineAttribute ma = (MineAttribute) attrs [0];
                if (ma.types [0] != typeof (int)){
                        Console.WriteLine ("failed");
			return 1;
		}
                if (ma.types [1] != typeof (string)){
                        Console.WriteLine ("failed");
			return 2;
		}
                if (ma.types [2] != typeof (object [])){
                        Console.WriteLine ("failed");
			return 3;
		}
		Console.WriteLine ("OK");
                return 0;
        }
}

public class Blah {

	int i;

	public int Value {

		[Simple ("Foo!")]
		get {
			return i;
		}

		[Simple ("Bar !")]
		set {
			i = value;
		}
	}

	[Simple ((string) null)]
	int Another ()
	{
		return 1;
	}
	
	public static int Main ()
	{
		//
		// We need a better test which does reflection to check if the
		// attributes have actually been applied etc.
		//

		return Foo.MM ();
	}

}
