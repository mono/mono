//
// System.Web.UI.ViewStateOutputHashStream
//
// Authors:
//   Gonzalo Paniagua (gonzalo@ximian.com)
//
// Copyright (c) 2005 Novell, Inc (http://www.novell.com)
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
using System.IO;
using System.Security.Cryptography;

namespace System.Web.UI {
	class ViewStateOutputHashStream : CryptoStream {
		MemoryStream hash;
		MemoryStream data;

		public ViewStateOutputHashStream (MemoryStream ms, int seed) :
			base (ms, SHA1.Create (), CryptoStreamMode.Write)
		{
			this.hash = ms;
			this.data = new MemoryStream ();
			base.Write (BitConverter.GetBytes (seed), 0, 4);
		}

		public override void Write (byte[] buffer, int offset, int count)
		{
			data.Write (buffer, offset, count);
			base.Write (buffer, offset, count);
		}

		public string GetBase64String ()
		{
			FlushFinalBlock ();
			if (data.Length == 0)
				return null;

			data.Write (hash.GetBuffer (), 0, (int) hash.Length);
			return Convert.ToBase64String (data.GetBuffer (), 0, (int) data.Length);
		}
	}
}

