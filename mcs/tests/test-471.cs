// Compiler options: /doc:test-471.xml

using System;

/// <summary><see cref="AAttribute" /></summary>
[Obsolete("whatever", true)]
public class AAttribute : Attribute {
}

class Demo {
	static void Main ()
	{
	}
}
