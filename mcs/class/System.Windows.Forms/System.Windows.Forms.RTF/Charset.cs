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
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Peter Bartok	(pbartok@novell.com)
//
//

using System;

// COMPLETE

namespace System.Windows.Forms.RTF {

#if RTF_LIB
	public
#else
	internal
#endif
	class Charset {
		#region Local Variables
		private CharsetType	id;
		private CharsetFlags	flags;
		private Charcode	code;
		private string		file;
		#endregion	// Local Variables

		#region Public Constructors
		public Charset() {
			flags = CharsetFlags.Read | CharsetFlags.Switch;
			id = CharsetType.General;
			file = string.Empty;
			this.ReadMap();
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		public Charcode Code {
			get {
				return code;
			}

			set {
				code = value;
			}
		}

		public CharsetFlags Flags {
			get {
				return flags;
			}

			set {
				flags = value;
			}
		}

		public CharsetType ID {
			get {
				return id;
			}

			set {
				switch(value) {
					case CharsetType.Symbol: {
						id = CharsetType.Symbol;
						return;
					}

					default:
					case CharsetType.General: {
						id = CharsetType.General;
						return;
					}
				}
			}
		}

		public string File {
			get {
				return file;
			}

			set {
				if (file != value) {
					file = value;
				}
			}
		}

		public StandardCharCode this[int c] {
			get {
				return code[c];
			}
		}

		#endregion	// Public Instance Properties

		#region Public Instance Methods
		public bool ReadMap() {
			switch (id) {
				case CharsetType.General: {
					if (file == string.Empty) {
						code = Charcode.AnsiGeneric;
						return true;
					}
					// FIXME - implement reading charmap from file...
					return true;
				}

				case CharsetType.Symbol: {
					if (file == string.Empty) {
						code = Charcode.AnsiSymbol;
						return true;
					}

					// FIXME - implement reading charmap from file...
					return true;
				}

				default: {
					return false;
				}
			}
		}

		public char StdCharCode(string name) {
			// FIXME - finish this
			return ' ';
			
		}

		public string StdCharName(char code) {
			// FIXME - finish this
			return String.Empty;
		}
		#endregion	// Public Instance Methods
	}
}
