//
// System.Data.ProviderBase.AbstractDbErrorCollection
//
// Authors:
//	Konstantin Triger <kostat@mainsoft.com>
//	Boris Kirzner <borisk@mainsoft.com>
//	
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
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


namespace System.Data.ProviderBase {


	using System.Collections;
	using java.sql;
	using System.Data.Common;

	[Serializable]
	public abstract class AbstractDbErrorCollection : ICollection, IEnumerable {
		private ArrayList _list;

		protected AbstractDbErrorCollection(SQLException e, AbstractDBConnection connection) {
			_list = new ArrayList();

			while(e != null) {
				_list.Add(CreateDbError(e, connection));
				e = e.getNextException();
			}
		}

		protected abstract AbstractDbError CreateDbError(SQLException e, AbstractDBConnection connection);
		/**
		 * Gets the error at the specified index.
		 *
		 * @param index of the error
		 * @return Error on specified index
		 */
		protected AbstractDbError GetDbItem(int index) {
			return (AbstractDbError)_list[index];
		}

		/**
		 * Adds new Error to the collection
		 *
		 * @param value new OleDbError
		 */
		public void Add(object value) {
			_list.Add(value);
		}
        
		public int Count {
			get {
				return _list.Count;
			}
		}

		public IEnumerator GetEnumerator() {
			return _list.GetEnumerator();
		}
        
		public void CopyTo(System.Array arr, int index) {
			_list.CopyTo(arr, index);
		}

		bool ICollection.IsSynchronized {
			get {
				return _list.IsSynchronized;
			}
		}

		Object ICollection.SyncRoot {
			get {
				return _list.SyncRoot;
			}
		}
        
	}
}