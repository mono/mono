//
// This is a test for as expression
// in custom attribute constructors
//

using System;

class MyAttribute : Attribute {

	public MyAttribute (string s)
	{
	}
}

[My (null as string)]
class X {

	static void Main ()
	{
	}
}
