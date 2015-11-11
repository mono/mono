using System;
using System.IO;
using System.Reflection;

namespace System.Runtime.Loader
{
    public abstract class AssemblyLoadContext
    {
        protected abstract Assembly Load(AssemblyName assemblyName);

        protected Assembly LoadFromAssemblyPath(string assemblyPath)
        {
            if (assemblyPath == null)
                throw new ArgumentNullException("assemblyPath");

            if (!Path.IsPathRooted(assemblyPath))
                throw new ArgumentException("Gimme an absolute path " + assemblyPath + " XXX " + Path.GetPathRoot(assemblyPath), "assemblyPath");

            return Assembly.LoadFrom (assemblyPath);
        }

        public Assembly LoadFromAssemblyName(AssemblyName assemblyName)
        {
            // AssemblyName is mutable. Cache the expected name before anybody gets a chance to modify it.
            string requestedSimpleName = assemblyName.Name;

            Assembly assembly = Load(assemblyName);
            if (assembly == null)
                throw new FileLoadException("File not found", requestedSimpleName);

            return assembly;
        }

        public static AssemblyName GetAssemblyName(string assemblyPath)
        {
            if (!File.Exists (assemblyPath))
                throw new Exception ("file not found");
            return new AssemblyName (Path.GetFileName (assemblyPath));
        }
    }
}
