//
// Authors:
//      Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2010 Novell, Inc (http://www.novell.com)
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
using System.Collections.Specialized;
using System.Web.Configuration;
using System.Web.Util;

namespace System.Web
{
	sealed class HttpHeaderCollection : NameValueCollection
	{
		bool? headerCheckingEnabled;

		bool HeaderCheckingEnabled {
			get {
				if (headerCheckingEnabled == null)
					headerCheckingEnabled = HttpRuntime.Section.EnableHeaderChecking;

				return (bool)headerCheckingEnabled;
			}
		}
				
		public override void Add (string name, string value)
		{
			EncodeAndSetHeader (name, value, false);
		}

		public override void Set (string name, string value)
		{
			EncodeAndSetHeader (name, value, true);
		}

		void EncodeAndSetHeader (string name, string value, bool replaceExisting)
		{
			if (String.IsNullOrEmpty (name) || String.IsNullOrEmpty (value))
				return;

			string encName, encValue;
			if (HeaderCheckingEnabled) {
#if NET_4_0
				HttpEncoder.Current.HeaderNameValueEncode (name, value, out encName, out encValue);
#else
				HttpEncoder.HeaderNameValueEncode (name, value, out encName, out encValue);
#endif
			} else {
				encName = name;
				encValue = value;
			}

			if (replaceExisting)
				base.Set (encName, encValue);
			else
				base.Add (encName, encValue);
		}
	}
}
