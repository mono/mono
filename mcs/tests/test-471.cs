using System;

class AAttribute : Attribute
{
	public string Value;
	
	public AAttribute (string s)
	{
		Value = s;
	}
}

[A (value)]
class MainClass
{
	const string value = null;
	
	public static int Main ()
	{
		var attr = typeof (MainClass).GetCustomAttributes (false) [0] as AAttribute;
		if (attr.Value != null)
			return 1;
		
		return 0;
	}
} 
