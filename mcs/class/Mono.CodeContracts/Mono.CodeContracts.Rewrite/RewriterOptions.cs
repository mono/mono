using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mono.CodeContracts.Rewrite {
	public class RewriterOptions {

		public RewriterOptions ()
		{
			// Initialise to defaults
			this.Debug = true;
			this.Level = 4;
			this.WritePdbFile = true;
			this.Rewrite = true;
			this.BreakIntoDebugger = false;
			this.ThrowOnFailure = false;
			
			this.ForceAssemblyRename = null;
		}

		public AssemblyRef Assembly { get; set; }
		public bool Debug { get; set; }
		public int Level { get; set; }
		public bool WritePdbFile { get; set; }
		public bool Rewrite { get; set; }
		public bool BreakIntoDebugger { get; set; }
		public bool ThrowOnFailure { get; set; }
		public AssemblyRef OutputFile { get; set; }
		
		public string ForceAssemblyRename { get; set; }

	}
}
