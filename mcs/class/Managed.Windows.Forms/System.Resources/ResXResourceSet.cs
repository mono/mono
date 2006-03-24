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

// COMPLETE

using System;
using System.IO;
using System.Collections;

namespace System.Resources {
	public class ResXResourceSet : ResourceSet {
		#region Local Variables

		#endregion	// Local Variables

		#region Public Constructors
		public ResXResourceSet(Stream stream) {
			this.Reader = new ResXResourceReader(stream);
			this.Table = new Hashtable();
			this.ReadResources();
		}

		public ResXResourceSet(string fileName) {
			this.Reader = new ResXResourceReader(fileName);
			this.Table = new Hashtable();
			this.ReadResources();
		}
		#endregion	// Public Constructors

		#region Public Instance Methods
		public override Type GetDefaultReader() {
			return typeof(ResXResourceReader);
		}

		public override Type GetDefaultWriter() {
			return typeof(ResXResourceWriter);
		}
		#endregion	// Public Instance Methods
	}
}
