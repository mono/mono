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

		private string arguments="";
		
		public string Arguments {
			get {
				return(arguments);
			}
			set {
				arguments=value;
			}
		}

		private bool create_no_window=false;
		
		public bool CreateNoWindow {
			get {
				return(create_no_window);
			}
			set {
				create_no_window=value;
			}
		}

		[MonoTODO("Need to read the env block somehow")]
		public StringDictionary EnvironmentVariables {
			get {
				return(null);
			}
		}
		private bool error_dialog=false;
		
		public bool ErrorDialog {
			get {
				return(error_dialog);
			}
			set {
				error_dialog=value;
			}
		}

		private IntPtr error_dialog_parent_handle=(IntPtr)0;
		
		public IntPtr ErrorDialogParentHandle {
			get {
				return(error_dialog_parent_handle);
			}
			set {
				error_dialog_parent_handle=value;
			}
		}

		private string filename="";
		
		public string FileName {
			get {
				return(filename);
			}
			set {
				filename=value;
			}
		}

		private bool redirect_standard_error=false;
		
		public bool RedirectStandardError {
			get {
				return(redirect_standard_error);
			}
			set {
				redirect_standard_error=value;
			}
		}

		private bool redirect_standard_input=false;
		
		public bool RedirectStandardInput {
			get {
				return(redirect_standard_input);
			}
			set {
				redirect_standard_input=value;
			}
		}

		private bool redirect_standard_output=false;
		
		public bool RedirectStandardOutput {
			get {
				return(redirect_standard_output);
			}
			set {
				redirect_standard_output=value;
			}
		}

		private bool use_shell_execute=true;
		
		public bool UseShellExecute {
			get {
				return(use_shell_execute);
			}
			set {
				use_shell_execute=value;
			}
		}

		private string verb="";
		
		public string Verb {
			get {
				return(verb);
			}
			set {
				verb=value;
			}
		}

		[MonoTODO]
		public string[] Verbs {
			get {
				return(null);
			}
		}

		private ProcessWindowStyle window_style=ProcessWindowStyle.Normal;
		
		public ProcessWindowStyle WindowStyle {
			get {
				return(window_style);
			}
			set {
				window_style=value;
			}
		}

		private string working_directory="";
		
		public string WorkingDirectory {
			get {
				return(working_directory);
			}
			set {
				working_directory=value;
			}
		}
	}
}
