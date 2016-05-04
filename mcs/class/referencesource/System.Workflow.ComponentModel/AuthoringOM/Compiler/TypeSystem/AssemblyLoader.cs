namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.ComponentModel.Design;
    using System.IO;

    internal class AssemblyLoader
    {
        private Assembly assembly = null;
        private AssemblyName assemblyName = null;
        private TypeProvider typeProvider = null;
        private bool isLocalAssembly;

        internal AssemblyLoader(TypeProvider typeProvider, string filePath)
        {
            this.isLocalAssembly = false;
            this.typeProvider = typeProvider;
            if (File.Exists(filePath))
            {
                AssemblyName asmName = AssemblyName.GetAssemblyName(filePath);
                if (asmName != null)
                {
                    // Try loading the assembly using type resolution service first.
                    ITypeResolutionService trs = (ITypeResolutionService)typeProvider.GetService(typeof(ITypeResolutionService));
                    if (trs != null)
                    {
                        try
                        {
                            this.assembly = trs.GetAssembly(asmName);
                            // 

                            if (this.assembly == null && asmName.GetPublicKeyToken() != null && (asmName.GetPublicKeyToken().GetLength(0) == 0) && asmName.GetPublicKey() != null && (asmName.GetPublicKey().GetLength(0) == 0))
                            {
                                AssemblyName partialName = (AssemblyName)asmName.Clone();
                                partialName.SetPublicKey(null);
                                partialName.SetPublicKeyToken(null);
                                this.assembly = trs.GetAssembly(partialName);
                            }
                        }
                        catch
                        {
                            // Eat up any exceptions!
                        }
                    }

                    // If type resolution service wasn't available or it failed use Assembly.Load
                    if (this.assembly == null)
                    {
                        try
                        {
                            if (MultiTargetingInfo.MultiTargetingUtilities.IsFrameworkReferenceAssembly(filePath))
                            {
                                this.assembly = Assembly.Load(asmName.FullName);
                            }
                            else
                            {
                                this.assembly = Assembly.Load(asmName);
                            }
                        }
                        catch
                        {
                            // Eat up any exceptions!
                        }
                    }
                }
                // If Assembly.Load also failed, use Assembly.LoadFrom
                if (this.assembly == null)
                {
                    this.assembly = Assembly.LoadFrom(filePath);
                }
            }
            else
            {
                // TypeProvider will handle this and report the error
                throw new FileNotFoundException();
            }
        }

        internal AssemblyLoader(TypeProvider typeProvider, Assembly assembly, bool isLocalAssembly)
        {
            this.isLocalAssembly = isLocalAssembly;
            this.typeProvider = typeProvider;
            this.assembly = assembly;
        }

        internal Type GetType(string typeName)
        {
            // 
            if (this.assembly != null)
            {
                Type type = null;
                try
                {
                    type = this.assembly.GetType(typeName);
                }
                catch (ArgumentException)
                {
                    // we eat these exeptions in our type system
                }
                if ((type != null) && (type.IsPublic || type.IsNestedPublic || (this.isLocalAssembly && type.Attributes != TypeAttributes.NestedPrivate)))
                    return type;
            }
            return null;
        }

        internal Type[] GetTypes()
        {
            List<Type> filteredTypes = new List<Type>();
            if (this.assembly != null)
            {
                // 
                foreach (Type type in this.assembly.GetTypes())
                {
                    // 
                    if (type.IsPublic || (this.isLocalAssembly && type.Attributes != TypeAttributes.NestedPrivate))
                        filteredTypes.Add(type);
                }
            }
            return filteredTypes.ToArray();
        }

        // we cache the AssemblyName as Assembly.GetName() is expensive
        internal AssemblyName AssemblyName
        {
            get
            {
                if (this.assemblyName == null)
                    this.assemblyName = this.assembly.GetName(true);

                return this.assemblyName;
            }
        }

        internal Assembly Assembly
        {
            get
            {
                return this.assembly;
            }
        }
    }
}
