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
// Author:
//	Jordi Mas i Hernandez <jordi@ximian.com>
//
//

using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace System.Windows.Forms
{
	[DefaultProperty("GridEditName")]
	[DesignTimeVisible(false)]
	[ToolboxItem(false)]
	[ClassInterface (ClassInterfaceType.AutoDispatch)]
	[ComVisible (true)]
	public class DataGridTextBox : TextBox
	{

		#region	Local Variables
		private bool isnavigating;
		private DataGrid grid;
		#endregion	// Local Variables

		#region Constructors
		public DataGridTextBox ()
		{
			isnavigating = true;
			grid = null;
			accepts_tab = true;
			accepts_return = false;

			SetStyle (ControlStyles.UserPaint | ControlStyles.StandardClick | ControlStyles.StandardDoubleClick, false);
			SetStyle (ControlStyles.FixedHeight, true);
		}
		#endregion

		#region Public Instance Properties
		public bool IsInEditOrNavigateMode {
			get { return isnavigating; }
			set { isnavigating = value; }
		}

		#endregion	// Public Instance Properties

		#region Public Instance Methods
		protected override void OnKeyPress (KeyPressEventArgs e)
		{
			if (!ReadOnly) {
				isnavigating = false;
				grid.ColumnStartedEditing (Bounds);
			}
			base.OnKeyPress (e);
		}

		protected override void OnMouseWheel (MouseEventArgs e)
		{
			// XXX we need to invoke a method on the datagrid to forward the mouse wheel - figure out what it is.
			//  base.OnMouseWheel is wrong, as the event is never raised by the textbox itself.
			// grid.OnMouseWheel (e);
		}

		protected internal override bool ProcessKeyMessage (ref Message m)
		{
			Keys key = (Keys) m.WParam.ToInt32 ();

			if (isnavigating && ProcessKeyPreview (ref m))
				return true;

			switch ((Msg)m.Msg) {
			case Msg.WM_CHAR:
				switch (key) {
				case Keys.Enter:
					isnavigating = true;
					return false;
				default:
					return ProcessKeyEventArgs (ref m);
				}

			case Msg.WM_KEYDOWN:
				switch (key) {
				case Keys.F2:
					SelectionStart = Text.Length;
					SelectionLength = 0;
					return false;

				case Keys.Left:
					if (SelectionStart == 0)
						return ProcessKeyPreview (ref m);
					return false;
				case Keys.Right:
					// Arrow keys go right until we hit the end of the text
					if (SelectionStart + SelectionLength >= Text.Length) {
						return ProcessKeyPreview (ref m);
					}
					return false;
				case Keys.Enter:
					return true;
				case Keys.Tab:
				case Keys.Up:
				case Keys.Down:
					return ProcessKeyPreview (ref m);
				}

				return ProcessKeyEventArgs(ref m);
			default:
				return false;
			}
		}

		public void SetDataGrid (DataGrid parentGrid)
		{
			grid = parentGrid;
		}

		protected override void WndProc (ref Message m)
		{
			switch ((Msg)m.Msg) {
			case Msg.WM_LBUTTONDOWN:
			case Msg.WM_LBUTTONDBLCLK:
				isnavigating = false;
				break;
			}

			base.WndProc (ref m);
		}

		#endregion	// Public Instance Methods

	}
}
