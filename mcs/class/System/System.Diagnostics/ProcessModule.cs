//
// System.Diagnostics.ProcessModule.cs
//
// Authors:
//   Dick Porter (dick@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002 Ximian, Inc.
// (C) 2003 Andreas Nahr
//

using System;
using System.ComponentModel;
using System.ComponentModel.Design;

namespace System.Diagnostics 
{
	#if (NET_1_0)
		[Designer ("System.Diagnostics.Design.ProcessModuleDesigner, System.Design, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof (IDesigner))]
	#endif
	#if (NET_1_1)
    		[Designer ("System.Diagnostics.Design.ProcessModuleDesigner, System.Design, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof (IDesigner))]
	#endif
	public class ProcessModule : Component 
	{
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
		
		[MonitoringDescription ("The base memory address of this module")]
		public IntPtr BaseAddress {
			get {
				return(baseaddr);
			}
		}

		[MonitoringDescription ("The base memory address of the entry point of this module")]
		public IntPtr EntryPointAddress {
			get {
				return(entryaddr);
			}
		}

		[MonitoringDescription ("The file name of this module")]
		public string FileName {
			get {
				return(filename);
			}
		}

		[Browsable (false)]
		public FileVersionInfo FileVersionInfo {
			get {
				return(version_info);
			}
		}

		[MonitoringDescription ("The memory needed by this module")]
		public int ModuleMemorySize {
			get {
				return(memory_size);
			}
		}

		[MonitoringDescription ("The name of this module")]
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
