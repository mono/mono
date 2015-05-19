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
//	Pedro Martínez Juliá <pedromj@gmail.com>
//

using System.Collections;
using System.ComponentModel;
using System.Drawing.Design;
using System.Drawing;

namespace System.Windows.Forms {

	[ToolboxBitmap ("")]
	[Designer ("System.Windows.Forms.Design.DataGridViewComboBoxColumnDesigner, " + Consts.AssemblySystem_Design,
		   "System.ComponentModel.Design.IDesigner")]
	public class DataGridViewComboBoxColumn : DataGridViewColumn
	{
		private bool autoComplete;
		private DataGridViewComboBoxDisplayStyle displayStyle;
		private bool displayStyleForCurrentCellOnly;
		private FlatStyle flatStyle;

		public DataGridViewComboBoxColumn ()
		{
			CellTemplate = new DataGridViewComboBoxCell();
			((DataGridViewComboBoxCell) CellTemplate).OwningColumnTemplate = this;
			SortMode = DataGridViewColumnSortMode.NotSortable;
			autoComplete = true;
			displayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton;
			displayStyleForCurrentCellOnly = false;
		}

		[Browsable (true)]
		[DefaultValue (true)]
		public bool AutoComplete {
			get { return autoComplete; }
			set { autoComplete = value; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override DataGridViewCell CellTemplate {
			get { return base.CellTemplate; }
			set {

				DataGridViewComboBoxCell cellTemplate = value as DataGridViewComboBoxCell;
				if (cellTemplate == null)
					throw new InvalidCastException ("Invalid cell tempalte type.");

				cellTemplate.OwningColumnTemplate = this;
				base.CellTemplate = cellTemplate;
			}
		}

		[AttributeProvider (typeof (IListSource))]
		[DefaultValue (null)]
		[RefreshProperties (RefreshProperties.Repaint)]
		public object DataSource {
			get {
				if (base.CellTemplate == null) {
					throw new InvalidOperationException("CellTemplate is null.");
				}
				return (base.CellTemplate as DataGridViewComboBoxCell).DataSource; }
			set {
				if (base.CellTemplate == null) {
					throw new InvalidOperationException("CellTemplate is null.");
				}
				(base.CellTemplate as DataGridViewComboBoxCell).DataSource = value;
			}
		}

		[Editor ("System.Windows.Forms.Design.DataMemberFieldEditor, " + Consts.AssemblySystem_Design,
			 "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[DefaultValue ("")]
		[TypeConverter ("System.Windows.Forms.Design.DataMemberFieldConverter, " + Consts.AssemblySystem_Design)]
		public string DisplayMember {
			get {
				if (base.CellTemplate == null) {
					throw new InvalidOperationException("CellTemplate is null.");
				}
				return (base.CellTemplate as DataGridViewComboBoxCell).DisplayMember;
			}
			set {
				if (base.CellTemplate == null) {
					throw new InvalidOperationException("CellTemplate is null.");
				}
				(base.CellTemplate as DataGridViewComboBoxCell).DisplayMember = value;
			}
		}

		[DefaultValue (DataGridViewComboBoxDisplayStyle.DropDownButton)]
		public DataGridViewComboBoxDisplayStyle DisplayStyle {
			get { return displayStyle; }
			set { displayStyle = value; }
		}

		[DefaultValue (false)]
		public bool DisplayStyleForCurrentCellOnly {
			get { return displayStyleForCurrentCellOnly; }
			set { displayStyleForCurrentCellOnly = value; }
		}

		[DefaultValue (1)]
		public int DropDownWidth {
			get {
				if (base.CellTemplate == null) {
					throw new InvalidOperationException("CellTemplate is null.");
				}
				return (base.CellTemplate as DataGridViewComboBoxCell).DropDownWidth;
			}
			set {
				if (value < 1) {
					throw new ArgumentException("Value is less than 1.");
				}
				if (base.CellTemplate == null) {
					throw new InvalidOperationException("CellTemplate is null.");
				}
				(base.CellTemplate as DataGridViewComboBoxCell).DropDownWidth = value;
			}
		}

		[DefaultValue (FlatStyle.Standard)]
		public FlatStyle FlatStyle {
			get { return flatStyle; }
			set { flatStyle = value; }
		}

		[Editor ("System.Windows.Forms.Design.StringCollectionEditor, " + Consts.AssemblySystem_Design,
			 "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public DataGridViewComboBoxCell.ObjectCollection Items {
			get {
				if (base.CellTemplate == null) {
					throw new InvalidOperationException("CellTemplate is null.");
				}
				return (base.CellTemplate as DataGridViewComboBoxCell).Items;
			}
		}

		[DefaultValue (8)]
		public int MaxDropDownItems {
			get {
				if (base.CellTemplate == null) {
					throw new InvalidOperationException("CellTemplate is null.");
				}
				return (base.CellTemplate as DataGridViewComboBoxCell).MaxDropDownItems;
			}
			set {
				if (base.CellTemplate == null) {
					throw new InvalidOperationException("CellTemplate is null.");
				}
				(base.CellTemplate as DataGridViewComboBoxCell).MaxDropDownItems = value;
			}
		}

		[DefaultValue (false)]
		public bool Sorted {
			get {
				if (base.CellTemplate == null) {
					throw new InvalidOperationException("CellTemplate is null.");
				}
				return (base.CellTemplate as DataGridViewComboBoxCell).Sorted;
			}
			set {
				if (base.CellTemplate == null) {
					throw new InvalidOperationException("CellTemplate is null.");
				}
				(base.CellTemplate as DataGridViewComboBoxCell).Sorted = value;
			}
		}

		[DefaultValue ("")]
		[Editor ("System.Windows.Forms.Design.DataMemberFieldEditor, " + Consts.AssemblySystem_Design,
			 "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[TypeConverter ("System.Windows.Forms.Design.DataMemberFieldConverter, " + Consts.AssemblySystem_Design)]
		public string ValueMember {
			get {
				if (base.CellTemplate == null) {
					throw new InvalidOperationException("CellTemplate is null.");
				}
				return (base.CellTemplate as DataGridViewComboBoxCell).ValueMember;
			}
			set {
				if (base.CellTemplate == null) {
					throw new InvalidOperationException("CellTemplate is null.");
				}
				(base.CellTemplate as DataGridViewComboBoxCell).ValueMember = value;
			}
		}

		internal void SyncItems (IList items)
		{
			if (DataSource != null || DataGridView == null)
				return;

			for (int i = 0; i < DataGridView.RowCount; i++) {
				DataGridViewComboBoxCell comboCell = DataGridView.Rows[i].Cells[base.Index] as DataGridViewComboBoxCell;
				if (comboCell != null) {
					comboCell.Items.ClearInternal ();
					comboCell.Items.AddRangeInternal (this.Items);
				}
			}
		}

		public override object Clone ()
		{
			DataGridViewComboBoxColumn col = (DataGridViewComboBoxColumn) base.Clone();
			col.autoComplete = this.autoComplete;
			col.displayStyle = this.displayStyle;
			col.displayStyleForCurrentCellOnly = this.displayStyleForCurrentCellOnly;
			col.flatStyle = this.flatStyle;
			col.CellTemplate = (DataGridViewComboBoxCell) this.CellTemplate.Clone();
			return col;
		}

		public override string ToString ()
		{
			return GetType().Name;
		}

	}

}
