using System;

internal class Program
{
	public static void Main ()
	{
		AttributeOrDefault ("firstItem", null);	
	}
	
	public static string AttributeOrDefault (string attribute, string defaultValue = null)
	{
		return "";
	}

	public static string AttributeOrDefault (string attribute, bool? klass, string defaultValue = null)
	{
		throw new ApplicationException ();
	}
}
