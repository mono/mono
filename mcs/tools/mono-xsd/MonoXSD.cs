///
/// MonoXSD.cs -- A reflection-based tool for dealing with XML Schema.
/// 
/// Author: Duncan Mak (duncan@ximian.com)
///
/// Copyright (C) 2003, Duncan Mak, 
///			Ximian, Inc. 
///

using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace Mono.Util {
        class MonoXSD {

                static BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
                static XmlSchema schema = new XmlSchema ();
                static readonly string xs = "http://www.w3.org/2001/XMLSchema";
                static Hashtable generatedSchemaTypes;

                static void Main (string [] args) 
                {
                        string assembly = args [0];

                        if (assembly.EndsWith (".dll") || assembly.EndsWith (".exe"))
                                try {
                                        WriteSchema (assembly);
                                } catch (ArgumentException e) {
                                        Console.WriteLine (e.Message + "\n");
                                        Environment.Exit (0);
                                }
                        
                        else {
                                Console.WriteLine ("Not supported.");
                                return;
                        }
                }

                /// <summary>
                ///     Writes a schema for each type in the assembly
                /// </summary>
                static void WriteSchema (string assembly) 
                {
                        Assembly a = Assembly.LoadFrom (assembly);
                        generatedSchemaTypes = new Hashtable ();

                        if (a == null)
                                throw new NullReferenceException ("Null assembly");

                        XmlSchemaType schemaType;

                        foreach (Type t in a.GetTypes ()) {
                                try {
                                        schemaType = WriteSchemaType (t);
                                } catch (ArgumentException e) {
                                        throw new ArgumentException (String.Format ("Error: We cannot process {0}\n{1}", assembly, e.Message));
                                }

                                if (schemaType == null)
                                        continue;

                                XmlSchemaElement schemaElement = WriteSchemaElement (t, schemaType);
                                schema.Items.Add (schemaElement);
                                schema.Items.Add (schemaType);
                        }

                        schema.ElementFormDefault = XmlSchemaForm.Qualified;
                        schema.Compile (new ValidationEventHandler (OnSchemaValidation));
                        schema.Write (Console.Out);
                        Console.WriteLine ();
                }

                /// <summary>
                ///     Given a Type and its associated schema type, add aa '<xs;element>' node 
                ///     to the schema.
                /// </summary>
                static XmlSchemaElement WriteSchemaElement (Type type, XmlSchemaType schemaType) 
                {
                        XmlSchemaElement schemaElement = new XmlSchemaElement ();
                        schemaElement.Name = type.Name;

                        if (schemaType.QualifiedName == null || schemaType.QualifiedName == XmlQualifiedName.Empty)
                                schemaElement.SchemaTypeName = new XmlQualifiedName (schemaType.Name);
                        
                        else
                                schemaElement.SchemaTypeName = schemaType.QualifiedName;
                       
                        if (schemaType is XmlSchemaComplexType)
                                schemaElement.IsNillable = true;

                        return schemaElement;
                }
		
                static void OnSchemaValidation (object sender, ValidationEventArgs args) 
                {
                        Console.WriteLine (args.Message);
                }


                /// <summary>
                ///     From a Type, create a corresponding ComplexType node to
                ///     represent this Type.
                /// </summary>
                static XmlSchemaType WriteSchemaType (Type type) 
                {
                        if (generatedSchemaTypes.Contains (type.FullName)) // Caching
                                return generatedSchemaTypes [type.FullName] as XmlSchemaType;

                        XmlSchemaType schemaType = new XmlSchemaType ();

                        if (!type.IsAbstract && typeof (System.Delegate).IsAssignableFrom (type))
                                return null;

                        if (type.IsEnum)
                                schemaType = WriteEnum (type);

                        else if (type != typeof (object) && type.BaseType != typeof (object)) {
                                
                                try {
                                        schemaType = WriteComplexType (type);
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
                                } catch (ArgumentException e) {
                                        throw new ArgumentException (String.Format ("There is an error in '{0}'\n\t{1}", type.Name, e.Message));
                                }
                                complexType.Particle = sequence;                        
                                generatedSchemaTypes.Add (type.FullName, complexType);

                                schemaType = complexType;
                        }

                        return schemaType;
                }

                static XmlSchemaType WriteEnum (Type type) 
                {
                        if (type.IsEnum == false)
                                throw new Exception (String.Format ("{0} is not an enumeration.", type.Name));

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

                static XmlSchemaType WriteArray (Type type) 
                {
                        XmlSchemaComplexType complexType = new XmlSchemaComplexType ();
                        string type_name = type.Name.Substring (0, type.Name.Length - 2);
                        complexType.Name = "ArrayOf" + type_name;

                        XmlSchemaSequence sequence = new XmlSchemaSequence ();
                        XmlSchemaElement element = new XmlSchemaElement ();

                        element.MinOccurs = 0;
                        element.MaxOccursString = "unbounded";
                        element.IsNillable = true;
                        element.Name = type_name.ToLower ();                   
                        element.SchemaTypeName = GetQualifiedName (
                                type.FullName.Substring (0, type.FullName.Length - 2));
                        
                        sequence.Items.Add (element);
                        complexType.Particle = sequence;

                        generatedSchemaTypes.Add (type.FullName, complexType);
                        return complexType;                        
                }


                /// <summary>
                ///     Handle derivation by extension. 
                ///     If type is null, it'll create a new complexType 
                ///     with an XmlAny node in its sequence child node.
                /// </summary>
                static XmlSchemaType WriteComplexType (Type type) 
                {
                        //
                        // Recursively generate schema for all parent types
                        //
                        if (type != null && type.BaseType == typeof (object))
                                return WriteSchemaType (type);

                        XmlSchemaComplexType complexType = new XmlSchemaComplexType ();
                        XmlSchemaSequence sequence;

                        if (type == null) {
                                complexType.IsMixed = true;
                                sequence = new XmlSchemaSequence ();
                                sequence.Items.Add (new XmlSchemaAny ());
                                complexType.Particle = sequence;
                                return complexType;
                        }
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
                        } catch (ArgumentException e) {
                                throw new ArgumentException (String.Format ("There is an error in '{0}'\n\t{1}", type.Name, e.Message));
                        }
                        
                        extension.BaseTypeName = new XmlQualifiedName (baseSchemaType.Name);
                        extension.Particle = sequence;

                        generatedSchemaTypes.Add (type.FullName, complexType);
                        return complexType;
                }       

                static XmlSchemaSequence PopulateSequence (FieldInfo [] fields, PropertyInfo [] properties) 
                {
                        if (fields == null && properties == null)
                                return null;

                        XmlSchemaSequence sequence = new XmlSchemaSequence ();
                        
                        try {
                                foreach (FieldInfo field in fields)
                                        AddElement (sequence, field, field.FieldType);

                        } catch (Exception e) {
                                throw e;
                        }

                        if (properties == null)
                                return sequence;

                        try {
                                foreach (PropertyInfo property in properties)
                                        AddElement (sequence, property, property.PropertyType);

                        } catch (ArgumentException e) {
                                throw e;
                        }
                        
                        return sequence;
                }

                static void AddElement (XmlSchemaSequence sequence, MemberInfo member, Type type) 
                {
                        //
                        // Only read/write properties are supported.
                        //
                        if (member is PropertyInfo) {
                                PropertyInfo p = (PropertyInfo) member;
                                if (! (p.CanRead && p.CanWrite))
                                        return;
                        }

                        //
                        // readonly fields are not supported.
                        //
                        if (member is FieldInfo) {
                                FieldInfo f = (FieldInfo) member;
                                if (f.IsInitOnly || f.IsLiteral )
                                        return;
                        }

                        //
                        // delegates are not supported.
                        //
                        if (!type.IsAbstract && typeof (System.Delegate).IsAssignableFrom (type))
                                return;

                        if (type.IsArray) {
                                XmlSchemaType arrayType = WriteArray (type);
                                schema.Items.Add (arrayType);
                        }

                        XmlSchemaElement element = new XmlSchemaElement ();
                        element.Name = member.Name;

                        XmlQualifiedName qname = GetQualifiedName (type);

                        if (qname == null)
                                throw new ArgumentException (String.Format ("The type '{0}' cannot be represented in XML Schema.", type.FullName));

                        if (qname != XmlQualifiedName.Empty)
                                element.SchemaTypeName = qname;

                        if (qname.Name == "xml") {
                                element.SchemaType = WriteComplexType (null);
                                element.SchemaTypeName = XmlQualifiedName.Empty; // 'xml' is just a temporary name
                        }

                        if (type.IsClass)
                                element.MinOccurs = 0;
                        else
                                element.MinOccurs = 1;
                        element.MaxOccurs = 1;

                        sequence.Items.Add (element);
                }

                static XmlQualifiedName GetQualifiedName (Type type) 
                {
                        if (type.Equals (typeof (System.Xml.XmlNode)))
                                return XmlQualifiedName.Empty;

                        else if (type.IsSubclassOf (typeof (System.Xml.XmlNode)))
                                return new XmlQualifiedName ("xml");

                        else if (type.IsArray) {
                                string array_type = type.Name.Substring (0, type.Name.Length - 2);
                                return new XmlQualifiedName ("ArrayOf" + array_type);
                        } else 
                                return GetQualifiedName (type.FullName);
                }

                ///<summary>
                ///	Populates element nodes inside a '<xs:sequence>' node.
                ///</summary>
                static XmlQualifiedName GetQualifiedName (string type) 
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
