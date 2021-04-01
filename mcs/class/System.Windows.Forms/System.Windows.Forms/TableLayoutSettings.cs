//
// TableLayoutSettings.cs
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
// Copyright (c) 2006 Jonathan Pobst
//
// Authors:
//	Jonathan Pobst (monkey@jpobst.com)
//

using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Windows.Forms.Layout;
using System.Runtime.Serialization;

namespace System.Windows.Forms 
{
	[Serializable]
	[TypeConverter (typeof (TableLayoutSettingsTypeConverter))]
	public sealed class TableLayoutSettings : LayoutSettings, ISerializable 
	{
		private TableLayoutColumnStyleCollection column_styles;
		private TableLayoutRowStyleCollection row_styles;
		private TableLayoutPanelGrowStyle grow_style;
		private int column_count;
		private int row_count;
		private Dictionary<Object, int> columns;
		private Dictionary<Object, int> column_spans;
		private Dictionary<Object, int> rows;
		private Dictionary<Object, int> row_spans;
		private IArrangedContainer panel;
		internal bool isSerialized;

		#region Internal Constructor
		internal TableLayoutSettings (IArrangedContainer panel)
		{
			this.column_styles = new TableLayoutColumnStyleCollection (panel);
			this.row_styles = new TableLayoutRowStyleCollection (panel);
			this.grow_style = TableLayoutPanelGrowStyle.AddRows;
			this.column_count = 0;
			this.row_count = 0;
			this.columns = new Dictionary<object, int> ();
			this.column_spans = new Dictionary<object, int> ();
			this.rows = new Dictionary<object, int> ();
			this.row_spans = new Dictionary<object, int> ();
			this.panel = panel;
		}

		internal TableLayoutSettings (IArrangedContainer panel, TableLayoutSettings settings)
		{
			this.column_styles = new TableLayoutColumnStyleCollection (panel);
			this.row_styles = new TableLayoutRowStyleCollection (panel);
			this.grow_style = settings.grow_style;
			this.column_count = settings.column_count;
			this.row_count = settings.row_count;
			this.columns = new Dictionary<object, int> (settings.columns);
			this.column_spans = new Dictionary<object, int> (settings.column_spans);
			this.rows = new Dictionary<object, int> (settings.rows);
			this.row_spans = new Dictionary<object, int> (settings.row_spans);
			this.panel = panel;
			foreach (ColumnStyle column_style in settings.column_styles)
				this.column_styles.Add(new ColumnStyle(column_style.SizeType, column_style.Width));
			foreach (RowStyle row_style in settings.row_styles)
				this.row_styles.Add(new RowStyle(row_style.SizeType, row_style.Height));
		}

		private TableLayoutSettings (SerializationInfo serializationInfo, StreamingContext context)
		{
			TypeConverter converter = TypeDescriptor.GetConverter (this);
			string text = serializationInfo.GetString ("SerializedString");
			if (!string.IsNullOrEmpty (text) && (converter != null)) {
				TableLayoutSettings settings = converter.ConvertFromInvariantString (text) as TableLayoutSettings;
				this.column_styles = settings.column_styles;
				this.row_styles = settings.row_styles;
				this.grow_style = settings.grow_style;
				this.column_count = settings.column_count;
				this.row_count = settings.row_count;
				this.columns = settings.columns;
				this.column_spans = settings.column_spans;
				this.rows = settings.rows;
				this.row_spans = settings.row_spans;
				this.panel = settings.panel;
				this.isSerialized = true;
			}
		}
		#endregion		

		#region Public Properties
		[DefaultValue (0)]
		public int ColumnCount {
			get { return this.column_count; }
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException();
					
				if (column_count != value) {
					column_count = value;
					if (panel != null)
						panel.PerformLayout (panel, "ColumnCount");
				}
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public TableLayoutColumnStyleCollection ColumnStyles {
			get { return this.column_styles; }
		}

		[DefaultValue (TableLayoutPanelGrowStyle.AddRows)]
		public TableLayoutPanelGrowStyle GrowStyle {
			get { return this.grow_style; }
			set {
				if (!Enum.IsDefined (typeof(TableLayoutPanelGrowStyle), value))
					throw new ArgumentException();
					
				if (grow_style != value) {
					grow_style = value;
					if (panel != null)
						panel.PerformLayout (panel, "GrowStyle");
				}
			}
		}
		
		public override LayoutEngine LayoutEngine {
			get {
				return System.Windows.Forms.Layout.TableLayout.Instance; 
			}
		}
		
		[DefaultValue (0)]
		public int RowCount {
			get { return this.row_count; }
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException ();

				if (row_count != value) {
					row_count = value;

					if (panel != null)
						panel.PerformLayout (panel, "Rows");
				}
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content)]
		public TableLayoutRowStyleCollection RowStyles {
			get { return row_styles; }
		}
		#endregion

