using System;

namespace System.Runtime.CompilerServices
{
    /// <summary>
    /// Indicates that a method is an extension method, or that a class or assembly contains extension methods.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly)]
    public sealed class ExtensionAttribute : Attribute { }
}
