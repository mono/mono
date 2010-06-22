//
// System.Web.UI.WebControls.DataKey.cs
//
// Authors:
//	Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2005-2010 Novell, Inc (http://www.novell.com)
//

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

using System;
using System.Collections;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public sealed class DataKeyArray : ICollection, IEnumerable, IStateManager
	{
		ArrayList keys;
		bool trackViewState;

		public DataKeyArray (ArrayList keys)
		{
			this.keys = keys;
		}
		
		public int Count {
			get { return keys.Count; }
		}

		public bool IsReadOnly {
			get { return false; }
		}

		public bool IsSynchronized {
			get { return false; }
		}

		public DataKey this [int index] {
			get { return (DataKey) keys [index]; }
		}

		public object SyncRoot {
			get { return this; }
		}

		public void CopyTo (DataKey[] array, int index)
		{
			foreach (DataKey current in this)
				array [index++] = current;
		}

		void ICollection.CopyTo(Array array, int index)
		{
			foreach(object current in this)
				array.SetValue(current, index++);
		}

		public IEnumerator GetEnumerator()
		{
			return keys.GetEnumerator();
		}

		void IStateManager.LoadViewState (object savedState)
		{
			if (savedState == null) return;
			object[] data = (object[]) savedState;
			for (int n=0; n<data.Length && n<keys.Count; n++)
				((IStateManager)keys[n]).LoadViewState (data [n]);
		}
		
		object IStateManager.SaveViewState ()
		{
			if (keys.Count == 0) return null;
			object[] data = new object [keys.Count];
			for (int n=0; n<keys.Count; n++)
				data [n] = ((IStateManager)keys[n]).SaveViewState ();
			return data;
		}
		
		void IStateManager.TrackViewState ()
		{
			trackViewState = true;
			foreach (IStateManager k in keys)
				k.TrackViewState ();
		}
		
		bool IStateManager.IsTrackingViewState {
			get { return trackViewState; }
		}
	}
}

