using System;

public class Class
{
	string Property { get { return " Property"; } }

	string Method ()
	{
		string methodVariable = "method variable";

		Func<string> outerAction = () => {
			// If methodVariable is not accessed here, the compiler does not crash
			string unused = methodVariable;

			string innerVariable = "inner variable";

			Func<string, string> middleAction = lambdaParameter => {
				// If any of the variables referenced are removed, the compiler does not crash.
				Func<string> innerFunc = () => lambdaParameter + innerVariable + Property;
				return innerFunc ();
			};

			return middleAction ("> ");
		};

		return outerAction ();
	}

	public static int Main ()
	{
		Class c = new Class ();
		string s = c.Method ();
		Console.WriteLine (s);
		if (s != "> inner variable Property")
			return 1;

		return 0;
	}
}