		#region Public Methods
		[DefaultValue (-1)]
		public TableLayoutPanelCellPosition GetCellPosition (Object control)
		{
			if (control == null)
				throw new ArgumentNullException ();

			int column;
			int row;

			if (!columns.TryGetValue (control, out column))
			{
				if (!(control is Control) || !columns.TryGetValue ((control as Control).Name, out column))
					column = -1;
			}
			if (!rows.TryGetValue (control, out row))
			{
				if (!(control is Control) || !rows.TryGetValue ((control as Control).Name, out row))
					row = -1;
			}

			return new TableLayoutPanelCellPosition (column, row);
		}

		[DefaultValue (-1)]
		public int GetColumn (Object control)
		{
			if (control == null)
				throw new ArgumentNullException ();

			int retval;

			if (columns.TryGetValue (control, out retval))
				return retval;
			if ((control is Control) && columns.TryGetValue ((control as Control).Name, out retval))
				return retval;

			return -1;
		}

		public int GetColumnSpan (Object control)
		{
			if (control == null)
				throw new ArgumentNullException ();

			int retval;

			if (column_spans.TryGetValue (control, out retval))
				return retval;
			if ((control is Control) && column_spans.TryGetValue ((control as Control).Name, out retval))
				return retval;

			return 1;
		}

		[DefaultValue (-1)]
		public int GetRow (Object control)
		{
			if (control == null)
				throw new ArgumentNullException ();

			int retval;

			if (rows.TryGetValue (control, out retval))
				return retval;
			if ((control is Control) && rows.TryGetValue ((control as Control).Name, out retval))
				return retval;

			return -1;
		}

		public int GetRowSpan (Object control)
		{
			if (control == null)
				throw new ArgumentNullException ();

			int retval;

			if (row_spans.TryGetValue (control, out retval))
				return retval;
			if ((control is Control) && row_spans.TryGetValue ((control as Control).Name, out retval))
				return retval;

			return 1;
		}

		[DefaultValue (-1)]
		public void SetCellPosition (Object control, TableLayoutPanelCellPosition cellPosition)
		{
			if (control == null)
				throw new ArgumentNullException ();

			columns[control] = cellPosition.Column;
			rows[control] = cellPosition.Row;

			if (panel != null)
				panel.PerformLayout ((IArrangedElement) control, "TableIndex");
		}

		public void SetColumn (Object control, int column)
		{
			if (control == null)
				throw new ArgumentNullException ();
			if (column < -1)
				throw new ArgumentException ();
				
			columns[control] = column;

			if (panel != null)
				panel.PerformLayout ((IArrangedElement) control, "TableIndex");
		}

		public void SetColumnSpan (Object control, int value)
		{
			if (control == null)
				throw new ArgumentNullException ();
			if (value < 1)
				throw new ArgumentOutOfRangeException ();

			column_spans[control] = value;

			if (panel != null)
				panel.PerformLayout ((IArrangedElement) control, "ColumnSpan");
		}

		public void SetRow (Object control, int row)
		{
			if (control == null)
				throw new ArgumentNullException ();
			if (row < -1)
				throw new ArgumentException ();

			rows[control] = row;

			if (panel != null)
				panel.PerformLayout ((IArrangedElement) control, "TableIndex");
		}

		public void SetRowSpan (Object control, int value)
		{
			if (control == null)
				throw new ArgumentNullException ();
			if (value < 1)
				throw new ArgumentOutOfRangeException ();

			row_spans[control] = value;
			
			if (panel != null)
				panel.PerformLayout ((IArrangedElement) control, "RowSpan");
		}
		#endregion

		#region Internal Methods
		internal List<ControlInfo> GetControls ()
		{
			List<ControlInfo> list = new List<ControlInfo>();
			foreach (KeyValuePair <object, int> control in columns) {
				ControlInfo info = new ControlInfo();
				info.Control = control.Key;
				info.Col = GetColumn(control.Key);
				info.ColSpan = GetColumnSpan (control.Key);
				info.Row = GetRow (control.Key);
				info.RowSpan = GetRowSpan (control.Key);
				list.Add (info);
			}
			return list;
		}

		#endregion

		#region ISerializable Members
		void ISerializable.GetObjectData (SerializationInfo si, StreamingContext context)
		{
			TableLayoutSettingsTypeConverter conv = new TableLayoutSettingsTypeConverter ();
			string text = conv.ConvertToInvariantString (this);
			si.AddValue ("SerializedString", text);
		}
		#endregion
		
		internal class StyleConverter : TypeConverter
		{
			public override object ConvertFrom (ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
			{
				if ((value == null) || !(value is String))
					return base.ConvertFrom (context, culture, value);

				return Enum.Parse (typeof (StyleConverter), (string)value, true);
			}

			public override object ConvertTo (ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value, Type destinationType)
			{
				if ((value == null) || !(value is StyleConverter) || (destinationType != typeof (string)))
					return base.ConvertTo (context, culture, value, destinationType);

				return ((StyleConverter)value).ToString ();
			}
		}
	}

	internal struct ControlInfo
	{
		public object Control;
		public int	Row;
		public int RowSpan;
		public int Col;
		public int ColSpan;
	}
}
