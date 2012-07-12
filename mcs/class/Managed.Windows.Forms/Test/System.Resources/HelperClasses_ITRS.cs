using System;
using System.ComponentModel.Design;
using System.Reflection;

namespace MonoTests.System.Resources {
    public class DummyTypeResolutionService : ITypeResolutionService {
        public Assembly GetAssembly (AssemblyName name, bool throwOnError)
        {
            return null;
        }

        public Assembly GetAssembly (AssemblyName name)
        {
            return null;
        }

        public string GetPathOfAssembly (AssemblyName name)
        {
            return null;
        }

        public Type GetType (string name, bool throwOnError, bool ignoreCase)
        {
            return null;
        }

        public Type GetType (string name, bool throwOnError)
        {
            return null;
        }

        public Type GetType (string name)
        {
            return null;
        }

        public void ReferenceAssembly (AssemblyName name)
        {

        }
    }

    public class AlwaysReturnSerializableSubClassTypeResolutionService : ITypeResolutionService {
        public Assembly GetAssembly (AssemblyName name, bool throwOnError)
        {
            throw new NotImplementedException ("I was accessed");
        }

        public Assembly GetAssembly (AssemblyName name)
        {
            throw new NotImplementedException ("I was accessed");
        }

        public string GetPathOfAssembly (AssemblyName name)
        {
            throw new NotImplementedException ("I was accessed");
        }

        public Type GetType (string name, bool throwOnError, bool ignoreCase)
        {
            return typeof (serializableSubClass);
        }

        public Type GetType (string name, bool throwOnError)
        {
            return typeof (serializableSubClass);
        }

        public Type GetType (string name)
        {
            return typeof (serializableSubClass);
        }

        public void ReferenceAssembly (AssemblyName name)
        {

        }

    }

    public class AlwaysReturnIntTypeResolutionService : ITypeResolutionService {
        public Assembly GetAssembly (AssemblyName name, bool throwOnError)
        {
            throw new NotImplementedException ("I was accessed");
        }

        public Assembly GetAssembly (AssemblyName name)
        {
            throw new NotImplementedException ("I was accessed");
        }

        public string GetPathOfAssembly (AssemblyName name)
        {
            throw new NotImplementedException ("I was accessed");
        }

        public Type GetType (string name, bool throwOnError, bool ignoreCase)
        {
            return typeof (Int32);
        }

        public Type GetType (string name, bool throwOnError)
        {
            return typeof (Int32);
        }

        public Type GetType (string name)
        {
            return typeof (Int32);
        }

        public void ReferenceAssembly (AssemblyName name)
        {

        }
    }

    public class ExceptionalTypeResolutionService : ITypeResolutionService {
        public Assembly GetAssembly (AssemblyName name, bool throwOnError)
        {
            throw new NotImplementedException ("I was accessed");
        }

        public Assembly GetAssembly (AssemblyName name)
        {
            throw new NotImplementedException ("I was accessed");
        }

        public string GetPathOfAssembly (AssemblyName name)
        {
            throw new NotImplementedException ("I was accessed");
        }

        public Type GetType (string name, bool throwOnError, bool ignoreCase)
        {
            throw new NotImplementedException ("I was accessed");
        }

        public Type GetType (string name, bool throwOnError)
        {
            throw new NotImplementedException ("I was accessed");
        }

        public Type GetType (string name)
        {
            throw new NotImplementedException ("I was accessed");
        }

        public void ReferenceAssembly (AssemblyName name)
        {

        }
    }

}
