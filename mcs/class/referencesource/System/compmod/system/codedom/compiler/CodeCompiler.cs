//------------------------------------------------------------------------------
// <copyright file="CodeCompiler.cs" company="Microsoft">
// 
// <OWNER>Microsoft</OWNER>
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.CodeDom.Compiler {
    using System.Text;

    using System.Diagnostics;
    using System;
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;
    using System.IO;
    using System.Collections;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Reflection;
    using System.CodeDom;
    using System.Globalization;
    using System.Runtime.Versioning;

    /// <devdoc>
    /// <para>Provides a
    /// base
    /// class for code compilers.</para>
    /// </devdoc>
    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public abstract class CodeCompiler : CodeGenerator, ICodeCompiler {

        /// <internalonly/>
        CompilerResults ICodeCompiler.CompileAssemblyFromDom(CompilerParameters options, CodeCompileUnit e) {
            if( options == null) {
                throw new ArgumentNullException("options");
            }

            try {                
                return FromDom(options, e);
            }
            finally {
                options.TempFiles.SafeDelete();
            }
        }

        /// <internalonly/>
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        CompilerResults ICodeCompiler.CompileAssemblyFromFile(CompilerParameters options, string fileName) {
            if( options == null) {
                throw new ArgumentNullException("options");
            }

            try {
                return FromFile(options, fileName);
            }
            finally {
                options.TempFiles.SafeDelete();
            }
        }

        /// <internalonly/>
        CompilerResults ICodeCompiler.CompileAssemblyFromSource(CompilerParameters options, string source) {
            if( options == null) {
                throw new ArgumentNullException("options");
            }

            try {
                return FromSource(options, source);
            }
            finally {
                options.TempFiles.SafeDelete();
            }
        }

        /// <internalonly/>
        CompilerResults ICodeCompiler.CompileAssemblyFromSourceBatch(CompilerParameters options, string[] sources) {
            if( options == null) {
                throw new ArgumentNullException("options");
            }

            try {
                return FromSourceBatch(options, sources);
            }
            finally {
                options.TempFiles.SafeDelete();
            }
        }
        
        /// <internalonly/>
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        CompilerResults ICodeCompiler.CompileAssemblyFromFileBatch(CompilerParameters options, string[] fileNames) {
            if( options == null) {
                throw new ArgumentNullException("options");
            }
            if (fileNames == null)
                throw new ArgumentNullException("fileNames");

            try {
                // Try opening the files to make sure they exists.  This will throw an exception
                // if it doesn't
                foreach (string fileName in fileNames) {
                    using (Stream str = File.OpenRead(fileName)) { }
                }

                return FromFileBatch(options, fileNames);
            }
            finally {
                options.TempFiles.SafeDelete();
            }
        }

        /// <internalonly/>
        CompilerResults ICodeCompiler.CompileAssemblyFromDomBatch(CompilerParameters options, CodeCompileUnit[] ea) {
            if( options == null) {
                throw new ArgumentNullException("options");
            }

            try {
                return FromDomBatch(options, ea);
            }
            finally {
                options.TempFiles.SafeDelete();
            }
        }
        
        /// <devdoc>
        /// <para>
        /// Gets
        /// or sets the file extension to use for source files.
        /// </para>
        /// </devdoc>
        protected abstract string FileExtension {
            get;
        }

        /// <devdoc>
        /// <para>Gets or
        /// sets the name of the compiler executable.</para>
        /// </devdoc>
        protected abstract string CompilerName {
            get;
        }


        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        internal void Compile(CompilerParameters options, string compilerDirectory, string compilerExe, string arguments, ref string outputFile, ref int nativeReturnValue, string trueArgs) {
            string errorFile = null;
            outputFile = options.TempFiles.AddExtension("out");
            
            // We try to execute the compiler with a full path name.
            string fullname = Path.Combine(compilerDirectory, compilerExe);
            if (File.Exists(fullname)) {
                string trueCmdLine = null;
                if (trueArgs != null)
                    trueCmdLine = "\"" + fullname + "\" " + trueArgs;
                nativeReturnValue = Executor.ExecWaitWithCapture(options.SafeUserToken, "\"" + fullname + "\" " + arguments, Environment.CurrentDirectory, options.TempFiles, ref outputFile, ref errorFile, trueCmdLine);
            }
            else {
                throw new InvalidOperationException(SR.GetString(SR.CompilerNotFound, fullname));
            }
        }

        /// <devdoc>
        /// <para>
        /// Compiles the specified compile unit and options, and returns the results
        /// from the compilation.
        /// </para>
        /// </devdoc>
        protected virtual CompilerResults FromDom(CompilerParameters options, CodeCompileUnit e) {
            if( options == null) {
                throw new ArgumentNullException("options");
            }
            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
                        
            CodeCompileUnit[] units = new CodeCompileUnit[1];
            units[0] = e;
            return FromDomBatch(options, units);
        }

        /// <devdoc>
        /// <para>
        /// Compiles the specified file using the specified options, and returns the
        /// results from the compilation.
        /// </para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        protected virtual CompilerResults FromFile(CompilerParameters options, string fileName) {
            if( options == null) {
                throw new ArgumentNullException("options");
            }
            if (fileName == null)
                throw new ArgumentNullException("fileName");

            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
            
            // Try opening the file to make sure it exists.  This will throw an exception
            // if it doesn't
            using (Stream str = File.OpenRead(fileName)) { }

            string[] filenames = new string[1];
            filenames[0] = fileName;
            return FromFileBatch(options, filenames);
        }
        
         /// <devdoc>
         /// <para>
         /// Compiles the specified source code using the specified options, and
         /// returns the results from the compilation.
         /// </para>
         /// </devdoc>
         protected virtual CompilerResults FromSource(CompilerParameters options, string source) {
             if( options == null) {
                 throw new ArgumentNullException("options");
             }

            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();

            string[] sources = new string[1];
            sources[0] = source;

            return FromSourceBatch(options, sources);
        }
        
        /// <devdoc>
        /// <para>
        /// Compiles the specified compile units and
        /// options, and returns the results from the compilation.
        /// </para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        protected virtual CompilerResults FromDomBatch(CompilerParameters options, CodeCompileUnit[] ea) {
            if( options == null) {
                throw new ArgumentNullException("options");
            }
            if (ea == null)
                throw new ArgumentNullException("ea");

            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();

            string[] filenames = new string[ea.Length];

            CompilerResults results = null;
            
#if !FEATURE_PAL
            // the extra try-catch is here to mitigate exception filter injection attacks. 
            try {
                WindowsImpersonationContext impersonation = Executor.RevertImpersonation();
                try {
#endif // !FEATURE_PAL
                    for (int i = 0; i < ea.Length; i++) {
                        if (ea[i] == null)
                            continue;       // the other two batch methods just work if one element is null, so we'll match that. 
                        
                        ResolveReferencedAssemblies(options, ea[i]);
                        filenames[i] = options.TempFiles.AddExtension(i + FileExtension);
                        Stream temp = new FileStream(filenames[i], FileMode.Create, FileAccess.Write, FileShare.Read);
                        try {
                            using (StreamWriter sw = new StreamWriter(temp, Encoding.UTF8)){
                                ((ICodeGenerator)this).GenerateCodeFromCompileUnit(ea[i], sw, Options);
                                sw.Flush();
                            }
                        }
                        finally {
                            temp.Close();
                        }
                    }

                    results = FromFileBatch(options, filenames);
#if !FEATURE_PAL
                }
                finally {
                    Executor.ReImpersonate(impersonation);
                }
            }
            catch {
                throw;
            }
#endif // !FEATURE_PAL
            return results;
        }

        /// <devdoc>
        ///    <para>
        ///       Because CodeCompileUnit and CompilerParameters both have a referenced assemblies 
        ///       property, they must be reconciled. However, because you can compile multiple
        ///       compile units with one set of options, it will simply merge them.
        ///    </para>
        /// </devdoc>
        private void ResolveReferencedAssemblies(CompilerParameters options, CodeCompileUnit e) {
            if (e.ReferencedAssemblies.Count > 0) {
                foreach(string assemblyName in e.ReferencedAssemblies) {
                    if (!options.ReferencedAssemblies.Contains(assemblyName)) {
                        options.ReferencedAssemblies.Add(assemblyName);
                    }
                }
            }
        }

        /// <devdoc>
        /// <para>
        /// Compiles the specified files using the specified options, and returns the
        /// results from the compilation.
        /// </para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        protected virtual CompilerResults FromFileBatch(CompilerParameters options, string[] fileNames) {
            if( options == null) {
                throw new ArgumentNullException("options");
            }
            if (fileNames == null)
                throw new ArgumentNullException("fileNames");

            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();

            string outputFile = null;
            int retValue = 0;

            CompilerResults results = new CompilerResults(options.TempFiles);
            SecurityPermission perm1 = new SecurityPermission(SecurityPermissionFlag.ControlEvidence);
            perm1.Assert();
            try {
#pragma warning disable 618
               results.Evidence = options.Evidence;
#pragma warning restore 618
            }
            finally {
                 SecurityPermission.RevertAssert();
            }
            bool createdEmptyAssembly = false;

            if (options.OutputAssembly == null || options.OutputAssembly.Length == 0) {
                string extension = (options.GenerateExecutable) ? "exe" : "dll";
                options.OutputAssembly = results.TempFiles.AddExtension(extension, !options.GenerateInMemory);

                // Create an empty assembly.  This is so that the file will have permissions that
                // we can later access with our current credential.  If we don't do this, the compiler
                // could end up creating an assembly that we cannot open 
                new FileStream(options.OutputAssembly, FileMode.Create, FileAccess.ReadWrite).Close();
                createdEmptyAssembly = true;
            }

