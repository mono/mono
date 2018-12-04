using System;
using System.Xaml;
using System.Xaml.Schema;

namespace DrtXaml
{
    internal static class ExtensionMethods
    {
        internal static XamlType GetXamlType(
            this XamlSchemaContext schemaContext, string xamlNamespace, string name)
        {
            XamlTypeName typeName = new XamlTypeName(xamlNamespace, name);
            return schemaContext.GetXamlType(typeName);
        }

        internal static string GetAssemblyName(this Type type)
        {
            return type.Assembly.GetName().Name;
        }
    }
}
