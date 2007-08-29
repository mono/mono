//
// System.Windows.Forms.Design.SelectionFrame
//
// Authors:
//	  Ivan N. Zlatev (contact i-nZ.net)
//
// (C) 2006-2007 Ivan N. Zlatev

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
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;


namespace System.Windows.Forms.Design
{   
	// This is not a control!
	//  
	internal class SelectionFrame
	{

		public SelectionFrame (Control control)
		{
			if (control == null)
				throw new ArgumentNullException ("control");
				
			_control = control;
		}


		private Rectangle _bounds;
		private Control _control;
		private Rectangle[] _handles = new Rectangle[8];
		private GrabHandle _handle = GrabHandle.None;
		private const int BORDER_SIZE = 7;

		
#region Properties
		private enum GrabHandle {
			None = -1,
			TopLeft = 0,
			TopMiddle,
			TopRight,
			Right,
			BottomRight,
			BottomMiddle,
			BottomLeft,
			Left,
			Border // the border surrounding the control.
		}		   

		public Rectangle Bounds {
			get {
				_bounds.X = _control.Location.X - BORDER_SIZE;
				_bounds.Y = _control.Location.Y - BORDER_SIZE;
				_bounds.Width = _control.Width + BORDER_SIZE *2;
				_bounds.Height = _control.Height + BORDER_SIZE *2;

				return _bounds; 
			}
			set { 
				_bounds = value;
				_control.Bounds = _bounds;
			}
		}

		private SelectionRules SelectionRules {
			get {
				SelectionRules result = SelectionRules.AllSizeable;

				if (_control.Site != null) {
					IDesignerHost host = _control.Site.GetService (typeof (IDesignerHost)) as IDesignerHost;
					if (host != null) {
						ControlDesigner designer = host.GetDesigner (_control) as ControlDesigner;
						if (designer != null)
							result = designer.SelectionRules;
					}
				}

				return result;
			}
		}
		
		public Control Control {
			get { return _control; }
			set { 
				if (value != null)
					_control = value;
			}
		}

		public Control Parent {
			get {
				if (_control.Parent == null)
					return _control;
				else
					return _control.Parent;
			}
		}

		private GrabHandle GrabHandleSelected {
			get { return _handle; }
			set { _handle = value; }
		}

		private bool PrimarySelection{
			get {
				bool result = false;
				if (this.Control != null && this.Control.Site != null) {
					ISelectionService selection = this.Control.Site.GetService (typeof (ISelectionService)) as ISelectionService;
					if (selection != null && selection.PrimarySelection == this.Control)
						result = true;
				}
				return result;
			}
		}
#endregion


#region Drawing
		public void OnPaint (Graphics gfx)
		{
			DrawFrame (gfx);
			DrawGrabHandles (gfx);		  
		}
	
