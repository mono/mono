//
// System.Diagnostics.ProcessThreadCollection.cs
//
// Authors:
//	Dick Porter (dick@ximian.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System.Collections;

namespace System.Diagnostics {
	public class ProcessThreadCollection : ReadOnlyCollectionBase {
		[MonoTODO]
		protected ProcessThreadCollection() {
		}

		[MonoTODO]
		public ProcessThreadCollection(ProcessThread[] processThreads) {
		}
		
		[MonoTODO]
		public ProcessThread this[int index] {
			get {
				return(null);
			}
		}

		[MonoTODO]
		public int Add(ProcessThread thread) {
			return(0);
		}

		[MonoTODO]
		public bool Contains(ProcessThread thread) {
			return(false);
		}

		[MonoTODO]
		public void CopyTo(ProcessThread[] array, int index) {
		}

		[MonoTODO]
		public int IndexOf(ProcessThread thread) {
			return(0);
		}

		[MonoTODO]
		public void Insert(int index, ProcessThread thread) {
		}

		[MonoTODO]
		public void Remove(ProcessThread thread) {
		}
	}
}
