//
// System.Diagnostics.ProcessStartInfo.cs
//
// Authors:
//   Dick Porter (dick@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

//
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

using Microsoft.Win32;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Runtime.InteropServices;

namespace System.Diagnostics 
{
	[TypeConverter (typeof (ExpandableObjectConverter))]
	[PermissionSet (SecurityAction.LinkDemand, Unrestricted = true)]
	[StructLayout (LayoutKind.Sequential)]
	public sealed class ProcessStartInfo 
	{
		/* keep these fields in this order and in sync with metadata/process.h */
		private string arguments;
		private IntPtr error_dialog_parent_handle = (IntPtr)0;
		private string filename;
		private string verb;
		private string working_directory;
		private StringDictionary envVars;
		private bool create_no_window = false;
		private bool error_dialog = false;
		private bool redirect_standard_error = false;
		private bool redirect_standard_input = false;
		private bool redirect_standard_output = false;
		private bool use_shell_execute = true;
		private ProcessWindowStyle window_style = ProcessWindowStyle.Normal;
		private Encoding encoding_stderr, encoding_stdout;
		private string username, domain;
		private SecureString password;
		private bool load_user_profile;

		public ProcessStartInfo() 
		{
		}

		public ProcessStartInfo(string filename) 
		{
			this.filename = filename;
		}

		public ProcessStartInfo(string filename, string arguments) 
		{
			this.filename = filename;
			this.arguments = arguments;
		}

		[RecommendedAsConfigurable (true), DefaultValue ("")]
		[TypeConverter ("System.Diagnostics.Design.StringValueConverter, " + Consts.AssemblySystem_Design)]

		[MonitoringDescription ("Command line agruments for this process.")]
		[NotifyParentPropertyAttribute (true)]
		public string Arguments {
			get {
				return arguments ?? string.Empty;
			}
			set {
				arguments = value;
			}
		}
		
		[DefaultValue (false)]
		[MonitoringDescription ("Start this process with a new window.")]
		[NotifyParentPropertyAttribute (true)]
		public bool CreateNoWindow {
			get {
				return(create_no_window);
			}
			set {
				create_no_window = value;
			}
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content), DefaultValue (null)]
		[Editor ("System.Diagnostics.Design.StringDictionaryEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[MonitoringDescription ("Environment variables used for this process.")]
		[NotifyParentPropertyAttribute (true)]
		public StringDictionary EnvironmentVariables {
			get {
				if (envVars == null) {
					// check for non-Unix platforms - see FAQ for more details
					// http://www.mono-project.com/FAQ:_Technical#How_to_detect_the_execution_platform_.3F
					int platform = (int) Environment.OSVersion.Platform;
					if ((platform != 4) && (platform != 128)) {
						envVars = new StringDictionary ();
					} else {
						envVars = new CaseSensitiveStringDictionary ();						
					}

					foreach (DictionaryEntry entry in Environment.GetEnvironmentVariables ())
						envVars.Add ((string) entry.Key, (string) entry.Value);
				}

				return envVars;
			}
		}
		
		internal bool HaveEnvVars {
			get { return (envVars != null); }
		}
		
		[DefaultValue (false)]
		[MonitoringDescription ("Thread shows dialogboxes for errors.")]
		[NotifyParentPropertyAttribute (true)]
		public bool ErrorDialog {
			get {
				return(error_dialog);
			}
			set {
				error_dialog = value;
			}
		}
		
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden), Browsable (false)]
		public IntPtr ErrorDialogParentHandle {
			get {
				return(error_dialog_parent_handle);
			}
			set {
				error_dialog_parent_handle = value;
			}
		}
		
