// Compiler options: -r:gtest-532-lib.dll

using System;

public class DictionaryServicesContainer : IServicesContainer
{
	public void Register<I, T> () where T : I
	{
		throw new NotImplementedException ();
	}

	public void Register<I> (object instance)
	{
		throw new NotImplementedException ();
	}

	public I Resolve<I> ()
	{
		throw new NotImplementedException ();
	}

	public static void Main ()
	{
		new DictionaryServicesContainer ();
	}
}
