// ****************************************************************
// Copyright 2007, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org/?p=license&r=2.4
// ****************************************************************
using System;
using System.Reflection;
using System.Collections;

namespace NUnit.Core.Extensibility
{
    public class FrameworkRegistry : IFrameworkRegistry
    {
        #region Instance Fields
        /// <summary>
        /// List of FrameworkInfo structs for supported frameworks
        /// </summary>
        private Hashtable testFrameworks = new Hashtable();
        #endregion

        #region IFrameworkRegistry Members
        /// <summary>
        /// Register a framework. NUnit registers itself using this method. Add-ins that
        /// work with or emulate a different framework may register themselves as well.
        /// </summary>
        /// <param name="frameworkName">The name of the framework</param>
        /// <param name="assemblyName">The name of the assembly that framework users reference</param>
        public void Register(string frameworkName, string assemblyName)
        {
            testFrameworks[frameworkName] = new TestFramework(frameworkName, assemblyName);
        }
		#endregion

		#region Other Methods
        /// <summary>
        /// Get a list of known frameworks referenced by an assembly
        /// </summary>
        /// <param name="assembly">The assembly to be examined</param>
        /// <returns>A list of AssemblyNames</returns>
        public IList GetReferencedFrameworks(Assembly assembly)
        {
            ArrayList referencedAssemblies = new ArrayList();

            foreach (AssemblyName assemblyRef in assembly.GetReferencedAssemblies())
            {
                foreach (TestFramework info in testFrameworks.Values)
                {
                    if (assemblyRef.Name == info.AssemblyName)
                    {
                        referencedAssemblies.Add(assemblyRef);
                        break;
                    }
                }
            }

            return referencedAssemblies;
        }
        #endregion
    }
}
