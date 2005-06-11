//
// RegExpObject.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) 2003, Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
// (C) Copyright 2005, Novell Inc (http://novell.com)
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
using System.Text;

namespace Microsoft.JScript {

	public class RegExpObject : JSObject {

		private string _source;
		private bool _ignoreCase;
		private bool _global;
		private bool _multiline;
			
		public override string ToString ()
		{
			StringBuilder str = new StringBuilder ();
			str.Append ("/");
			str.Append (_source);
			str.Append ("/");

			if (_ignoreCase)
				str.Append ("i");
			if(_global)
				str.Append ("g");
			if (_multiline)
				str.Append ("m");

			return str.ToString ();
		}

		public string source {
			get { return _source; }
		}

		public bool ignoreCase {
			get { return _ignoreCase; }
		}

		public bool global {
			get { return _global; }
		}
		
		public bool multiline {
			get { return _multiline; }
		}

		public Object lastIndex {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		internal RegExpObject  (string pattern, bool ignoreCase, bool global, bool multiLine)
		{
			_source = pattern;
			_ignoreCase = ignoreCase;
			_global = global;
			_multiline = multiLine;
		}
	}
}
