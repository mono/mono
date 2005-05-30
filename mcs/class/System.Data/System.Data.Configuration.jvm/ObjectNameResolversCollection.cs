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