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

// NOT COMPLETE

using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace System.Windows.Forms
{
	[DefaultProperty("GridEditName")]
	[DesignTimeVisible(false)]
	[ToolboxItem(false)]
	public class DataGridTextBox : TextBox
	{

		#region	Local Variables
		private bool isedit;
		private DataGrid grid;
		#endregion	// Local Variables

		#region Constructors
		public DataGridTextBox ()
		{
			isedit = true;
			grid = null;
			accepts_tab = true;

			SetStyle (ControlStyles.UserPaint | ControlStyles.StandardClick | ControlStyles.StandardDoubleClick, false);
			SetStyle (ControlStyles.FixedHeight, true);
		}
		#endregion

		#region Public Instance Properties
		public bool IsInEditOrNavigateMode {
			get {
				return isedit;
			}
			set {
				if (value != isedit) {
					isedit = value;
				}
			}
		}

		#endregion	// Public Instance Properties

		#region Public Instance Methods
		protected override void OnKeyPress (KeyPressEventArgs e)
		{			
			grid.is_changing = true;
			grid.InvalidateCurrentRowHeader ();
			base.OnKeyPress (e);
		}

		protected override void OnMouseWheel (MouseEventArgs e)
		{
			base.OnMouseWheel (e);
		}

		protected internal override bool ProcessKeyMessage (ref Message m)
		{
			Keys key = (Keys) m.WParam.ToInt32 ();

			// If we decide DataGrid needs to process we call grid.ProcessKeyPreviewInternal and return true
			// If we want TextBox to handle the key , we return false;

			// We only care about KEYDOWN messages
			if (m.Msg != (int)Msg.WM_KEYDOWN) {
				return false;
			}

			// Anything with control key pressed is for DataGrid
			if ((Control.ModifierKeys & Keys.Control) != 0) {
				return grid.ProcessKeyPreviewInternal(ref m);
			}

			if (isedit) {
				switch (key) {
					case Keys.F2: {
						this.SelectionStart = this.Text.Length;
						this.SelectionLength = 0;
						return true;
					}

					case Keys.Up:
					case Keys.Down:
					case Keys.PageDown:
					case Keys.PageUp: {
						isedit = false;
						grid.EndEdit(false);
						grid.ProcessKeyPreviewInternal(ref m);
						return true;
					}

					case Keys.Escape: {
						isedit = false;
						grid.EndEdit (true);
						return true;
					}

					case Keys.Enter: {
						isedit = false;
						grid.EndEdit (false);
						grid.ProcessKeyPreviewInternal(ref m);
						return true;
					}

					case Keys.Left: {
						if (base.SelectionStart == 0) {
							isedit = false;
							grid.EndEdit (false);
							grid.ProcessKeyPreviewInternal(ref m);
							return true;
						}
						return false;
					}

					case Keys.Right: {
						// Arrow keys go right until we hit the end of the text
						if ((base.SelectionStart + base.SelectionLength) == base.Text.Length) {
							isedit = false;
							grid.EndEdit (false);
							grid.ProcessKeyPreviewInternal(ref m);
							return true;
						}
						return false;
					}

					case Keys.Tab: {
						isedit = false;
						grid.EndEdit(false);
						grid.ProcessKeyPreviewInternal(ref m);
						return true;
					}

					default: {
						return base.ProcessKeyMessage(ref m);
					}
				}
			}

			return base.ProcessKeyMessage(ref m);
		}

		public void SetDataGrid (DataGrid parentGrid)
		{
			grid = parentGrid;
		}

		protected override void WndProc (ref Message m)
		{
			base.WndProc (ref m);
		}

		#endregion	// Public Instance Methods

	}
}
