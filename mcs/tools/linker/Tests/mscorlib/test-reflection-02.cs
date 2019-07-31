using System.Reflection;
using System.Runtime.InteropServices;

class C
{
	[DllImport ("libc")]
	public static extern void pinvoke ();

	public static int Main ()
	{
		var mi = typeof (C).GetMethod ("pinvoke");
		var data = mi.CustomAttributes;
		
		int counter = 0;
		foreach (var entry in data) {
			++counter;

			if (entry.AttributeType == typeof (PreserveSigAttribute))
				continue;

			if (entry.AttributeType == typeof (DllImportAttribute)) {
				if ((string) entry.ConstructorArguments [0].Value != "libc")
					return 3;

				// PreserveSig
				if ((bool)entry.NamedArguments [4].TypedValue.Value != true)
					return 4;

				continue;
			}

			return 1;
		}

		if (counter != 2)
			return 2;

		return 0;
	}
}
