//
// System.Drawing.PrivateFontCollection.cs
//
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
// Author: Konstantin Triger (kostat@mainsoft.com)
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
using System.IO;
using awt = java.awt;
using io = java.io;
using vmw.common;

namespace System.Drawing.Text
{
	/// <summary>
	/// Summary description for PrivateFontCollection.
	/// </summary>
	public sealed class PrivateFontCollection : FontCollection
	{
		public PrivateFontCollection()
		{
		}

		public void AddFontFile(string filename) {
			using(FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read)) {
				io.InputStream stream = vmw.common.IOUtils.ToInputStream (fs);
				awt.Font font = awt.Font.createFont(awt.Font.TRUETYPE_FONT, stream);
				AddFont(font);
			}
		}
#if INTPTR_SUPPORT
		public void AddMemoryFont(IntPtr memory, int length) {
		}
#endif
	}
}
