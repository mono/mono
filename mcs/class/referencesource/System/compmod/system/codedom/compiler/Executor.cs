//------------------------------------------------------------------------------
// <copyright file="Executor.cs" company="Microsoft">
// 
// <OWNER>Microsoft</OWNER>
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.CodeDom.Compiler {

    using System.Diagnostics;
    using System;
    using System.ComponentModel;
    using System.Text;
    using System.Threading;
    using System.IO;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.CodeDom;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Principal;
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.ConstrainedExecution;
    using System.Runtime.Versioning;
        
    /// <devdoc>
    ///    <para>
    ///       Provides command execution functions for the CodeDom compiler.
    ///    </para>
    /// </devdoc>
    [PermissionSet(SecurityAction.LinkDemand, Unrestricted = true)]
    public static class Executor {

        // How long (in milliseconds) do we wait for the program to terminate
        private const int ProcessTimeOut = 600000;

        /// <devdoc>
        ///    <para>
        ///       Gets the runtime install directory.
        ///    </para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        internal static string GetRuntimeInstallDirectory() {
            return RuntimeEnvironment.GetRuntimeDirectory();
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        private static FileStream CreateInheritedFile(string file) {
            return new FileStream(file, FileMode.CreateNew, FileAccess.Write, FileShare.Read | FileShare.Inheritable);
        }

        /// <devdoc>
        /// </devdoc>
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public static void ExecWait(string cmd, TempFileCollection tempFiles) {
            string outputName = null;
            string errorName = null;
            ExecWaitWithCapture(cmd, tempFiles, ref outputName, ref errorName);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public static int ExecWaitWithCapture(string cmd, TempFileCollection tempFiles, ref string outputName, ref string errorName) {
            return ExecWaitWithCapture(null, cmd, Environment.CurrentDirectory, tempFiles, ref outputName, ref errorName, null);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public static int ExecWaitWithCapture(string cmd, string currentDir, TempFileCollection tempFiles, ref string outputName, ref string errorName) {
            return ExecWaitWithCapture(null, cmd, currentDir, tempFiles, ref outputName, ref errorName, null);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public static int ExecWaitWithCapture(IntPtr userToken, string cmd, TempFileCollection tempFiles, ref string outputName, ref string errorName) {
            return ExecWaitWithCapture(new SafeUserTokenHandle(userToken, false), cmd, Environment.CurrentDirectory, tempFiles, ref outputName, ref errorName, null);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public static int ExecWaitWithCapture(IntPtr userToken, string cmd, string currentDir, TempFileCollection tempFiles, ref string outputName, ref string errorName) {
            return ExecWaitWithCapture(new SafeUserTokenHandle(userToken, false), cmd, Environment.CurrentDirectory, tempFiles, ref outputName, ref errorName, null);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        internal static int ExecWaitWithCapture(SafeUserTokenHandle userToken, string cmd, string currentDir, TempFileCollection tempFiles, ref string outputName, ref string errorName, string trueCmdLine) {
            int retValue = 0;
            // Undo any current impersonation, call ExecWaitWithCaptureUnimpersonated, and reimpersonate
#if !FEATURE_PAL
            // the extra try-catch is here to mitigate exception filter injection attacks. 
            try {
                WindowsImpersonationContext impersonation = RevertImpersonation();
                try {
#endif 
                    
                    // Execute the process
                    retValue = ExecWaitWithCaptureUnimpersonated(userToken, cmd, currentDir, tempFiles, ref outputName, ref errorName, trueCmdLine);
#if !FEATURE_PAL
                } finally {
                    ReImpersonate(impersonation);
                }
            }
            catch {
                throw;
            }
#endif 
            return retValue;
        }

        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        private static unsafe int ExecWaitWithCaptureUnimpersonated(SafeUserTokenHandle userToken, string cmd, string currentDir, TempFileCollection tempFiles, ref string outputName, ref string errorName, string trueCmdLine) {

            IntSecurity.UnmanagedCode.Demand();

            FileStream output;
            FileStream error;

            int retValue = 0;

            if (outputName == null || outputName.Length == 0)
                outputName = tempFiles.AddExtension("out");

            if (errorName == null || errorName.Length == 0)
                errorName = tempFiles.AddExtension("err");

            // Create the files
            output = CreateInheritedFile(outputName);
            error = CreateInheritedFile(errorName);

            bool success = false;
            SafeNativeMethods.PROCESS_INFORMATION pi = new SafeNativeMethods.PROCESS_INFORMATION();
            SafeProcessHandle procSH = new SafeProcessHandle();
            SafeThreadHandle threadSH = new SafeThreadHandle();
            SafeUserTokenHandle primaryToken = null;

            try {
                // Output the command line...
                StreamWriter sw = new StreamWriter(output, Encoding.UTF8);
                sw.Write(currentDir);
                sw.Write("> ");
                // 'true' command line is used in case the command line points to
                // a response file
                sw.WriteLine(trueCmdLine != null ? trueCmdLine : cmd);
                sw.WriteLine();
                sw.WriteLine();
                sw.Flush();

                NativeMethods.STARTUPINFO si = new NativeMethods.STARTUPINFO();

                si.cb = Marshal.SizeOf(si);
#if FEATURE_PAL
                si.dwFlags = NativeMethods.STARTF_USESTDHANDLES;
#else //!FEATURE_PAL
                si.dwFlags = NativeMethods.STARTF_USESTDHANDLES | NativeMethods.STARTF_USESHOWWINDOW;
                si.wShowWindow = NativeMethods.SW_HIDE;
#endif //!FEATURE_PAL
                si.hStdOutput = output.SafeFileHandle;
                si.hStdError = error.SafeFileHandle;
                si.hStdInput = new SafeFileHandle(UnsafeNativeMethods.GetStdHandle(NativeMethods.STD_INPUT_HANDLE), false);

                //
                // Prepare the environment
                //
#if PLATFORM_UNIX

                StringDictionary environment = new CaseSensitiveStringDictionary();

#else

                StringDictionary environment = new StringDictionary ();

#endif // PLATFORM_UNIX

                // Add the current environment
                foreach ( DictionaryEntry entry in Environment.GetEnvironmentVariables () )
                    environment[(string) entry.Key] = (string) entry.Value;

                // Add the flag to indicate restricted security in the process
                environment["_ClrRestrictSecAttributes"] = "1";

                #if DEBUG
                environment["OANOCACHE"] = "1";
                #endif

                // set up the environment block parameter
                byte[] environmentBytes = EnvironmentBlock.ToByteArray(environment, false);
                fixed (byte* environmentBytesPtr = environmentBytes) {
                    IntPtr environmentPtr = new IntPtr((void*)environmentBytesPtr);

                    if (userToken == null || userToken.IsInvalid) {
                        RuntimeHelpers.PrepareConstrainedRegions();
                        try {} finally {
                           success = NativeMethods.CreateProcess(
                                                       null,       // String lpApplicationName, 
                                                       new StringBuilder(cmd), // String lpCommandLine, 
                                                       null,       // SECURITY_ATTRIBUTES lpProcessAttributes, 
                                                       null,       // SECURITY_ATTRIBUTES lpThreadAttributes, 
                                                       true,       // bool bInheritHandles, 
                                                       0,          // int dwCreationFlags, 
                                                       environmentPtr, // IntPtr lpEnvironment, 
                                                       currentDir, // String lpCurrentDirectory, 
                                                       si,         // STARTUPINFO lpStartupInfo, 
                                                       pi);        // PROCESS_INFORMATION lpProcessInformation);
                            if ( pi.hProcess!= (IntPtr)0 && pi.hProcess!= (IntPtr)NativeMethods.INVALID_HANDLE_VALUE)
                                procSH.InitialSetHandle(pi.hProcess);  
                            if ( pi.hThread != (IntPtr)0 && pi.hThread != (IntPtr)NativeMethods.INVALID_HANDLE_VALUE)
                               threadSH.InitialSetHandle(pi.hThread);
                        }
                    }
                    else {
#if FEATURE_PAL
                        throw new NotSupportedException();
#else
                        success = SafeUserTokenHandle.DuplicateTokenEx(
                                                    userToken,
                                                    NativeMethods.TOKEN_ALL_ACCESS,
                                                    null,
                                                    NativeMethods.IMPERSONATION_LEVEL_SecurityImpersonation,
                                                    NativeMethods.TOKEN_TYPE_TokenPrimary,
                                                    out primaryToken
                                                    );


                        if (success) {
                            RuntimeHelpers.PrepareConstrainedRegions();
                            try {} finally {
                               success = NativeMethods.CreateProcessAsUser(
                                                           primaryToken,  // int token,
                                                           null,       // String lpApplicationName, 
                                                           cmd,        // String lpCommandLine, 
                                                           null,       // SECURITY_ATTRIBUTES lpProcessAttributes, 
                                                           null,       // SECURITY_ATTRIBUTES lpThreadAttributes, 
                                                           true,       // bool bInheritHandles, 
                                                           0,          // int dwCreationFlags, 
                                                           new HandleRef(null, environmentPtr), // IntPtr lpEnvironment, 
                                                           currentDir, // String lpCurrentDirectory, 
                                                           si,         // STARTUPINFO lpStartupInfo, 
                                                           pi);        // PROCESS_INFORMATION lpProcessInformation);
                               if ( pi.hProcess!= (IntPtr)0 && pi.hProcess!= (IntPtr)NativeMethods.INVALID_HANDLE_VALUE)
                                   procSH.InitialSetHandle(pi.hProcess);  
                               if ( pi.hThread != (IntPtr)0 && pi.hThread != (IntPtr)NativeMethods.INVALID_HANDLE_VALUE)
                                  threadSH.InitialSetHandle(pi.hThread);
                            }
                        }
#endif // !FEATURE_PAL
                    }
                }
            }
            finally {
                // Close the file handles
                if (!success && (primaryToken != null && !primaryToken.IsInvalid)) {
                    primaryToken.Close();
                    primaryToken = null;
                }

                output.Close();
                error.Close();
            }
            
            if (success) {

                try {
                    bool signaled;
                    ProcessWaitHandle pwh = null;
                    try {
                        pwh = new ProcessWaitHandle(procSH);
                        signaled = pwh.WaitOne(ProcessTimeOut, false);
                    } finally {
                        if (pwh != null)
                            pwh.Close();
                    }

                    // Check for timeout
                    if (!signaled) {
                        throw new ExternalException(SR.GetString(SR.ExecTimeout, cmd), NativeMethods.WAIT_TIMEOUT);
                    }

                    // Check the process's exit code
                    int status = NativeMethods.STILL_ACTIVE;
                    if (!NativeMethods.GetExitCodeProcess(procSH, out status)) {
                        throw new ExternalException(SR.GetString(SR.ExecCantGetRetCode, cmd), Marshal.GetLastWin32Error());
                    }

                    retValue = status;
                }
                finally {
                    procSH.Close();
                    threadSH.Close();
                    if (primaryToken != null && !primaryToken.IsInvalid)
                        primaryToken.Close();
                }
            }
            else {
                int err = Marshal.GetLastWin32Error();
                if (err == NativeMethods.ERROR_NOT_ENOUGH_MEMORY)
                    throw new OutOfMemoryException();

                Win32Exception win32Exception = new Win32Exception(err);
                ExternalException ex = new ExternalException(SR.GetString(SR.ExecCantExec, cmd), win32Exception);
                throw ex;
            }

            return retValue;
        }

#if !FEATURE_PAL
        [PermissionSet(SecurityAction.LinkDemand, Unrestricted = true)]
        [ResourceExposure(ResourceScope.Process)]
        [ResourceConsumption(ResourceScope.Process)]
        [SecurityPermission(SecurityAction.Assert, ControlPrincipal = true, UnmanagedCode = true)]
        internal static WindowsImpersonationContext RevertImpersonation() {            
            return WindowsIdentity.Impersonate(new IntPtr(0));
        }
#endif // !FEATURE_PAL

#if !FEATURE_PAL
        internal static void ReImpersonate(WindowsImpersonationContext impersonation){
            impersonation.Undo();
        }
#endif // !FEATURE_PAL

    }

}

