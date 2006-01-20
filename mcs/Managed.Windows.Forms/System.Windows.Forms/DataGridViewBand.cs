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

	public class DataGridViewBand : DataGridViewElement, ICloneable, IDisposable {

		// private ContextMenuStrip contextMenuStrip = null;
		private DataGridViewCellStyle defaultCellStyle;
		private Type defaultHeaderCellType;
		private bool displayed;
		private bool frozen = false;
		private int index = -1;
		private bool readOnly = false;
		private DataGridViewTriState resizable = DataGridViewTriState.True;
		private bool selected = false;
		private object tag = null;
		private bool visible = true;
		private DataGridViewHeaderCell headerCellCore;
		private bool isRow;
		private DataGridViewCellStyle inheritedStyle = null;

		~DataGridViewBand () {
			Dispose();
		}

		/*
		public virtual ContextMenuStrip ContextMenuStrip {get; set;}
		*/

		public virtual DataGridViewCellStyle DefaultCellStyle {
			get {
				if (defaultCellStyle == null) {
					defaultCellStyle = new DataGridViewCellStyle();
				}
				return defaultCellStyle;
			}
			set { defaultCellStyle = value; }
		}

		public Type DefaultHeaderCellType {
			get { return defaultHeaderCellType; }
			set {
				if (value.IsSubclassOf(typeof(DataGridViewHeaderCell))) {
					throw new ArgumentException("Type is not DataGridViewHeaderCell or a derived type.");
				}
				defaultHeaderCellType = value;
			}
		}

		public virtual bool Displayed {
			get { return displayed; }
		}

		public virtual bool Frozen {
			get { return frozen; }
			set { frozen = value; }
		}

		public bool HasDefaultCellStyle {
			get { return (defaultCellStyle != null); }
		}

		public int Index {
			get { return index; }
		}

		public virtual DataGridViewCellStyle InheritedStyle {
			get { return inheritedStyle; }
		}

		public virtual bool ReadOnly {
			get { return readOnly; }
			set { readOnly = value; }
		}

		public virtual DataGridViewTriState Resizable {
			get { return resizable; }
			set { resizable = value; }
		}

		public virtual bool Selected {
			get { return selected; }
			set {
				if (DataGridView == null) {
					throw new InvalidOperationException("Cant select a row non associated with a DataGridView.");
				}
				selected = value;
			}
		}

		public object Tag {
			get { return tag; }
			set { tag = value; }
		}

		public virtual bool Visible {
			get { return visible; }
			set { visible = value; }
		}

		public virtual object Clone () {
			DataGridViewBand result = new DataGridViewBand();
			//////////////////////////////
			return result;
		}

		//public sealed void Dispose () {
		public void Dispose () {
		}

		public override string ToString () {
			return this.GetType().Name + ": " + index.ToString() + ".";
		}

		protected DataGridViewHeaderCell HeaderCellCore {
			get { return headerCellCore; }
			set { headerCellCore = value; }
		}

		protected bool IsRow {
			get { return isRow; }
		}

		protected virtual void Dispose (bool disposing) {
		}

		protected override void OnDataGridViewChanged () {
		}

		internal virtual void SetIndex (int index) {
			this.index = index;
		}

	}

}

#endif
