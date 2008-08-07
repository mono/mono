// CS0182: An attribute argument must be a constant expression, typeof expression or array creation expression
// Line: 4

[A (true is bool)]
class AAttribute : System.Attribute
{
	public AAttribute (bool b)
	{
	}
}
