using System;

[AttributeUsage (AttributeTargets.Class)]
	public class SimpleAttribute : Attribute {

		string name = null;
		
		public SimpleAttribute (string name)
		{
			this.name = name;
		}
	}

[Simple ("Dummy")]
	public class Blah {

		[Simple ("Dummy")]
		public static int Main ()
		{
			Console.WriteLine ("A dummy app which tests attribute emission");

			return 0;
		}
	}
					   
	
