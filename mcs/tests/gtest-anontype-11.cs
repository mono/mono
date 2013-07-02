// Compiler options: -checked

using System;

internal sealed class Alpha
{
	public Alpha (string value)
	{
		m_name = value;
	}

	public override int GetHashCode ()
	{
		return int.MaxValue & m_name.GetHashCode ();
	}

	private string m_name;
}

internal sealed class Beta
{
	public Beta (string value)
	{
		m_address = value;
	}

	public override int GetHashCode ()
	{
		return int.MaxValue & m_address.GetHashCode ();
	}

	private string m_address;
}

internal static class Program
{
	public static int Main ()
	{
		var a = new { First = new Alpha ("joe bob"), Second = new Beta ("main street") };
		Console.WriteLine ("hash = {0}", a.GetHashCode ());
		return 0;
	}
} 

