using System;
[AttributeUsage (AttributeTargets.Class, AllowMultiple = true)]
	public class SimpleAttribute : Attribute {

		string name = null;

		public string MyNamedArg;

		private string secret;
		
		public SimpleAttribute (string name)
		{
			this.name = name;
		}

		public string AnotherArg {
			get {
				return secret;
			}
			set {
				secret = value;
			}
		}
		
	}

[Simple ("Dummy", MyNamedArg = "Dude!")]
[Simple ("Vids", MyNamedArg = "Raj", AnotherArg = "Foo")]	
	public class Blah {

		public static int Main ()
		{
			Console.WriteLine ("A dummy app which tests attribute emission");
			return 0;
		}
	}

	
