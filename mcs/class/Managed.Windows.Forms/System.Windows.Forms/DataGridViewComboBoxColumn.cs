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


#if NET_2_0

namespace System.Windows.Forms {

	public class DataGridViewComboBoxColumn : DataGridViewColumn {

		private bool autoComplete;
		private DataGridViewComboBoxDisplayStyle displayStyle;
		private bool displayStyleForCurrentCellsOnly;
		private FlatStyle flatStyle;

		public DataGridViewComboBoxColumn () {
			CellTemplate = new DataGridViewComboBoxCell();
			SortMode = DataGridViewColumnSortMode.NotSortable;
			autoComplete = true;
			displayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton;
			displayStyleForCurrentCellsOnly = false;
		}

		public bool AutoComplete {
			get { return autoComplete; }
			set { autoComplete = value; }
		}

		public override DataGridViewCell CellTemplate {
			get { return base.CellTemplate; }
			set { base.CellTemplate = value as DataGridViewComboBoxCell; }
		}

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

		public DataGridViewComboBoxDisplayStyle DisplayStyle {
			get { return displayStyle; }
			set { displayStyle = value; }
		}

		public bool DisplayStyleForCurrentCellsOnly {
			get { return displayStyleForCurrentCellsOnly; }
			set { displayStyleForCurrentCellsOnly = value; }
		}

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

		public FlatStyle FlatStyle {
			get { return flatStyle; }
			set { flatStyle = value; }
		}

		public DataGridViewComboBoxCell.ObjectCollection Items {
			get {
				if (base.CellTemplate == null) {
					throw new InvalidOperationException("CellTemplate is null.");
				}
				return (base.CellTemplate as DataGridViewComboBoxCell).Items;
			}
		}

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

		public override object Clone () {
			DataGridViewComboBoxColumn col = (DataGridViewComboBoxColumn) base.Clone();
			col.autoComplete = this.autoComplete;
			col.displayStyle = this.displayStyle;
			col.displayStyleForCurrentCellsOnly = this.displayStyleForCurrentCellsOnly;
			col.flatStyle = this.flatStyle;
			col.CellTemplate = (DataGridViewComboBoxCell) this.CellTemplate.Clone();
			return col;
		}

		public override string ToString () {
			return GetType().Name;
		}

	}

}

#endif
