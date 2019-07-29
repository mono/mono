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


using System;
using System.ComponentModel;

namespace System.Windows.Forms {

	[Designer ("System.Windows.Forms.Design.DataGridViewColumnDesigner, " + Consts.AssemblySystem_Design,
		   "System.ComponentModel.Design.IDesigner")]
	[TypeConverter (typeof (DataGridViewColumnConverter))]
	[ToolboxItem ("")]
	[DesignTimeVisible (false)]
	public class DataGridViewColumn : DataGridViewBand, IComponent, IDisposable {
		private bool auto_generated;
		private DataGridViewAutoSizeColumnMode autoSizeMode;
		private DataGridViewCell cellTemplate;
		private ContextMenuStrip contextMenuStrip;
		private string dataPropertyName;
		private int displayIndex;
		private int dividerWidth;
		private float fillWeight;
		private bool frozen;
		private DataGridViewColumnHeaderCell headerCell;
		private bool isDataBound;
		private int minimumWidth = 5;
		private string name = "";
		private bool readOnly;
		private ISite site;
		private DataGridViewColumnSortMode sortMode;
		private string toolTipText;
		private Type valueType;
		private bool visible = true;
		private int width = 100;
		private int dataColumnIndex;

		private bool headerTextSet = false;

		public DataGridViewColumn () {
			cellTemplate = null;
			base.DefaultCellStyle = new DataGridViewCellStyle();
			readOnly = false;
			headerCell = new DataGridViewColumnHeaderCell();
			headerCell.SetColumnIndex(Index);
			headerCell.Value = string.Empty;
			displayIndex = -1;
			dataColumnIndex = -1;
			dataPropertyName = string.Empty;
			fillWeight = 100.0F;
			sortMode = DataGridViewColumnSortMode.NotSortable;
			SetState (DataGridViewElementStates.Visible);
		}

		public DataGridViewColumn (DataGridViewCell cellTemplate) : this () {
			this.cellTemplate = (DataGridViewCell) cellTemplate.Clone();
		}

