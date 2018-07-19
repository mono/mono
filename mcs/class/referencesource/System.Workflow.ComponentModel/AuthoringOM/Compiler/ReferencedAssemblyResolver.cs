namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Reflection;
    using System.Collections.Specialized;
    using System.ComponentModel.Design;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Design;

    #region Class Assembly resolver

    // In the cases where the WorkflowCompiler is invoked directly, we need to deserialize the
    // xoml before we can create the real TypeProvider, hence it is necessary to look at the
    // referenced assemblies for any types that fail to load. In the VS scenarios, the TypeProvider
    // has already been created and the WorkflowMarkupSerializer will use it first.
    internal sealed class ReferencedAssemblyResolver
    {
        private StringCollection referencedAssemblies = new StringCollection();
        private Assembly localAssembly;

        private bool resolving = false;

        public ReferencedAssemblyResolver(StringCollection referencedAssemblies, Assembly localAssembly)
        {
            this.referencedAssemblies = referencedAssemblies;
            this.localAssembly = localAssembly;
        }

        public Assembly ResolveEventHandler(object sender, ResolveEventArgs args)
        {
            return ResolveAssembly(args.Name);
        }

        internal void SetLocalAssembly(Assembly localAsm)
        {
            this.localAssembly = localAsm;
        }

        private Assembly ResolveAssembly(string name)
        {
            if (this.resolving)
                return null;

            // First look for the local assembly.
            if (this.localAssembly != null && name == this.localAssembly.FullName)
                return this.localAssembly;

            try
            {
                this.resolving = true;

                AssemblyName assemblyName = new AssemblyName(name);

                // Then try the referenced assemblies.
                foreach (string assemblyPath in this.referencedAssemblies)
                {
                    try
                    {
                        AssemblyName referenceAssemblyName = AssemblyName.GetAssemblyName(assemblyPath);
                        if (referenceAssemblyName != null && ParseHelpers.AssemblyNameEquals(referenceAssemblyName, assemblyName))
                        {
                            Assembly reference = null;
                            try
                            {
                                reference = Assembly.Load(referenceAssemblyName);
                            }
                            catch
                            {
                                reference = Assembly.LoadFrom(assemblyPath);
                            }
                            return reference;
                        }
                    }
                    catch
                    {
                        // Eat up any exceptions!
                    }

                }
            }
            finally
            {
                this.resolving = false;
            }
            return null;
        }
    }

    #endregion
}
