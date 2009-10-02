class X {
	// Check installed compiler
	static void Generic<T> () { }

	static int Main ()
	{
		// Check installed mscorlib
		// Type is included in Mono 2.0+, and .NET 2.0 SP1+
		object o = typeof (System.Runtime.GCLatencyMode);
		return 0;
	}
}
