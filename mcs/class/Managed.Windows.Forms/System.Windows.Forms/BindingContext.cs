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
// Copyright (c) 2004 Novell, Inc.
//
// Authors:
//	Peter Bartok	pbartok@novell.com
//

// NOT COMPLETE

using System.Collections;
using System.ComponentModel;

namespace System.Windows.Forms {
	public class BindingContext : ICollection, IEnumerable {
		#region Public Constructors
		public BindingContext() {
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
		[MonoTODO]
		public bool IsReadOnly {
			get {
				throw new NotImplementedException();
			}
		}

		public BindingManagerBase this[object dataSource] {
			get {
				return this[dataSource, String.Empty];
			}
		}

		[MonoTODO]
		public BindingManagerBase this[object dataSource, string dataMember] {
			get {
				throw new NotImplementedException();
			}
		}

		
		#endregion	// Public Instance Properties

		#region Public Instance Methods
		public bool Contains(object dataSource) {
			return Contains(dataSource, String.Empty);
		}

		[MonoTODO]
		public bool Contains(object dataSource, string dataMember) {
			throw new NotImplementedException();
		}
		#endregion	// Public Instance Methods

		#region Protected Instance Methods
		protected internal void Add(object dataSource, BindingManagerBase listManager) {
			AddCore(dataSource, listManager);
		}

		[MonoTODO]
		protected virtual void AddCore(object dataSource, BindingManagerBase listManager) {
			throw new NotImplementedException();
		}

		protected internal void Clear() {
			ClearCore();
		}

		[MonoTODO]
		protected virtual void ClearCore() {
			throw new NotImplementedException();
		}

		protected virtual void OnCollectionChanged(System.ComponentModel.CollectionChangeEventArgs ccevent) {
			if (CollectionChanged!=null) CollectionChanged(this, ccevent);
		}

		protected internal void Remove(object dataSource) {
			RemoveCore(dataSource);
		}

		[MonoTODO]
		protected virtual void RemoveCore(object dataSource) {
			throw new NotImplementedException();
		}
		#endregion	// Protected Instance Methods

		#region Events
		public event CollectionChangeEventHandler CollectionChanged;
		#endregion	// Events

		#region ICollection Interfaces
		[MonoTODO]
		void ICollection.CopyTo(Array array, int index) {
			throw new NotImplementedException();
		}

		[MonoTODO]
		int ICollection.Count {
			get {
				throw new NotImplementedException();
			}
		}

		bool ICollection.IsSynchronized {
			get {
				return false;
			}
		}

		object ICollection.SyncRoot {
			get {
				return this;
			}
		}

		#endregion	// ICollection Interfaces

		#region IEnumerable Interfaces
		[MonoTODO]
		IEnumerator IEnumerable.GetEnumerator() {
			throw new NotImplementedException();
		}
		#endregion	// IEnumerable Interfaces
	}
}
