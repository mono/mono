using System;

interface II
{

}

class X
{
	static void Foo (II a = default (II), II b = default, II c = (II) null)
	{
	}

	static int Main ()
	{
		// Check installed mscorlib
		// Type is included in Mono 2.4+, and .NET 3.5 SP1
		object o = typeof (System.Runtime.InteropServices.AllowReversePInvokeCallsAttribute);
		
		// It should crash but double check it in case of very old old runtime
		if (o == null)
			return 1;

		var consts = o.GetType ().Assembly.GetType ("Consts");
		if (consts == null) {
			// We could be bootraping on cygwin using .net runtime
			var assembly = o.GetType ().Assembly;
			if (assembly.GetName ().Version >= new Version (4, 0) && assembly.Location.Contains ("Microsoft.NET"))
				return 0;

			return 2;
		}

		var field = consts.GetField ("MonoVersion");
		if (field == null)
			return 3;

		Version version;
		if (!Version.TryParse (field.GetValue (null) as string, out version))
			return 4;

		Version min_mono_version;
#if __MonoCS__
		min_mono_version = new Version (6, 5);
#else
		min_mono_version = new Version (6, 5);
#endif

		if (version < min_mono_version)
			return 5;

		return 0;
	}
}
