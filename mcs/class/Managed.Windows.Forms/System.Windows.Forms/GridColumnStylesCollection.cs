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
//	Jordi Mas i Hernadez <jordi@ximian.com>
//

using System;
using System.Collections;
using System.ComponentModel;

namespace System.Windows.Forms
{
	public class GridColumnStylesCollection : BaseCollection, IList
	{
		private ArrayList items;
		private DataGridTableStyle owner;

		internal GridColumnStylesCollection (DataGridTableStyle tablestyle)
		{
			items = new ArrayList ();
			owner = tablestyle;
		}

		#region Public Instance Properties
		public DataGridColumnStyle this[string columnName] {
			get {
				for (int i = 0; i < items.Count; i++) {
					DataGridColumnStyle column = (DataGridColumnStyle) items[i];
					if (column.MappingName == columnName) {
						return column;
					}
				}

				return null;
			}
		}

		public DataGridColumnStyle this[int index] {
			get {
				return (DataGridColumnStyle) items[index];
			}
		}

		[MonoTODO]
		public DataGridColumnStyle this[PropertyDescriptor propDesc] {
			get {
				throw new NotImplementedException ();
			}
		}

		protected override ArrayList List {
			get { return items; }
		}

		int ICollection.Count {
			get { return items.Count;}
		}

		bool ICollection.IsSynchronized {
			get { return false; }
		}

		object ICollection.SyncRoot {
			get { return this;}
		}

		bool IList.IsFixedSize {
			get { return false; }
		}

		bool IList.IsReadOnly {
			get { return false;}
		}

		object IList.this [int index] {
			get {
				return items[index];
			}
			set {
				throw new NotSupportedException ();
			}
		}

		#endregion Public Instance Properties

		#region Public Instance Methods
		public virtual int Add (DataGridColumnStyle column)
		{
			return items.Add (column);
		}

		public void AddRange (DataGridColumnStyle[] columns)
		{
			foreach (DataGridColumnStyle mi in columns)
				Add (mi);
		}

		public void Clear ()
		{
			items.Clear ();
		}

		public bool Contains (DataGridColumnStyle column)
		{
			return items.Contains (column);
		}

		[MonoTODO]
		public bool Contains (PropertyDescriptor propDesc)
		{
			throw new NotImplementedException ();
		}

		public bool Contains (string name)
		{
			return (this[name] != null);
		}

		void ICollection.CopyTo (Array dest, int index)
		{
			items.CopyTo (dest, index);
		}

		IEnumerator IEnumerable.GetEnumerator ()
		{
			return items.GetEnumerator ();
		}

		int IList.Add (object value)
		{
			return items.Add (value);
		}

		void IList.Clear ()
		{
			items.Clear ();
		}

		bool IList.Contains (object value)
		{
			return items.Contains (value);
		}

		int IList.IndexOf (object value)
		{
			return items.IndexOf (value);
		}

		void IList.Insert (int index, object value)
		{
			throw new NotSupportedException ();
		}

		void IList.Remove (object value)
		{
			items.Remove (value);
		}

		void IList.RemoveAt (int index)
		{
			items.RemoveAt (index);
		}

		[MonoTODO]
		public void ResetPropertyDescriptors ()
		{
			throw new NotImplementedException ();
		}

		#endregion Public Instance Methods

		#region Events
		#endregion Events
	}
}
