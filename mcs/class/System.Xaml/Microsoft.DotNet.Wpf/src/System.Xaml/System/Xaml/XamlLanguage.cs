// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Markup;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Threading;
using System.Xaml.Schema;
using System.Xaml.MS.Impl;
using System.Globalization;

namespace System.Xaml
{
    public static class XamlLanguage
    {
        public const string Xaml2006Namespace = "http://schemas.microsoft.com/winfx/2006/xaml";
        public const string Xml1998Namespace = "http://www.w3.org/XML/1998/namespace";
        internal const string SWMNamespace = "System.Windows.Markup";

        internal const string PreferredPrefix = "x";

        private const string x_AsyncRecords = "AsyncRecords";
        private const string x_Arguments = "Arguments";
        private const string x_Class = "Class";
        private const string x_ClassModifier = "ClassModifier";
        private const string x_Code = "Code";
        private const string x_ConnectionId = "ConnectionId";
        private const string x_FactoryMethod = "FactoryMethod";
        private const string x_FieldModifier = "FieldModifier";
        private const string x_Initialization = "_Initialization";
        private const string x_Items = "_Items";
        private const string x_Key = "Key";
        private const string x_Members = "Members";
        private const string x_ClassAttributes = "ClassAttributes";
        private const string x_Name = "Name";
        private const string x_PositionalParameters = "_PositionalParameters";
        private const string x_Shared = "Shared";
        private const string x_Subclass = "Subclass";
        private const string x_SynchronousMode = "SynchronousMode";
        private const string x_TypeArguments = "TypeArguments";
        private const string x_Uid = "Uid";
        private const string x_UnknownContent = "_UnknownContent";

        private const string xml_Space = "space";
        private const string xml_Lang = "lang";
        private const string xml_Base = "base";

        private static ReadOnlyCollection<string> s_xamlNamespaces =
            new ReadOnlyCollection<string>(new string[] { Xaml2006Namespace });
        private static ReadOnlyCollection<string> s_xmlNamespaces =
            new ReadOnlyCollection<string>(new string[] { Xml1998Namespace });

        private static Lazy<XamlSchemaContext> s_schemaContext =
            new Lazy<XamlSchemaContext>(GetSchemaContext);

        private static Lazy<XamlType> s_array =
            new Lazy<XamlType>(() => GetXamlType(typeof(ArrayExtension)));
        private static Lazy<XamlType> s_null =
            new Lazy<XamlType>(() => GetXamlType(typeof(NullExtension)));
        private static Lazy<XamlType> s_reference =
            new Lazy<XamlType>(() => GetXamlType(typeof(Reference)));
        private static Lazy<XamlType> s_static =
            new Lazy<XamlType>(() => GetXamlType(typeof(StaticExtension)));
        private static Lazy<XamlType> s_type =
            new Lazy<XamlType>(() => GetXamlType(typeof(TypeExtension)));
        private static Lazy<XamlType> s_string =
            new Lazy<XamlType>(() => GetXamlType(typeof(string)));
        private static Lazy<XamlType> s_double =
            new Lazy<XamlType>(() => GetXamlType(typeof(double)));
        private static Lazy<XamlType> s_int32 =
            new Lazy<XamlType>(() => GetXamlType(typeof(int)));
        private static Lazy<XamlType> s_boolean =
            new Lazy<XamlType>(() => GetXamlType(typeof(bool)));
        private static Lazy<XamlType> s_member =
            new Lazy<XamlType>(() => GetXamlType(typeof(MemberDefinition)));
        private static Lazy<XamlType> s_property =
            new Lazy<XamlType>(() => GetXamlType(typeof(PropertyDefinition)));
        private static Lazy<XamlType> s_xDataHolder =
            new Lazy<XamlType>(() => GetXamlType(typeof(XData)));
        // These aren't language built-ins, but we need them because they're used as the type of directives
        private static Lazy<XamlType> s_object =
            new Lazy<XamlType>(() => GetXamlType(typeof(object)));
        private static Lazy<XamlType> s_listOfObject =
            new Lazy<XamlType>(() => GetXamlType(typeof(List<object>)));
        private static Lazy<XamlType> s_listOfMembers =
            new Lazy<XamlType>(() => GetXamlType(typeof(List<MemberDefinition>)));
        private static Lazy<XamlType> s_listOfAttributes =
            new Lazy<XamlType>(() => GetXamlType(typeof(List<Attribute>)));
        // These aren't language built-ins, but we use them in schema
        private static Lazy<XamlType> s_markupExtension =
            new Lazy<XamlType>(() => GetXamlType(typeof(MarkupExtension)));
        private static Lazy<XamlType> s_iNameScope =
            new Lazy<XamlType>(() => GetXamlType(typeof(INameScope)));
        private static Lazy<XamlType> s_iXmlSerializable =
            new Lazy<XamlType>(() => GetXamlType(typeof(System.Xml.Serialization.IXmlSerializable)), true);
        // This isn't a language built-in, but we use it in ObjectWriter
        private static Lazy<XamlType> s_positionalParameterDescriptor =
            new Lazy<XamlType>(() => GetXamlType(typeof(PositionalParameterDescriptor)), true);

