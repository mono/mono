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
		private ProcessModule[] modules;
		
		[MonoTODO]
		protected ProcessModuleCollection() {
		}

		public ProcessModuleCollection(ProcessModule[] processModules) {
			modules=processModules;
		}
		
		public ProcessModule this[int index] {
			get {
				return(modules[index]);
			}
		}

		public bool Contains(ProcessModule module) {
			foreach(ProcessModule test in modules) {
				if(module==test) {
					return(true);
				}
			}
			
			return(false);
		}

		[MonoTODO]
		public void CopyTo(ProcessModule[] array, int index) {
		}

		public int IndexOf(ProcessModule module) {
			int i;

			for(i=0; i<modules.Length; i++) {
				if(modules[i]==module) {
					return(i);
				}
			}
			
			// FIXME!
			return(0);
		}
	}
}
