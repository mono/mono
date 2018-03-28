// Copyright (c) Microsoft Corporation. All rights reserved. 
//  
// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// WHETHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED 
// WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
// THE ENTIRE RISK OF USE OR RESULTS IN CONNECTION WITH THE USE OF THIS CODE 
// AND INFORMATION REMAINS WITH THE USER. 
//  


/*********************************************************************
 * NOTE: A copy of this file exists at: WF\Common\Shared
 * The two files must be kept in sync.  Any change made here must also
 * be made to WF\Common\Shared\CompilerHelpers.cs
*********************************************************************/

namespace System.Workflow.Activities.Common
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Collections.Generic;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Workflow.ComponentModel;
    using Microsoft.CSharp;
    using Microsoft.VisualBasic;
    using System.Reflection;
    using Microsoft.Win32;
    using System.Security;
    using System.ComponentModel;
    using System.IO;
    using System.Diagnostics.CodeAnalysis;
    using System.Workflow.ComponentModel.Compiler;

    internal enum SupportedLanguages
    {
        VB,
        CSharp
    }

    internal static class CompilerHelpers
    {
        private const string CompilerVersionKeyword = "CompilerVersion";

        private static Dictionary<Type, Dictionary<string, CodeDomProvider>> providers = null;
        private static object providersLock = new object();

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static CodeDomProvider CreateCodeProviderInstance(Type type)
        {
            return CreateCodeProviderInstance(type, string.Empty);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static CodeDomProvider CreateCodeProviderInstance(Type type, string compilerVersion)
        {
            CodeDomProvider provider = null;
            if (string.IsNullOrEmpty(compilerVersion))
            {
                if (type == typeof(CSharpCodeProvider))
                    provider = new CSharpCodeProvider();
                else if (type == typeof(VBCodeProvider))
                    provider = new VBCodeProvider();
                else
                    provider = (CodeDomProvider)Activator.CreateInstance(type);
            }
            else
            {
                //otherwise pass the compiler version parameter into it
                Dictionary<string, string> options = new Dictionary<string, string>();
                options.Add(CompilerHelpers.CompilerVersionKeyword, compilerVersion);
                provider = (CodeDomProvider)Activator.CreateInstance(type, new object[] { options });
            }

            return provider;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        static CodeDomProvider GetCodeProviderInstance(Type type, string compilerVersion)
        {
            CodeDomProvider provider;
            lock (providersLock)
            {
                if (providers == null)
                {
                    providers = new Dictionary<Type, Dictionary<string, CodeDomProvider>>();
                }

                Dictionary<string, CodeDomProvider> typedProviders;
                if (!providers.TryGetValue(type, out typedProviders))
                {
                    typedProviders = new Dictionary<string, CodeDomProvider>();
                    providers.Add(type, typedProviders);
                }

                if (!typedProviders.TryGetValue(compilerVersion, out provider))
                {
                    provider = CreateCodeProviderInstance(type, compilerVersion);
                    typedProviders.Add(compilerVersion, provider);
                }
            }

            return provider;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static CodeDomProvider GetCodeDomProvider(SupportedLanguages language)
        {
            return CompilerHelpers.GetCodeDomProvider(language, string.Empty);
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static CodeDomProvider GetCodeDomProvider(SupportedLanguages language, string compilerVersion)
        {
            if (language == SupportedLanguages.CSharp)
            {
                return GetCodeProviderInstance(typeof(CSharpCodeProvider), compilerVersion);
            }
            else
            {
                return GetCodeProviderInstance(typeof(VBCodeProvider), compilerVersion);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static SupportedLanguages GetSupportedLanguage(IServiceProvider serviceProvider)
        {
            SupportedLanguages supportedLanguage = SupportedLanguages.CSharp;
            IWorkflowCompilerOptionsService workflowCompilerOptions = serviceProvider.GetService(typeof(IWorkflowCompilerOptionsService)) as IWorkflowCompilerOptionsService;
            if (workflowCompilerOptions != null)
                supportedLanguage = GetSupportedLanguage(workflowCompilerOptions.Language);
            return supportedLanguage;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal static SupportedLanguages GetSupportedLanguage(string language)
        {
            SupportedLanguages supportedLanguage = SupportedLanguages.CSharp;
            if (!String.IsNullOrEmpty(language) &&
                (string.Compare(language, "VB", StringComparison.OrdinalIgnoreCase) == 0 ||
                string.Compare(language, "VisualBasic", StringComparison.OrdinalIgnoreCase) == 0))
                supportedLanguage = SupportedLanguages.VB;
            return supportedLanguage;
        }
    }
}
