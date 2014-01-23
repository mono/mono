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


using System.ComponentModel;

namespace System.Windows.Forms {

	public class DataGridViewBand : DataGridViewElement, ICloneable, IDisposable {

		private ContextMenuStrip contextMenuStrip = null;
		private DataGridViewCellStyle defaultCellStyle;
		private Type defaultHeaderCellType;
		private bool displayed;
		private bool frozen = false;
		private int index = -1;
		private bool readOnly = false;
		private DataGridViewTriState resizable = DataGridViewTriState.NotSet;
		private bool selected = false;
		private object tag = null;
		private bool visible = true;
		private DataGridViewHeaderCell headerCellCore;
		private bool isRow;
		private DataGridViewCellStyle inheritedStyle = null;

		internal DataGridViewBand ()
		{
			defaultHeaderCellType = typeof (DataGridViewHeaderCell);
			isRow = this is DataGridViewRow;
		}

		~DataGridViewBand ()
		{
			Dispose (false);
		}

		[DefaultValue (null)]
		public virtual ContextMenuStrip ContextMenuStrip {
			get { return contextMenuStrip; }
			set { contextMenuStrip = value; }
		}

		[Browsable (false)]
		public virtual DataGridViewCellStyle DefaultCellStyle {
			get {
				if (defaultCellStyle == null) {
					defaultCellStyle = new DataGridViewCellStyle();
				}
				return defaultCellStyle;
			}
			set { defaultCellStyle = value; }
		}

		[Browsable (false)]
		public Type DefaultHeaderCellType {
			get { return defaultHeaderCellType; }
			set {
				if (!value.IsSubclassOf(typeof(DataGridViewHeaderCell))) {
					throw new ArgumentException("Type is not DataGridViewHeaderCell or a derived type.");
				}
				defaultHeaderCellType = value;
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public virtual bool Displayed {
			get { return displayed; }
		}

		[DefaultValue (false)]
		public virtual bool Frozen {
			get { return frozen; }
			set {
				if (frozen != value) {
					frozen = value;
					if (frozen)
						SetState (State | DataGridViewElementStates.Frozen);
					else
						SetState (State & ~DataGridViewElementStates.Frozen);
				}
			}
		}

		[Browsable (false)]
		public bool HasDefaultCellStyle {
			get { return (defaultCellStyle != null); }
		}

		[Browsable (false)]
		public int Index {
			get { return index; }
		}

		[Browsable (false)]
		public virtual DataGridViewCellStyle InheritedStyle {
			get { return inheritedStyle; }
		}

		[DefaultValue (false)]
		public virtual bool ReadOnly {
			get { return readOnly; }
			set {
				if (readOnly != value) {
					readOnly = value;
					if (readOnly)
						SetState (State | DataGridViewElementStates.ReadOnly);
					else
						SetState (State & ~DataGridViewElementStates.ReadOnly);
				}
			}
		}

		[Browsable (true)]
		public virtual DataGridViewTriState Resizable {
			get { 
				if (resizable == DataGridViewTriState.NotSet && DataGridView != null) {
					return DataGridView.AllowUserToResizeColumns ? DataGridViewTriState.True : DataGridViewTriState.False;
				}
				return resizable; }
			set {
				if (value != resizable) {
					resizable = value;
					if (resizable == DataGridViewTriState.True)
						SetState (State | DataGridViewElementStates.Resizable);
					else
						SetState (State & ~DataGridViewElementStates.Resizable);
				}
			}
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public virtual bool Selected {
			get { return selected; }
			set {
				if (DataGridView == null) {
					throw new InvalidOperationException("Cant select a row non associated with a DataGridView.");
				}
				if (isRow) {
					DataGridView.SetSelectedRowCoreInternal (Index, value);
				} else {
					DataGridView.SetSelectedColumnCoreInternal (Index, value);
				}
			}
		}
		
		internal bool SelectedInternal {
			get {
				return selected;
			}
			set {
				if (selected != value) {
					selected = value;

					if (selected)
						SetState (State | DataGridViewElementStates.Selected);
					else
						SetState (State & ~DataGridViewElementStates.Selected);
				}
			}
		}

		internal bool DisplayedInternal {
			get { return displayed; }
			set {
				if (value != displayed) {
					displayed = value;
					if (displayed)
						SetState (State | DataGridViewElementStates.Displayed);
					else
						SetState (State & ~DataGridViewElementStates.Displayed);
				}
			}
		}
		
		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public object Tag {
			get { return tag; }
			set { tag = value; }
		}

		[DefaultValue (true)]
		public virtual bool Visible {
			get { return visible; }
			set {
				if (visible != value) {
					visible = value;
					if (visible)
						SetState (State | DataGridViewElementStates.Visible);
					else
						SetState (State & ~DataGridViewElementStates.Visible);
				}
			}
		}

		public virtual object Clone ()
		{
			DataGridViewBand result = new DataGridViewBand();
			//////////////////////////////
			return result;
		}

		//public sealed void Dispose () {
		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		public override string ToString ()
		{
			return this.GetType().Name + ": " + index.ToString() + ".";
		}

		[Browsable (false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		protected DataGridViewHeaderCell HeaderCellCore {
			get { return headerCellCore; }
			set { headerCellCore = value; }
		}

		protected bool IsRow {
			get { return isRow; }
		}

		protected virtual void Dispose (bool disposing)
		{
		}

		protected override void OnDataGridViewChanged ()
		{
		}

		internal virtual void SetIndex (int index)
		{
			this.index = index;
		}

	}

}

