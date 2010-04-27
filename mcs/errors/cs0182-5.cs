// CS0182: An attribute argument must be a constant expression, typeof expression or array creation expression
// Line: 4

[A (false ? new object () : null)]
class AAttribute : System.Attribute
{
	public AAttribute (object value)
	{
	}
}
