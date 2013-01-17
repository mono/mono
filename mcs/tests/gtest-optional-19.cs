using System;

public class MainClass
{
	public static void Main ()
	{
		Wrap (r => r.Find ());
		Wrap<IPackage> (r => r.Find ());
	}

	static void Wrap<T> (Func<IPackageRepository, T> factory, T defaultValue = null) where T : class
	{
	}
}

public interface IPackage
{
}

public interface IPackageRepository
{
	IPackage Find ();
}