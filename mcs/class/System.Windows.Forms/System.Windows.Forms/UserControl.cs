//
// System.Windows.Forms.UpDownEventHandler.cs
//
// Authors:
//	//unknown
//   Dennis Hayes (dennish@Raytek.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//
using System;
using System.Drawing;

namespace System.Windows.Forms {

	public class UserControl : ContainerControl{

		// --- Properties ---
		[MonoTODO]
		protected override Size DefaultSize {

			get { throw new NotImplementedException(); }
		}

		[MonoTODO]
		public override string Text {

			get {
				throw new NotImplementedException();
			}
			set {
				throw new NotImplementedException();
			}
		}

		// --- Constructor ---
		[MonoTODO]
		public UserControl()
		{
			
		}

		// --- Methods ---
		[MonoTODO]
		protected override void OnCreateControl()
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		protected virtual void OnLoad(EventArgs e)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		protected override void OnMouseDown(MouseEventArgs e)
		{
			throw new NotImplementedException();
		}

		[MonoTODO]
		protected override void WndProc(ref Message m)
		{
			throw new NotImplementedException();
		}

		// --- Events ---
		[MonoTODO]
		public event EventHandler Load;
	}
}
