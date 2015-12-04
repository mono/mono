//------------------------------------------------------------------------------
// <copyright file="ProcessStartInfo.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Diagnostics {
    using System.Threading;
    using System.Runtime.InteropServices;
    using System.ComponentModel;
    using System.Diagnostics;
    using System;
    using System.Security;
    using System.Security.Permissions;
    using Microsoft.Win32;
    using System.IO;   
    using System.ComponentModel.Design;
    using System.Collections.Specialized;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;
    using System.Runtime.Versioning;

    /// <devdoc>
    ///     A set of values used to specify a process to start.  This is
    ///     used in conjunction with the <see cref='System.Diagnostics.Process'/>
    ///     component.
    /// </devdoc>

    [
    TypeConverter(typeof(ExpandableObjectConverter)),
    // Disabling partial trust scenarios
    PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"),
    HostProtection(SharedState=true, SelfAffectingProcessMgmt=true)
    ]
    public sealed class ProcessStartInfo {
        string fileName;
        string arguments;
        string directory;
        string verb;
        ProcessWindowStyle windowStyle;
        bool errorDialog;
        IntPtr errorDialogParentHandle;
#if FEATURE_PAL
        bool useShellExecute = false;
#else //FEATURE_PAL
        bool useShellExecute = true;
        string userName;
        string domain;
        SecureString password;
        string passwordInClearText;
        bool loadUserProfile;
#endif //FEATURE_PAL
        bool redirectStandardInput = false;
        bool redirectStandardOutput = false;       
        bool redirectStandardError = false;
        Encoding standardOutputEncoding;
        Encoding standardErrorEncoding; 
        
        bool createNoWindow = false;
        WeakReference weakParentProcess;
        internal StringDictionary environmentVariables;

        /// <devdoc>
        ///     Default constructor.  At least the <see cref='System.Diagnostics.ProcessStartInfo.FileName'/>
        ///     property must be set before starting the process.
        /// </devdoc>
        public ProcessStartInfo() {
        }

        internal ProcessStartInfo(Process parent) {
            this.weakParentProcess = new WeakReference(parent);
        }

        /// <devdoc>
        ///     Specifies the name of the application or document that is to be started.
        /// </devdoc>
        [ResourceExposure(ResourceScope.Machine)]
        public ProcessStartInfo(string fileName) {
            this.fileName = fileName;
        }

        /// <devdoc>
        ///     Specifies the name of the application that is to be started, as well as a set
        ///     of command line arguments to pass to the application.
        /// </devdoc>
        [ResourceExposure(ResourceScope.Machine)]
        public ProcessStartInfo(string fileName, string arguments) {
            this.fileName = fileName;
            this.arguments = arguments;
        }

        /// <devdoc>
        ///    <para>
        ///       Specifies the verb to use when opening the filename. For example, the "print"
        ///       verb will print a document specified using <see cref='System.Diagnostics.ProcessStartInfo.FileName'/>.
        ///       Each file extension has it's own set of verbs, which can be obtained using the
        ///    <see cref='System.Diagnostics.ProcessStartInfo.Verbs'/> property.
        ///       The default verb can be specified using "".
        ///    </para>
        ///    <note type="rnotes">
        ///       Discuss 'opening' vs. 'starting.' I think the part about the
        ///       default verb was a dev comment.
        ///       Find out what
        ///       that means.
        ///    </note>
        /// </devdoc>
        [
        DefaultValueAttribute(""), 
        TypeConverter("System.Diagnostics.Design.VerbConverter, " + AssemblyRef.SystemDesign), 
        MonitoringDescription(SR.ProcessVerb),
        NotifyParentProperty(true)
        ]
        public string Verb {
            get {
                if (verb == null) return string.Empty;
                return verb;
            }
            set {
                verb = value;
            }
        }

        /// <devdoc>
        ///     Specifies the set of command line arguments to use when starting the application.
        /// </devdoc>
        [
        DefaultValueAttribute(""), 
        MonitoringDescription(SR.ProcessArguments), 
        SettingsBindable(true),
        TypeConverter("System.Diagnostics.Design.StringValueConverter, " + AssemblyRef.SystemDesign),
        NotifyParentProperty(true)
        ]
        public string Arguments {
            get {
                if (arguments == null) return string.Empty;
                return arguments;
            }
            set {
                arguments = value;
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [
        DefaultValue(false), 
        MonitoringDescription(SR.ProcessCreateNoWindow), 
        NotifyParentProperty(true)
        ]
        public bool CreateNoWindow {
            get { return createNoWindow; }
            set { createNoWindow = value; }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [
            Editor("System.Diagnostics.Design.StringDictionaryEditor, " + AssemblyRef.SystemDesign, "System.Drawing.Design.UITypeEditor, " + AssemblyRef.SystemDrawing),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
            DefaultValue( null ), 
            MonitoringDescription(SR.ProcessEnvironmentVariables),
            NotifyParentProperty(true)     
        ]
        public StringDictionary EnvironmentVariables {
            [ResourceExposure(ResourceScope.Machine)]
            [ResourceConsumption(ResourceScope.Machine)]
            get {
                // Note:
                // Creating a detached ProcessStartInfo will pre-populate the environment
                // with current environmental variables. 

                // When used with an existing Process.ProcessStartInfo the following behavior
                //  * Desktop - Populates with current Environment (rather than that of the process)
                                
                if (environmentVariables == null) {
#if PLATFORM_UNIX
                    environmentVariables = new CaseSensitiveStringDictionary();
#else
                    environmentVariables = new StringDictionaryWithComparer ();
#endif // PLATFORM_UNIX
                    
                    // if not in design mode, initialize the child environment block with all the parent variables
                    if (!(this.weakParentProcess != null &&
                          this.weakParentProcess.IsAlive &&
                          ((Component)this.weakParentProcess.Target).Site != null &&
                          ((Component)this.weakParentProcess.Target).Site.DesignMode)) {
                        
                        foreach (DictionaryEntry entry in System.Environment.GetEnvironmentVariables())
                            environmentVariables.Add((string)entry.Key, (string)entry.Value);
                    }
                
                }
                return environmentVariables;
            }
        }

        private IDictionary<string,string> environment;

        [
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
            DefaultValue(null),
            NotifyParentProperty(true)
        ]
        public IDictionary<string, string> Environment {
            get {
                if (environment == null) {
                    environment = this.EnvironmentVariables.AsGenericDictionary();
                }

                return environment;
            }
        }
      
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [
        DefaultValue(false), 
        MonitoringDescription(SR.ProcessRedirectStandardInput),
        NotifyParentProperty(true)
        ]
        public bool RedirectStandardInput {
            get { return redirectStandardInput; }
            set { redirectStandardInput = value; }
        }
        
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [
        DefaultValue(false), 
        MonitoringDescription(SR.ProcessRedirectStandardOutput),
        NotifyParentProperty(true)
        ]
        public bool RedirectStandardOutput {
            get { return redirectStandardOutput; }
            set { redirectStandardOutput = value; }
        }
        
        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [
        DefaultValue(false), 
        MonitoringDescription(SR.ProcessRedirectStandardError),
        NotifyParentProperty(true)
        ]
        public bool RedirectStandardError {
            get { return redirectStandardError; }
            set { redirectStandardError = value; }
        }

        
        public Encoding StandardErrorEncoding {
            get { return standardErrorEncoding; }
            set { standardErrorEncoding = value; }
        }

        public Encoding StandardOutputEncoding {
            get { return standardOutputEncoding; }
            set { standardOutputEncoding = value; }
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [
        DefaultValue(true), 
        MonitoringDescription(SR.ProcessUseShellExecute),
        NotifyParentProperty(true)
        ]
        public bool UseShellExecute {
            get { return useShellExecute; }
            set { useShellExecute = value; }
        }

#if !FEATURE_PAL

        /// <devdoc>
        ///     Returns the set of verbs associated with the file specified by the
        ///     <see cref='System.Diagnostics.ProcessStartInfo.FileName'/> property.
        /// </devdoc>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string[] Verbs {
            [ResourceExposure(ResourceScope.None)]
            [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
            get {
                ArrayList verbs = new ArrayList();
                RegistryKey key = null;
                string extension = Path.GetExtension(FileName);
                try {
                    if (extension != null && extension.Length > 0) {
                        key = Registry.ClassesRoot.OpenSubKey(extension);
                        if (key != null) {
                            string value = (string)key.GetValue(String.Empty);
                            key.Close();
                            key = Registry.ClassesRoot.OpenSubKey(value + "\\shell");
                            if (key != null) {
                                string[] names = key.GetSubKeyNames();
                                for (int i = 0; i < names.Length; i++)
                                    if (string.Compare(names[i], "new", StringComparison.OrdinalIgnoreCase) != 0)
                                        verbs.Add(names[i]);
                                key.Close();
                                key = null;
                            }
                        }
                    }
                }
                finally {
                    if (key != null) key.Close();
                }
                string[] temp = new string[verbs.Count];
                verbs.CopyTo(temp, 0);
                return temp;
            }
        }

        [NotifyParentProperty(true)]
        public string UserName {
            get { 
                if( userName == null) {
                    return string.Empty;
                }
                else {
                    return userName;                     
                }
            } 
            set { userName = value; }
        }
        
        public SecureString Password {
            get { return password; } 
            set { password = value; }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string PasswordInClearText {
            get { return passwordInClearText; }
            set { passwordInClearText = value; }
        }
        
        [NotifyParentProperty(true)]
        public string Domain {
            get { 
                if( domain == null) {
                    return string.Empty;
                }
                else {
                    return domain;                     
                }                
            }  
            set { domain = value;}
        }
        
        [NotifyParentProperty(true)]
        public bool LoadUserProfile { 
            get { return loadUserProfile;} 
            set { loadUserProfile = value; }
        }

#endif // !FEATURE_PAL

        /// <devdoc>
        ///    <para>
        ///       Returns or sets the application, document, or URL that is to be launched.
        ///    </para>
        /// </devdoc>
        [
        DefaultValue(""),
        Editor("System.Diagnostics.Design.StartFileNameEditor, " + AssemblyRef.SystemDesign, "System.Drawing.Design.UITypeEditor, " + AssemblyRef.SystemDrawing), 
        MonitoringDescription(SR.ProcessFileName),
        SettingsBindable(true),
        TypeConverter("System.Diagnostics.Design.StringValueConverter, " + AssemblyRef.SystemDesign),
        NotifyParentProperty(true)
        ]
        public string FileName {
            [ResourceExposure(ResourceScope.Machine)]
            get {
                if (fileName == null) return string.Empty;
                return fileName;
            }
            [ResourceExposure(ResourceScope.Machine)]
            set { fileName = value;}
        }

        /// <devdoc>
        ///     Returns or sets the initial directory for the process that is started.
        ///     Specify "" to if the default is desired.
        /// </devdoc>
        [
        DefaultValue(""), 
        MonitoringDescription(SR.ProcessWorkingDirectory), 
        Editor("System.Diagnostics.Design.WorkingDirectoryEditor, " + AssemblyRef.SystemDesign, "System.Drawing.Design.UITypeEditor, " + AssemblyRef.SystemDrawing),
        SettingsBindable(true),
        TypeConverter("System.Diagnostics.Design.StringValueConverter, " + AssemblyRef.SystemDesign),
        NotifyParentProperty(true)
        ]
        public string WorkingDirectory {
            [ResourceExposure(ResourceScope.Machine)]
            get {
                if (directory == null) return string.Empty;
                return directory;
            }
            [ResourceExposure(ResourceScope.Machine)]
            set {
                directory = value;
            }
        }

        /// <devdoc>
        ///     Sets or returns whether or not an error dialog should be displayed to the user
        ///     if the process can not be started.
        /// </devdoc>
        [
        DefaultValueAttribute(false), 
        MonitoringDescription(SR.ProcessErrorDialog),
        NotifyParentProperty(true)
        ]
        public bool ErrorDialog {
            get { return errorDialog;}
            set { errorDialog = value;}
        }

        /// <devdoc>
        ///     Sets or returns the window handle to use for the error dialog that is shown
        ///     when a process can not be started.  If <see cref='System.Diagnostics.ProcessStartInfo.ErrorDialog'/>
        ///     is true, this specifies the parent window for the dialog that is shown.  It is
        ///     useful to specify a parent in order to keep the dialog in front of the application.
        /// </devdoc>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IntPtr ErrorDialogParentHandle {
            get { return errorDialogParentHandle;}
            set { errorDialogParentHandle = value;}
        }

        /// <devdoc>
        ///     Sets or returns the style of window that should be used for the newly created process.
        /// </devdoc>
        [
        DefaultValueAttribute(System.Diagnostics.ProcessWindowStyle.Normal), 
        MonitoringDescription(SR.ProcessWindowStyle),
        NotifyParentProperty(true)
        ]
        public ProcessWindowStyle WindowStyle {
            get { return windowStyle;}
            set {
                if (!Enum.IsDefined(typeof(ProcessWindowStyle), value)) 
                    throw new InvalidEnumArgumentException("value", (int)value, typeof(ProcessWindowStyle));
                    
                windowStyle = value;
            }
        }
    }
}
