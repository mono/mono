using System;

class X {
	// Check installed compiler
	static void Generic<T> ()
	{
		// we use 'var' all around in the compiler sources
		var x = new X ();
	}
	
	void DefaultParametersAvailable (int i = 3)
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

		if (version < new Version (3, 8))
			return 5;

		return 0;
	}
}