        private static Lazy<XamlType> s_char =
            new Lazy<XamlType>(() => GetXamlType(typeof(Char)), true);
        private static Lazy<XamlType> s_single =
            new Lazy<XamlType>(() => GetXamlType(typeof(Single)), true);
        private static Lazy<XamlType> s_byte =
            new Lazy<XamlType>(() => GetXamlType(typeof(Byte)), true);
        private static Lazy<XamlType> s_int16 =
            new Lazy<XamlType>(() => GetXamlType(typeof(Int16)), true);
        private static Lazy<XamlType> s_int64 =
            new Lazy<XamlType>(() => GetXamlType(typeof(Int64)), true);
        private static Lazy<XamlType> s_decimal =
            new Lazy<XamlType>(() => GetXamlType(typeof(Decimal)), true);
        private static Lazy<XamlType> s_uri =
            new Lazy<XamlType>(() => GetXamlType(typeof(Uri)), true);
        private static Lazy<XamlType> s_timespan =
            new Lazy<XamlType>(() => GetXamlType(typeof(TimeSpan)), true);

        private static Lazy<ReadOnlyCollection<XamlType>> s_allTypes =
            new Lazy<ReadOnlyCollection<XamlType>>(GetAllTypes);

        private static Lazy<XamlDirective> s_asyncRecords =
            new Lazy<XamlDirective>(() => GetXamlDirective(x_AsyncRecords,
                String, BuiltInValueConverter.Int32, AllowedMemberLocations.Attribute), true);
        private static Lazy<XamlDirective> s_arguments =
            new Lazy<XamlDirective>(() => GetXamlDirective(x_Arguments, 
                s_listOfObject.Value, null, AllowedMemberLocations.Any), true);
        private static Lazy<XamlDirective> s_class =
            new Lazy<XamlDirective>(() => GetXamlDirective(x_Class));
        private static Lazy<XamlDirective> s_classModifier =
            new Lazy<XamlDirective>(() => GetXamlDirective(x_ClassModifier));
        private static Lazy<XamlDirective> s_code =
            new Lazy<XamlDirective>(() => GetXamlDirective(x_Code));
        private static Lazy<XamlDirective> s_connectionId =
            new Lazy<XamlDirective>(() => GetXamlDirective(x_ConnectionId, 
                s_string.Value, BuiltInValueConverter.Int32, AllowedMemberLocations.Any), true);
        private static Lazy<XamlDirective> s_factoryMethod =
            new Lazy<XamlDirective>(() => GetXamlDirective(x_FactoryMethod, 
                s_string.Value, BuiltInValueConverter.String, AllowedMemberLocations.Any), true);
        private static Lazy<XamlDirective> s_fieldModifier =
            new Lazy<XamlDirective>(() => GetXamlDirective(x_FieldModifier));
        private static Lazy<XamlDirective> s_items =
            new Lazy<XamlDirective>(() => GetXamlDirective(x_Items, 
                s_listOfObject.Value, null, AllowedMemberLocations.Any), true);
        private static Lazy<XamlDirective> s_initialization =
            new Lazy<XamlDirective>(() => GetXamlDirective(x_Initialization,
                s_object.Value, null, AllowedMemberLocations.Any), true);
        private static Lazy<XamlDirective> s_key =
            new Lazy<XamlDirective>(() => GetXamlDirective(x_Key, 
                s_object.Value, BuiltInValueConverter.String, AllowedMemberLocations.Any), true);
        private static Lazy<XamlDirective> s_members =
            new Lazy<XamlDirective>(() => GetXamlDirective(x_Members,
                s_listOfMembers.Value, null, AllowedMemberLocations.MemberElement), true);
        private static Lazy<XamlDirective> s_classAttributes =
            new Lazy<XamlDirective>(() => GetXamlDirective(x_ClassAttributes,
                s_listOfAttributes.Value, null, AllowedMemberLocations.MemberElement), true);
        private static Lazy<XamlDirective> s_name =
            new Lazy<XamlDirective>(() => GetXamlDirective(x_Name));
        private static Lazy<XamlDirective> s_positionalParameters =
            new Lazy<XamlDirective>(() => GetXamlDirective(x_PositionalParameters,
                s_listOfObject.Value, null, AllowedMemberLocations.Any), true);
        private static Lazy<XamlDirective> s_shared =
            new Lazy<XamlDirective>(() => GetXamlDirective(x_Shared), true);
        private static Lazy<XamlDirective> s_subclass =
            new Lazy<XamlDirective>(() => GetXamlDirective(x_Subclass), true);
        private static Lazy<XamlDirective> s_synchronousMode =
            new Lazy<XamlDirective>(() => GetXamlDirective(x_SynchronousMode));
        private static Lazy<XamlDirective> s_typeArguments =
            new Lazy<XamlDirective>(() => GetXamlDirective(x_TypeArguments));
        private static Lazy<XamlDirective> s_uid =
            new Lazy<XamlDirective>(() => GetXamlDirective(x_Uid));
        private static Lazy<XamlDirective> s_unknownContent =
            new Lazy<XamlDirective>(() => GetXamlDirective(x_UnknownContent, AllowedMemberLocations.MemberElement, MemberReflector.UnknownReflector), true);

