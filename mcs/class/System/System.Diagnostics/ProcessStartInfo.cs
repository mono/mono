//
// System.Diagnostics.ProcessStartInfo.cs
//
// Authors:
//	Dick Porter (dick@ximian.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System.Collections.Specialized;

namespace System.Diagnostics {
	public sealed class ProcessStartInfo {
		[MonoTODO]
		public ProcessStartInfo() {
		}

		[MonoTODO]
		public ProcessStartInfo(string filename) {
		}

		[MonoTODO]
		public ProcessStartInfo(string filename, string arguments) {
		}

		[MonoTODO]
		public string Arguments {
			get {
				return("");
			}
			set {
			}
		}

		[MonoTODO]
		public bool CreateNoWindow {
			get {
				return(false);
			}
			set {
			}
		}

		[MonoTODO]
		public StringDictionary EnvironmentVariables {
			get {
				return(null);
			}
		}

		[MonoTODO]
		public bool ErrorDialog {
			get {
				return(false);
			}
			set {
			}
		}

		[MonoTODO]
		public IntPtr ErrorDialogParentHandle {
			get {
				return((IntPtr)0);
			}
			set {
			}
		}

		[MonoTODO]
		public string FileName {
			get {
				return("file name");
			}
			set {
			}
		}

		[MonoTODO]
		public bool RedirectStandardError {
			get {
				return(false);
			}
			set {
			}
		}

		[MonoTODO]
		public bool RedirectStandardInput {
			get {
				return(false);
			}
			set {
			}
		}

		[MonoTODO]
		public bool RedirectStandardOutput {
			get {
				return(false);
			}
			set {
			}
		}

		[MonoTODO]
		public bool UseShellExecute {
			get {
				return(false);
			}
			set {
			}
		}

		[MonoTODO]
		public string Verb {
			get {
				return("verb");
			}
			set {
			}
		}

		[MonoTODO]
		public string[] Verbs {
			get {
				return(null);
			}
		}

		[MonoTODO]
		public ProcessWindowStyle WindowStyle {
			get {
				return(ProcessWindowStyle.Normal);
			}
			set {
			}
		}

		[MonoTODO]
		public string WorkingDirectory {
			get {
				return(".");
			}
			set {
			}
		}
	}
}
