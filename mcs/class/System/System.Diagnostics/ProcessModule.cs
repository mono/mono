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
		private IntPtr baseaddr;
		private IntPtr entryaddr;
		private string filename;
		private FileVersionInfo version_info;
		private int memory_size;
		private string modulename;
		
		internal ProcessModule(IntPtr baseaddr, IntPtr entryaddr,
				       string filename,
				       FileVersionInfo version_info,
				       int memory_size, string modulename) {
			this.baseaddr=baseaddr;
			this.entryaddr=entryaddr;
			this.filename=filename;
			this.version_info=version_info;
			this.memory_size=memory_size;
			this.modulename=modulename;
	}
		
		public IntPtr BaseAddress {
			get {
				return(baseaddr);
			}
		}

		public IntPtr EntryPointAddress {
			get {
				return(entryaddr);
			}
		}

		public string FileName {
			get {
				return(filename);
			}
		}

		public FileVersionInfo FileVersionInfo {
			get {
				return(version_info);
			}
		}

		public int ModuleMemorySize {
			get {
				return(memory_size);
			}
		}

		public string ModuleName {
			get {
				return(modulename);
			}
		}

		public override string ToString() {
			return(this.ModuleName);
		}
	}
}