#if FEATURE_PAL
            results.TempFiles.AddExtension("ildb");
#else
            results.TempFiles.AddExtension("pdb");
#endif


            string args = CmdArgsFromParameters(options) + " " + JoinStringArray(fileNames, " ");

            // Use a response file if the compiler supports it
            string responseFileArgs = GetResponseFileCmdArgs(options, args);
            string trueArgs = null;
            if (responseFileArgs != null) {
                trueArgs = args;
                args = responseFileArgs;
            }

            Compile(options, Executor.GetRuntimeInstallDirectory(), CompilerName, args, ref outputFile, ref retValue, trueArgs);

            results.NativeCompilerReturnValue = retValue;

            // only look for errors/warnings if the compile failed or the caller set the warning level
            if (retValue != 0 || options.WarningLevel > 0) {

                FileStream outputStream = new FileStream(outputFile, FileMode.Open,
                    FileAccess.Read, FileShare.ReadWrite);
                try {
                    if (outputStream.Length > 0) {
                        // The output of the compiler is in UTF8
                        StreamReader sr = new StreamReader(outputStream, Encoding.UTF8);
                        string line;
                        do {
                            line = sr.ReadLine();
                            if (line != null) { 
                                results.Output.Add(line);

                                ProcessCompilerOutputLine(results, line);
                            }
                        } while (line != null);
                    }
                }
                finally {
                    outputStream.Close();
                }

                // Delete the empty assembly if we created one
                if (retValue != 0 && createdEmptyAssembly)
                    File.Delete(options.OutputAssembly);
            }

            if (!results.Errors.HasErrors && options.GenerateInMemory) {
                FileStream fs = new FileStream(options.OutputAssembly, FileMode.Open, FileAccess.Read, FileShare.Read);
                try {
                    int fileLen = (int)fs.Length;
                    byte[] b = new byte[fileLen];
                    fs.Read(b, 0, fileLen);
                    SecurityPermission perm = new SecurityPermission(SecurityPermissionFlag.ControlEvidence);
                    perm.Assert();
                    try {
#pragma warning disable 618 // Load with evidence is obsolete - this warning is passed on via the options parameter
                       results.CompiledAssembly = Assembly.Load(b,null,options.Evidence);
#pragma warning restore 618
                    }
                    finally {
                       SecurityPermission.RevertAssert();
                    }
                }
                finally {
                    fs.Close();
                }
            }
            else {

                results.PathToAssembly = options.OutputAssembly;
            }

            return results;
        }

        /// <devdoc>
        /// <para>Processes the specified line from the specified <see cref='System.CodeDom.Compiler.CompilerResults'/> .</para>
        /// </devdoc>
        protected abstract void ProcessCompilerOutputLine(CompilerResults results, string line);

        /// <devdoc>
        /// <para>
        /// Gets the command arguments from the specified <see cref='System.CodeDom.Compiler.CompilerParameters'/>.
        /// </para>
        /// </devdoc>
        protected abstract string CmdArgsFromParameters(CompilerParameters options);

        /// <devdoc>
        /// <para>[To be supplied.]</para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        protected virtual string GetResponseFileCmdArgs(CompilerParameters options, string cmdArgs) {

            string responseFileName = options.TempFiles.AddExtension("cmdline");

            Stream temp = new FileStream(responseFileName, FileMode.Create, FileAccess.Write, FileShare.Read);
            try {
                using (StreamWriter sw = new StreamWriter(temp, Encoding.UTF8)) {
                    sw.Write(cmdArgs);
                    sw.Flush();
                }
            }
            finally {
                temp.Close();
            }

            return "@\"" + responseFileName + "\"";
        }

        /// <devdoc>
        /// <para>
        /// Compiles the specified source code strings using the specified options, and
        /// returns the results from the compilation.
        /// </para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        protected virtual CompilerResults FromSourceBatch(CompilerParameters options, string[] sources) {
            if( options == null) {
                throw new ArgumentNullException("options");
            }
            if (sources == null)
                throw new ArgumentNullException("sources");

            new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();

            string[] filenames = new string[sources.Length];

            CompilerResults results = null;
