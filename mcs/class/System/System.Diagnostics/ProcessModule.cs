//
// System.Diagnostics.ProcessModule.cs
//
// Authors:
//   Dick Porter (dick@ximian.com)
//
// (C) 2002 Ximian, Inc.
//

using System;
using System.ComponentModel;

namespace System.Diagnostics {
	public class ProcessModule : Component {
		[MonoTODO]
		public IntPtr BaseAddress {
			get {
				return((IntPtr)0);
			}
		}

		[MonoTODO]
		public IntPtr EntryPointAddress {
			get {
				return((IntPtr)0);
			}
		}

		[MonoTODO]
		public string FileName {
			get {
				return("filename");
			}
		}

		[MonoTODO]
		public FileVersionInfo FileVersionInfo {
			get {
				return(null);
			}
		}

		[MonoTODO]
		public int ModuleMemorySize {
			get {
				return(0);
			}
		}

		[MonoTODO]
		public string ModuleName {
			get {
				return("module name");
			}
		}

		public override string ToString() {
			return(this.ModuleName);
		}
	}
}
