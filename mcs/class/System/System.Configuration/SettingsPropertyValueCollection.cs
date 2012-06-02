//
// System.Web.UI.WebControls.SettingsPropertyValueCollection.cs
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

	public class SettingsPropertyValueCollection : ICloneable, ICollection, IEnumerable
	{
		Hashtable items;
		bool isReadOnly;

		public SettingsPropertyValueCollection ()
		{
			items = new Hashtable();
		}

		public void Add (SettingsPropertyValue property)
		{
			if (isReadOnly)
				throw new NotSupportedException ();

			/* actually do the add */
			items.Add (property.Name, property);
		}

		internal void Add (SettingsPropertyValueCollection vals)
		{
			foreach (SettingsPropertyValue val in vals) {
				Add (val);
			}
		}

		public void Clear ()
		{
			if (isReadOnly)
				throw new NotSupportedException ();

			items.Clear ();
		}

		public object Clone ()
		{
			SettingsPropertyValueCollection col = new SettingsPropertyValueCollection ();
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

			items.Remove (name);
		}

		public void SetReadOnly ()
		{
			isReadOnly = true;
		}

		public int Count {
			get {
				return items.Count;
			}
		}

		public bool IsSynchronized {
			get {
				throw new NotImplementedException ();
			}
		}

		public SettingsPropertyValue this [ string name ] {
			get {
				return (SettingsPropertyValue) items [ name ];
			}
		}

		public object SyncRoot {
			get {
				throw new NotImplementedException ();
			}
		}
	}

}

