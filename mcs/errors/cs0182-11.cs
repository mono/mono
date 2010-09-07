// CS0182: An attribute argument must be a constant expression, typeof expression or array creation expression
// Line: 13

using System;

class MyAttribute : Attribute
{
	public MyAttribute (object s)
	{
	}
}

[My (decimal.MaxValue)]
class X
{
}
