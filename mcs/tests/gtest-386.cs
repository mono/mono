using System;
using System.Linq.Expressions;

public struct MyType
{
	int value;

	public MyType (int value)
	{
		this.value = value;
	}
	public static MyType operator - (MyType a)
	{
		return new MyType (-a.value);
	}
}

class C
{
	public static int Main ()
	{
		MyType? x = null;
		MyType? y = -x;
		
		checked {
			float? f = float.MinValue;
			f = -f + 1;
			int? i = int.MinValue;
			try {
				i = -i;
				return 1;
			} catch (OverflowException) { }
		}		
		
		return 0;
	}
}
