//
// DateConstructor.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;

namespace Microsoft.JScript {

	public class DateConstructor : ScriptFunction {

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