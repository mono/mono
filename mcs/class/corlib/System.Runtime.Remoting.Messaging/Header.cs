//
// System.Runtime.Remoting.Messaging.Header.cs
//
// Author:
//   Dan Lewis (dihlewis@yahoo.co.uk)
//
// (C) 2002
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

using System.Collections;

namespace System.Runtime.Remoting.Messaging {

	[Serializable]
#if NET_2_0
	[System.Runtime.InteropServices.ComVisible (true)]
#endif
	public class Header {
		public Header (string name, object value) :
			this (name, value, true)
		{
		}

		public Header (string name, object value, bool must_understand) :
			this (name, value, must_understand, null)
		{
		}

		public Header (string name, object value, bool must_understand, string header_namespace) {
			this.Name = name;
			this.Value = value;
			this.MustUnderstand = must_understand;
			this.HeaderNamespace = header_namespace;
		}

		// fields

		public string HeaderNamespace;

		public bool MustUnderstand;

		public string Name;

		public object Value;
	}
}
