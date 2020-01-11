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
//	olivier Dufour	olivier.duff@free.fr
//
//

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Security.Permissions;
using System.ComponentModel;

namespace System.Windows.Forms
{
	public class DataGridViewLinkCell : DataGridViewCell
	{

		public DataGridViewLinkCell ()
		{
			activeLinkColor = Color.Red;
			linkColor = Color.FromArgb (0, 0, 255);
			trackVisitedState = true;
			visited_link_color = Color.FromArgb (128, 0, 128);
		}

		#region Public Methods

		public override object Clone ()
		{
			DataGridViewLinkCell clone = (DataGridViewLinkCell)base.Clone ();
			
			clone.activeLinkColor = this.activeLinkColor;
			clone.linkColor = this.linkColor;
			clone.linkVisited = this.linkVisited;
			clone.linkBehavior = this.linkBehavior;
			clone.visited_link_color = this.visited_link_color;
			clone.trackVisitedState = this.trackVisitedState;
			clone.useColumnTextForLinkValue = this.useColumnTextForLinkValue;
			
			return clone;
		}

		public override string ToString ()
		{
			return string.Format ("DataGridViewLinkCell {{ ColumnIndex={0}, RowIndex={1} }}", ColumnIndex, RowIndex);
		}

		#endregion

		#region Protected Methods

		protected override AccessibleObject CreateAccessibilityInstance ()
		{
			return new DataGridViewLinkCellAccessibleObject (this);
		}
		
		protected override Rectangle GetContentBounds (Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex)
		{
			if (DataGridView == null)
				return Rectangle.Empty;

			object o = FormattedValue;
			Size s = Size.Empty;

			if (o != null) {
				s = DataGridViewCell.MeasureTextSize (graphics, o.ToString (), cellStyle.Font, TextFormatFlags.Default);
				s.Height += 3;
			} else {
				return new Rectangle (1, 10, 0, 0);
			}

			return new Rectangle (1, (OwningRow.Height - s.Height) / 2 - 1, s.Width, s.Height);
		}
		
		protected override Rectangle GetErrorIconBounds (Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex)
		{
			if (DataGridView == null || string.IsNullOrEmpty (ErrorText))
				return Rectangle.Empty;

			Size error_icon = new Size (12, 11);
			return new Rectangle (new Point (Size.Width - error_icon.Width - 5, (Size.Height - error_icon.Height) / 2), error_icon);
		}
		
		protected override Size GetPreferredSize (Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex, Size constraintSize)
		{
			object o = FormattedValue;

			if (o != null) {
				Size s = DataGridViewCell.MeasureTextSize (graphics, o.ToString (), cellStyle.Font, TextFormatFlags.Default);
				s.Height = Math.Max (s.Height, 20);
				s.Width += 4;
				return s;
			} else
				return new Size (21, 20);
		}
		
		protected override object GetValue (int rowIndex)
		{
			if (useColumnTextForLinkValue)
				return (OwningColumn as DataGridViewLinkColumn).Text;
				
			return base.GetValue (rowIndex);
		}

		protected override bool KeyUpUnsharesRow (KeyEventArgs e, int rowIndex)
		{
			if (e.KeyCode != Keys.Space
				&& trackVisitedState == true
				&& linkVisited == false
				&& !e.Shift 
				&& !e.Control
				&& !e.Alt)
				return true;

			return false;
		}
		
		protected override bool MouseDownUnsharesRow (DataGridViewCellMouseEventArgs e)
		{
			return true;
		}
		
		protected override bool MouseLeaveUnsharesRow (int rowIndex)
		{
			return (linkState != LinkState.Normal);
		}

		protected override bool MouseMoveUnsharesRow (DataGridViewCellMouseEventArgs e)
		{
			if (linkState == LinkState.Hover)
				return true;

			return false;
		}

		protected override bool MouseUpUnsharesRow (DataGridViewCellMouseEventArgs e)
		{
			return (linkState == LinkState.Hover);
		}
		
		protected override void OnKeyUp (KeyEventArgs e, int rowIndex)
		{
			if ((e.KeyData & Keys.Space) == Keys.Space) {
				linkState = LinkState.Normal;
				DataGridView.InvalidateCell (this);
			}
		}

		protected override void OnMouseDown (DataGridViewCellMouseEventArgs e)
		{
			base.OnMouseDown (e);
			
			linkState = LinkState.Active;
			DataGridView.InvalidateCell (this);
		}
		
