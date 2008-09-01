using System;
using System.Collections;

class Test
{
	class Yp
	{
		public IEnumerable fail ()
		{
			return null;
		}
	}

	static Yp YP = new Yp ();

	public static void Main ()
	{

	}

	public static IEnumerable syntax_error (object _Message, object _List)
	{
		{
			yield break;
		}

		foreach (bool l1 in YP.fail ()) {
			yield return false;
		}
	}
}
