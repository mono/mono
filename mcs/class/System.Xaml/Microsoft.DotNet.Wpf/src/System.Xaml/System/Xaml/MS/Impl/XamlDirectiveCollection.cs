using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Markup;
using System.Collections.ObjectModel;
using System.Text;

#if SILVERLIGHTXAML
using MS.Internal.Xaml.Schema;
#else
using System.Xaml.Schema;
#endif

#if SILVERLIGHTXAML
namespace MS.Internal.Xaml.MS.Impl
#else
namespace System.Xaml.MS.Impl
#endif   
{
    internal class XamlDirectiveCollection
    {
        Dictionary<string, DirectiveProperty> _properties;
        Dictionary<string, XamlType> _elements;

        public XamlDirectiveCollection(XamlSchemaContext context, DirectiveInfo[] infos)
        {
            _properties = new Dictionary<string, DirectiveProperty>();
            _elements = new Dictionary<string, XamlType>();
            if (infos != null)
            {
                foreach (DirectiveInfo info in infos)
                {
                    if (info is DirectivePropertyInfo)
                    {
                        DirectivePropertyInfo dpi = info as DirectivePropertyInfo;
                        DirectiveProperty dirProperty = new DirectiveProperty(context, dpi);
                        _properties.Add(dpi.Name, dirProperty);
                    }
                    if (info is DirectiveTypeInfo)
                    {
                        DirectiveTypeInfo dti = info as DirectiveTypeInfo;
                        XamlType xamlType = new XamlType(dti.Type, context);
                        _elements.Add(dti.Name, xamlType);
                    }
                }
            }
        }

        public XamlProperty GetDirectiveProperty(string name)
        {
            DirectiveProperty dirProperty;
            _properties.TryGetValue(name, out dirProperty);
            return dirProperty;
        }

        public XamlType GetDirectiveElement(string name)
        {
            XamlType dirType;
            _elements.TryGetValue(name, out dirType);
            return dirType;
        }
    }

    internal class DirectiveInfo
    {
        public readonly string Name;
        public readonly Type Type;

        public DirectiveInfo(string name, Type type)
        {
            Name = name;
            Type = type;
        }
    }

    [DebuggerDisplay("{Name} typeof({Type.Name} Loc:{AllowedLocation})")]
    internal class DirectivePropertyInfo : DirectiveInfo
    {
        public readonly XamlTextSyntax TextSyntax;
        public readonly AllowedMemberLocation AllowedLocation;
        public readonly IList<string> NamespaceList;

        public DirectivePropertyInfo(string name, Type type, XamlTextSyntax ts, AllowedMemberLocation location, IList<string> namespaceList)
            :base(name, type)
        {
            TextSyntax = ts;
            AllowedLocation = location;
            NamespaceList = namespaceList;
        }
    }

    internal class DirectiveTypeInfo : DirectiveInfo
    {
        public DirectiveTypeInfo(Type type)
            : base(type.Name, type) { }
        public DirectiveTypeInfo(string name, Type type) : base(name, type) { }
    }

    internal static class Xaml2006Directives
    {
        public const string PreferredPrefix = "x";
        public const string Uri = "http://schemas.microsoft.com/winfx/2006/xaml";
        public static ReadOnlyCollection<string> NamespaceList = new ReadOnlyCollection<string>(new Collection<string>() { Xaml2006Directives.Uri });

        // Properties
        public const string x_TypeArguments = "TypeArguments";
        public const string x_Class = "Class";
        public const string x_Key = "Key";
        public const string x_Name = "Name";
        public const string x_Uid = "Uid";
        public const string x_SynchronousMode = "SynchronousMode";
        public const string x_ClassModifier = "ClassModifier";
        public const string x_FieldModifier = "FieldModifier";
        public const string x_FactoryMethod = "FactoryMethod";
        public const string x_Arguments = "Arguments";
        public const string x_ConnectionId = "ConnectionId";
        public const string x_Shared = "Shared";
        public const string x_AsyncRecords = "AsyncRecords";

        // Elements
        public const string x_XData = "XData";

        // Types
        public const string x_Type = "Type";
        public const string x_Null = "Null";

        public static DirectiveInfo[] DirectiveInfoTable = new DirectiveInfo[] {
            // Properties
            new DirectivePropertyInfo(x_Class, typeof(string), XamlTextSyntax.StringSyntax,
                                    AllowedMemberLocation.Attribute, Xaml2006Directives.NamespaceList),

            new DirectivePropertyInfo(x_Key, typeof(string), XamlTextSyntax.StringSyntax,
                                    AllowedMemberLocation.Any, Xaml2006Directives.NamespaceList),

            new DirectivePropertyInfo(x_Name, typeof(string), XamlTextSyntax.StringSyntax,
                                    AllowedMemberLocation.Attribute, Xaml2006Directives.NamespaceList),

            new DirectivePropertyInfo(x_Uid, typeof(string), XamlTextSyntax.StringSyntax,
                                    AllowedMemberLocation.Attribute, Xaml2006Directives.NamespaceList),

            new DirectivePropertyInfo(x_SynchronousMode, typeof(string), XamlTextSyntax.StringSyntax,
                                    AllowedMemberLocation.Attribute, Xaml2006Directives.NamespaceList),

            new DirectivePropertyInfo(x_ClassModifier, typeof(string), XamlTextSyntax.StringSyntax,
                                    AllowedMemberLocation.Attribute, Xaml2006Directives.NamespaceList),

            new DirectivePropertyInfo(x_FieldModifier, typeof(string), XamlTextSyntax.StringSyntax,
                                    AllowedMemberLocation.Attribute, Xaml2006Directives.NamespaceList),

            new DirectivePropertyInfo(x_TypeArguments, typeof(string), XamlTextSyntax.StringSyntax,
                                    AllowedMemberLocation.Attribute, Xaml2006Directives.NamespaceList),

            new DirectivePropertyInfo(x_FactoryMethod, typeof(string), XamlTextSyntax.StringSyntax,
                                    AllowedMemberLocation.Any, Xaml2006Directives.NamespaceList),

            new DirectivePropertyInfo(x_Arguments, typeof(List<object>), XamlTextSyntax.NoSyntax,
                                    AllowedMemberLocation.MemberElement, Xaml2006Directives.NamespaceList),

            new DirectivePropertyInfo(x_ConnectionId, typeof(string), XamlTextSyntax.Int32Syntax,
                                    AllowedMemberLocation.Any, Xaml2006Directives.NamespaceList),

            new DirectivePropertyInfo(x_Shared, typeof(string), XamlTextSyntax.StringSyntax,
                                        AllowedMemberLocation.Attribute, Xaml2006Directives.NamespaceList),

            new DirectivePropertyInfo(x_AsyncRecords, typeof(string), XamlTextSyntax.Int32Syntax,
                                        AllowedMemberLocation.Attribute, Xaml2006Directives.NamespaceList),

            // Real CLR types added to this namespace.
            // **IMPORTANT**
            // All types listed in this datastructure also need to be listed in XamlTypeList below (and vice versa)
            new DirectiveTypeInfo(x_Type, typeof(System.Xaml.Replacements.TypeExtension)),
            new DirectiveTypeInfo(typeof(String)),
            new DirectiveTypeInfo(typeof(Int32)),
            new DirectiveTypeInfo(typeof(Boolean)),
            new DirectiveTypeInfo(typeof(Double)),
            new DirectiveTypeInfo(typeof(Reference)),
        };

        // Real CLR types added to this namespace.
        // **IMPORTANT**
        // All types listed in this datastructure also need to be listed in the bottom of the DirectiveInfoTable above (and vice versa).
        // We use Dictionary<K,T> since HashTable still requires a 2nd parameter to Add.  We actually don't need T (the object), so just pass in null.
        public static Dictionary<Type, object> XamlTypeList = new Dictionary<Type, object>() { 
            {typeof(System.Xaml.Replacements.TypeExtension),null},
            {typeof(String),null},
            {typeof(Int32),null},
            {typeof(Boolean),null},
            {typeof(Double), null},
            {typeof(Reference),null},
        };

    }

    internal static class XmlDirectives
    {
        public const string Uri = "http://www.w3.org/XML/1998/namespace";
        public static ReadOnlyCollection<string> NamespaceList = new ReadOnlyCollection<string>(new Collection<string>() { XmlDirectives.Uri }); 
        public const string xml_Space = "space";
        public const string xml_Lang = "lang";
        public const string xml_Base = "base";

        public static DirectiveInfo[] DirectiveInfoTable = new DirectiveInfo[] {
                new DirectivePropertyInfo(xml_Space, typeof(string), XamlTextSyntax.StringSyntax,
                                        AllowedMemberLocation.Attribute, XmlDirectives.NamespaceList),

                new DirectivePropertyInfo(xml_Lang, typeof(string), XamlTextSyntax.StringSyntax,
                                        AllowedMemberLocation.Attribute, XmlDirectives.NamespaceList),

                // xml:base appears in the NodeStream but never in XML text.
                new DirectivePropertyInfo(xml_Base, typeof(string), XamlTextSyntax.StringSyntax,
                                        AllowedMemberLocation.None, XmlDirectives.NamespaceList),
        };
    }
}
