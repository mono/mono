// created on 09/04/2003 at 18:58
//
//	System.Runtime.Serialization.Formatters.Soap.SoapTypeMapper
//
//	Authors:
//		Jean-Marc Andre (jean-marc.andre@polymtl.ca)
//

using System;
using System.Reflection;
using System.Collections;
using System.Runtime.Remoting;
using System.Xml.Serialization;
using System.Xml.Schema;

namespace System.Runtime.Serialization.Formatters.Soap {
	
	internal class SoapTypeMapper {
		private static Hashtable _mappingTable = new Hashtable();
		private static Hashtable _invertMappingTable = new Hashtable();
		
		static SoapTypeMapper() {
			InitMappingTable();
			
		}
		
		// returns the SoapTypeMapping corresponding to the System.Type
		public static SoapTypeMapping GetSoapType(Type type) {
			object rtnMapping;

			if (type.IsByRef) type = type.GetElementType ();

			
			if(type.IsArray){
					rtnMapping = _mappingTable[typeof(System.Array)];
			}
			else {
				rtnMapping = (object) _mappingTable[type];
				
				if(rtnMapping == null){
					string sTypeNamespace;
					AssemblyName assName = type.Assembly.GetName();
					if(assName.Name.StartsWith("mscorlib")) sTypeNamespace = "http://schemas.microsoft.com/clr/ns/"+type.Namespace;
					else sTypeNamespace = SoapServices.CodeXmlNamespaceForClrTypeNamespace(type.Namespace, type.Assembly.FullName);
					string sType = type.FullName;
					string sTypeName = type.FullName;
					if(type.Namespace != null && type.Namespace.Length > 0) sTypeName = sTypeName.Remove(0, type.Namespace.Length+1);
					SoapTypeMapping newType =  new SoapTypeMapping(type, sTypeName, sTypeNamespace, false, type.IsPrimitive, type.IsValueType, true);
					
					_mappingTable.Add(type, newType);
					_invertMappingTable.Add(newType, type);
					
					return newType;
				}
					
			}
			
			return (SoapTypeMapping)rtnMapping;
		}
		
		// returns the Type corresponding to the SoapTypeMapping
		public static Type GetType(SoapTypeMapping mapping) {
			object rtnObject;
			rtnObject = _invertMappingTable[mapping];
			
			if(rtnObject == null && mapping.TypeNamespace.Length != 0) {
				string typeNamespace;
				string assemblyName;
				SoapServices.DecodeXmlNamespaceForClrTypeNamespace(mapping.TypeNamespace, out typeNamespace, out assemblyName);
				rtnObject = Type.GetType(typeNamespace+"."+mapping.TypeName);
				
				if(rtnObject == null) {
					Assembly ass =Assembly.Load(assemblyName);
					if(ass != null) {
						rtnObject = ass.GetType(typeNamespace+"."+mapping.TypeName, true);
					}
				}
			}
			
			return (Type)rtnObject;
		}

		private static void RegisterSchemaType (Type type, string typeName, bool canBeValue, bool isPrimitive,bool isValueType,bool needId)
		{
			SoapTypeMapping mapping =  new SoapTypeMapping (type, typeName, canBeValue, isPrimitive, isValueType, needId);
			_mappingTable.Add(type, mapping);
			_invertMappingTable.Add(mapping, type);
		}
		
		// initialize the mapping tables
		private static void InitMappingTable() {
			SoapTypeMapping mapping;
			
			// the primitive type "System.String"
			mapping =  new SoapTypeMapping(typeof(string), "string", true, false, false, true);
			_mappingTable.Add(typeof(string),mapping);
			_invertMappingTable.Add(mapping, typeof(string));
			mapping =  new SoapTypeMapping(typeof(string), "string", XmlSchema.Namespace, true, false, false, true);
			_invertMappingTable.Add(mapping, typeof(string));

			RegisterSchemaType (typeof(short), "short", true, true, true, false);
			RegisterSchemaType (typeof(ushort), "unsignedShort", true, true, true, false);
			RegisterSchemaType (typeof(int), "int", true, true, true, false);
			RegisterSchemaType (typeof(uint), "unsignedInt", true, true, true, false);
			RegisterSchemaType (typeof(long), "long", true, true, true, false);
			RegisterSchemaType (typeof(ulong), "unsignedLong", true, true, true, false);
			RegisterSchemaType (typeof(decimal), "decimal", true, true, true, false);
			RegisterSchemaType (typeof(sbyte), "byte", true, true, true, false);
			RegisterSchemaType (typeof(byte), "unsignedByte", true, true, true, false);
			RegisterSchemaType (typeof(DateTime), "dateTime", true, true, true, false);
			RegisterSchemaType (typeof(TimeSpan), "duration", true, true, true, false);
			RegisterSchemaType (typeof(double), "double", true, true, true, false);
			RegisterSchemaType (typeof(Char), "char", true, true, true, false);
			RegisterSchemaType (typeof(bool), "boolean", true, true, true, false);
			RegisterSchemaType (typeof(System.Single), "float", true, true, true, false);
			RegisterSchemaType (typeof(System.Array), "Array", false, false, false, true);
			
			mapping = new SoapTypeMapping(typeof(object), "anyType", "http://www.w3.org/2001/XMLSchema", false, false, false, true);
			_mappingTable.Add(typeof(object), mapping);
			_invertMappingTable.Add(mapping, typeof(object));
			
			mapping = new SoapTypeMapping(typeof(System.Runtime.Serialization.Formatters.SoapFault), "Fault", "http://schemas.xmlsoap.org/soap/envelope/", false, false, false, true);
			_mappingTable.Add(typeof(System.Runtime.Serialization.Formatters.SoapFault), mapping);
			_invertMappingTable.Add(mapping, typeof(System.Runtime.Serialization.Formatters.SoapFault));
			
			
		}
	}
}
