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
//					AssemblyName assName = new AssemblyName()
//					assName.FullName = asse
					Assembly ass =Assembly.Load(assemblyName);
					if(ass != null) {
						rtnObject = ass.GetType(typeNamespace+"."+mapping.TypeName, true);
					}
				}
			}
			
			return (Type)rtnObject;
		}
		
		// initialize the mapping tables
		private static void InitMappingTable() {
			SoapTypeMapping mapping;
			
			// the primitive type "System.String"
			mapping =  new SoapTypeMapping(typeof(string), "string", true, false, false, true);
			_mappingTable.Add(typeof(string),mapping);
			_invertMappingTable.Add(mapping, typeof(string));
			mapping =  new SoapTypeMapping(typeof(string), "string", "http://www.w3.org/2001/XMLSchema", true, false, false, true);
			_invertMappingTable.Add(mapping, typeof(string));
			
			// the primitive type "System.Int16"
			mapping =  new SoapTypeMapping(typeof(short), "short", "http://www.w3.org/2001/XMLSchema", true, true, true, false);
			_mappingTable.Add(typeof(short), mapping);
			_invertMappingTable.Add(mapping, typeof(short));
			
			// the primitive type "System.Int32"
			mapping =  new SoapTypeMapping(typeof(int), "int", "http://www.w3.org/2001/XMLSchema", true, true, true, false);
			_mappingTable.Add(typeof(int), mapping);
			_invertMappingTable.Add(mapping, typeof(int));
			
			// the primitive type "System.Boolean"
			mapping =  new SoapTypeMapping(typeof(bool), "boolean", "http://www.w3.org/2001/XMLSchema", true, true, true, false);
			_mappingTable.Add(typeof(bool), mapping);
			_invertMappingTable.Add(mapping, typeof(bool));
			
			// the primitive type "System.long"
			mapping =  new SoapTypeMapping(typeof(long), "long", "http://www.w3.org/2001/XMLSchema", true, true, true, false);
			_mappingTable.Add(typeof(long), mapping);
			_invertMappingTable.Add(mapping, typeof(long));
			
			// the primitive type "System.double"
			mapping =  new SoapTypeMapping(typeof(double), "double", "http://www.w3.org/2001/XMLSchema", true, true, true, false);
			_mappingTable.Add(typeof(double), mapping);
			_invertMappingTable.Add(mapping, typeof(double));
			
			// the primitive type "System.Char"
			mapping =  new SoapTypeMapping(typeof(Char), "Char", "http://www.w3.org/2001/XMLSchema", true, true, true, false);
			_mappingTable.Add(typeof(Char), mapping);
			_invertMappingTable.Add(mapping, typeof(Char));
			
			// the primitive type "System.Single"
			mapping = new SoapTypeMapping(typeof(System.Single), "float", "http://www.w3.org/2001/XMLSchema", true, true, true, false);
			_mappingTable.Add(typeof(System.Single), mapping);
			_invertMappingTable.Add(mapping, typeof(System.Single));
			
			mapping =  new SoapTypeMapping(typeof(System.Array), "Array", false, false, false, true);
			_mappingTable.Add(typeof(System.Array), mapping);
			_invertMappingTable.Add(mapping, typeof(System.Array));
			
			mapping = new SoapTypeMapping(typeof(object), "anyType", "http://www.w3.org/2001/XMLSchema", false, false, false, true);
			_mappingTable.Add(typeof(object), mapping);
			_invertMappingTable.Add(mapping, typeof(object));
			
			mapping = new SoapTypeMapping(typeof(System.Runtime.Serialization.Formatters.SoapFault), "Fault", "http://schemas.xmlsoap.org/soap/envelope/", false, false, false, true);
			_mappingTable.Add(typeof(System.Runtime.Serialization.Formatters.SoapFault), mapping);
			_invertMappingTable.Add(mapping, typeof(System.Runtime.Serialization.Formatters.SoapFault));
			
			
		}
	}
}