        private static Lazy<XamlDirective> s_base =
            new Lazy<XamlDirective>(() => GetXmlDirective(xml_Base));
        private static Lazy<XamlDirective> s_lang =
            new Lazy<XamlDirective>(() => GetXmlDirective(xml_Lang));
        private static Lazy<XamlDirective> s_space =
            new Lazy<XamlDirective>(() => GetXmlDirective(xml_Space));

        private static Lazy<ReadOnlyCollection<XamlDirective>> s_allDirectives =
            new Lazy<ReadOnlyCollection<XamlDirective>>(GetAllDirectives);

        public static IList<string> XamlNamespaces { get { return s_xamlNamespaces; } }
        public static IList<string> XmlNamespaces { get { return s_xmlNamespaces; } }

        public static XamlType Array { get { return s_array.Value; } }
        public static XamlType Member { get { return s_member.Value; } }
        public static XamlType Null { get { return s_null.Value; } }
        public static XamlType Property { get { return s_property.Value; } }
        public static XamlType Reference { get { return s_reference.Value; } }
        public static XamlType Static { get { return s_static.Value; } }
        public static XamlType Type { get { return s_type.Value; } }
        public static XamlType String { get { return s_string.Value; } }
        public static XamlType Double { get { return s_double.Value; } }
        public static XamlType Int32 { get { return s_int32.Value; } }
        public static XamlType Boolean { get { return s_boolean.Value; } }
        public static XamlType XData { get { return s_xDataHolder.Value; } }
        
        public static XamlType Object { get { return s_object.Value; } }
        public static XamlType Char { get { return s_char.Value; } }
        public static XamlType Single { get { return s_single.Value; } }
        public static XamlType Byte { get { return s_byte.Value; } }
        public static XamlType Int16 { get { return s_int16.Value; } }
        public static XamlType Int64 { get { return s_int64.Value; } }
        public static XamlType Decimal { get { return s_decimal.Value; } }
        public static XamlType Uri { get { return s_uri.Value; } }
        public static XamlType TimeSpan { get { return s_timespan.Value; } }

        public static ReadOnlyCollection<XamlType> AllTypes { get { return s_allTypes.Value; } }

