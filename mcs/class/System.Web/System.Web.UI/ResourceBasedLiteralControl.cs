//
// System.Web.UI.ResourceBasedLiteralControl.cs
//
// Author:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// Copyright (C) 2009 Novell, Inc (http://www.novell.com)
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

using System.Runtime.InteropServices;
using System.Text;

namespace System.Web.UI {
        class ResourceBasedLiteralControl : LiteralControl
	{
		IntPtr ptr;
		int length;

		public ResourceBasedLiteralControl (IntPtr ptr, int length)
		{
			EnableViewState = false;
			AutoID = false;
			this.ptr = ptr;
			this.length = length;
		}

		public override string Text {
			get {
				if (length == -1)
					return base.Text;

				byte [] bytes = new byte [length];
				Marshal.Copy (ptr, bytes, 0, length);
				return Encoding.UTF8.GetString (bytes);
			}
			set {
				length = -1;
				base.Text = value;
			}
		}

#if NET_2_0
		protected internal
#else		
		protected
#endif		
		override void Render (HtmlTextWriter writer)
		{
			if (length == -1) {
				writer.Write (base.Text);
				return;
			}

			HttpWriter hw = writer.GetHttpWriter ();
			if (hw == null || hw.Response.ContentEncoding.CodePage != 65001) {
				byte [] bytes = new byte [length];
				Marshal.Copy (ptr, bytes, 0, length);
				writer.Write (Encoding.UTF8.GetString (bytes));
				bytes = null;
				return;
			}

			hw.WriteUTF8Ptr (ptr, length);
		}
	}
}

