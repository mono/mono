#if CODEPLEX_40
using System;
#else
using System; using Microsoft;
#endif

namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Indicates that a method is an extension method, or that a class or assembly contains extension methods.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly)]
    public sealed class ExtensionAttribute : Attribute { }
}