		[RecommendedAsConfigurable (true), DefaultValue ("")]
		[Editor ("System.Diagnostics.Design.StartFileNameEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[TypeConverter ("System.Diagnostics.Design.StringValueConverter, " + Consts.AssemblySystem_Design)]
		[MonitoringDescription ("The name of the resource to start this process.")]
		[NotifyParentPropertyAttribute (true)]
		public string FileName {
			get {
				return filename ?? string.Empty;
			}
			set {
				filename = value;
			}
		}
		
		[DefaultValue (false)]
		[MonitoringDescription ("Errors of this process are redirected.")]
		[NotifyParentPropertyAttribute (true)]
		public bool RedirectStandardError {
			get {
				return(redirect_standard_error);
			}
			set {
				redirect_standard_error = value;
			}
		}
		
		[DefaultValue (false)]
		[MonitoringDescription ("Standard input of this process is redirected.")]
		[NotifyParentPropertyAttribute (true)]
		public bool RedirectStandardInput {
			get {
				return(redirect_standard_input);
			}
			set {
				redirect_standard_input = value;
			}
		}
		
		[DefaultValue (false)]
		[MonitoringDescription ("Standard output of this process is redirected.")]
		[NotifyParentPropertyAttribute (true)]
		public bool RedirectStandardOutput {
			get {
				return(redirect_standard_output);
			}
			set {
				redirect_standard_output = value;
			}
		}
		
		public Encoding StandardErrorEncoding {
			get { return encoding_stderr; }
			set { encoding_stderr = value; }
		}

		public Encoding StandardOutputEncoding {
			get { return encoding_stdout; }
			set { encoding_stdout = value; }
		}
		
		[DefaultValue (true)]
		[MonitoringDescription ("Use the shell to start this process.")]
		[NotifyParentPropertyAttribute (true)]
		public bool UseShellExecute {
			get {
				return(use_shell_execute);
			}
			set {
				use_shell_execute = value;
			}
		}
		
		[DefaultValue ("")]
		[TypeConverter ("System.Diagnostics.Design.VerbConverter, " + Consts.AssemblySystem_Design)]
		[MonitoringDescription ("The verb to apply to a used document.")]
		[NotifyParentPropertyAttribute (true)]
		public string Verb {
			get {
				return verb ?? string.Empty;
			}
			set {
				verb = value;
			}
		}

		static readonly string [] empty = new string [0];

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden), Browsable (false)]
		public string[] Verbs {
			get {
#if MOBILE
				return empty;
#else
				switch (Environment.OSVersion.Platform) {
				case (PlatformID)4:
				case (PlatformID)6:
				case (PlatformID)128:
					return empty; // no verb on non-Windows
				default:
					string ext = filename == null | filename.Length == 0 ?
						null : Path.GetExtension (filename);
					if (ext == null)
						return empty;

					RegistryKey rk = null, rk2 = null, rk3 = null;
					try {
						rk = Registry.ClassesRoot.OpenSubKey (ext);
						string k = rk != null ? rk.GetValue (null) as string : null;
						rk2 = k != null ? Registry.ClassesRoot.OpenSubKey (k) : null;
						rk3 = rk2 != null ? rk2.OpenSubKey ("shell") : null;
						return rk3 != null ? rk3.GetSubKeyNames () : null;
					} finally {
						if (rk3 != null)
							rk3.Close ();
						if (rk2 != null)
							rk2.Close ();
						if (rk != null)
							rk.Close ();
					}
				}
#endif
			}
		}
		
		[DefaultValue (typeof (ProcessWindowStyle), "Normal")]
		[MonitoringDescription ("The window style used to start this process.")]
		[NotifyParentPropertyAttribute (true)]
		public ProcessWindowStyle WindowStyle {
			get {
				return(window_style);
			}
			set {
				window_style = value;
			}
		}
		
		[RecommendedAsConfigurable (true), DefaultValue ("")]
		[Editor ("System.Diagnostics.Design.WorkingDirectoryEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		[TypeConverter ("System.Diagnostics.Design.StringValueConverter, " + Consts.AssemblySystem_Design)]
		[MonitoringDescription ("The initial directory for this process.")]
		[NotifyParentPropertyAttribute (true)]
		public string WorkingDirectory {
			get {
				return working_directory ?? string.Empty;
			}
			set {
				working_directory = value;
			}
		}

		[NotifyParentPropertyAttribute (true)]
		public bool LoadUserProfile {
			get { return load_user_profile; }
			set { load_user_profile = value; }
		}

		[NotifyParentPropertyAttribute (true)]
		public string UserName {
			get { return username ?? string.Empty; }
			set { username = value; }
		}

		[NotifyParentPropertyAttribute (true)]
		public string Domain {
			get { return domain ?? string.Empty; }
			set { domain = value; }
		}

		public SecureString Password {
			get { return password; }
			set { password = value; }
		}
	}
}
