//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Hosting
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Runtime;
    using System.Diagnostics.CodeAnalysis;
    using System.Activities.Presentation.Hosting;
    using System.IO;
    using System.Linq;

    //This class is required by the TypeBrowser - it allows browsing defined types either in VS scenario or in
    //rehosted scenario. The types are divided into two categories - types defined in local assembly (i.e. the one 
    //contained in current project - for that assembly, types are loaded using GetTypes() method), and all other
    //referenced types - for them, type list is loaded using GetExportedTypes() method.
    //
    //if this object is not set in desinger's Items collection or both members are null, the type 
    //browser will not display "Browse for types" option.
    [Fx.Tag.XamlVisible(false)]
    public sealed class AssemblyContextControlItem : ContextItem
    {
        public AssemblyName LocalAssemblyName
        { get; set; }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification = "This is by design")]
        public IList<AssemblyName> ReferencedAssemblyNames
        {
            get;
            set;
        }

        public override Type ItemType
        {
            get { return typeof(AssemblyContextControlItem); }
        }

        public IEnumerable<string> AllAssemblyNamesInContext
        {
            get
            {
                if ((LocalAssemblyName != null) && LocalAssemblyName.CodeBase != null && (File.Exists(new Uri(LocalAssemblyName.CodeBase).LocalPath)))
                {
                    yield return LocalAssemblyName.FullName;
                }
                foreach (AssemblyName assemblyName in GetEnvironmentAssemblyNames())
                {
                    //avoid returning local name twice
                    if (LocalAssemblyName == null || !assemblyName.FullName.Equals(LocalAssemblyName.FullName, StringComparison.Ordinal))
                    {
                        yield return assemblyName.FullName;
                    }
                }

            }
        }

        public IEnumerable<AssemblyName> GetEnvironmentAssemblyNames()
        {
            if (this.ReferencedAssemblyNames != null)
            {
                return this.ReferencedAssemblyNames;
            }
            else
            {
                List<AssemblyName> assemblyNames = new List<AssemblyName>();
                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (!assembly.IsDynamic)
                    {
                        assemblyNames.Add(assembly.GetName());
                    }
                }
                return assemblyNames;
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", 
            Justification = "Multi-Targeting makes sense")]
        public IEnumerable<Assembly> GetEnvironmentAssemblies (IMultiTargetingSupportService multiTargetingService)
        {
            if (this.ReferencedAssemblyNames == null)
            {
                return AppDomain.CurrentDomain.GetAssemblies().Where<Assembly>(assembly => !assembly.IsDynamic);
            }
            else
            {
                List<Assembly> assemblies = new List<Assembly>();
                foreach (AssemblyName assemblyName in this.ReferencedAssemblyNames)
                {
                    Assembly assembly = GetAssembly(assemblyName, multiTargetingService);
                    if (assembly != null)
                    {
                        assemblies.Add(assembly);
                    }
                }
                return assemblies;
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            Justification = "Multi-Targeting makes sense")]
        public static Assembly GetAssembly(AssemblyName assemblyName, IMultiTargetingSupportService multiTargetingService)
        {
            Assembly assembly = null;
            try
            {
                if (multiTargetingService != null)
                {
                    assembly = multiTargetingService.GetReflectionAssembly(assemblyName);
                }
                else
                {
                    assembly = Assembly.Load(assemblyName);
                }
            }  
            catch (FileNotFoundException)
            {
                //this exception may occur if current project is not compiled yet
            }
            catch (FileLoadException)
            {
                //the assembly could not be loaded, ignore the error
            }
            catch (BadImageFormatException)
            {
                //bad assembly
            }
            return assembly;
        }
    }
}