        public static XamlDirective Arguments { get { return s_arguments.Value; } }
        public static XamlDirective AsyncRecords { get { return s_asyncRecords.Value; } }
        public static XamlDirective Class { get { return s_class.Value; } }
        public static XamlDirective ClassModifier { get { return s_classModifier.Value; } }
        public static XamlDirective Code { get { return s_code.Value; } }
        public static XamlDirective ConnectionId { get { return s_connectionId.Value; } }
        public static XamlDirective FactoryMethod { get { return s_factoryMethod.Value; } }
        public static XamlDirective FieldModifier { get { return s_fieldModifier.Value; } }
        public static XamlDirective Items { get { return s_items.Value; } }
        public static XamlDirective Initialization { get { return s_initialization.Value; } }
        public static XamlDirective Key { get { return s_key.Value; } }
        public static XamlDirective Members { get { return s_members.Value; } }
        public static XamlDirective ClassAttributes { get { return s_classAttributes.Value; } }
        public static XamlDirective Name { get { return s_name.Value; } }
        public static XamlDirective PositionalParameters { get { return s_positionalParameters.Value; } }
        public static XamlDirective Shared { get { return s_shared.Value; } }
        public static XamlDirective Subclass { get { return s_subclass.Value; } }
        public static XamlDirective SynchronousMode { get { return s_synchronousMode.Value; } }
        public static XamlDirective TypeArguments { get { return s_typeArguments.Value; } }
        public static XamlDirective Uid { get { return s_uid.Value; } }
        public static XamlDirective UnknownContent { get { return s_unknownContent.Value; } }

        public static XamlDirective Base { get { return s_base.Value; } }
        public static XamlDirective Lang { get { return s_lang.Value; } }
        // This isn't a node stream directive, we should get rid of it
        public static XamlDirective Space { get { return s_space.Value; } }

        public static ReadOnlyCollection<XamlDirective> AllDirectives { get { return s_allDirectives.Value; } }

        internal static XamlType MarkupExtension { get { return s_markupExtension.Value; } }
        internal static XamlType INameScope { get { return s_iNameScope.Value; } }
        internal static XamlType PositionalParameterDescriptor { get { return s_positionalParameterDescriptor.Value; } }
        // Careful: accessing this faults in System.Xml
        internal static XamlType IXmlSerializable { get { return s_iXmlSerializable.Value; } }

        internal static string TypeAlias(Type type)
        {
            if (type.Equals(typeof(MemberDefinition)))
            {
                return KnownStrings.Member;
            }
            if (type.Equals(typeof(PropertyDefinition)))
            {
                return KnownStrings.Property;
            }
            return null;
        }

        internal static XamlDirective LookupXamlDirective(string name)
        {
            switch (name)
            {
                case x_AsyncRecords:
                    return AsyncRecords;
                case x_Arguments:
                    return Arguments;
                case x_Class:
                    return Class;
                case x_ClassModifier:
                    return ClassModifier;
                case x_Code:
                    return Code;
                case x_ConnectionId:
                    return ConnectionId;
                case x_FactoryMethod:
                    return FactoryMethod;
                case x_FieldModifier:
                    return FieldModifier;
                case x_Initialization:
                    return Initialization;
                case x_Items:
                    return Items;
                case x_Key:
                    return Key;
                case x_Members:
                    return Members;
                case x_ClassAttributes:
                    return ClassAttributes;
                case x_Name:
                    return Name;
                case x_PositionalParameters:
                    return PositionalParameters;
                case x_Shared:
                    return Shared;
                case x_Subclass:
                    return Subclass;
                case x_SynchronousMode:
                    return SynchronousMode;
                case x_TypeArguments:
                    return TypeArguments;
                case x_Uid:
                    return Uid;
                case x_UnknownContent:
                    return UnknownContent;
                default:
                    return null;
            }
        }

        internal static XamlType LookupXamlType(string typeNamespace, string typeName)
        {
            if (XamlNamespaces.Contains(typeNamespace))
            {
                switch (typeName)
                {
                    case "Array":
                    case "ArrayExtension":
                        return Array;
                    case "Member":
                        return Member;
                    case "Null":
                    case "NullExtension":
                        return Null;
                    case "Property":
                        return Property;
                    case "Reference":
                    case "ReferenceExtension":
                        return Reference;
                    case "Static":
                    case "StaticExtension":
                        return Static;
                    case "Type":
                    case "TypeExtension":
                        return Type;
                    case "String":
                        return String;
                    case "Double":
                        return Double;
                    case "Int16":
                        return Int16;
                    case "Int32":
                        return Int32;
                    case "Int64":
                        return Int64;
                    case "Boolean":
                        return Boolean;
                    case "XData":
                        return XData;
                    case "Object":
                        return Object;
                    case "Char":
                        return Char;
                    case "Single":
                        return Single;
                    case "Byte":
                        return Byte;
                    case "Decimal":
                        return Decimal;
                    case "Uri":
                        return Uri;
                    case "TimeSpan":
                        return TimeSpan;
                    default:
                        return null;
                }
            }
            return null;
        }

