//
// NumberConstructor.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

namespace Microsoft.JScript.Tmp
{
	using System;

	public class NumberConstructor : ScriptFunction
	{
		public const double MAX_VALUE = Double.MaxValue;
		public const double MIN_VALUE = Double.Epsilon;
		public const double NaN = Double.NaN;
		public const double NEGATIVE_INFINITY = Double.NegativeInfinity;
		public const double POSITIVE_INFINITY = Double.PositiveInfinity;
		
		[JSFunctionAttribute (JSFunctionAttributeEnum.HasVarArgs)]
		public new NumberObject CreateInstance (params Object [] args)
		{
			throw new NotImplementedException ();
		}

		public double Invoke (Object arg)
		{
			throw new NotImplementedException ();
		}
	}
}