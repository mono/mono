using System;
using System.Linq;
using System.Collections.Generic;

class X
{
	public static void Execute<TArg>(TArg args)
	{
		Action a = () => {
			List<string> s = new List<string> () {
				"test"
			};

			object res = null;
			var res2 = s.Select(acrl => acrl.Select(acr => res)).ToArray ();
		};

		a ();
	}

	public static void Main ()
	{
		Execute<string> (null);
	}
}