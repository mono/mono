//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Activation
{
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections;
    using System.IO;
    using System.Reflection;
    using System.Runtime;
    using System.Security;
    using System.Web.Compilation;

    [Fx.Tag.SecurityNote(Critical = "Entry-point from asp.net, called outside PermitOnly context." +
        "Also needs to elevate in order to inherit from BuildProvider and call methods on the base class.")]
#pragma warning disable 618 // have not moved to the v4 security model yet
    [SecurityCritical(SecurityCriticalScope.Everything)]
#pragma warning restore 618
    [BuildProviderAppliesTo(BuildProviderAppliesTo.Web)]
    [ServiceActivationBuildProvider]
    //class needs to be public for TypeForwarding from System.ServiceModel
    public sealed class ServiceBuildProvider : BuildProvider
    {
        ServiceParser parser;

        public override CompilerType CodeCompilerType
        {
            get
            {
                return GetCodeCompilerType();
            }
        }

        CompilerType GetCodeCompilerType()
        {
            EnsureParsed();
            return parser.CompilerType;
        }

        protected override CodeCompileUnit GetCodeCompileUnit(out IDictionary linePragmasTable)
        {
            CodeSnippetCompileUnit ccu = parser.GetCodeModel() as CodeSnippetCompileUnit;
            linePragmasTable = parser.GetLinePragmasTable();
            return ccu;
        }

        void EnsureParsed()
        {
            if (parser == null)
            {
                parser = new ServiceParser(VirtualPath, this);
                parser.Parse(ReferencedAssemblies);
            }
        }

        public override BuildProviderResultFlags GetResultFlags(CompilerResults results)
        {
            return BuildProviderResultFlags.ShutdownAppDomainOnChange;
        }

        public override void GenerateCode(AssemblyBuilder assemblyBuilder)
        {
            GenerateCodeCore(assemblyBuilder);
        }

        void GenerateCodeCore(AssemblyBuilder assemblyBuilder)
        {
            if (assemblyBuilder == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("assemblyBuilder");
            }

            CodeCompileUnit codeCompileUnit = parser.GetCodeModel();

            // Bail if we have nothing we need to compile
            //
            if (codeCompileUnit == null)
                return;

            // Add the code unit and then add all the assemblies 
            //
            assemblyBuilder.AddCodeCompileUnit(this, codeCompileUnit);
            if (parser.AssemblyDependencies != null)
            {
                foreach (Assembly assembly in parser.AssemblyDependencies)
                {
                    assemblyBuilder.AddAssemblyReference(assembly);
                }
            }
        }

        public override string GetCustomString(CompilerResults results)
        {
            return GetCustomStringCore(results);
        }

        string GetCustomStringCore(CompilerResults results)
        {
            return parser.CreateParseString((results == null) ? null : results.CompiledAssembly);
        }

        public override System.Collections.ICollection VirtualPathDependencies
        {
            get
            {
                return parser.SourceDependencies;
            }
        }

        internal CompilerType GetDefaultCompilerTypeForLanguageInternal(string language)
        {
            return GetDefaultCompilerTypeForLanguage(language);
        }

        internal CompilerType GetDefaultCompilerTypeInternal()
        {
            return GetDefaultCompilerType();
        }

        internal TextReader OpenReaderInternal()
        {
            return OpenReader();
        }
    }
}
