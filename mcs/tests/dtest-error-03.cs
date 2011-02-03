using System;
using Microsoft.CSharp.RuntimeBinder;
using System.Dynamic;
using System.Runtime.CompilerServices;

public class C
{
}

public class Test
{
	public static int Main ()
	{
		var getter = CallSite<Func<CallSite, object, object>>.Create (
						 Binder.GetMember (
						 CSharpBinderFlags.None, "n", null, new[] {
							 CSharpArgumentInfo.Create (CSharpArgumentInfoFlags.None, null) }));

		try {
			getter.Target (getter, new C ());
		} catch (RuntimeBinderException e) {
			if (e.Message == "`C' does not contain a definition for `n'")
				return 0;

			return 2;
		}

		return 1;
	}
}
