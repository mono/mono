// Compiler options: -warnaserror -doc:xml-074.xml
using System.Collections.Generic;

 /// <summary>The Test</summary>
 public class Test
 {
	/// <summary>The Foo</summary>
	protected Dictionary<string, object> Foo { get; set; } = new Dictionary<string, object>();

	/// <summary>Tests the Foo</summary>
	protected bool TestFoo;

	static void Main ()
	{
	}
 }
