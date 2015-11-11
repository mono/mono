using System;
using System.IO;
using System.Reflection;

namespace System.Runtime.Loader
{
    public static class AssemblyExtensions
    {
        public static Type[] GetTypes(this Assembly assembly)
        {
            return assembly.GetTypes ();
        }
    }
}
