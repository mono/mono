//
// System.Diagnostics.ProcessStartInfo.cs
//
// Authors:
//   Dick Porter (dick@ximian.com)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) 2002 Ximian, Inc.  http://www.ximian.com
//

using System.ComponentModel;
using System.Collections.Specialized;

namespace System.Diagnostics 
{
	[TypeConverter (typeof (ExpandableObjectConverter))]
	public sealed class ProcessStartInfo 
	{

		private string arguments = "";
		private bool create_no_window = false;
		private bool error_dialog = false;
		private IntPtr error_dialog_parent_handle = (IntPtr)0;
		private string filename = "";
		private bool redirect_standard_error = false;
		private bool redirect_standard_input = false;
		private bool redirect_standard_output = false;
		private bool use_shell_execute = true;
		private string verb = "";
		private ProcessWindowStyle window_style = ProcessWindowStyle.Normal;
		private string working_directory = "";

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
		#if (NET_1_0)
			[TypeConverter ("System.Diagnostics.Design.StringValueConverter, System.Design, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		#endif
		#if (NET_1_1)
    			[TypeConverter ("System.Diagnostics.Design.StringValueConverter, System.Design, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		#endif
		[MonitoringDescription ("Command line agruments for this process.")]
		public string Arguments {
			get {
				return(arguments);
			}
			set {
				arguments = value;
			}
		}
		
		[DefaultValue (false)]
		[MonitoringDescription ("Start this process with a new window.")]
		public bool CreateNoWindow {
			get {
				return(create_no_window);
			}
			set {
				create_no_window = value;
			}
		}

		[MonoTODO("Need to read the env block somehow")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Content), DefaultValue (null)]
		#if (NET_1_0)
			[Editor ("System.Diagnostics.Design.StringDictionaryEditor, System.Design, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		#endif
		#if (NET_1_1)
    			[Editor ("System.Diagnostics.Design.StringDictionaryEditor, System.Design, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		#endif
		[MonitoringDescription ("Environment variables used for this process.")]
		public StringDictionary EnvironmentVariables {
			get {
				throw new NotImplementedException();
			}
		}
		
		[DefaultValue (false)]
		[MonitoringDescription ("Thread shows dialogboxes for errors.")]
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
		#if (NET_1_0)
			[Editor ("System.Diagnostics.Design.StartFileNameEditor, System.Design, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
			[TypeConverter ("System.Diagnostics.Design.StringValueConverter, System.Design, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		#endif
		#if (NET_1_1)
			[Editor ("System.Diagnostics.Design.StartFileNameEditor, System.Design, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    			[TypeConverter ("System.Diagnostics.Design.StringValueConverter, System.Design, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		#endif
		[MonitoringDescription ("The name of the resource to start this process.")]
		public string FileName {
			get {
				return(filename);
			}
			set {
				filename = value;
			}
		}
		
		[DefaultValue (false)]
		[MonitoringDescription ("Errors of this process are redirected.")]
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
		public bool RedirectStandardInput {
			get {
				return(redirect_standard_input);
			}
			set {
				redirect_standard_input = value;
			}
		}
		
		[DefaultValue (false)]
		[MonitoringDescription ("Standart output of this process is redirected.")]
		public bool RedirectStandardOutput {
			get {
				return(redirect_standard_output);
			}
			set {
				redirect_standard_output = value;
			}
		}
		
		[DefaultValue (true)]
		[MonitoringDescription ("Use the shell to start this process.")]
		public bool UseShellExecute {
			get {
				return(use_shell_execute);
			}
			set {
				use_shell_execute = value;
			}
		}
		
		[RecommendedAsConfigurable (true), DefaultValue ("")]
		#if (NET_1_0)
			[TypeConverter ("System.Diagnostics.Design.VerbConverter, System.Design, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		#endif
		#if (NET_1_1)
    			[TypeConverter ("System.Diagnostics.Design.VerbConverter, System.Design, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		#endif
		[MonitoringDescription ("The verb to apply to a used document.")]
		public string Verb {
			get {
				return(verb);
			}
			set {
				verb = value;
			}
		}

		[MonoTODO]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden), Browsable (false)]
		public string[] Verbs {
			get {
				return(null);
			}
		}
		
		[DefaultValue (typeof (ProcessWindowStyle), "Normal")]
		[MonitoringDescription ("The window style used to start this process.")]
		public ProcessWindowStyle WindowStyle {
			get {
				return(window_style);
			}
			set {
				window_style = value;
			}
		}
		
		[RecommendedAsConfigurable (true), DefaultValue ("")]
		#if (NET_1_0)
			[Editor ("System.Diagnostics.Design.WorkingDirectoryEditor, System.Design, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
			[TypeConverter ("System.Diagnostics.Design.StringValueConverter, System.Design, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		#endif
		#if (NET_1_1)
			[Editor ("System.Diagnostics.Design.WorkingDirectoryEditor, System.Design, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    			[TypeConverter ("System.Diagnostics.Design.StringValueConverter, System.Design, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		#endif
		[MonitoringDescription ("The initial directory for this process.")]
		public string WorkingDirectory {
			get {
				return(working_directory);
			}
			set {
				working_directory = value;
			}
		}
	}
}
