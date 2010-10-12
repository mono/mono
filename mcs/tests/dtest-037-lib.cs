// Compiler options: -t:library

public class External
{
	public dynamic DynamicProperty { get; set; }
	public dynamic Field;
	
	public dynamic Method (dynamic d)
	{
		return d;
	}
	
	public void MethodOut (out dynamic d)
	{
		d = decimal.MaxValue;
	}
}

