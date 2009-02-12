// Compiler options: -warnaserror

using System;
[assembly: CLSCompliant (true)]

public class Base
{
	public virtual void Test (int[] a)
	{
	}
}

public class CLSClass : Base
{
	public override void Test (params int[] b)
	{
	}
	
	public static void Main ()
	{
	}
}
