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
// Copyright (c) 2004 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Peter Bartok	(pbartok@novell.com)
//
//

// NOT COMPLETE - Empty, has wrong signature (and is partially stubbed for dependend assemblies) until DataGrid is implemented

using System.ComponentModel;
using System.Drawing;

namespace System.Windows.Forms {
	public class DataGrid : Control {
		#region	Local Variables
		internal bool				allow_sorting;
		internal bool				caption_visible;
		internal string				data_member;
		internal object				data_source;
		internal Color				header_forecolor;
		internal GridTableStylesCollection	table_styles;
		#endregion	// Local Variables

		#region Public Constructors
		[MonoTODO]
		public DataGrid() {
			allow_sorting = false;
			caption_visible = true;
			data_member = string.Empty;
			header_forecolor = Color.Black;
			table_styles = new GridTableStylesCollection();
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		public bool AllowSorting {
			get {
				return allow_sorting;
			}

			set {
				if (allow_sorting != value) {
					allow_sorting = value;
				}
			}
		}

		public bool CaptionVisible {
			get {
				return caption_visible;
			}

			set {
				if (caption_visible != value) {
					caption_visible = value;

					OnCaptionVisibleChanged(EventArgs.Empty);
				}
			}
		}

		public string DataMember {
			get {
				return data_member;
			}

			set {
				if (data_member != value) {
					data_member = value;
				}
			}
		}

		public object DataSource {
			get {
				return data_source;
			}

			set {
				if (data_source != value) {
					data_source = value;

					OnDataSourceChanged(EventArgs.Empty);
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

		public GridTableStylesCollection TableStyles {
			get {
				return table_styles;
			}
		}
		#endregion	// Public Instance Properties

		#region Protected Instance Properties
		#endregion	// Protected Instance Properties

		#region Public Instance Methods
		[MonoTODO]
		public void Expand(int Row) {
			throw new NotImplementedException();
		}
		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		protected virtual void OnCaptionVisibleChanged(EventArgs e) {
			if (CaptionVisibleChanged != null) {
				CaptionVisibleChanged(this, e);
			}
		}

		protected virtual void OnDataSourceChanged(EventArgs e) {
			if (DataSourceChanged != null) {
				DataSourceChanged(this, e);
			}
		}
		#endregion	// Protected Instance Methods

		#region Events
		public event EventHandler CaptionVisibleChanged;
		public event EventHandler DataSourceChanged;
		#endregion	// Events
	}
}
