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
//
// Author:
//   Miguel de Icaza (miguel@gnome.org)
//
// (C) 2004 Novell, Inc.
//

#if NET_2_0
using System;
using System.ComponentModel;
using System.Collections;
using System.Windows.Forms.Layout;

namespace System.Windows.Forms {

	[Serializable]
	public sealed class TableLayoutSettings : LayoutSettings {
		//TableLayoutPanel panel;
		TableLayoutColumnStyleCollection column_styles;
		TableLayoutRowStyleCollection row_styles;
		TableLayoutPanelGrowStyle grow_style;
		LayoutEngine layout_engine;
		int column_count;
		int row_count;

		internal TableLayoutSettings (TableLayoutPanel panel)
		{
			//this.panel = panel;
			column_count = 0;
			row_count = 0;
			grow_style = TableLayoutPanelGrowStyle.AddRows;
			column_styles = new TableLayoutColumnStyleCollection (panel);
			row_styles = new TableLayoutRowStyleCollection (panel);
			
			layout_engine = new TableLayout ();
		}

		[DefaultValue (0)]
		public int ColumnCount {
			get {
				return column_count;
			}

			set {
				column_count = value;
			}
		}

		[DefaultValue (0)]
		public int RowCount {
			get {
				return row_count;
			}

			set {
				row_count = value;
			}
		}

		[DefaultValue(TableLayoutPanelGrowStyle.AddRows)]
		public TableLayoutPanelGrowStyle GrowStyle {
			get {
				return grow_style;
			}

			set {
				grow_style = value;
			}
		}

		public override LayoutEngine LayoutEngine {
			get {
				return layout_engine;
			}
		}
				
		public TableLayoutColumnStyleCollection ColumnStyles {
			get {
				return column_styles;
			}
		}

		public TableLayoutRowStyleCollection RowStyle {
			get {
				return row_styles;
			}
		}
	}
}
#endif
