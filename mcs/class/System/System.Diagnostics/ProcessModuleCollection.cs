//
// System.Diagnostics.ProcessModuleCollection.cs
//
// Authors:
//	Dick Porter (dick@ximian.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System.Collections;

namespace System.Diagnostics {
	public class ProcessModuleCollection : ReadOnlyCollectionBase {
		[MonoTODO]
		protected ProcessModuleCollection() {
		}

		[MonoTODO]
		public ProcessModuleCollection(ProcessModule[] processModules) {
		}
		
		[MonoTODO]
		public ProcessModule this[int index] {
			get {
				return(null);
			}
		}

		[MonoTODO]
		public bool Contains(ProcessModule module) {
			return(false);
		}

		[MonoTODO]
		public void CopyTo(ProcessModule[] array, int index) {
		}

		[MonoTODO]
		public int IndexOf(ProcessModule module) {
			return(0);
		}
	}
}