		private void DrawGrabHandles (Graphics gfx)
		{
			GraphicsState state = gfx.Save();
			gfx.TranslateTransform (this.Bounds.X, this.Bounds.Y);

			for (int i = 0; i < _handles.Length; i++) {
				_handles[i].Width = BORDER_SIZE;
				_handles[i].Height = BORDER_SIZE;
			}

			SelectionRules rules = this.SelectionRules;
			bool primarySelection = this.PrimarySelection;
			bool enabled = false;

			_handles[(int) GrabHandle.TopLeft].Location = new Point (0,0);
			if (this.CheckSelectionRules (rules, SelectionRules.TopSizeable | SelectionRules.LeftSizeable))
				enabled = true;

			ControlPaint.DrawGrabHandle (gfx, _handles[(int)GrabHandle.TopLeft], primarySelection, enabled);
			enabled = false;

			_handles[(int) GrabHandle.TopMiddle].Location = new Point ((this.Bounds.Width - BORDER_SIZE) / 2, 0);
			if (this.CheckSelectionRules (rules, SelectionRules.TopSizeable))
				enabled = true;

			ControlPaint.DrawGrabHandle (gfx, _handles[(int)GrabHandle.TopMiddle], primarySelection, enabled);
			enabled = false;

			_handles[(int) GrabHandle.TopRight].Location = new Point (this.Bounds.Width - BORDER_SIZE, 0);
			if (this.CheckSelectionRules (rules, SelectionRules.TopSizeable | SelectionRules.RightSizeable))
				enabled = true;

			ControlPaint.DrawGrabHandle (gfx, _handles[(int)GrabHandle.TopRight], primarySelection, enabled);
			enabled = false;

			_handles[(int) GrabHandle.Right].Location = new Point (this.Bounds.Width - BORDER_SIZE,
							 (this.Bounds.Height - BORDER_SIZE) / 2);
			if (this.CheckSelectionRules (rules, SelectionRules.RightSizeable))
				enabled = true;

			ControlPaint.DrawGrabHandle (gfx, _handles[(int)GrabHandle.Right], primarySelection, enabled);
			enabled = false;

			_handles[(int) GrabHandle.BottomRight].Location = new Point (this.Bounds.Width - BORDER_SIZE,
							this.Bounds.Height - BORDER_SIZE);
			if (this.CheckSelectionRules (rules, SelectionRules.BottomSizeable | SelectionRules.RightSizeable))
				enabled = true;

			ControlPaint.DrawGrabHandle (gfx, _handles[(int)GrabHandle.BottomRight], primarySelection, enabled);
			enabled = false;

			_handles[(int) GrabHandle.BottomMiddle].Location = new Point ((this.Bounds.Width - BORDER_SIZE) / 2,
							 this.Bounds.Height - BORDER_SIZE);
			if (this.CheckSelectionRules (rules, SelectionRules.BottomSizeable))
				enabled = true;

			ControlPaint.DrawGrabHandle (gfx, _handles[(int)GrabHandle.BottomMiddle], primarySelection, enabled);
			enabled = false;

			_handles[(int) GrabHandle.BottomLeft].Location = new Point (0, this.Bounds.Height - BORDER_SIZE);
			if (this.CheckSelectionRules (rules, SelectionRules.BottomSizeable | SelectionRules.LeftSizeable))
				enabled = true;

			ControlPaint.DrawGrabHandle (gfx, _handles[(int)GrabHandle.BottomLeft], primarySelection, enabled);
			enabled = false;

			_handles[(int) GrabHandle.Left].Location = new Point (0, (this.Bounds.Height - BORDER_SIZE) / 2);
			if (this.CheckSelectionRules (rules, SelectionRules.LeftSizeable))
				enabled = true;

			ControlPaint.DrawGrabHandle (gfx, _handles[(int)GrabHandle.Left], primarySelection, enabled);
			gfx.Restore (state);
		}
		
		protected void DrawFrame (Graphics gfx)
		{
			Color negativeColor =  Color.FromArgb ((byte)~(_control.Parent.BackColor.R), 
								(byte)~(_control.Parent.BackColor.G), 
								(byte)~(_control.Parent.BackColor.B));
			Pen pen = new Pen (new HatchBrush (HatchStyle.Percent30, negativeColor, Color.FromArgb (0)), BORDER_SIZE);
			gfx.DrawRectangle (pen, this.Control.Bounds);
		}
#endregion


#region Dragging
		private bool _resizing = false;


		public bool SetCursor (int x, int y)
		{
			bool modified = false;

			if (!_resizing) {
				GrabHandle handle = PointToGrabHandle (this.PointToClient (Control.MousePosition));
				if (handle != GrabHandle.None)
					modified = true;

				if (handle == GrabHandle.TopLeft)
					Cursor.Current = Cursors.SizeNWSE;
				else if (handle == GrabHandle.TopMiddle)
					Cursor.Current = Cursors.SizeNS;
				else if (handle == GrabHandle.TopRight)
					Cursor.Current = Cursors.SizeNESW;
				else if (handle == GrabHandle.Right)
					Cursor.Current = Cursors.SizeWE;
				else if (handle == GrabHandle.BottomRight)
					Cursor.Current = Cursors.SizeNWSE;
				else if (handle == GrabHandle.BottomMiddle)
					Cursor.Current = Cursors.SizeNS;
				else if (handle == GrabHandle.BottomLeft)
					Cursor.Current = Cursors.SizeNESW;
				else if (handle == GrabHandle.Left)
					Cursor.Current = Cursors.SizeWE;
				else
					Cursor.Current = Cursors.Default;
			}
			return modified;
		}
		
		// container coordinates
		public void ResizeBegin (int x, int y)
		{
			this.GrabHandleSelected = PointToGrabHandle (this.PointToClient (this.Parent.PointToScreen (new Point (x, y))));

			if (this.GrabHandleSelected != GrabHandle.None)
				_resizing = true;
		}

