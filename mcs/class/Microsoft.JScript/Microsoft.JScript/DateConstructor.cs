//
// DateConstructor.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

namespace Microsoft.JScript
{
	using System;

	public class DateConstructor : ScriptFunction
	{
		[JSFunctionAttribute (JSFunctionAttributeEnum.HasVarArgs)]
		public new DateObject CreateInstance (params Object[] args)
		{
			throw new NotImplementedException ();
		}

		public String Invoke ()
		{
			throw new NotImplementedException ();
		}

		[JSFunctionAttribute(0, JSBuiltin.Date_parse)]
		public static double parse (String str)
		{
			throw new NotImplementedException ();
		}

		[JSFunctionAttribute(0, JSBuiltin.Date_UTC)]
		public static double UTC (Object year, Object month, Object date, 
					  Object hours, Object minutes, Object seconds, Object ms)
		{
			throw new NotImplementedException ();
		}
	}
}