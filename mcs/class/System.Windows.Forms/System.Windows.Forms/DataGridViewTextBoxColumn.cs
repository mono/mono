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

using System.Drawing;
using System.ComponentModel;

namespace System.Windows.Forms {

	[ToolboxBitmap ("")]
	public class DataGridViewTextBoxColumn : DataGridViewColumn {

		private int maxInputLength;

		public DataGridViewTextBoxColumn ()
		{
			base.CellTemplate = new DataGridViewTextBoxCell();
			maxInputLength = 32767;
			base.SortMode = DataGridViewColumnSortMode.Automatic;
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public override DataGridViewCell CellTemplate {
			get { return base.CellTemplate; }
			set { base.CellTemplate = value as DataGridViewTextBoxCell; }
		}

		[DefaultValue (32767)]
		public int MaxInputLength {
			get { return maxInputLength; }
			set {
				if (value < 0) {
					throw new ArgumentOutOfRangeException("Value is less than 0.");
				}
				maxInputLength = value;
			}
		}

		[DefaultValue (DataGridViewColumnSortMode.Automatic)]
		public new DataGridViewColumnSortMode SortMode {
			get { return base.SortMode; }
			set { base.SortMode = value; }
		}

		public override string ToString ()
		{
			return string.Format ("DataGridViewTextBoxColumn {{ Name={0}, Index={1} }}", Name, Index);
		}



	}

}

