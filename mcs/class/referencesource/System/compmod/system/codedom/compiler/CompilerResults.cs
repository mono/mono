//------------------------------------------------------------------------------
// <copyright file="CompilerResults.cs" company="Microsoft">
// 
// <OWNER>Microsoft</OWNER>
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.CodeDom.Compiler {
    using System;
    using System.CodeDom;
    using System.Reflection;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Policy;
    using System.Runtime.Serialization;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Runtime.Versioning;
    using System.IO;


    /// <devdoc>
    ///    <para>
    ///       Represents the results
    ///       of compilation from the compiler.
    ///    </para>
    /// </devdoc>
    [Serializable()]
    [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public class CompilerResults {
        private CompilerErrorCollection errors = new CompilerErrorCollection();
        private StringCollection output = new StringCollection();
        private Assembly compiledAssembly;
        private string pathToAssembly;
        private int nativeCompilerReturnValue;
        private TempFileCollection tempFiles;
        private Evidence evidence;

        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of <see cref='System.CodeDom.Compiler.CompilerResults'/>
        ///       that uses the specified
        ///       temporary files.
        ///    </para>
        /// </devdoc>
        [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
        public CompilerResults(TempFileCollection tempFiles) {
            this.tempFiles = tempFiles;
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the temporary files to use.
        ///    </para>
        /// </devdoc>
        public TempFileCollection TempFiles {
            [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
            get {
                return tempFiles;
            }

            [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
            set {
                tempFiles = value;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Set the evidence for partially trusted scenarios.
        ///    </para>
        /// </devdoc>
        [Obsolete("CAS policy is obsolete and will be removed in a future release of the .NET Framework. Please see http://go2.microsoft.com/fwlink/?LinkId=131738 for more information.")]
        public Evidence Evidence {
            [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
            get {
                Evidence e = null;
                if (evidence != null)
                    e = evidence.Clone();
                return e;
            }

            [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
            [SecurityPermissionAttribute( SecurityAction.Demand, ControlEvidence = true )]
            set {
                if (value != null)
                    evidence = value.Clone();
                else
                    evidence = null;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       The compiled assembly.
        ///    </para>
        /// </devdoc>
        public Assembly CompiledAssembly {
            [SecurityPermissionAttribute(SecurityAction.Assert, Flags=SecurityPermissionFlag.ControlEvidence)]
            [ResourceExposure(ResourceScope.None)]
            [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)]
            get {
                if (compiledAssembly == null && pathToAssembly != null) {
                    AssemblyName assemName = new AssemblyName();
                    assemName.CodeBase = pathToAssembly;
#pragma warning disable 618 // Load with evidence is obsolete - this warning is passed on via the Evidence property
                    compiledAssembly = Assembly.Load(assemName,evidence);
#pragma warning restore 618
                }
                return compiledAssembly;
            }

            [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
            set {
                compiledAssembly = value;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the collection of compiler errors.
        ///    </para>
        /// </devdoc>
        public CompilerErrorCollection Errors {
            get {
                return errors;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the compiler output messages.
        ///    </para>
        /// </devdoc>
        public StringCollection Output {
            [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
            get {
                return output;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the path to the assembly.
        ///    </para>
        /// </devdoc>
        public string PathToAssembly {
            [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
            [ResourceExposure(ResourceScope.Machine)]
            get {
                return pathToAssembly;
            }

            [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
            [ResourceExposure(ResourceScope.Machine)]
            set {
                pathToAssembly = value;
            }
        }

        /// <devdoc>
        ///    <para>
        ///       Gets or sets the compiler's return value.
        ///    </para>
        /// </devdoc>
        public int NativeCompilerReturnValue {
            get {
                return nativeCompilerReturnValue;
            }

            [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
            set {
                nativeCompilerReturnValue = value;
            }
        }
    }
}

