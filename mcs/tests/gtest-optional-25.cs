using System;

public class Usage
{
	public static void Main ()
	{
		var bug = new Bug ();
		string[] tags = bug.MethodWithOptionalParameter<string> (0);
	}
}

public class Bug
{
	public TValue[] MethodWithOptionalParameter<TValue> (int index, TValue[] defaultValue = null)
	{
		return null;
	}
}

