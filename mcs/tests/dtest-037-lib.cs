// Compiler options: -t:library

public interface I<T>
{
	T Value { get; }
}

public class AI: I<object>
{
	public object Value { get; set; }
}

public class External
{
	public dynamic DynamicProperty { get; set; }
	public dynamic Field;
	public dynamic[,] FieldArray;
	
	public dynamic Method (dynamic d)
	{
		return d;
	}
	
	public void MethodOut (out dynamic d)
	{
		d = decimal.MaxValue;
	}
	
	public I<dynamic>[] Method2 (dynamic d)
	{
		return new [] { new AI () { Value = d }};
	}
	
	// Same as Method2 to check we are interning dynamic
	public I<dynamic>[] Method3 (dynamic d)
	{
		return d;
	}
}

public class CI<T> : I<T>
{
	public T Value { get; set; }
}

public class CI2 : CI<dynamic>
{
}

