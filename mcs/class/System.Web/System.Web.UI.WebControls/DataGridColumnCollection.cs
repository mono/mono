//
// System.Web.UI.WebControls.DataGridColumnCollection
//
// Authors:
//	Ben Maurer (bmaurer@novell.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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

using System.Collections;
using System.ComponentModel;

namespace System.Web.UI.WebControls {
	public sealed class DataGridColumnCollection : ICollection, IStateManager {
		public DataGridColumnCollection (DataGrid owner, ArrayList columns)
		{
			this.owner = owner;
			this.columns = columns;
		}
	
	
		public void Add (DataGridColumn column)
		{
			columns.Add (column);
			column.Set_Owner (owner);
			if (track)
				((IStateManager) column).TrackViewState ();
		}
	
		public void AddAt (int index, DataGridColumn column)
		{
			columns.Insert (index, column);
			column.Set_Owner (owner);
			if (track)
				((IStateManager) column).TrackViewState ();
		}
	
		public void Clear ()
		{
			columns.Clear ();
		}
	
		public void CopyTo (Array array, int index)
		{
			columns.CopyTo (array, index);
		}
	
		public IEnumerator GetEnumerator ()
		{
			return columns.GetEnumerator ();
		}
	
		public int IndexOf (DataGridColumn column)
		{
			return columns.IndexOf (column);
		}

		[Obsolete ("figure out what you need with me")]
		internal void OnColumnsChanged ()
		{
			// Do something
		}
	
		public void Remove (DataGridColumn column)
		{
			columns.Remove (column);
		}
	
		public void RemoveAt (int index)
		{
			columns.RemoveAt (index);
		}
	
		void System.Web.UI.IStateManager.LoadViewState (object savedState)
		{
			object [] o = (object []) savedState;
			if (o == null)
				return;

			int i = 0;
			foreach (IStateManager ism in this)
				ism.LoadViewState (o [i++]);
		}
	
		object System.Web.UI.IStateManager.SaveViewState ()
		{
			object [] o = new object [Count];

			int i = 0;
			foreach (IStateManager ism in this)
				o [i++] = ism.SaveViewState ();

			foreach (object a in o)
				if (a != null)
					return o;
			return null;
		}
	
		void System.Web.UI.IStateManager.TrackViewState ()
		{
			track = true;
			foreach (IStateManager ism in this)
				ism.TrackViewState ();
		}

		[Browsable(false)]
		public int Count {
			get { return columns.Count; }
		}
	
		bool IStateManager.IsTrackingViewState {
			get { return track; }
		}
	
		[Browsable(false)]
		public bool IsReadOnly {
			get { return columns.IsReadOnly; }
		}
	
		[Browsable(false)]
		public bool IsSynchronized {
			get { return columns.IsSynchronized; }	
		}

		[Browsable(false)]
		public DataGridColumn this [int index] {
			get { return (DataGridColumn) columns [index]; }
		}

		[Browsable(false)]
		public object SyncRoot {
			get { return columns.SyncRoot; }
		}
	
		DataGrid owner;
		ArrayList columns;
		bool track;
	}
}
