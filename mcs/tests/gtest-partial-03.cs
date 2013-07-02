using System.Collections.Generic;

class Variable
{
}

internal partial class Test<T>
{
}

internal partial class Test<T> where T : IList<Variable>
{
	public Test (T t)
	{
		var val = t.Count;
	}
}

internal partial class Test<T>
{
}

class CC
{
	public static void Main ()
	{
		new Test<List<Variable>> (new List<Variable> ());
	}
}