		[DefaultValue (DataGridViewAutoSizeColumnMode.NotSet)]
		[RefreshProperties (RefreshProperties.Repaint)]
		public DataGridViewAutoSizeColumnMode AutoSizeMode {
			get { return autoSizeMode; }
			set {
				if (autoSizeMode != value) {
					DataGridViewAutoSizeColumnMode old_value = autoSizeMode;
					autoSizeMode = value;
					
					if (DataGridView != null) {
						DataGridView.OnAutoSizeColumnModeChanged (new DataGridViewAutoSizeColumnModeEventArgs (this, old_value));
						DataGridView.AutoResizeColumnsInternal ();
					}
				}
			}
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public virtual DataGridViewCell CellTemplate {
			get { return cellTemplate; }
			set { cellTemplate = value; }
		}

		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public Type CellType {
			get {
				if (cellTemplate == null) {
					return null;
				}
				return cellTemplate.GetType();
			}
		}

		[DefaultValue (null)]
		public override ContextMenuStrip ContextMenuStrip {
			get { return contextMenuStrip; }
			set {
				if (contextMenuStrip != value) {
					contextMenuStrip = value;
					if (DataGridView != null) {
						DataGridView.OnColumnContextMenuStripChanged(new DataGridViewColumnEventArgs(this));
					}
				}
			}
		}

		[Browsable (true)]
		[DefaultValue ("")]
		[Editor ("System.Windows.Forms.Design.DataGridViewColumnDataPropertyNameEditor, " + Consts.AssemblySystem_Design,
			 typeof (System.Drawing.Design.UITypeEditor))]
		[TypeConverter ("System.Windows.Forms.Design.DataMemberFieldConverter, " + Consts.AssemblySystem_Design)]
		public string DataPropertyName {
			get { return dataPropertyName; }
			set {
				if (dataPropertyName != value) {
					dataPropertyName = value;
					if (DataGridView != null) {
						DataGridView.OnColumnDataPropertyNameChanged(new DataGridViewColumnEventArgs(this));
					}
				}
			}
		}

		[Browsable (true)]
		public override DataGridViewCellStyle DefaultCellStyle {
			get {
				return base.DefaultCellStyle;
			}
			set {
				if (DefaultCellStyle != value) {
					base.DefaultCellStyle = value;
					if (DataGridView != null) {
						DataGridView.OnColumnDefaultCellStyleChanged(new DataGridViewColumnEventArgs(this));
					}
				}
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public int DisplayIndex {
			get {
				if (displayIndex < 0) {
					return Index;
				}
				return displayIndex;
			}
			set {
				if (displayIndex != value) {
					if (value < 0 || value > Int32.MaxValue) {
						throw new ArgumentOutOfRangeException("DisplayIndex is out of range");
					}
					displayIndex = value;
					if (DataGridView != null) {
						DataGridView.Columns.RegenerateSortedList ();
						DataGridView.OnColumnDisplayIndexChanged(new DataGridViewColumnEventArgs(this));
					}
				}
			}
		}

		internal int DisplayIndexInternal {
			get { return DisplayIndex; }
			set { displayIndex = value; }
		}

		internal int DataColumnIndex {
			get { return dataColumnIndex; }
			set { 
				dataColumnIndex = value;
				if (dataColumnIndex >= 0)
					isDataBound = true;
			}
		}

		[DefaultValue (0)]
		public int DividerWidth {
			get { return dividerWidth; }
			set {
				if (dividerWidth != value) {
					dividerWidth = value;
					if (DataGridView != null) {
						DataGridView.OnColumnDividerWidthChanged(new DataGridViewColumnEventArgs(this));
					}
				}
			}
		}

		[DefaultValue (100)]
		public float FillWeight {
			get { return fillWeight; }
			set {
				fillWeight = value;
				/* When the System.Windows.Forms.DataGridViewColumn.InheritedAutoSizeMode property value is System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill, the column is resized along with other columns in that mode so that all visible columns in the control exactly fill the horizontal width of the available display area. All fill-mode columns in the control divide the available space in proportions determined by their System.Windows.Forms.DataGridViewColumn.FillWeight property values. For more information about column fill mode, see Column Fill Mode in the Windows Forms DataGridView Control.

The maximum sum of System.Windows.Forms.DataGridViewColumn.FillWeight values for all columns in a System.Windows.Forms.DataGridView control is 65535.
				*/
			}
		}

		[DefaultValue (false)]
		[RefreshProperties (RefreshProperties.All)]
		public override bool Frozen {
			get { return frozen; }
			set { frozen = value; }
		}
		/* When a column is frozen, all the columns to its left (or to its right in right-to-left languages) are frozen as well. The frozen and unfrozen columns form two groups. If column repositioning is enabled by setting the System.Windows.Forms.DataGridView.AllowUserToOrderColumns property to true, the user cannot drag a column from one group to the other.
Example */

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public DataGridViewColumnHeaderCell HeaderCell {
			get {
				return headerCell;
			}
			set {
				if (headerCell != value) {
					headerCell = value;
					headerCell.SetDataGridView(DataGridView);
					headerCell.SetColumnIndex(Index);
					if (DataGridView != null) {
						DataGridView.OnColumnHeaderCellChanged(new DataGridViewColumnEventArgs(this));
					}
				}
			}
		}

		[Localizable (true)]
		public string HeaderText {
			get {
				if (headerCell.Value == null) {
					return String.Empty;
				}
				return (string) headerCell.Value;
			}
			set {
				headerCell.Value = value;
				headerTextSet = true;
			}
		}

		internal bool AutoGenerated { get { return auto_generated; } set { auto_generated = value; } }
		internal bool HeaderTextSet { get { return headerTextSet; } }
		
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public DataGridViewAutoSizeColumnMode InheritedAutoSizeMode {
			get {
				if (this.DataGridView == null)
					return this.autoSizeMode;
				
				if (this.autoSizeMode != DataGridViewAutoSizeColumnMode.NotSet)
					return this.autoSizeMode;
				
				switch (this.DataGridView.AutoSizeColumnsMode) {
				case DataGridViewAutoSizeColumnsMode.AllCells:
					return DataGridViewAutoSizeColumnMode.AllCells;
				case DataGridViewAutoSizeColumnsMode.AllCellsExceptHeader:
					return DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
				case DataGridViewAutoSizeColumnsMode.ColumnHeader:
					return DataGridViewAutoSizeColumnMode.ColumnHeader;
				case DataGridViewAutoSizeColumnsMode.DisplayedCells:
					return DataGridViewAutoSizeColumnMode.DisplayedCells;
				case DataGridViewAutoSizeColumnsMode.DisplayedCellsExceptHeader:
					return DataGridViewAutoSizeColumnMode.DisplayedCellsExceptHeader;
				case DataGridViewAutoSizeColumnsMode.Fill:
					return DataGridViewAutoSizeColumnMode.Fill;
				default:
					return DataGridViewAutoSizeColumnMode.None;
				}				
			}
		}

		[Browsable (false)]
		public override DataGridViewCellStyle InheritedStyle {
			get {
				if (DataGridView == null) {
					return base.DefaultCellStyle;
				}
				else {
					if (base.DefaultCellStyle == null) {
						return DataGridView.DefaultCellStyle;
					}
					else {
						DataGridViewCellStyle style = (DataGridViewCellStyle) base.DefaultCellStyle.Clone();
						/////// Combination with dataGridView.DefaultCellStyle
						return style;
					}
				}
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public bool IsDataBound {
			get { return isDataBound; }
		}

		[DefaultValue (5)]
		[RefreshProperties (RefreshProperties.Repaint)]
		[Localizable (true)]
		public int MinimumWidth {
			get { return minimumWidth; }
			set {
				if (minimumWidth != value) {
					if (value < 2 || value > Int32.MaxValue) {
						throw new ArgumentOutOfRangeException("MinimumWidth is out of range");
					}
					minimumWidth = value;
					if (DataGridView != null) {
						DataGridView.OnColumnMinimumWidthChanged(new DataGridViewColumnEventArgs(this));
					}
				}
			}
		}

		[Browsable (false)]
		public string Name {
			get { return name; }
			set {
				if (name != value) {
					if (value == null)
						name = string.Empty;
					else
						name = value;
					if (!headerTextSet) {
						headerCell.Value = name;
					}
					if (DataGridView != null) {
						DataGridView.OnColumnNameChanged(new DataGridViewColumnEventArgs(this));
					}
				}
			}
		}

		public override bool ReadOnly {
			get {

				if (DataGridView != null && DataGridView.ReadOnly)
					return true;

				return readOnly;
			}
			set { readOnly = value; }
		}

		public override DataGridViewTriState Resizable {
			get { return base.Resizable; }
			set { base.Resizable = value; }
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public ISite Site {
			get { return site; }
			set { site = value; }
		}

		[DefaultValue (DataGridViewColumnSortMode.NotSortable)]
		public DataGridViewColumnSortMode SortMode {
			get { return sortMode; }
			set {
				if (DataGridView != null && value == DataGridViewColumnSortMode.Automatic) {
					if (DataGridView.SelectionMode == DataGridViewSelectionMode.FullColumnSelect ||
					    DataGridView.SelectionMode == DataGridViewSelectionMode.ColumnHeaderSelect)
						throw new InvalidOperationException ("Column's SortMode cannot be set to Automatic "+
										     "while the DataGridView control's SelectionMode "+
										     "is set to FullColumnSelect or ColumnHeaderSelect.");
				}

				if (sortMode != value) {
					sortMode = value;
					if (DataGridView != null) {
						DataGridView.OnColumnSortModeChanged(new DataGridViewColumnEventArgs(this));
					}
				}
			}
		}

		[DefaultValue ("")]
		[Localizable (true)]
		public string ToolTipText {
			get {
				if (toolTipText == null)
					return string.Empty;
				return toolTipText; }
			set {
				if (toolTipText != value) {
					toolTipText = value;
					if (DataGridView != null) {
						DataGridView.OnColumnToolTipTextChanged(new DataGridViewColumnEventArgs(this));
					}
				}
			}
		}

		[Browsable (false)]
		[DefaultValue (null)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public Type ValueType {
			get { return valueType; }
			set { valueType = value; }
		}

		[DefaultValue (true)]
		[Localizable (true)]
		public override bool Visible {
			get { return visible; }
			set {
				visible = value;
				if (DataGridView != null)
					DataGridView.Invalidate ();
			}
		}

		[Localizable (true)]
		[RefreshProperties (RefreshProperties.Repaint)]
		public int Width {
			get { return width; }
			set {
				if (width != value) {
					if (value < minimumWidth) {
						throw new ArgumentOutOfRangeException("Width is less than MinimumWidth");
					}
					width = value;
					if (DataGridView != null)  {
						DataGridView.Invalidate ();
						DataGridView.OnColumnWidthChanged(new DataGridViewColumnEventArgs(this));
					}

				}
			}
		}

		// XXX should we do something like Component.Events?
		[Browsable (false)]
		[EditorBrowsable (EditorBrowsableState.Advanced)]
		public event EventHandler Disposed;

		public override object Clone () {
			return this.MemberwiseClone();
			/*
			DataGridViewColumn result = new DataGridViewColumn();
			return result;
			*/
		}

		[MonoTODO("Actually calculate width")]
		public virtual int GetPreferredWidth (DataGridViewAutoSizeColumnMode autoSizeColumnMode, bool fixedHeight) {
			switch (autoSizeColumnMode) {
			case DataGridViewAutoSizeColumnMode.NotSet:
			case DataGridViewAutoSizeColumnMode.None:
			case DataGridViewAutoSizeColumnMode.Fill:
				throw new ArgumentException("AutoSizeColumnMode is invalid");
			}
			if (fixedHeight) {
				return MinimumWidth;
			}
			else {
				return MinimumWidth;
			}
		}

		public override string ToString () {
			return Name + ", Index: " + base.Index.ToString() + ".";
		}

		protected override void Dispose (bool disposing) {
			if (disposing) {
			}
		}

		internal override void SetDataGridView (DataGridView dataGridView) {
			if (sortMode == DataGridViewColumnSortMode.Automatic && dataGridView != null && dataGridView.SelectionMode == DataGridViewSelectionMode.FullColumnSelect) {
				throw new InvalidOperationException ("Column's SortMode cannot be set to Automatic while the DataGridView control's SelectionMode is set to FullColumnSelect.");
			}
			
			base.SetDataGridView (dataGridView);
			headerCell.SetDataGridView(dataGridView);
		}

		internal override void SetIndex (int index) {
			base.SetIndex(index);
			headerCell.SetColumnIndex(Index);
		}

		internal override void SetState (DataGridViewElementStates state) {
			if (State != state) {
				base.SetState(state);
				if (DataGridView != null) {
					DataGridView.OnColumnStateChanged(new DataGridViewColumnStateChangedEventArgs(this, state));
				}
			}
		}

	}
	
	internal class DataGridViewColumnConverter : TypeConverter
	{
	}
}

