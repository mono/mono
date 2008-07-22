// CS3008: Identifier `System.Error.__ComObject' is not CLS-compliant
// Line: 9
// Compiler options: -warnaserror -warn:1

using System;
[assembly:CLSCompliant(true)]

namespace System.Error {
	public class __ComObject : MarshalByRefObject {
		private __ComObject ()
		{
		}
	}
}