		private bool CheckSelectionRules (SelectionRules rules, SelectionRules toCheck)
		{
			return ((rules & toCheck) == toCheck);
		}

		// container coordinates returns deltaBounds
		public Rectangle ResizeContinue (int x, int y)
		{
			//Console.WriteLine ("ResizeContinue: " + x + " : " + y);
			//Console.WriteLine ("GrabHandleSelected: " + GrabHandleSelected);

			Rectangle bounds = (Rectangle)TypeDescriptor.GetProperties (_control)["Bounds"].GetValue (_control);
			Rectangle deltaBounds = bounds;
			Point pointerLocation = new Point (x, y);
			SelectionRules rules = this.SelectionRules;
			int top, height, left, width = 0;

			if (_resizing && this.GrabHandleSelected != GrabHandle.None && rules != SelectionRules.Locked) {
				if (this.GrabHandleSelected == GrabHandle.TopLeft &&
					CheckSelectionRules (rules, SelectionRules.LeftSizeable | SelectionRules.TopSizeable)) {

					top = _control.Top;
					height = _control.Height;
					left = _control.Left;
					width = _control.Width;

					if (pointerLocation.Y < _control.Bottom) {
						top = pointerLocation.Y;
						height = _control.Bottom - pointerLocation.Y;
					}
					if (pointerLocation.X < _control.Right) {
						left = pointerLocation.X;
						width = _control.Right - pointerLocation.X;
						bounds = new Rectangle (left, top, width, height);
					}
				}
				else if (this.GrabHandleSelected == GrabHandle.TopRight &&
					CheckSelectionRules (rules, SelectionRules.TopSizeable | SelectionRules.RightSizeable)) {

					top = _control.Top;
					height = _control.Height;
					width = _control.Width;

					if (pointerLocation.Y < _control.Bottom) {
						top = pointerLocation.Y;
						height = _control.Bottom - pointerLocation.Y;
					}
					width = pointerLocation.X - _control.Left;
					bounds = new Rectangle (_control.Left, top, width, height);
				}
				else if (GrabHandleSelected == GrabHandle.TopMiddle && CheckSelectionRules (rules, SelectionRules.TopSizeable)) {
					if (pointerLocation.Y < _control.Bottom) {
						top = pointerLocation.Y;
						height = _control.Bottom - pointerLocation.Y;
						bounds = new Rectangle (_control.Left, top, _control.Width, height);
					}
				}
				else if (this.GrabHandleSelected == GrabHandle.Right && CheckSelectionRules (rules, SelectionRules.RightSizeable)) {
					width = pointerLocation.X - _control.Left;
					bounds = new Rectangle (_control.Left, _control.Top, width, _control.Height);
				}
				else if (this.GrabHandleSelected == GrabHandle.BottomRight && 
					CheckSelectionRules (rules, SelectionRules.BottomSizeable | SelectionRules.RightSizeable)) {

					width = pointerLocation.X - _control.Left;
					height = pointerLocation.Y - _control.Top;
					bounds = new Rectangle (_control.Left, _control.Top, width, height);
				}
				else if (GrabHandleSelected == GrabHandle.BottomMiddle && CheckSelectionRules (rules, SelectionRules.BottomSizeable)) {
					height = pointerLocation.Y - _control.Top;
					bounds = new Rectangle (_control.Left, _control.Top, _control.Width, height);
				}
				else if (GrabHandleSelected == GrabHandle.BottomLeft &&
					CheckSelectionRules (rules, SelectionRules.BottomSizeable | SelectionRules.LeftSizeable)) {

					height = _control.Height;
					left = _control.Left;
					width = _control.Width;

					if (pointerLocation.X < _control.Right) {
						left = pointerLocation.X;
						width = _control.Right - pointerLocation.X;
					}
					height = pointerLocation.Y - _control.Top;
					bounds = new Rectangle (left, _control.Top, width, height);
				}
				else if (GrabHandleSelected == GrabHandle.Left && CheckSelectionRules (rules, SelectionRules.LeftSizeable)) {
					if (pointerLocation.X < _control.Right) {
						left = pointerLocation.X;
						width = _control.Right - pointerLocation.X;
						bounds = new Rectangle (left, _control.Top, width, _control.Height);
					}
				}

				//Console.WriteLine ("bounds: " + bounds.ToString ());
				TypeDescriptor.GetProperties (_control)["Bounds"].SetValue (_control, bounds);
				
			}
			
			this.Parent.Refresh ();
			deltaBounds.X = bounds.X - deltaBounds.X;
			deltaBounds.Y = bounds.Y - deltaBounds.Y;
			deltaBounds.Height = bounds.Height - deltaBounds.Height;
			deltaBounds.Width = bounds.Width - deltaBounds.Width;
			return deltaBounds;
		}
		
		
		public void ResizeEnd (bool cancel)
		{
			this.GrabHandleSelected = GrabHandle.None;
			_resizing = false;
		}

