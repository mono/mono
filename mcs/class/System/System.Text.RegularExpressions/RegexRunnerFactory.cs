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
	/* I'm just guessing that this is the correct place for this
	 * attribute, and that the option is correct.  It shuts up
	 * CorCompare for this undocumented class.
	 */
	[EditorBrowsable (EditorBrowsableState.Never)]
	public abstract class RegexRunnerFactory {
		protected RegexRunnerFactory () {
			throw new NotImplementedException ("RegexRunnerFactory is not supported by Mono.");
		}

		protected internal abstract RegexRunner CreateInstance ();
	}
}
