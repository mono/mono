///
/// MonoXSD.cs -- A reflection-based tool for dealing with XML Schema.
///
/// Author: Duncan Mak (duncan@ximian.com)
///
/// Copyright (C) 2003, Duncan Mak,
///                     Ximian, Inc.
///

using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Mono.Util {

        public class Driver {

                public static readonly string helpString =
                        "MonoXSD.exe - a utility for generating schema or class files\n\nMonoXSD.exe <assembly>.dll|<assembly>.exe [/output:] [/type]\n";

                static void Main (string [] args) {

                        if (args.Length < 1) {
                                Console.WriteLine (helpString);
                                Environment.Exit (0);
                        }

                        string input = args [0];
                        string lookup_type = null;
                        string output_dir = null;

                        if (input.EndsWith (".dll") || input.EndsWith (".exe")) {

                                if (args.Length >= 2 && args [1].StartsWith ("/o"))
                                        output_dir = args [1].Substring (args [1].IndexOf (':') + 1);

                                if (args.Length >= 3 && args [2].StartsWith ("/t"))
                                        lookup_type = args [2].Substring (args [2].IndexOf (':') + 1);

                                MonoXSD xsd = new MonoXSD ();

                                try {
                                        xsd.WriteSchema (input, lookup_type, output_dir);
                                } catch (ArgumentException e) {
                                        Console.WriteLine (e.Message + "\n");
                                        Environment.Exit (0);
                                }
                        } else {
                                Console.WriteLine ("Not supported.");
                                return;
                        }
                }
        }


        public class MonoXSD {
                Hashtable attributes;
                int fileCount = 0;
                bool isText = false;
                BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
                XmlSchema schema = new XmlSchema ();
                readonly string xs = "http://www.w3.org/2001/XMLSchema";
                Hashtable generatedSchemaTypes;

                public int FileCount {
                        set { fileCount = value; }
                }

                /// <summary>
                ///     Writes a schema for each type in the assembly
                /// </summary>
                public void WriteSchema (string assembly, string lookup_type, string output_dir) {

                        Assembly a;

                        try {
                                a = Assembly.LoadFrom (assembly);
                        } catch (Exception e) {
                                Console.WriteLine ("Cannot use {0}, {1}", assembly, e.Message);
                                return;
                        }

                        generatedSchemaTypes = new Hashtable ();

                        XmlSchemaType schemaType;

                        foreach (Type t in a.GetTypes ()) {

                                if (lookup_type != null && t.Name != lookup_type)
                                        continue;

                                try {
                                        schemaType = WriteSchemaType (t);
                                } catch (ArgumentException e) {
                                        throw new ArgumentException (String.Format ("Error: We cannot process {0}\n{1}", assembly, e.Message));
                                }

                                if (schemaType == null)
                                        continue; // skip

                                XmlSchemaElement schemaElement = WriteSchemaElement (t, schemaType);
                                schema.Items.Add (schemaElement);
                                schema.Items.Add (schemaType);
                        }

                        schema.ElementFormDefault = XmlSchemaForm.Qualified;
                        schema.Compile (new ValidationEventHandler (OnSchemaValidation));

                        string output = String.Format ("schema{0}.xsd", fileCount);

                        if (output_dir != null)
                                output = Path.Combine (output_dir, output);

                        XmlTextWriter writer = new XmlTextWriter (output, Encoding.UTF8);
                        writer.Formatting = Formatting.Indented;
                        schema.Write (writer);
                        Console.WriteLine ("\nWriting {0}.", output);
                }

                /// <summary>
                ///     Given a Type and its associated schema type, add aa '<xs;element>' node
                ///     to the schema.
                /// </summary>
                public XmlSchemaElement WriteSchemaElement (Type type, XmlSchemaType schemaType)
                {
                        XmlSchemaElement schemaElement = new XmlSchemaElement ();
                        schemaElement.Name = type.Name;

                        if (schemaType.QualifiedName == null)
                                schemaElement.SchemaTypeName = new XmlQualifiedName (schemaType.Name);

                        else
                                schemaElement.SchemaTypeName = schemaType.QualifiedName;

                        if (schemaType is XmlSchemaComplexType)
                                schemaElement.IsNillable = true;

                        return schemaElement;
                }

                public void OnSchemaValidation (object sender, ValidationEventArgs args)
                {
                        Console.WriteLine (args.Message);
                }


                /// <summary>
                ///     From a Type, create a corresponding ComplexType node to
                ///     represent this Type.
                /// </summary>
                public XmlSchemaType WriteSchemaType (Type type)
                {
                        if (generatedSchemaTypes.Contains (type.FullName)) // Caching
                                return generatedSchemaTypes [type.FullName] as XmlSchemaType;

                        XmlSchemaType schemaType = new XmlSchemaType ();

                        attributes = new Hashtable ();

                        if (!type.IsAbstract && typeof (System.Delegate).IsAssignableFrom (type))
                                return null;

                        if (type.IsEnum)
                                return WriteEnumType (type);

                        else if (type != typeof (object) && type.BaseType != typeof (object)) {
                                try {
                                        schemaType = WriteComplexSchemaType (type);
                                } catch (ArgumentException e) {
                                        throw e;
                                }

                        } else {

                                XmlSchemaComplexType complexType = new XmlSchemaComplexType ();
                                complexType.Name = type.Name;
                                FieldInfo [] fields = type.GetFields (flags);
                                PropertyInfo [] properties = type.GetProperties (flags);
                                XmlSchemaSequence sequence;

                                try {
                                        sequence = PopulateSequence (fields, properties);

                                        if (attributes != null) {
                                                foreach (object o in attributes.Keys) {
                                                        MemberInfo member = o as MemberInfo;
                                                        Type attribute_type = attributes [o] as Type;

                                                        if (attribute_type == typeof (System.Xml.Schema.XmlSchemaAnyAttribute))
                                                                complexType.AnyAttribute = new XmlSchemaAnyAttribute ();
                                                        else
                                                                complexType.Attributes.Add (WriteSchemaAttribute (member, attribute_type));
                                                }
                                        }

                                } catch (ArgumentException e) {
                                        throw new ArgumentException (String.Format ("There is an error in '{0}'\n\t{1}", type.Name, e.Message));
                                }

                                if (isText) {
                                        complexType.IsMixed = true;
                                        isText = false;
                                }

                                complexType.Particle = sequence;
                                generatedSchemaTypes.Add (type.FullName, complexType);

                                schemaType = complexType;
                        }

                        return schemaType;
                }

                public XmlSchemaAttribute WriteSchemaAttribute (MemberInfo member, Type attribute_type)
                {
                        if (member == null || attribute_type == null)
                                return null;

                        XmlSchemaAttribute attribute = new XmlSchemaAttribute ();
                        attribute.Name = member.Name;
                        attribute.SchemaTypeName = GetQualifiedName (attribute_type.FullName);

                        object [] attrs = member.GetCustomAttributes (false);

                        return attribute;
                }

                public XmlSchemaType WriteEnumType (Type type)
                {
                        if (type.IsEnum == false)
                                throw new Exception (String.Format ("{0} is not an enumeration.", type.Name));

                        if (generatedSchemaTypes.Contains (type.FullName)) // Caching
                                return null;

                        XmlSchemaSimpleType simpleType = new XmlSchemaSimpleType ();
                        simpleType.Name = type.Name;
                        FieldInfo [] fields = type.GetFields ();

                        XmlSchemaSimpleTypeRestriction simpleRestriction = new XmlSchemaSimpleTypeRestriction ();
                        simpleType.Content = simpleRestriction;
                        simpleRestriction.BaseTypeName = new XmlQualifiedName ("string", xs);

                        foreach (FieldInfo field in fields) {
                                if (field.IsSpecialName)
                                        continue;

                                XmlSchemaEnumerationFacet e = new XmlSchemaEnumerationFacet ();
                                e.Value = field.Name;
                                simpleRestriction.Facets.Add (e);
                        }

                        generatedSchemaTypes.Add (type.FullName, simpleType);
                        return simpleType;
                }

                public XmlSchemaType WriteArrayType (Type type, MemberInfo member)
                {
                        if (generatedSchemaTypes.Contains (type.FullName)) // Caching
                                return null;

                        XmlSchemaComplexType complexType = new XmlSchemaComplexType ();

                        XmlQualifiedName qname = GetQualifiedName (type);

                        if (qname == null)
                                complexType.Name = type.Name;
                        else
                                complexType.Name = qname.Name;

                        XmlSchemaSequence sequence = new XmlSchemaSequence ();
                        XmlSchemaElement element = new XmlSchemaElement ();

                        element.MinOccurs = 0;
                        element.MaxOccursString = "unbounded";
                        element.IsNillable = true;
                        element.Name = qname.Name.ToLower ();

                        object [] attrs = member.GetCustomAttributes (false);

                        if (attrs.Length > 0) {
                                foreach (object o in attrs) {
                                        if (o is XmlArrayItemAttribute) {
                                                if (type.IsArray == false)
                                                        throw new ArgumentException (
                                                                String.Format ("XmlArrayAttribute is not applicable to {0}, because it is not an array.",
                                                                member.Name));

                                                XmlArrayItemAttribute attr = (XmlArrayItemAttribute) o;

                                                if (attr.ElementName.Length != 0)
                                                        element.Name = attr.ElementName;

                                                continue;
                                        }

                                        if (o is XmlAnyElementAttribute)
                                                return null;
                                }
                        }

                        element.SchemaTypeName = GetQualifiedName (
                                type.FullName.Substring (0, type.FullName.Length - 2));

                        sequence.Items.Add (element);
                        complexType.Particle = sequence;

                        generatedSchemaTypes.Add (type.FullName, complexType);
                        return complexType;
                }

                public XmlSchemaType WriteComplexSchemaType ()
                {
                        XmlSchemaComplexType complexType = new XmlSchemaComplexType ();
                        XmlSchemaSequence sequence;
                        complexType.IsMixed = true;
                        sequence = new XmlSchemaSequence ();
                        sequence.Items.Add (new XmlSchemaAny ());
                        complexType.Particle = sequence;
                        return complexType;
                }


                /// <summary>
                ///     Handle derivation by extension.
                ///     If type is null, it'll create a new complexType
                ///     with an XmlAny node in its sequence child node.
                /// </summary>
                public XmlSchemaType WriteComplexSchemaType (Type type)
                {
                        //
                        // Recursively generate schema for all parent types
                        //
                        if (type != null && type.BaseType == typeof (object))
                                return WriteSchemaType (type);

                        XmlSchemaComplexType complexType = new XmlSchemaComplexType ();
                        XmlSchemaSequence sequence;
                        XmlSchemaComplexContentExtension extension = new XmlSchemaComplexContentExtension ();
                        XmlSchemaComplexContent content = new XmlSchemaComplexContent ();

                        complexType.ContentModel = content;
                        content.Content = extension;

                        XmlSchemaType baseSchemaType = WriteSchemaType (type.BaseType);

                        complexType.Name = type.Name;

                        FieldInfo [] fields = type.GetFields (flags);
                        PropertyInfo [] properties = type.GetProperties (flags);

                        try {
                                sequence = PopulateSequence (fields, properties);
                                if (attributes != null) {
                                        foreach (object o in attributes) {
                                                MemberInfo member = (MemberInfo) o;
                                                Type attribute_type = (Type) attributes [o];

                                                complexType.Attributes.Add (WriteSchemaAttribute (member, attribute_type));
                                        }
                                }
                        } catch (ArgumentException e) {
                                throw new ArgumentException (String.Format ("There is an error in '{0}'\n\t{1}", type.Name, e.Message));
                        }

                        extension.BaseTypeName = new XmlQualifiedName (baseSchemaType.Name);
                        extension.Particle = sequence;

                        generatedSchemaTypes.Add (type.FullName, complexType);
                        return complexType;
                }

                public XmlSchemaSequence PopulateSequence (FieldInfo [] fields, PropertyInfo [] properties)
                {
                        if (fields.Length == 0 && properties.Length == 0)
                                return null;

                        XmlSchemaSequence sequence = new XmlSchemaSequence ();

                        try {
                                foreach (FieldInfo field in fields)
                                        if (IsXmlAttribute (field))
                                                attributes.Add (field, field.FieldType);
                                        else if (IsXmlAnyAttribute (field))
                                                attributes.Add (field,
                                                        typeof (System.Xml.Schema.XmlSchemaAnyAttribute));
                                        else
                                                AddElement (sequence, field, field.FieldType);

                        } catch (Exception e) {
                                throw e;
                        }

                        if (properties.Length == 0)
                                return sequence;
                        try {
                                foreach (PropertyInfo property in properties)
                                        if (IsXmlAttribute (property))
                                                attributes.Add (property, property.PropertyType);
                                        else if (IsXmlAnyAttribute (property))
                                                attributes.Add (property,
                                                        typeof (System.Xml.Schema.XmlSchemaAnyAttribute));
                                        else {
                                                AddElement (sequence, property, property.PropertyType);
                                        }

                        } catch (ArgumentException e) {
                                throw e;
                        }

                        return sequence;
                }

                public bool IsXmlAttribute (MemberInfo member)
                {
                        object [] attrs = member.GetCustomAttributes (
                                typeof (System.Xml.Serialization.XmlAttributeAttribute), false);

                        if (attrs.Length == 0)
                                return false;
                        else
                                return true;
                }

                public bool IsXmlAnyAttribute (MemberInfo member) {
                        object [] attrs = member.GetCustomAttributes (
                                typeof (System.Xml.Serialization.XmlAnyAttributeAttribute), false);

                        if (attrs.Length == 0)
                                return false;
                        else
                                return true;
                }

                ///<summary>
                ///     Populates element nodes inside a '<xs:sequence>' node.
                ///</summary>
                public void AddElement (XmlSchemaSequence sequence, MemberInfo member, Type type)
                {
                        //
                        // Only read/write properties are supported.
                        //
                        if (member is PropertyInfo) {
                                PropertyInfo p = (PropertyInfo) member;
                                if ((p.CanRead && p.CanWrite) == false)
                                        return;
                        }

                        //
                        // readonly fields are not supported.
                        //
                        if (member is FieldInfo) {
                                FieldInfo f = (FieldInfo) member;
                                if (f.IsInitOnly || f.IsLiteral)
                                        return;
                        }

                        //
                        // delegates are not supported.
                        //
                        if (!type.IsAbstract && typeof (System.Delegate).IsAssignableFrom (type))
                                return;

                        //
                        // If it's an array, write a SchemaType for the type of array
                        //
                        if (type.IsArray) {
                                XmlSchemaType arrayType = WriteArrayType (type, member);
                                if (arrayType != null)
                                        schema.Items.Add (arrayType);
                        }

                        XmlSchemaElement element = new XmlSchemaElement ();

                        element.Name = member.Name;
                        XmlQualifiedName schema_type_name = GetQualifiedName (type);

                         if (type.IsEnum) {
                                element.SchemaTypeName = new XmlQualifiedName (type.Name);

                         } else if (schema_type_name.Name == "xml") {
                                 element.SchemaType = WriteComplexSchemaType ();
                                 element.SchemaTypeName = XmlQualifiedName.Empty; // 'xml' is just a temporary name

                         } else if (schema_type_name == null) {
                                throw new ArgumentException (String.Format ("The type '{0}' cannot be represented in XML Schema.", type.FullName));

                         } else  // this is the normal case
                                element.SchemaTypeName = schema_type_name;

                        object [] attrs = member.GetCustomAttributes (false);

                        if (attrs.Length > 0) {
                                foreach (object o in attrs) {
                                        if (o is XmlElementAttribute) {
                                                XmlElementAttribute attr = (XmlElementAttribute) o;

                                                if (attr.DataType != null && attr.DataType.Length != 0)
                                                        element.SchemaTypeName = new XmlQualifiedName (attr.DataType, xs);
                                                if (attr.ElementName != null && attr.ElementName.Length != 0)
                                                        element.Name = attr.ElementName;

                                                continue;
                                        }

                                        if (o is XmlArrayAttribute) {

                                                if (type.IsArray == false)
                                                        throw new ArgumentException (
                                                                String.Format ("XmlArrayAttribute is not applicable to {0}, because it is not an array.",
                                                                                member.Name));

                                                XmlArrayAttribute attr = (XmlArrayAttribute) o;

                                                if (attr.ElementName.Length != 0)
                                                        element.Name = attr.ElementName;

                                                continue;
                                        }

                                        //
                                        // isText signals that the mixed="true" in the schema type.
                                        //
                                        if (o is XmlTextAttribute) {
                                                isText = true;
                                                return;
                                        }

                                        if (o is XmlAnyElementAttribute) {
                                                XmlSchemaAny any = new XmlSchemaAny ();
                                                any.MinOccurs = 0;
                                                any.MaxOccursString = "unbounded";
                                                sequence.Items.Add (any);
                                                return;
                                        }
                                }
                        }

                        if (type.IsClass)
                                element.MinOccurs = 0;
                        else if (type.IsValueType)
                                element.MinOccurs = 1;

                        element.MaxOccurs = 1;

                        sequence.Items.Add (element);
                }

                public XmlQualifiedName GetQualifiedName (Type type)
                {
                        //
                        // XmlAttributes are not saved.
                        //
                        if (type.Equals (typeof (System.Xml.XmlAttribute)))
                                return null;

                        //
                        // Other derivatives of XmlNode are saved specially,
                        // as indicated by this "xml" flag.
                        //
                        if (type.Equals (typeof (System.Xml.XmlNode))
                                || type.IsSubclassOf (typeof (System.Xml.XmlNode)))
                                return new XmlQualifiedName ("xml");

                        if (type.IsArray) {
                                TextInfo ti = CultureInfo.CurrentCulture.TextInfo;
                                string type_name = type.FullName.Substring (0, type.FullName.Length - 2);

                                XmlQualifiedName qname = GetQualifiedName (type_name);
                                string array_type;

                                if (qname != null)
                                        array_type = ti.ToTitleCase (qname.Name);
                                else
                                        array_type = ti.ToTitleCase (type.Name.Substring (0, type.Name.Length - 2));

                                return new XmlQualifiedName ("ArrayOf" + array_type);
                        }

                        return GetQualifiedName (type.FullName);
                }

                public XmlQualifiedName GetQualifiedName (string type)
                {
                        string type_name;

                        switch (type) {
                                case "System.Uri":
                                        type_name =  "anyURI";
                                        break;
                                case "System.Boolean":
                                        type_name = "Boolean";
                                        break;
                                case "System.SByte":
                                        type_name = "Byte";
                                        break;
                                case "System.DateTime":
                                        type_name = "dateTime";
                                        break;
                                case "System.Decimal":
                                        type_name = "decimal";
                                        break;
                                case "System.Double":
                                        type_name = "Double";
                                        break;
                                case "System.Int16":
                                        type_name = "short";
                                        break;
                                case "System.Int32":
                                        type_name =  "int";
                                        break;
                                case "System.Int64":
                                        type_name = "long";
                                        break;
                                case "System.Xml.XmlQualifiedName":
                                        type_name = "QName";
                                        break;
                                case "System.TimeSpan":
                                        type_name = "duration";
                                        break;
                                case "System.String":
                                        type_name = "string";
                                        break;
                                case "System.UInt16":
                                        type_name = "unsignedShort";
                                        break;
                                case "System.UInt32":
                                        type_name = "unsignedInt";
                                        break;
                                case "System.UInt64":
                                        type_name = "unsignedLong";
                                        break;
                                default:
                                        type_name = null;
                                        break;
                        }

                        if (type_name == null)
                                return null;

                        else {
                                XmlQualifiedName name = new XmlQualifiedName (type_name, xs);
                                return name;
                        }
                }
        }
}