		public void Resize (Rectangle deltaBounds)
		{
			SelectionRules rules = this.SelectionRules;

			if (this.CheckSelectionRules (rules, SelectionRules.Locked) || !this.CheckSelectionRules (rules, SelectionRules.Moveable))
				return;
			
			Rectangle bounds = (Rectangle)TypeDescriptor.GetProperties (_control)["Bounds"].GetValue (_control);

			if (CheckSelectionRules (rules, SelectionRules.LeftSizeable)) {
				bounds.X += deltaBounds.X;
				bounds.Width += deltaBounds.Width;
			}
			if (CheckSelectionRules (rules, SelectionRules.RightSizeable) && !CheckSelectionRules (rules, SelectionRules.LeftSizeable)) {
				bounds.Y += deltaBounds.Y;
				bounds.Width += deltaBounds.Width;
			}
			if (CheckSelectionRules (rules, SelectionRules.TopSizeable)) {
				bounds.Y += deltaBounds.Y;
				bounds.Height += deltaBounds.Height;
			}
			if (CheckSelectionRules (rules, SelectionRules.BottomSizeable) && !CheckSelectionRules (rules, SelectionRules.TopSizeable)) {
				bounds.Height += deltaBounds.Height;
			}

			TypeDescriptor.GetProperties (_control)["Bounds"].SetValue (_control, bounds);
		}
#endregion


#region Utility methods

		public bool HitTest (int x, int y)
		{
			if (PointToGrabHandle (this.PointToClient (this.Parent.PointToScreen (new Point (x, y)))) != GrabHandle.None)
				return true;
			else
				return false;
		}
		
		private GrabHandle PointToGrabHandle (Point pointerLocation)
		{
			GrabHandle result = GrabHandle.None;
			
			if (IsCursorOnGrabHandle (pointerLocation, _handles[0]))
				result = GrabHandle.TopLeft;
			else if (IsCursorOnGrabHandle (pointerLocation, _handles[1]))
				result = GrabHandle.TopMiddle;
			else if (IsCursorOnGrabHandle (pointerLocation, _handles[2]))
				result = GrabHandle.TopRight;
			else if (IsCursorOnGrabHandle (pointerLocation, _handles[3]))
				result = GrabHandle.Right;
			else if (IsCursorOnGrabHandle (pointerLocation, _handles[4]))
				result = GrabHandle.BottomRight;
			else if (IsCursorOnGrabHandle (pointerLocation, _handles[5]))
				result = GrabHandle.BottomMiddle;
			else if (IsCursorOnGrabHandle (pointerLocation, _handles[6]))
				result = GrabHandle.BottomLeft;
			else if (IsCursorOnGrabHandle (pointerLocation, _handles[7]))
				result = GrabHandle.Left;
			else
				result = GrabHandle.None;
			
			return result;
		}
		
		private bool IsCursorOnGrabHandle (Point pointerLocation, Rectangle handleRectangle)
		{   
			if (pointerLocation.X >= handleRectangle.X &&
				pointerLocation.X <= handleRectangle.X + handleRectangle.Width &&
				pointerLocation.Y >= handleRectangle.Y &&
				pointerLocation.Y <= handleRectangle.Y + handleRectangle.Height) {
				return true;
			}
			return false;				   
		}

		private Point PointToClient (Point screenPoint)
		{
			Point pointerLocation = this.Parent.PointToClient (screenPoint);
			pointerLocation.X = pointerLocation.X - this.Bounds.X;
			pointerLocation.Y = pointerLocation.Y - this.Bounds.Y;
			return pointerLocation;			 
		}
#endregion
		
	}
}