		protected override void OnMouseLeave (int rowIndex)
		{
			base.OnMouseLeave (rowIndex);
			
			linkState = LinkState.Normal;
			DataGridView.InvalidateCell (this);
			DataGridView.Cursor = parent_cursor;
		}
		
		protected override void OnMouseMove (DataGridViewCellMouseEventArgs e)
		{
			base.OnMouseMove (e);
			
			if (linkState != LinkState.Hover) {
				linkState = LinkState.Hover;
				DataGridView.InvalidateCell (this);
				parent_cursor = DataGridView.Cursor;
				DataGridView.Cursor = Cursors.Hand;
			}
		}
		
		protected override void OnMouseUp (DataGridViewCellMouseEventArgs e)
		{
			base.OnMouseUp (e);
			
			linkState = LinkState.Hover;
			LinkVisited = true;
			DataGridView.InvalidateCell (this);
		}

		protected override void Paint (Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates cellState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
		{
			base.Paint (graphics, clipBounds, cellBounds, rowIndex, cellState, value, formattedValue, errorText, cellStyle, advancedBorderStyle, paintParts);
		}
		
		internal override void PaintPartContent (Graphics graphics, Rectangle cellBounds, int rowIndex, DataGridViewElementStates cellState, DataGridViewCellStyle cellStyle, object formattedValue)
		{
			Font font = cellStyle.Font;

			switch (LinkBehavior) {
				case LinkBehavior.AlwaysUnderline:
				case LinkBehavior.SystemDefault:
					font = new Font (font, FontStyle.Underline);
					break;
				case LinkBehavior.HoverUnderline:
					if (linkState == LinkState.Hover)
						font = new Font (font, FontStyle.Underline);
					break;
			}

			Color color;
			
			if (linkState == LinkState.Active)
				color = ActiveLinkColor;
			else if (linkVisited)
				color = VisitedLinkColor;
			else
				color = LinkColor;

			TextFormatFlags flags = TextFormatFlags.EndEllipsis | TextFormatFlags.VerticalCenter | TextFormatFlags.TextBoxControl;

			cellBounds.Height -= 2;
			cellBounds.Width -= 2;

			if (formattedValue != null)
				TextRenderer.DrawText (graphics, formattedValue.ToString (), font, cellBounds, color, flags);
		}
		#endregion

		#region Private fields

		private Color activeLinkColor;
		private LinkBehavior linkBehavior;
		private Color linkColor;
		private bool linkVisited;
		private Cursor parent_cursor;
		private bool trackVisitedState;
		private bool useColumnTextForLinkValue;
		private Color visited_link_color;
		private LinkState linkState;

		#endregion

		#region Public properties

		public Color ActiveLinkColor {
			get { return activeLinkColor; }
			set { activeLinkColor = value; }
		}

		[DefaultValue (LinkBehavior.SystemDefault)]
		public LinkBehavior LinkBehavior {
			get { return linkBehavior; }
			set { linkBehavior = value; }
		}
		public Color LinkColor {
			get { return linkColor; }
			set { linkColor = value; }
		}
		public bool LinkVisited {
			get { return linkVisited; }
			set { linkVisited = value; }
		}
		[DefaultValue (true)]
		public bool TrackVisitedState {
			get { return trackVisitedState; }
			set { trackVisitedState = value; }
		}
		[DefaultValue (false)]
		public bool UseColumnTextForLinkValue {
			get { return useColumnTextForLinkValue; }
			set { useColumnTextForLinkValue = value; }
		}

		public Color VisitedLinkColor {
			get { return visited_link_color; }
			set { visited_link_color = value; }
		}

		public override Type ValueType {
			get { return base.ValueType == null ? typeof (object) : base.ValueType; }
		}

		public override Type EditType {
			get { return null; }
		}
		public override Type FormattedValueType {
			get { return typeof(string); }
		}

		#endregion

		protected class DataGridViewLinkCellAccessibleObject : DataGridViewCell.DataGridViewCellAccessibleObject
		{
			public DataGridViewLinkCellAccessibleObject (DataGridViewCell owner) : base(owner) 
			{
				//DO NOTHING
			}

			[MonoTODO ("Stub, does nothing")]
			[SecurityPermission (SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
			public override void DoDefaultAction ()
			{
				//DataGridViewLinkCell cell = base.Owner as DataGridViewLinkCell;
				//if (cell.DataGridView != null && cell.RowIndex == -1)
				//        throw new InvalidOperationException ();
			}

			public override int GetChildCount ()
			{
				return -1;
			}

			public override string DefaultAction { get { return "Click"; } }
		}
		
	}
}
