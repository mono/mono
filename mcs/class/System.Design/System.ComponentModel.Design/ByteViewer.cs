//
// System.ComponentModel.Design.ByteViewer
//
// Authors:
//      Martin Willemoes Hansen (mwh@sysrq.dk)
//
// (C) 2003 Martin Willemoes Hansen
//

using System.Windows.Forms;

namespace System.ComponentModel.Design
{
	public class ByteViewer : Control
	{
		[MonoTODO]
		public ByteViewer()
		{
		}

		public override ISite Site {
			[MonoTODO]
			get { throw new NotImplementedException(); } 
			[MonoTODO]
			set { throw new NotImplementedException(); }
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

		[MonoTODO]
		protected override void OnResize (EventArgs e)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		~ByteViewer()
		{
		}
	}

}
