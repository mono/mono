//
// LenientDatePrototype.cs:
//
// Author:
//	Cesar Lopez Nataren (cesar@ciencias.unam.mx)
//
// (C) 2003, Cesar Lopez Nataren
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

	public sealed class LenientDatePrototype : DatePrototype {  

		public new Object constructor;
		public new Object getTime;
		public new Object getYear;
		public new Object getFullYear;
		public new Object getUTCFullYear;
		public new Object getMonth;
		public new Object getUTCMonth;
		public new Object getDate;
		public new Object getUTCDate;
		public new Object getDay;
		public new Object getUTCDay;
		public new Object getHours;
		public new Object getUTCHours;
		public new Object getMinutes;
		public new Object getUTCMinutes;
		public new Object getSeconds;
		public new Object getUTCSeconds;
		public new Object getMilliseconds;
		public new Object getUTCMilliseconds;
		public new Object getVarDate;
		public new Object getTimezoneOffset;
		public new Object setTime;
		public new Object setMilliseconds;
		public new Object setUTCMilliseconds;
		public new Object setSeconds;
		public new Object setUTCSeconds;
		public new Object setMinutes;
		public new Object setUTCMinutes;
		public new Object setHours;
		public new Object setUTCHours;
		public new Object setDate;
		public new Object setUTCDate;
		public new Object setMonth;
		public new Object setUTCMonth;
		public new Object setFullYear;
		public new Object setUTCFullYear;
		public new Object setYear;
		public new Object toGMTString;
		public new Object toDateString;
		public new Object toLocaleDateString;
		public new Object toLocaleString;
		public new Object toLocaleTimeString;
		public new Object toString;
		public new Object toTimeString;
		public new Object toUTCString;
		public new Object valueOf;
	}
}
