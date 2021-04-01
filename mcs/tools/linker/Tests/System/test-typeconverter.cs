using System;
using System.ComponentModel;
using System.Globalization;

public class FrameworkTypeConverters
{		
	public static int Main ()
	{
   		var typeConverter = TypeDescriptor.GetConverter (typeof (bool));
		var res = typeConverter.ConvertFromInvariantString ("true");
		if ((bool) res != true)
			return 1;

		return 0;
	}
}
