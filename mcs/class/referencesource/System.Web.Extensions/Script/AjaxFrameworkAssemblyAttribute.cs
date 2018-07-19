namespace System.Web.Script {
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;

    [
    AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false),
    SuppressMessage("Microsoft.Performance","CA1813:AvoidUnsealedAttributes", Justification="Intentially allowing for ajax assemblies to have a custom attribute type in order to override the virtual members. Result of looking for this attribute is cached.")
    ]
    public class AjaxFrameworkAssemblyAttribute : Attribute {
        protected internal virtual Assembly GetDefaultAjaxFrameworkAssembly(Assembly currentAssembly) {
            return currentAssembly;
        }
    }
}
