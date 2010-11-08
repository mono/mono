using System;
using System.Reflection;
using System.Runtime.InteropServices;

public delegate int D ([In] ref int arg);

class B
{
	public static int Main ()
	{
		var methods = typeof (D).GetMethods ();
		foreach (var m in methods) {
			var pi = m.GetParameters ();
			switch (m.Name) {
			case "Invoke":
				if (!pi[0].IsIn)
					return 1;
				break;
			case "BeginInvoke":
				if (!pi[0].IsIn)
					return 2;
				break;
			case "EndInvoke":
				if (!pi[0].IsIn)
					return 3;

				break;
			}
		}

		return 0;
	}
}