// 
// System.Data.Configuration.ObjectNameResolversCollection.cs
//
// Authors:
//	Konstantin Triger <kostat@mainsoft.com>
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

using System.Collections;
using System.Configuration;
using System.Xml;
using System.Text.RegularExpressions;

namespace System.Data.Configuration {
	sealed class ObjectNameResolversCollection : IEnumerable, ICollection {

		ArrayList _resolvers;

		#region ctors

		internal ObjectNameResolversCollection(ObjectNameResolversCollection parent) {
			_resolvers = new ArrayList();

			if (parent != null)
				_resolvers.AddRange(parent);
		}

		#endregion

		#region methods

		internal void Add(ObjectNameResolver value) {
			_resolvers.Add(value);
		}

		internal void Sort() {
			_resolvers.Sort();
		}

		#endregion

		#region props

		internal ObjectNameResolver this[int index] {
			get {
				return (ObjectNameResolver)_resolvers[index];
			}
		}

		#endregion

		#region IEnumerable Members

		public IEnumerator GetEnumerator() {
			// TODO:  Add ObjectNameResolversCollection.GetEnumerator implementation
			return _resolvers.GetEnumerator();
		}

		#endregion

		#region ICollection Members

		public bool IsSynchronized {
			get {
				// TODO:  Add ObjectNameResolversCollection.IsSynchronized getter implementation
				return _resolvers.IsSynchronized;
			}
		}

		public int Count {
			get {
				// TODO:  Add ObjectNameResolversCollection.Count getter implementation
				return _resolvers.Count;
			}
		}

		public void CopyTo(Array array, int index) {
			// TODO:  Add ObjectNameResolversCollection.CopyTo implementation
			_resolvers.CopyTo(array, index);
		}

		public object SyncRoot {
			get {
				// TODO:  Add ObjectNameResolversCollection.SyncRoot getter implementation
				return _resolvers.SyncRoot;
			}
		}

		#endregion
	}

}