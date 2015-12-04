//------------------------------------------------------------------------------
// <copyright file="CompilerInfo.cs" company="Microsoft">
// 
// <OWNER>[....]</OWNER>
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.CodeDom.Compiler {
    using System;
    using System.Reflection;
    using System.Security.Permissions;
    using System.CodeDom.Compiler;
    using System.Configuration;
    using System.Collections.Generic;
    using System.Diagnostics;

    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    public sealed class CompilerInfo {
        internal String _codeDomProviderTypeName; // This can never by null
        internal CompilerParameters _compilerParams; // This can never by null
        internal String[] _compilerLanguages; // This can never by null
        internal String[] _compilerExtensions; // This can never by null
        internal String configFileName;
        internal IDictionary<string, string> _providerOptions;  // This can never be null
        internal int configFileLineNumber;
        internal Boolean _mapped;
        
        private Type type;
        
        private CompilerInfo() {} // Not createable

        public String[] GetLanguages() { 
            return CloneCompilerLanguages();
        }

        public String[] GetExtensions() { 
            return CloneCompilerExtensions();
        }

        public Type CodeDomProviderType { 
            get {
                if( type == null) {
                    lock(this) {
                        if( type == null) {
                            type = Type.GetType(_codeDomProviderTypeName);
                            if (type == null) {                    
                                if( configFileName == null) {
                                    throw new ConfigurationErrorsException(SR.GetString(SR.Unable_To_Locate_Type, 
                                                                      _codeDomProviderTypeName, string.Empty, 0));
                                }
                                else {
                                    throw new ConfigurationErrorsException(SR.GetString(SR.Unable_To_Locate_Type,
                                                                      _codeDomProviderTypeName), configFileName, configFileLineNumber);
                                }
                            }
                        }                                                        
                    }
                }
                    
                return type;
            }
        }

        public bool IsCodeDomProviderTypeValid {
            get {
                Type type = Type.GetType(_codeDomProviderTypeName);
                return (type != null);
            }
        }

        public CodeDomProvider CreateProvider() {
            // if the provider defines an IDictionary<string, string> ctor and
            // provider options have been provided then call that and give it the 
            // provider options dictionary.  Otherwise call the normal one.

            Debug.Assert(_providerOptions != null, "Created CompilerInfo w/ null _providerOptions");

            if (_providerOptions.Count > 0) {
                ConstructorInfo ci = CodeDomProviderType.GetConstructor(new Type[] { typeof(IDictionary<string, string>) });
                if (ci != null) {
                    return (CodeDomProvider)ci.Invoke(new object[] { _providerOptions });
                }
            }

            return (CodeDomProvider)Activator.CreateInstance(CodeDomProviderType);
        }

        public CodeDomProvider CreateProvider(IDictionary<String, String> providerOptions)
        {
            if (providerOptions == null)
                throw new ArgumentNullException("providerOptions");

            ConstructorInfo constructor = CodeDomProviderType.GetConstructor(new Type[] { typeof(IDictionary<string, string>) });
            if (constructor != null) {
                return (CodeDomProvider)constructor.Invoke(new object[] { providerOptions });
            }
            else
                throw new InvalidOperationException(SR.GetString(SR.Provider_does_not_support_options, CodeDomProviderType.ToString()));

        }

        public CompilerParameters CreateDefaultCompilerParameters() {
            return CloneCompilerParameters();
        }


        internal CompilerInfo(CompilerParameters compilerParams, String codeDomProviderTypeName, String[] compilerLanguages, String[] compilerExtensions) {
            _compilerLanguages = compilerLanguages;
            _compilerExtensions = compilerExtensions;
            _codeDomProviderTypeName = codeDomProviderTypeName;
            if (compilerParams == null)
                compilerParams = new CompilerParameters();

            _compilerParams = compilerParams;
        }

        internal CompilerInfo(CompilerParameters compilerParams, String codeDomProviderTypeName) {
            _codeDomProviderTypeName = codeDomProviderTypeName;
            if (compilerParams == null)
                compilerParams = new CompilerParameters();

            _compilerParams = compilerParams;
        }


        public override int GetHashCode() {
            return _codeDomProviderTypeName.GetHashCode();
        }

        public override bool Equals(Object o) {
            CompilerInfo other = o as CompilerInfo;
            if (o == null)
                return false;

            return CodeDomProviderType == other.CodeDomProviderType &&
                CompilerParams.WarningLevel == other.CompilerParams.WarningLevel &&
                CompilerParams.IncludeDebugInformation == other.CompilerParams.IncludeDebugInformation &&
                CompilerParams.CompilerOptions == other.CompilerParams.CompilerOptions;
        }

        private CompilerParameters CloneCompilerParameters() {
            CompilerParameters copy = new CompilerParameters();
            copy.IncludeDebugInformation = _compilerParams.IncludeDebugInformation;
            copy.TreatWarningsAsErrors = _compilerParams.TreatWarningsAsErrors;
            copy.WarningLevel = _compilerParams.WarningLevel;
            copy.CompilerOptions = _compilerParams.CompilerOptions;
            return copy;
        }

        private String[] CloneCompilerLanguages() {
            String[] compilerLanguages = new String[_compilerLanguages.Length]; 
            Array.Copy(_compilerLanguages, compilerLanguages, _compilerLanguages.Length);
            return compilerLanguages;
        }

        private String[] CloneCompilerExtensions() {
            String[] compilerExtensions = new String[_compilerExtensions.Length]; 
            Array.Copy(_compilerExtensions, compilerExtensions, _compilerExtensions.Length);
            return compilerExtensions;
        }

        internal CompilerParameters CompilerParams {
            get {
                return _compilerParams;
            }
        }

        // @
        internal IDictionary<string, string> ProviderOptions {
            get {
                return _providerOptions;
            }
        }
    }
}

