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
// Copyright (c) 2004 Novell, Inc.
//
// Authors:
//	Peter Bartok	pbartok@novell.com
//
//


// NOT COMPLETE - Empty (and some functions minimally stubbed for dependencies) until DataGrid is implemented

using System.ComponentModel;
using System.Drawing;

namespace System.Windows.Forms {
	public class DataGridTableStyle : Component {
		#region	Local Variables
		internal bool		allow_sorting;
		internal DataGrid	datagrid;
		internal Color		header_forecolor;
		internal string		mapping_name;
		#endregion	// Local Variables

		#region Constructors
		public DataGridTableStyle() {
			allow_sorting = false;
			datagrid = new DataGrid();
			header_forecolor = Color.Black;
			mapping_name = string.Empty;
		}
		#endregion

		#region Public Static Fields
		#endregion	// Public Static Fields

		#region Public Instance Properties
		public bool AllowSorting {
			get {
				return allow_sorting;
			}

			set {
				if (allow_sorting != value) {
					allow_sorting = value;

					OnAllowSortingChanged(EventArgs.Empty);
				}
			}
		}

		public virtual DataGrid DataGrid {
			get {
				return datagrid;
			}

			set {
				if (datagrid != value) {
					datagrid = value;
				}
			}
		}

		public Color HeaderForeColor {
			get {
				return header_forecolor;
			}

			set {
				if (header_forecolor != value) {
					header_forecolor = value;
				}
			}
		}

		public string MappingName {
			get {
				return mapping_name;
			}

			set {
				if (mapping_name != value) {
					mapping_name = value;

					OnMappingNameChanged(EventArgs.Empty);
				}
			}
		}
		#endregion	// Public Instance Properties

		#region Public Instance Methods
		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		protected virtual void OnMappingNameChanged(EventArgs e) {
			if (MappingNameChanged != null) {
				MappingNameChanged(this, e);
			}
		}

		protected virtual void OnAllowSortingChanged(EventArgs e) {
			if (AllowSortingChanged != null) {
				AllowSortingChanged(this, e);
			}
		}
		#endregion	// Protected Instance Methods

		#region Events
		public event EventHandler AllowSortingChanged;
		public event EventHandler MappingNameChanged;
		#endregion	// Events
	}
}
