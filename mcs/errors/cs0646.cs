// CS0646: Cannot specify the `DefaultMember' attribute on type containing an indexer
// Line : 8

using System;
using System.Reflection;

[DefaultMember ("Item")]
public class Foo {

	string bar;
	
	public static void Main ()
	{
		Console.WriteLine ("foo");
	}

	string this [int idx] {
		get {
			return "foo";
		}
		set {
			bar = value;
		}
	}
}
		
