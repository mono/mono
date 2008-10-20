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
// Authors:
//	Peter Bartok	(pbartok@novell.com)
//
//

using System.Collections;
using System.Security.Permissions;

namespace System.Web.UI.WebControls {

	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class DataGridItemCollection : ICollection, IEnumerable 
	{
		#region Fields
		ArrayList	array;
		#endregion	// Fields

		#region Public Constructors
		public DataGridItemCollection (ArrayList items) {
			array = items;
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		public int Count {
			get {
				return array.Count;
			}
		}

		public bool IsReadOnly {
			get {
				return array.IsReadOnly;
			}
		}

		public bool IsSynchronized {
			get {
				return array.IsSynchronized;
			}
		}

		public object SyncRoot {
			get {
				return array.SyncRoot;
			}
		}

		public DataGridItem this[int index] {
			get {
				return (DataGridItem)array[index];
			}
		}
		#endregion	// Public Instance Properties

		#region Public Instance Methods
		public void CopyTo(Array array, int index) {
			if ( !(array is DataGridItem[])) {
				throw new InvalidCastException("Target array must be DataGridItem[]");
			}

			if ((index + this.array.Count) >  array.Length) {
				throw new IndexOutOfRangeException("Target array not large enough to hold copied array.");
			}
			this.array.CopyTo(array, index);
		}

		public IEnumerator GetEnumerator() {
			return array.GetEnumerator();
		}
		#endregion	// Public Instance Methods
	}
}
