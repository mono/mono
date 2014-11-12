//------------------------------------------------------------------------------
// <copyright file="CompilerParameters.cs" company="Microsoft">
// 
// <OWNER>[....]</OWNER>
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.CodeDom.Compiler {
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Collections.Specialized;
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Security.Policy;
    using System.Runtime.Serialization;
    using System.Runtime.Versioning;

    /// <devdoc>
    ///    <para>
    ///       Represents the parameters used in to invoke the compiler.
    ///    </para>
    /// </devdoc>
    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    [Serializable]
    public class CompilerParameters {

        [OptionalField]
        private string coreAssemblyFileName = String.Empty;

        private StringCollection assemblyNames = new StringCollection();

        [OptionalField] 
        private StringCollection embeddedResources = new StringCollection();
        [OptionalField]         
        private StringCollection linkedResources = new StringCollection();
        
        private string outputName;
        private string mainClass;
        private bool generateInMemory = false;
        private bool includeDebugInformation = false;
        private int warningLevel = -1;  // -1 means not set (use compiler default)
        private string compilerOptions;
        private string win32Resource;
        private bool treatWarningsAsErrors = false;
        private bool generateExecutable = false;
        private TempFileCollection tempFiles;
        [NonSerializedAttribute]
        private SafeUserTokenHandle userToken;
        private Evidence evidence = null;

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of <see cref='System.CodeDom.Compiler.CompilerParameters'/>.
        ///    </para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        public CompilerParameters() :
            this(null, null) {
        }

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of <see cref='System.CodeDom.Compiler.CompilerParameters'/> using the specified
        ///       assembly names.
        ///    </para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.None)]
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
        public CompilerParameters(string[] assemblyNames) :
            this(assemblyNames, null, false) {
        }

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of <see cref='System.CodeDom.Compiler.CompilerParameters'/> using the specified
        ///       assembly names and output name.
        ///    </para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.Machine)]
        [ResourceConsumption(ResourceScope.Machine)]
        public CompilerParameters(string[] assemblyNames, string outputName) :
            this(assemblyNames, outputName, false) {
        }

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of <see cref='System.CodeDom.Compiler.CompilerParameters'/> using the specified
        ///       assembly names, output name and a whether to include debug information flag.
        ///    </para>
        /// </devdoc>
        [ResourceExposure(ResourceScope.Machine)]
        public CompilerParameters(string[] assemblyNames, string outputName, bool includeDebugInformation) {
            if (assemblyNames != null) {
                ReferencedAssemblies.AddRange(assemblyNames);
            }
            this.outputName = outputName;
            this.includeDebugInformation = includeDebugInformation;
        }


        /// <summary>
        /// The "core" or "standard" assembly that contains basic types such as <code>Object</code>, <code>Int32</code> and the like
        /// that is to be used for the compilation.<br />
        /// If the value of this property is an empty string (or <code>null</code>), the default core assembly will be used by the
        /// compiler (depending on the compiler version this may be <code>mscorlib.dll</code> or <code>System.Runtime.dll</code> in
        /// a Framework or reference assembly directory).<br />
        /// If the value of this property is not empty, CodeDOM will emit compiler options to not reference <em>any</em> assemblies
        /// implicitly during compilation. It will also explicitly reference the assembly file specified in this property.<br />
        /// For compilers that only implicitly reference the "core" or "standard" assembly by default, this option can be used on its own.
        /// For compilers that implicitly reference more assemblies on top of the "core" / "standard" assembly, using this option may require
        /// specifying additional entries in the <code>System.CodeDom.Compiler.<bold>ReferencedAssemblies</bold></code> collection.<br />
        /// Note: An <code>ICodeCompiler</code> / <code>CoodeDomProvider</code> implementation may choose to ignore this property.
        /// </summary>
        public string CoreAssemblyFileName {
            get {
                return coreAssemblyFileName;
            }
            set {
                coreAssemblyFileName = value;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets whether to generate an executable.
        ///    </para>
        /// </devdoc>
        public bool GenerateExecutable {
            get {
                return generateExecutable;
            }
            set {
                generateExecutable = value;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets whether to generate in memory.
        ///    </para>
        /// </devdoc>
        public bool GenerateInMemory {
            get {
                return generateInMemory;
            }
            set {
                generateInMemory = value;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the assemblies referenced by the source to compile.
        ///    </para>
        /// </devdoc>
        public StringCollection ReferencedAssemblies {
            get {
                return assemblyNames;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the main class.
        ///    </para>
        /// </devdoc>
        public string MainClass {
            get {
                return mainClass;
            }
            set {
                mainClass = value;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the output assembly.
        ///    </para>
        /// </devdoc>
        public string OutputAssembly {
            [ResourceExposure(ResourceScope.Machine)]
            get {
                return outputName;
            }
            [ResourceExposure(ResourceScope.Machine)]
            set {
                outputName = value;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the temp files.
        ///    </para>
        /// </devdoc>
        public TempFileCollection TempFiles {
            get {
                if (tempFiles == null)
                    tempFiles = new TempFileCollection();
                return tempFiles;
            }
            set {
                tempFiles = value;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets whether to include debug information in the compiled
        ///       executable.
        ///    </para>
        /// </devdoc>
        public bool IncludeDebugInformation {
            get {
                return includeDebugInformation;
            }
            set {
                includeDebugInformation = value;
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public bool TreatWarningsAsErrors {
            get {
                return treatWarningsAsErrors;
            }
            set {
                treatWarningsAsErrors = value;
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public int WarningLevel {
            get {
                return warningLevel;
            }
            set {
                warningLevel = value;
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string CompilerOptions {
            get {
                return compilerOptions;
            }
            set {
                compilerOptions = value;
            }
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public string Win32Resource {
            get {
                return win32Resource;
            }
            set {
                win32Resource = value;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the resources to be compiled into the target
        ///    </para>
        /// </devdoc>
        [ComVisible(false)]        
        public StringCollection EmbeddedResources {
            get {
                return embeddedResources;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the linked resources
        ///    </para>
        /// </devdoc>
        [ComVisible(false)]        
        public StringCollection LinkedResources {
            get {
                return linkedResources;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets user token to be employed when creating the compiler process.
        ///    </para>
        /// </devdoc>
        public IntPtr UserToken {
            get {
                if (userToken != null)
                    return userToken.DangerousGetHandle();
                else
                    return IntPtr.Zero;
            }
            set {
                if (userToken != null)
                    userToken.Close();
                
                userToken = new SafeUserTokenHandle(value, false);
            }
        }

        internal SafeUserTokenHandle SafeUserToken {
            get {
                return userToken;
            }
        }
        
        /// <devdoc>
        ///    <para>
        ///       Set the evidence for partially trusted scenarios.
        ///    </para>
        /// </devdoc>
        [Obsolete("CAS policy is obsolete and will be removed in a future release of the .NET Framework."
                + " Please see http://go2.microsoft.com/fwlink/?LinkId=131738 for more information.")]
        public Evidence Evidence {
            get {
                Evidence e = null;
                if (evidence != null)
                    e = evidence.Clone();
                return e;
            }

            [SecurityPermissionAttribute( SecurityAction.Demand, ControlEvidence = true )]
            set {
                if (value != null)
                    evidence = value.Clone();
                else
                    evidence = null;
            }
        }
    }
}
