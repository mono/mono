//
// assembly:	System
// namespace:	System.Text.RegularExpressions
// file:	RegexRunnerFactory.cs
//
// author:	Dan Lewis (dihlewis@yahoo.co.uk)
// 		(c) 2002

using System;

namespace System.Text.RegularExpressions {
	
	public abstract class RegexRunnerFactory {
		protected RegexRunnerFactory () {
			throw new NotImplementedException ("RegexRunnerFactory is not supported by Mono.");
		}

		protected internal abstract RegexRunner CreateInstance ();
	}
}
