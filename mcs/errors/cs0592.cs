// CS0592: The attribute `SimpleAttribute' is not valid on this declaration type. It is valid on `constructor' declarations only
// Line : 22

using System;

[AttributeUsage (AttributeTargets.Constructor, AllowMultiple = true)]
	public class SimpleAttribute : Attribute {

		string name = null;

		public string MyNamedArg;
		
		public SimpleAttribute (string name)
		{
			this.name = name;
		}
		
		
	}

[Simple ("Dummy", MyNamedArg = "Dude!")]
	public class Blah {

		public static void Main ()
		{
		}
	}
					   
	
