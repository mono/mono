using System;

namespace System.Diagnostics.CodeAnalysis
{
    /// <summary>
    /// Specifies that the attributed code should be excluded from code coverage
    /// collection.  Placing this attribute on a class/struct excludes all
    /// enclosed methods and properties from code coverage collection.
    /// </summary>
    [AttributeUsage(
        AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Constructor | AttributeTargets.Event | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Struct,
        Inherited = false,
        AllowMultiple = false
    )]
    public sealed class ExcludeFromCodeCoverageAttribute : Attribute
    {
        public ExcludeFromCodeCoverageAttribute()
        {
        }
    }
}
