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

                static BindingFlags instance_flag = BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance;
                static XmlSchema schema = new XmlSchema ();
		static readonly string xs = "http://www.w3.org/2001/XMLSchema";
                static Hashtable generatedSchemaTypes;

                static void Main (string [] args) 
                {
                        string assembly = args [0];

                        if (assembly.EndsWith (".dll") || assembly.EndsWith (".exe")) {
                                GenerateSchema (assembly);
                        } else {
                                Console.WriteLine ("Not supported.");
                                return;
                        }
                }

                /// <summary>
                ///     Writes a schema for each type in the assembly
                /// </summary>
                static void GenerateSchema (string assembly) 
                {
                        Assembly a = Assembly.LoadFrom (assembly);
                        generatedSchemaTypes = new Hashtable ();

                        if (a == null)
                                throw new NullReferenceException ("Null assembly");

                        foreach (Type t in a.GetTypes ()) {
                                XmlSchemaType schemaType = GenerateSchemaType (t);
                                XmlSchemaElement schemaElement = GenerateSchemaElement (t, schemaType);
                                schema.Items.Add (schemaElement);
                                schema.Items.Add (schemaType);
                        }

                        schema.Compile (new ValidationEventHandler (OnSchemaValidation));
                        schema.Write (Console.Out);
                        Console.WriteLine ();
                }

                /// <summary>
                ///     Given a Type and its associated schema type, add aa '<xs;element>' node 
                ///     to the schema.
                /// </summary>
                static XmlSchemaElement GenerateSchemaElement (Type type, XmlSchemaType schemaType) 
                {
                        XmlSchemaElement schemaElement = new XmlSchemaElement ();
                        schemaElement.Name = type.Name;
                        
                        if (schemaType.QualifiedName == null || schemaType.QualifiedName == XmlQualifiedName.Empty)
                                schemaElement.SchemaTypeName = new XmlQualifiedName (schemaType.Name);
                        else
                                schemaElement.SchemaTypeName = schemaType.QualifiedName;
                        
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
                static XmlSchemaType GenerateSchemaType (Type type) 
                {
                        if (generatedSchemaTypes.Contains (type.FullName))
                                return generatedSchemaTypes [type.FullName] as XmlSchemaType;

                        if (type != typeof (object) && type.BaseType != typeof (object)) 
                                return GenerateComplexContent (type);
                        
                        XmlSchemaComplexType complexType = new XmlSchemaComplexType ();
                        complexType.Name = type.Name;
                        FieldInfo [] fields = type.GetFields (instance_flag);
                        XmlSchemaSequence sequence = PopulateSequence (fields);
                        complexType.Particle = sequence;

                        generatedSchemaTypes.Add (type.FullName, complexType);
	
                        return complexType;
                }

                /// <summary>
                ///     Handle schema derivation by extension.
                /// </summary>
                static XmlSchemaType GenerateComplexContent (Type type) 
                {
                        XmlSchemaType baseSchemaType = GenerateSchemaType (type.BaseType);

                        XmlSchemaComplexType complexType = new XmlSchemaComplexType ();
                        complexType.Name = type.Name;

			FieldInfo [] fields = type.GetFields (instance_flag);

			XmlSchemaComplexContent content = new XmlSchemaComplexContent ();
                        XmlSchemaComplexContentExtension extension = new XmlSchemaComplexContentExtension ();
                        XmlSchemaSequence sequence = PopulateSequence (fields);

			extension.Particle = sequence;
                        extension.BaseTypeName = new XmlQualifiedName (baseSchemaType.Name);
                        content.Content = extension;
                        complexType.ContentModel = content;

			return complexType;
                }       

                static XmlSchemaSequence PopulateSequence (FieldInfo [] fields) 
                {
                        XmlSchemaSequence sequence = new XmlSchemaSequence ();

                        foreach (FieldInfo field in fields) {
                                XmlSchemaElement fieldElement = new XmlSchemaElement ();
                                fieldElement.Name = field.Name;
                                fieldElement.SchemaTypeName = GetSchemaTypeQName (field);
                                sequence.Items.Add (fieldElement);
                        }

                        return sequence;
                }

		///<summary>
		///	Populates element nodes inside a '<xs:sequence>' node.
		///</summary>
                static XmlQualifiedName GetSchemaTypeQName (FieldInfo field) 
                {
                        string type_name;

                        switch (field.FieldType.FullName) {
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
                                        type_name = String.Empty;
                                        break;
                        }       
                                
                        if (type_name == String.Empty)
                                throw new Exception (String.Format ("Can't convert {0} to an applicable Schema Type", field.Name));

			else {
                                XmlQualifiedName name = new XmlQualifiedName (type_name, xs);
                                return name;                                                
                        }                        
                }
        }
}


