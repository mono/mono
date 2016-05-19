using System;
using System.Collections.Generic;
using System.Text;
using System.Xaml;
using System.Reflection;
using System.Runtime;
using XamlBuildTask;

namespace Microsoft.Build.Tasks.Xaml
{
    class XamlNsReplacingContext : XamlSchemaContext
    {
        string localAssemblyName;
        string realAssemblyName;
        IDictionary<Type, XamlNsReplacingType> MasterTypeList;

        public XamlNsReplacingContext(IEnumerable<Assembly> referenceAssemblies, string localAssemblyName, string realAssemblyName)
            : base(referenceAssemblies)
        {
            this.localAssemblyName = localAssemblyName;
            this.realAssemblyName = realAssemblyName;
            MasterTypeList = new Dictionary<Type, XamlNsReplacingType>();
        }
        
        public override XamlType GetXamlType(Type type)
        {
            if (type == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("type"));
            }
            XamlNsReplacingType xamlType = null;
            if (!MasterTypeList.TryGetValue(type, out xamlType))
            {
                xamlType = new XamlNsReplacingType(type, this, localAssemblyName, realAssemblyName);
                MasterTypeList.Add(type, xamlType);
            }
            return xamlType;
        }

        protected override XamlType GetXamlType(string xamlNamespace, string name, params XamlType[] typeArguments)
        {
            XamlType xamlType = base.GetXamlType(xamlNamespace, name, typeArguments);
            if (xamlType == null || xamlType.IsUnknown)
            {
                xamlNamespace = XamlBuildTaskServices.UpdateClrNamespaceUriWithLocalAssembly(xamlNamespace, this.localAssemblyName, this.realAssemblyName);
                xamlType = base.GetXamlType(xamlNamespace, name, typeArguments);
            }
            else if (!xamlType.UnderlyingType.Assembly.ReflectionOnly &&
                xamlType.UnderlyingType.Assembly != typeof(object).Assembly)
            {
                // Types from XamlLanguage are live; but we want the ROL equivalent, so that we can validate
                // against expected member types. We do this by looking it up via its clr-namespace form.
                // Note this means that the resulting XamlType will only have its clr-namespace, not the XAML2006 namespace.
                IList<string> namespaces = xamlType.GetXamlNamespaces();
                Fx.Assert(namespaces.Contains(XamlLanguage.Xaml2006Namespace) && xamlType.TypeArguments == null,
                    "This should only happen for XamlLanguage types, none of which are generic");
                string clrNamespace = namespaces[namespaces.Count - 1];
                XamlType rolType = base.GetXamlType(clrNamespace, xamlType.UnderlyingType.Name);
                if (rolType != null)
                {
                    xamlType = rolType;
                }
            }
            return xamlType;
        }
    }

    class XamlNsReplacingType : XamlType
    {
        string localAssemblyName;
        string realAssemblyName;
        List<string> namespaces;

        public XamlNsReplacingType(Type underlyingType, XamlSchemaContext context, string localAssemblyName, string realAssemblyName)
            : base(underlyingType, context)
        {
            this.localAssemblyName = localAssemblyName;
            this.realAssemblyName = realAssemblyName;
            namespaces = null;
        }

        public override IList<string> GetXamlNamespaces()
        {
            if (namespaces == null)
            {
                namespaces = new List<string>();
                IList<string> originalNamespaces = base.GetXamlNamespaces();

                foreach (var ns in originalNamespaces)
                {
                    namespaces.Add(XamlBuildTaskServices.UpdateClrNamespaceUriWithLocalAssembly(ns, this.localAssemblyName, this.realAssemblyName));
                }
            }
            return namespaces;
        }
    }
}
