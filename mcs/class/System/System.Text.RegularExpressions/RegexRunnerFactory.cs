//
// assembly:	System
// namespace:	System.Text.RegularExpressions
// file:	RegexRunnerFactory.cs
//
// author:	Dan Lewis (dihlewis@yahoo.co.uk)
// 		(c) 2002

using System;
using System.ComponentModel;

namespace System.Text.RegularExpressions {
	[EditorBrowsable (EditorBrowsableState.Never)]
	public abstract class RegexRunnerFactory {
		protected RegexRunnerFactory () {
		}

		protected internal abstract RegexRunner CreateInstance ();
	}
}
