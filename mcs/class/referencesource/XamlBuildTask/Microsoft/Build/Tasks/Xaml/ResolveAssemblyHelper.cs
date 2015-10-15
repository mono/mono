//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Build.Tasks.Xaml
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Reflection;
    using System.IO;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;

    internal class ResolveAssemblyHelper
    {
        static string fileNotfound = String.Empty;
        IList<string> references;

        public ResolveAssemblyHelper(IList<string> references)
        {
            this.references = references;
        }

        public Dictionary<string, Assembly> ReferencedAssemblies
        { 
            get;
            internal set; 
        }

        // We are storing the file not found in this static variable so that
        // later on we can print out the file name for the
        // filenotfound exception thrown by CLR. 
        // Ok to use static here as there will be only one instance of this 
        // process per app domain.
        internal static string FileNotFound
        {
            get { return fileNotfound; }
        }

        [SuppressMessage(FxCop.Category.Reliability, FxCop.Rule.AvoidCallingProblematicMethods,
            Justification = "Using LoadFrom to avoid loading through Fusion and load from the exact path specified")]
        public Assembly ResolveLocalProjectReferences(object sender, ResolveEventArgs args)
        {
            // Currently we are return the assembly just by matching the short name
            // of the assembly. Filed 



            AssemblyName targetAssemblyName = new AssemblyName(args.Name);
            string targetName = targetAssemblyName.Name;
            Assembly targetAssembly = null;
            if (this.ReferencedAssemblies != null)
            {
                this.ReferencedAssemblies.TryGetValue(targetName, out targetAssembly);
            }

            if (targetAssembly != null)
            {
                return targetAssembly;
            }

            foreach (string reference in this.references)
            {
                if (string.Equals(targetName, Path.GetFileNameWithoutExtension(reference), StringComparison.OrdinalIgnoreCase))
                {
                    Assembly assembly = Assembly.LoadFrom(reference);
                    if (assembly != null)
                    {
                        targetAssembly = assembly;
                        if (this.ReferencedAssemblies == null)
                        {
                            this.ReferencedAssemblies = new Dictionary<string, Assembly>(StringComparer.OrdinalIgnoreCase);
                        }
                        this.ReferencedAssemblies.Add(targetName, assembly);
                    }
                    break;
                }
            }
            if (targetAssembly == null)
            {
                fileNotfound = targetName;
            }
            return targetAssembly;
        }
    }
}
