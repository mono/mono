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
// Copyright (c) 2004 Novell, Inc. (http://www.novell.com)
//
// Authors:
//
//
//

// NOT COMPLETE - This is a placeholder until our contributors get a chance to check their code in

using System;
using System.IO;

namespace System.Windows.Forms {
	public class OpenFileDialog : FileDialog {
		#region Public Constructors
		public OpenFileDialog() {
			throw new NotImplementedException ();
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		public bool Multiselect {
			get {
				throw new NotImplementedException ();
			}

			set {
				throw new NotImplementedException ();
			}
		}

		public bool ReadOnlyChecked {
			get {
				throw new NotImplementedException ();
			}

			set {
				throw new NotImplementedException ();
			}
		}
		public bool ShowReadOnly {
			get {
				throw new NotImplementedException ();
			}

			set {
				throw new NotImplementedException ();
			}
		}

		#endregion	// Public Instance Properties

		#region Public Instance Methods
		public Stream OpenFile() {
			throw new NotImplementedException ();
		}
		#endregion	// Public Instance Methods

		[MonoTODO]
		public override void Reset() {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override bool RunDialog(IntPtr hwndOwner) {
			throw new NotImplementedException();
		}


	}
}
