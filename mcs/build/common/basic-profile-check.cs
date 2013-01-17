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

		return 0;
	}
}
