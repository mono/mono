// cs3008-7.cs: Identifier `System.Error.__ComObject' is not CLS-compliant
// Line: 8

using System;
[assembly:CLSCompliant(true)]

namespace System.Error {
	public class __ComObject : MarshalByRefObject {
		private __ComObject ()
		{
		}
	}
}
