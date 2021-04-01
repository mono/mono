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
using System.ComponentModel;
using System.Text;
using System.Drawing;

namespace System.Windows.Forms
{
	[ToolboxBitmap ("")]
	public class DataGridViewLinkColumn : DataGridViewColumn
	{
		
		public DataGridViewLinkColumn ()
		{
			base.CellTemplate = new DataGridViewLinkCell ();
		}

		public override object Clone ()
		{
			DataGridViewLinkColumn clone = (DataGridViewLinkColumn)base.Clone ();
			clone.CellTemplate = (DataGridViewCell) this.CellTemplate.Clone ();
			return clone;
		}

		public override string ToString ()
		{
			return base.ToString ();
		}

		#region private fields

		private string text = string.Empty;

		#endregion

		#region Public Properties

		public Color ActiveLinkColor {
			get {
				DataGridViewLinkCell template = CellTemplate as DataGridViewLinkCell;
				if (template == null)
					throw new InvalidOperationException ("CellTemplate is null when getting this property.");
				return template.ActiveLinkColor; 
			}
			set {
				if (this.ActiveLinkColor == value) 
					return;

				DataGridViewLinkCell template = CellTemplate as DataGridViewLinkCell;
				if (template == null)
					throw new InvalidOperationException ("CellTemplate is null when getting this property.");

				template.ActiveLinkColor = value;
				if (DataGridView == null)
					return;
				foreach (DataGridViewRow row in DataGridView.Rows) {
					DataGridViewLinkCell cell = row.Cells[Index] as DataGridViewLinkCell;
					if (cell != null)
						cell.ActiveLinkColor = value;
				}
				DataGridView.InvalidateColumn (Index);

			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public override DataGridViewCell CellTemplate {
			get { return base.CellTemplate; }
			set { base.CellTemplate = value as DataGridViewLinkCell; }
		}


		[DefaultValue (LinkBehavior.SystemDefault)]
		public LinkBehavior LinkBehavior {
			get	{
				DataGridViewLinkCell template = CellTemplate as DataGridViewLinkCell;
				if (template == null)
					throw new InvalidOperationException ("CellTemplate is null when getting this property.");
				return template.LinkBehavior;
			}
			set	{
				if (this.LinkBehavior == value) 
					return;

				DataGridViewLinkCell template = CellTemplate as DataGridViewLinkCell;
				if (template == null)
					throw new InvalidOperationException ("CellTemplate is null when getting this property.");

				template.LinkBehavior = value;
				if (DataGridView == null)
					return;
				foreach (DataGridViewRow row in DataGridView.Rows)
				{
					DataGridViewLinkCell cell = row.Cells[Index] as DataGridViewLinkCell;
					if (cell != null)
						cell.LinkBehavior = value;
				}
				DataGridView.InvalidateColumn (Index);
			}
		}

		public Color LinkColor {
			get	{
				DataGridViewLinkCell template = CellTemplate as DataGridViewLinkCell;
				if (template == null)
					throw new InvalidOperationException ("CellTemplate is null when getting this property.");
				return template.LinkColor;
			}
			set	{
				if (this.LinkColor == value)
					return;
				DataGridViewLinkCell template = CellTemplate as DataGridViewLinkCell;
				if (template == null)
					throw new InvalidOperationException ("CellTemplate is null when getting this property.");
				template.LinkColor = value;
				if (DataGridView == null)
					return;
				foreach (DataGridViewRow row in DataGridView.Rows)
				{
					DataGridViewLinkCell cell = row.Cells[Index] as DataGridViewLinkCell;
					if (cell != null)
						cell.LinkColor = value;
				}
				DataGridView.InvalidateColumn (Index);
			}
		}
		[MonoInternalNote ("")]
		[DefaultValue ((string) null)]
		public string Text {
			get {
				return text;
			}
			set {
				if (this.Text == value)
					return;
				text = value;
				if (DataGridView == null)
					return;
				foreach (DataGridViewRow row in DataGridView.Rows)
				{
					DataGridViewLinkCell cell = row.Cells[Index] as DataGridViewLinkCell;
					if (cell != null && cell.UseColumnTextForLinkValue)
						cell.Value = value;
				}
				DataGridView.InvalidateColumn (Index);
			}
		}

		//When TrackVisitedState is true, the VisitedLinkColor property value is used to display links that have already been visited.
		[DefaultValue (true)]
		public bool TrackVisitedState {
			get {
				DataGridViewLinkCell template = CellTemplate as DataGridViewLinkCell;
				if (template == null)
					throw new InvalidOperationException ("CellTemplate is null when getting this property.");
				return template.TrackVisitedState;
			}
			set {
				if (this.TrackVisitedState == value)
					return;
				DataGridViewLinkCell template = CellTemplate as DataGridViewLinkCell;
				if (template == null)
					throw new InvalidOperationException ("CellTemplate is null when getting this property.");
				template.TrackVisitedState = value;
				if (DataGridView == null)
					return;
				foreach (DataGridViewRow row in DataGridView.Rows)
				{
					DataGridViewLinkCell cell = row.Cells[Index] as DataGridViewLinkCell;
					if (cell != null)
						cell.TrackVisitedState = value;
				}
				DataGridView.InvalidateColumn (Index);
			}
		}

		// true if the Text property value is displayed as the link text; false if the cell FormattedValue property value is displayed as the link text. The default is false.
		[DefaultValue (false)]
		public bool UseColumnTextForLinkValue {
			get {
				DataGridViewLinkCell template = CellTemplate as DataGridViewLinkCell;
				if (template == null)
					throw new InvalidOperationException ("CellTemplate is null when getting this property.");
				return template.UseColumnTextForLinkValue;
			}
			set {
				if (this.UseColumnTextForLinkValue == value)
					return;
				DataGridViewLinkCell template = CellTemplate as DataGridViewLinkCell;
				if (template == null)
					throw new InvalidOperationException ("CellTemplate is null when getting this property.");
				template.UseColumnTextForLinkValue = value;
				if (DataGridView == null)
					return;
				foreach (DataGridViewRow row in DataGridView.Rows)
				{
					DataGridViewLinkCell cell = row.Cells[Index] as DataGridViewLinkCell;
					if (cell != null)
						cell.UseColumnTextForLinkValue = value;
				}
				DataGridView.InvalidateColumn (Index);
			}
		}

		//If the TrackVisitedState property is set to false, the VisitedLinkColor property is ignored.
		public Color VisitedLinkColor {
			get {
				DataGridViewLinkCell template = CellTemplate as DataGridViewLinkCell;
				if (template == null)
					throw new InvalidOperationException ("CellTemplate is null when getting this property.");
				return template.VisitedLinkColor;
			}
			set {
				if (this.VisitedLinkColor == value)
					return;
				DataGridViewLinkCell template = CellTemplate as DataGridViewLinkCell;
				if (template == null)
					throw new InvalidOperationException ("CellTemplate is null when getting this property.");
				template.VisitedLinkColor = value;
				if (DataGridView == null)
					return;
				foreach (DataGridViewRow row in DataGridView.Rows)
				{
					DataGridViewLinkCell cell = row.Cells[Index] as DataGridViewLinkCell;
					if (cell != null)
						cell.VisitedLinkColor = value;
				}
				DataGridView.InvalidateColumn (Index);
			}
		}
		#endregion
	}
}
