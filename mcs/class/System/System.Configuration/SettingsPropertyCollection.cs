//
// System.Web.UI.WebControls.SettingsPropertyCollection.cs
//
// Authors:
//	Chris Toshok (toshok@ximian.com)
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

using System;
using System.Collections;

namespace System.Configuration
{

	public class SettingsPropertyCollection : ICloneable, ICollection, IEnumerable
	{
		Hashtable items;
		bool isReadOnly;

		public SettingsPropertyCollection ()
		{
			items = new Hashtable();
		}

		public void Add (SettingsProperty property)
		{
			if (isReadOnly)
				throw new NotSupportedException ();

			OnAdd (property);

			/* actually do the add */
			items.Add (property.Name, property);

			OnAddComplete (property);
		}

		public void Clear ()
		{
			if (isReadOnly)
				throw new NotSupportedException ();

			OnClear ();

			/* actually do the clear */
			items.Clear ();

			OnClearComplete();
		}

		public object Clone ()
		{
			SettingsPropertyCollection col = new SettingsPropertyCollection ();
			col.items = (Hashtable)items.Clone ();

			return col;
		}

		public void CopyTo (Array array, int index)
		{
			items.Values.CopyTo (array, index);
		}

		public IEnumerator GetEnumerator ()
		{
			return items.Values.GetEnumerator();
		}

		public void Remove (string name)
		{
			if (isReadOnly)
				throw new NotSupportedException ();

			SettingsProperty property = (SettingsProperty)items[name];

			OnRemove (property);

			/* actually do the remove */
			items.Remove (name);

			OnRemoveComplete (property);
		}

		public void SetReadOnly ()
		{
			isReadOnly = true;
		}

		protected virtual void OnAdd (SettingsProperty property)
		{
		}

		protected virtual void OnAddComplete (SettingsProperty property)
		{
		}

		protected virtual void OnClear ()
		{
		}

		protected virtual void OnClearComplete ()
		{
		}

		protected virtual void OnRemove (SettingsProperty property)
		{
		}

		protected virtual void OnRemoveComplete (SettingsProperty property)
		{
		}

		public int Count {
			get {
				return items.Count;
			}
		}

		public bool IsSynchronized {
			get { return false; }
		}

		public SettingsProperty this [ string name ] {
			get {
				return (SettingsProperty)items [name];
			}
		}

		public object SyncRoot {
			get { return this; }
		}
	}

}

