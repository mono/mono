//
// LenientGlobalObject.cs:
//
// Author: Cesar Octavio Lopez Nataren
//
// (C) Cesar Octavio Lopez Nataren, <cesar@ciencias.unam.mx>
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

	public sealed class LenientGlobalObject : GlobalObject {

		public new Object Infinity;
		public new Object NaN;
		public new Object undefined;
	
		// Function properties
		public new Object decodeURI;
		public new Object decodeURIComponent;
		public new Object encodeURI;
		public new Object encodeURIComponent;
		public new Object escape;
		public new Object eval;
		public new Object isNaN;
		public new Object isFinite;
		public new Object parseInt;
		public new Object parseFloat;
		public new Object GetObject;
		public new Object ScriptEngine;
		public new Object ScriptEngineBuildVersion;
		public new Object ScriptEngineMajorVersion;
		public new Object ScriptEngineMinorVersion;
		public new Object unescape;

		// built in types
		public new Object boolean;
		public new Object @byte;
		public new Object @char;
		public new Object @decimal;
		public new Object @double;
		public new Object @float;
		public new Object @int;
		public new Object @long;
		public new Object @sbyte;
		public new Object @short;
		public new Object @void;
		public new Object @uint;
		public new Object @ulong;
		public new Object @ushort;

		new public Object ActiveXObject {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		new public Object Array {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		new public Object Boolean {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		new public Object Date {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		new public Object Enumerator {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		new public Object Error {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		new public Object EvalError {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		new public Object Function {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		new public Object Math {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		new public Object Number {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		new public Object Object {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		new public Object RangeError {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		new public Object ReferenceError {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		new public Object RegExp {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		new public Object String {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		new public Object SyntaxError {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		new public Object TypeError {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		new public Object URIError {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		new public Object VBArray {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
	}
}