#if !FEATURE_PAL
            // the extra try-catch is here to mitigate exception filter injection attacks. 
            try {
                WindowsImpersonationContext impersonation = Executor.RevertImpersonation();
                try {      
#endif // !FEATURE_PAL
                    for (int i = 0; i < sources.Length; i++) {
                        string name = options.TempFiles.AddExtension(i + FileExtension);
                        Stream temp = new FileStream(name, FileMode.Create, FileAccess.Write, FileShare.Read);
                        try {
                            using (StreamWriter sw = new StreamWriter(temp, Encoding.UTF8)) {
                                sw.Write(sources[i]);
                                sw.Flush();
                            }
                        }
                        finally {
                            temp.Close();
                        }
                        filenames[i] = name;
                   }
                   results = FromFileBatch(options, filenames);
#if !FEATURE_PAL
                }
                finally {
                    Executor.ReImpersonate(impersonation);
                }
            }   
            catch {
                throw;
            }
#endif // !FEATURE_PAL

            return results;
        }

        /// <devdoc>
        /// <para>Joins the specified string arrays.</para>
        /// </devdoc>
        protected static string JoinStringArray(string[] sa, string separator) {
            if (sa == null || sa.Length == 0)
                return String.Empty;

            if (sa.Length == 1) {
                return "\"" + sa[0] + "\"";
            }

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < sa.Length - 1; i++) {
                sb.Append("\"");
                sb.Append(sa[i]);
                sb.Append("\"");
                sb.Append(separator);
            }
            sb.Append("\"");
            sb.Append(sa[sa.Length - 1]);
            sb.Append("\"");

            return sb.ToString();
        }
    }
}