        // The XAML names of MemberDefinition and PropertyDefinition don't match their CLR type names.
        // LookupXamlType will still find them in the XAML namespace, but they won't be found through 
        // clr-namespace lookup. This method handles that special case.
        internal static Type LookupClrNamespaceType(AssemblyNamespacePair nsPair, string typeName)
        {
            if (nsPair.ClrNamespace == SWMNamespace && nsPair.Assembly == typeof(XamlLanguage).Assembly)
            {
                switch (typeName)
                {
                    case "Member":
                        return typeof(MemberDefinition);
                    case "Property":
                        return typeof(PropertyDefinition);
                    default:
                        return null;
                }
            }
            return null;
        }
        
        internal static XamlDirective LookupXmlDirective(string name)
        {
            switch (name)
            {
                case xml_Base:
                    return Base;
                case xml_Lang:
                    return Lang;
                case xml_Space:
                    return Space;
                default:
                    return null;
            }
        }

        private static ReadOnlyCollection<XamlType> GetAllTypes()
        {
            XamlType[] result = new XamlType[] { Array, Member, Null, Property, Reference, Static, Type, String, Double, Int16, Int32, Int64, Boolean, XData, Object, Char, Single, Byte, Decimal, Uri, TimeSpan };
            return new ReadOnlyCollection<XamlType>(result);
        }

        private static ReadOnlyCollection<XamlDirective> GetAllDirectives()
        {
            XamlDirective[] result = new XamlDirective[]
                { Arguments, AsyncRecords, Class, Code, ClassModifier, ConnectionId, FactoryMethod, FieldModifier,
                    Key, Initialization, Items, Members, ClassAttributes, Name, PositionalParameters, Shared, Subclass, 
                    SynchronousMode, TypeArguments, Uid, UnknownContent, Base, Lang, Space};
            return new ReadOnlyCollection<XamlDirective>(result);
        }

        private static XamlSchemaContext GetSchemaContext()
        {
            // System.Xaml and WindowsBase
            Assembly[] assemblies = new Assembly[]
                { typeof(XamlLanguage).Assembly, typeof(MarkupExtension).Assembly };
            XamlSchemaContextSettings settings = 
                new XamlSchemaContextSettings { SupportMarkupExtensionsWithDuplicateArity = true };
            XamlSchemaContext result = new XamlSchemaContext(assemblies, settings);
            return result;
        }

        private static XamlDirective GetXamlDirective(string name)
        {
            return GetXamlDirective(name, String, BuiltInValueConverter.String, AllowedMemberLocations.Attribute);
        }

        private static XamlDirective GetXamlDirective(string name, AllowedMemberLocations allowedLocation, MemberReflector reflector)
        {
            XamlDirective result = new XamlDirective(s_xamlNamespaces, name, allowedLocation, reflector);
            return result;
        }

        private static XamlDirective GetXamlDirective(string name, XamlType xamlType,
            XamlValueConverter<TypeConverter> typeConverter, AllowedMemberLocations allowedLocation)
        {
            XamlDirective result = new XamlDirective(s_xamlNamespaces, name, xamlType,
                typeConverter, allowedLocation);
            return result;
        }

        private static XamlDirective GetXmlDirective(string name)
        {
            XamlDirective result = new XamlDirective(s_xmlNamespaces, name, String,
                BuiltInValueConverter.String, AllowedMemberLocations.Attribute);
            return result;
        }

        private static XamlType GetXamlType(Type type)
        {
            XamlType result = s_schemaContext.Value.GetXamlType(type);
            return result;
        }
    }


#if TARGETTING35SP1   
    public delegate T Initializer<T>();

    public struct Lazy<T> where T : class
    {
        private Initializer<T> m_init;
        private T m_value;
        public bool IsValueCreated
        {
            get { return m_value != null; }
        }
        public Lazy(Initializer<T> init)
        {
            m_init = init;
            m_value = null;
        }

        public T Value
        {
            get
            {
                if (m_value == null)
                {
                    T newValue = m_init();
                    if (Interlocked.CompareExchange(ref m_value, newValue, null) != null &&
                            newValue is IDisposable)
                    {
                        ((IDisposable)newValue).Dispose();
                    }
                }
            return m_value;
            }
        }
    }
#endif

}
