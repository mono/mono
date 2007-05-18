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

#if NET_2_0

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Security.Permissions;

namespace System.Windows.Forms
{
	public class DataGridViewLinkCell : DataGridViewCell
	{

		public DataGridViewLinkCell ()
		{
		}

		#region Public Methods

		[MonoTODO]
		public override object Clone ()
		{
			DataGridViewLinkCell clone = (DataGridViewLinkCell)base.Clone ();
			clone.activeLinkColor = this.activeLinkColor;
			return clone;
		}

		[MonoTODO]
		public override string ToString ()
		{
			return base.ToString ();
		}

		#endregion

		#region Protected Methods

		protected override AccessibleObject CreateAccessibilityInstance ()
		{
			return new DataGridViewLinkCellAccessibleObject (this);
		}
		[MonoTODO]
		protected override Rectangle GetContentBounds (Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override Rectangle GetErrorIconBounds (Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override Size GetPreferredSize (Graphics graphics, DataGridViewCellStyle cellStyle, int rowIndex, Size constraintSize)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override object GetValue (int rowIndex)
		{
			throw new NotImplementedException ();
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
		[MonoTODO]
		protected override bool MouseDownUnsharesRow (DataGridViewCellMouseEventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override bool MouseLeaveUnsharesRow (int rowIndex)
		{
			return (linkState != LinkState.Normal);
		}

		[MonoTODO]
		protected override bool MouseMoveUnsharesRow (DataGridViewCellMouseEventArgs e)
		{
			if (linkState == LinkState.Hover)
				return true;
			return false;
			//TODO check if color ever changed
			//true if the mouse pointer is over the link and the link is has not yet changed color to reflect the hover state; otherwise, false.
		}

		protected override bool MouseUpUnsharesRow (DataGridViewCellMouseEventArgs e)
		{
			return (linkState == LinkState.Hover);
		}
		[MonoTODO]
		protected override void OnKeyUp (KeyEventArgs e, int rowIndex)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void OnMouseDown (DataGridViewCellMouseEventArgs e)
		{
			base.OnMouseDown (e);
			linkState |= LinkState.Active;
			DataGridView.InvalidateCell (this);
		}
		[MonoTODO]
		protected override void OnMouseLeave (int rowIndex)
		{
			base.OnMouseLeave (rowIndex);
			linkState |= LinkState.Normal;
			DataGridView.InvalidateCell (this);
		}
		[MonoTODO]
		protected override void OnMouseMove (DataGridViewCellMouseEventArgs e)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		protected override void OnMouseUp (DataGridViewCellMouseEventArgs e)
		{
			base.OnMouseUp (e);
			linkState |= LinkState.Visited;
			LinkVisited = true;
			DataGridView.InvalidateCell (this);
			Action ();//load link but dot not know how
		}
		[MonoTODO]
		protected override void Paint (Graphics graphics, Rectangle clipBounds, Rectangle cellBounds, int rowIndex, DataGridViewElementStates cellState, object value, object formattedValue, string errorText, DataGridViewCellStyle cellStyle, DataGridViewAdvancedBorderStyle advancedBorderStyle, DataGridViewPaintParts paintParts)
		{

			this.PaintBorder (graphics, clipBounds, cellBounds, cellStyle, advancedBorderStyle);
			
			graphics.FillRectangle (new SolidBrush(cellStyle.BackColor), cellBounds);

			Font font = (Font)cellStyle.Font.Clone ();

			Color color;
			switch (LinkBehavior) {
				case LinkBehavior.AlwaysUnderline: {
					font = new Font (font.FontFamily, font.Size, font.Style | FontStyle.Underline, font.Unit);
					break;
				}
				case LinkBehavior.HoverUnderline: {
					if (linkState == LinkState.Hover)
						font = new Font (font.FontFamily, font.Size, font.Style | FontStyle.Underline, font.Unit);
					break;
				}
				case LinkBehavior.NeverUnderline: {
					break;
				}
				case LinkBehavior.SystemDefault: {
					//TODO get system behaviour
					// if system.underline it?
					//for moment underline all
					font = new Font (font.FontFamily, font.Size, font.Style | FontStyle.Underline, font.Unit);
					break;
				}
			}
			
			if (linkState == LinkState.Active)
				color = ActiveLinkColor;
			if (linkVisited)
				color = VisitedLinkColor;
			else
				color = LinkColor;
			
			graphics.DrawString (formattedValue.ToString(), font, new SolidBrush (color), clipBounds );

		}
		
		#endregion

		#region Private Methods

		private void Action ()
		{
			//TODO
		}

		#endregion

		#region Private fields

		private Color activeLinkColor;
		private LinkBehavior linkBehavior;
		private Color linkColor;
		private bool linkVisited;
		private bool trackVisitedState;
		private bool useColumnTextForLinkValue;
		private LinkState linkState;

		#endregion

		#region Public properties

		public Color ActiveLinkColor {
			get { return activeLinkColor; }
			set { activeLinkColor = value; }
		}

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
		public bool TrackVisitedState {
			get { return trackVisitedState; }
			set { trackVisitedState = value; }
		}
		public bool UseColumnTextForLinkValue {
			get { return useColumnTextForLinkValue; }
			set { useColumnTextForLinkValue = value; }
		}

		public Color VisitedLinkColor {
			get { return activeLinkColor; }
			set { activeLinkColor = value; }
		}

		public override Type ValueType {
			get { return base.ValueType; }
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

			[MonoTODO]
			[SecurityPermission (SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
			public override void DoDefaultAction ()
			{
				DataGridViewLinkCell cell = base.Owner as DataGridViewLinkCell;
				if (cell.DataGridView != null && cell.RowIndex == -1)
					throw new InvalidOperationException ();
				cell.Action ();
			}

			public override int GetChildCount ()
			{
				return -1;
			}

			public override string DefaultAction { get { return "Click"; } }
		}
		
	}
}

#endif