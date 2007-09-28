//
// System.ComponentModel.Design.ByteViewer
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
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

using System.Windows.Forms;

namespace System.ComponentModel.Design
{
	[DesignTimeVisible(false)]
	[ToolboxItem(false)]
#if NET_2_0
	public class ByteViewer : TableLayoutPanel
#else
	public class ByteViewer : Control
#endif
	{
		[MonoTODO]
		public ByteViewer()
		{
		}

		[MonoTODO]
		public virtual DisplayMode GetDisplayMode()
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public virtual void SaveToFile (string path)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public virtual byte[] GetBytes ()
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public virtual void SetBytes (byte[] bytes)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public virtual void SetDisplayMode (DisplayMode mode)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public virtual void SetFile (string path)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		public virtual void SetStartLine (int line)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		protected override void OnKeyDown (KeyEventArgs e)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		protected override void OnPaint (PaintEventArgs e)
		{
			throw new NotImplementedException();
		}

#if NET_2_0

		[MonoTODO]
		protected override void OnLayout (LayoutEventArgs e)
		{
			throw new NotImplementedException();
		}
#else
		[MonoTODO]
		protected override void OnResize (EventArgs e)
		{
			throw new NotImplementedException();
		}
#endif

		[MonoTODO]
		protected virtual void ScrollChanged (object source, EventArgs e)
		{
			throw new NotImplementedException ();
		}
	}

}
