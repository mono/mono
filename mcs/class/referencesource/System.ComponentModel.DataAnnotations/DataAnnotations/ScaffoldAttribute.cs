#if !SILVERLIGHT
using System.Diagnostics.CodeAnalysis;

namespace System.ComponentModel.DataAnnotations {
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "We want users to be able to extend this class")]
    public class ScaffoldColumnAttribute : Attribute {
        public bool Scaffold { get; private set; }

        public ScaffoldColumnAttribute(bool scaffold) {
            Scaffold = scaffold;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "We want users to be able to extend this class")]
    public class ScaffoldTableAttribute : Attribute {
        public bool Scaffold { get; private set; }

        public ScaffoldTableAttribute(bool scaffold) {
            Scaffold = scaffold;
        }
    }
}
#endif
