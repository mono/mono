//
// System.Runtime.Remoting.Metadata.W3cXsd2001.SoapDateTime
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//      Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) 2003 Martin Willemoes Hansen
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Globalization;

namespace System.Runtime.Remoting.Metadata.W3cXsd2001 
{
#if NET_2_0
	[System.Runtime.InteropServices.ComVisible (true)]
#endif
	public sealed class SoapDateTime
	{
		static readonly string[] _datetimeFormats = new string[] {
		  "yyyy-MM-ddTHH:mm:ss",
		  "yyyy-MM-ddTHH:mm:ss.f",
		  "yyyy-MM-ddTHH:mm:ss.ff",
		  "yyyy-MM-ddTHH:mm:ss.fff",
		  "yyyy-MM-ddTHH:mm:ss.ffff",
		  "yyyy-MM-ddTHH:mm:ss.fffff",
		  "yyyy-MM-ddTHH:mm:ss.ffffff",
		  "yyyy-MM-ddTHH:mm:ss.fffffff",
		  "yyyy-MM-ddTHH:mm:sszzz",
		  "yyyy-MM-ddTHH:mm:ss.fzzz",
		  "yyyy-MM-ddTHH:mm:ss.ffzzz",
		  "yyyy-MM-ddTHH:mm:ss.fffzzz",
		  "yyyy-MM-ddTHH:mm:ss.ffffzzz",
		  "yyyy-MM-ddTHH:mm:ss.fffffzzz",
		  "yyyy-MM-ddTHH:mm:ss.ffffffzzz",
		  "yyyy-MM-ddTHH:mm:ss.fffffffzzz",
		  "yyyy-MM-ddTHH:mm:ssZ",
		  "yyyy-MM-ddTHH:mm:ss.fZ",
		  "yyyy-MM-ddTHH:mm:ss.ffZ",
		  "yyyy-MM-ddTHH:mm:ss.fffZ",
		  "yyyy-MM-ddTHH:mm:ss.ffffZ",
		  "yyyy-MM-ddTHH:mm:ss.fffffZ",
		  "yyyy-MM-ddTHH:mm:ss.ffffffZ",
		  "yyyy-MM-ddTHH:mm:ss.fffffffZ"
		};

		public SoapDateTime()
		{
		}
		
		public static string XsdType {
			get { return "dateTime"; }
		}

		public static DateTime Parse (string value)
		{
			return DateTime.ParseExact (value, _datetimeFormats, null, DateTimeStyles.None);
		}

		public static string ToString(DateTime value)
		{
			return value.ToString("yyyy-MM-ddTHH:mm:ss.fffffffzzz", CultureInfo.InvariantCulture);
		}
	}
}
