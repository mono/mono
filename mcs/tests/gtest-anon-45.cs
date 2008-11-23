using System;

public class C
{
	public static int Main ()
	{
		new C ().AnyMethod<int> ();
		return 0;
	}

	public void AnyMethod<T> ()
	{
		Action outerAction = () => {
			string aString = "aString";
			Action<string> innerAction = innerActionParam => innerActionParam.Contains (aString);
		};

		outerAction ();
	}
}
