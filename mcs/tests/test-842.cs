using System;
using System.Reflection;

interface IA
{
	string this[int idx] { get; set; }
}

[DefaultMember ("Main")]
public class Foo : IA
{
	string bar;

	public static void Main ()
	{
		Console.WriteLine ("foo");
	}

	string IA.this[int idx] {
		get {
			return "foo";
		}
		set {
			bar = value;
		}
	}
}
		
