// Compiler options: -doc:xml-040.xml -warnaserror -warn:4
using System.Collections;

/// <summary><see cref="IDictionary.this[object]" /></summary>
public class Test {
	static void Main () {
	}

	/// <summary> test indexer doc </summary>
	public string this [string name] {
		get { return null; }
	}
}

