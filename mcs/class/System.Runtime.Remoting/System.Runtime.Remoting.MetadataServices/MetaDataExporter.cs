//
// System.Runtime.Remoting.MetadataServices.MetaDataExporter
//
// Authors:
//		Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) 2003 Novell, Inc
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Collections;
using System.IO;
using System.Text;
using System.Xml;
using System.Reflection;
using System.Net;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Metadata;
using System.Runtime.Serialization;

namespace System.Runtime.Remoting.MetadataServices
{
	internal class MetaDataExporter
	{
		public void ExportTypes (ServiceType[] servicetypes, SdlType sdltype, XmlTextWriter tw)
		{
			if (sdltype == SdlType.Sdl)	// Obsolete, we don't support this
				throw new NotSupportedException ();

			if (servicetypes.Length == 0) return;
			Type maint = servicetypes [0].ObjectType;
			
			Hashtable dataTypes = new Hashtable (); 
			ArrayList services = new ArrayList ();
			FindTypes (servicetypes, dataTypes, services);
			
			if (services.Count > 0)
				maint = ((ServiceType) services[0]).ObjectType;
			
			string serviceNs = GetXmlNamespace (maint, null);
			
			tw.Formatting = Formatting.Indented;
			tw.WriteStartElement ("definitions", MetaData.WsdlNamespace);
			tw.WriteAttributeString ("name", maint.Name);
			tw.WriteAttributeString ("targetNamespace", serviceNs);
			tw.WriteAttributeString ("xmlns", MetaData.XmlnsNamespace, MetaData.WsdlNamespace);
			tw.WriteAttributeString ("xmlns", "tns", MetaData.XmlnsNamespace, serviceNs);
			tw.WriteAttributeString ("xmlns", "xsd", MetaData.XmlnsNamespace, MetaData.SchemaNamespace);
			tw.WriteAttributeString ("xmlns", "xsi", MetaData.XmlnsNamespace, MetaData.SchemaInstanceNamespace);
			tw.WriteAttributeString ("xmlns", "suds", MetaData.XmlnsNamespace, MetaData.SudsNamespace);
			tw.WriteAttributeString ("xmlns", "wsdl", MetaData.XmlnsNamespace, MetaData.WsdlNamespace);
			tw.WriteAttributeString ("xmlns", "soapenc", MetaData.XmlnsNamespace, MetaData.SoapEncodingNamespace);
			tw.WriteAttributeString ("xmlns", "soap", MetaData.XmlnsNamespace, MetaData.SoapNamespace);
			
			int nums = 0;
			foreach (DictionaryEntry entry in dataTypes)
			{
				string ns = (string) entry.Key;
				if (tw.LookupPrefix (ns) != null) continue;
				tw.WriteAttributeString ("xmlns", "ns"+nums, MetaData.XmlnsNamespace, ns);
				nums++;
			}
			
			// Schema
			
			if (dataTypes.Count > 0)
			{
				tw.WriteStartElement ("types", MetaData.WsdlNamespace);
				foreach (DictionaryEntry entry in dataTypes)
				{
					SchemaInfo sinfo = (SchemaInfo) entry.Value;
					if (sinfo == null || sinfo.Types.Count == 0) continue;
					
					tw.WriteStartElement ("s", "schema", MetaData.SchemaNamespace);
					tw.WriteAttributeString ("targetNamespace", (string) entry.Key);
					tw.WriteAttributeString ("elementFormDefault", "unqualified");
					tw.WriteAttributeString ("attributeFormDefault", "unqualified");
					
					foreach (string ns in sinfo.Imports)
					{
						if (ns == (string) entry.Key) continue;
						tw.WriteStartElement ("import", MetaData.SchemaNamespace);
						tw.WriteAttributeString ("namespace", ns);
						tw.WriteEndElement ();
					}
					
					foreach (Type type in sinfo.Types)
						WriteDataTypeSchema (tw, type);
						
					tw.WriteEndElement ();
				}
				tw.WriteEndElement ();
			}
			
			// Bindings
			
/*			foreach (ServiceType st in servicetypes)
				WriteServiceBinding (tw, st);
*/
			foreach (ServiceType st in services)
				WriteServiceBinding (tw, st, dataTypes);

			// Service element
			
			tw.WriteStartElement ("service", MetaData.WsdlNamespace);
			if (services.Count > 0)
				tw.WriteAttributeString ("name", GetServiceName (maint));
			else
				tw.WriteAttributeString ("name", "Service");

			foreach (ServiceType st in services)
			{
				WriteServiceType (tw, st);
			}
			tw.WriteEndElement ();

			// Closing

			tw.WriteEndElement ();
			tw.Flush ();
		}
		
		void WriteServiceType (XmlTextWriter tw, ServiceType st)
		{
			tw.WriteStartElement ("port", MetaData.WsdlNamespace);
			tw.WriteAttributeString ("name", GetPortName (st.ObjectType));
			tw.WriteAttributeString ("binding", "tns:" + GetBindingName (st.ObjectType));
			
			if (st.Url != null)
			{
				tw.WriteStartElement ("soap","address", MetaData.SoapNamespace);
				tw.WriteAttributeString ("location", st.Url);
				tw.WriteEndElement ();
			}
			
			tw.WriteEndElement ();
		}
		
		void WriteServiceBinding  (XmlTextWriter tw, ServiceType st, Hashtable dataTypes)
		{
			Type type = st.ObjectType;
			string typeName = type.Name;
			MethodInfo[] mets = type.GetMethods (BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
			bool isService = IsService (type);
			
			// Messages
			
			if (isService)
			{
				foreach (MethodInfo met in mets)
				{
					if (met.DeclaringType.Assembly == typeof(object).Assembly) continue;
					
					ParameterInfo[] pars = met.GetParameters ();
					tw.WriteStartElement ("message", MetaData.WsdlNamespace);
					tw.WriteAttributeString ("name", typeName + "." + met.Name + "Input");
					foreach (ParameterInfo par in pars)
					{
						if (!par.ParameterType.IsByRef)
							WritePart (tw, par.Name, par.ParameterType, type);
					}
					tw.WriteEndElement ();	// message
					
					tw.WriteStartElement ("message", MetaData.WsdlNamespace);
					tw.WriteAttributeString ("name", typeName + "." + met.Name + "Output");
					
					if (met.ReturnType != typeof(void))
						WritePart (tw, "return", met.ReturnType, type);
					
					foreach (ParameterInfo par in pars)
					{
						if (par.ParameterType.IsByRef || par.IsOut)
							WritePart (tw, par.Name, par.ParameterType, type);
					}
					tw.WriteEndElement ();	// message
				}
			}
			
			// Port type
			
			tw.WriteStartElement ("portType", MetaData.WsdlNamespace);
			tw.WriteAttributeString ("name", typeName + "PortType");
			
			if (isService)
			{
				foreach (MethodInfo met in mets)
				{
					if (met.DeclaringType.Assembly == typeof(object).Assembly) continue;
					
					tw.WriteStartElement ("operation", MetaData.WsdlNamespace);
					tw.WriteAttributeString ("name", met.Name);
					
					StringBuilder sb = new StringBuilder ();
					ParameterInfo[] pars = met.GetParameters ();
					foreach (ParameterInfo par in pars)
					{
						if (sb.Length != 0) sb.Append (" ");
						sb.Append (par.Name);
					}
					tw.WriteAttributeString ("parameterOrder", sb.ToString ());
					
					tw.WriteStartElement ("input", MetaData.WsdlNamespace);
					tw.WriteAttributeString ("name", met.Name + "Request");
					tw.WriteAttributeString ("message", "tns:" + typeName + "." + met.Name + "Input");
					tw.WriteEndElement ();
					
					tw.WriteStartElement ("output", MetaData.WsdlNamespace);
					tw.WriteAttributeString ("name", met.Name + "Response");
					tw.WriteAttributeString ("message", "tns:" + typeName + "." + met.Name + "Output");
					tw.WriteEndElement ();
					
					tw.WriteEndElement ();	// operation
				}
			}
			tw.WriteEndElement ();	// portType
			
			// Binding
			
			tw.WriteStartElement ("binding", MetaData.WsdlNamespace);
			tw.WriteAttributeString ("name", typeName + "Binding");
			tw.WriteAttributeString ("type", "tns:" + typeName + "PortType");

			tw.WriteStartElement ("soap", "binding", MetaData.SoapNamespace);
			tw.WriteAttributeString ("style", "rpc");
			tw.WriteAttributeString ("transport", "http://schemas.xmlsoap.org/soap/http");
			tw.WriteEndElement ();
			
			WriteTypeSuds (tw, type);
			
			SchemaInfo sinfo = (SchemaInfo) dataTypes [GetXmlNamespace (type,null)];
			if (sinfo != null && !sinfo.SudsGenerated)
			{
				foreach (Type dt in sinfo.Types)
					WriteTypeSuds (tw, dt);
				sinfo.SudsGenerated = true;
			}
			
			if (isService)
			{
				foreach (MethodInfo met in mets)
				{
					if (met.DeclaringType.Assembly == typeof(object).Assembly) continue;
					
					tw.WriteStartElement ("operation", MetaData.WsdlNamespace);
					tw.WriteAttributeString ("name", met.Name);
					
					tw.WriteStartElement ("soap", "operation", MetaData.SoapNamespace);
					tw.WriteAttributeString ("soapAction", GetSoapAction (met));
					tw.WriteEndElement ();
					
					tw.WriteStartElement ("suds", "method", MetaData.SudsNamespace);
					tw.WriteAttributeString ("attributes", "public");
					tw.WriteEndElement ();
					
					tw.WriteStartElement ("input", MetaData.WsdlNamespace);
					tw.WriteAttributeString ("name", met.Name + "Request");
					WriteMessageBindingBody (tw, type);
					tw.WriteEndElement ();
					
					tw.WriteStartElement ("output", MetaData.WsdlNamespace);
					tw.WriteAttributeString ("name", met.Name + "Response");
					WriteMessageBindingBody (tw, type);
					tw.WriteEndElement ();
					
					tw.WriteEndElement ();	// operation
				}
			}
			tw.WriteEndElement ();	// binding
		}
		
		void WriteTypeSuds (XmlTextWriter tw, Type type)
		{
			if (type.IsArray || type.IsEnum)
			{
				return;
			}
			else if (type.IsInterface)
			{
				tw.WriteStartElement ("suds", "interface", MetaData.SudsNamespace);
				tw.WriteAttributeString ("type", GetQualifiedXmlType (tw, type, null));
				foreach (Type interf in type.GetInterfaces ()) {
					tw.WriteStartElement ("suds","extends", MetaData.SudsNamespace);
					tw.WriteAttributeString ("type", GetQualifiedXmlType (tw, interf, null));
					tw.WriteEndElement ();
				}
			}
			else if (type.IsValueType)
			{
				tw.WriteStartElement ("suds", "struct", MetaData.SudsNamespace);
				tw.WriteAttributeString ("type", GetQualifiedXmlType (tw, type, null));
				if (type.BaseType != typeof(ValueType))
					tw.WriteAttributeString ("extends", GetQualifiedXmlType (tw, type.BaseType, null));
			}
			else
			{
				tw.WriteStartElement ("suds", "class", MetaData.SudsNamespace);
				tw.WriteAttributeString ("type", GetQualifiedXmlType (tw, type, null));
				
				if (IsService (type))
				{
					if (type.IsMarshalByRef)
						tw.WriteAttributeString ("rootType", "MarshalByRefObject");
					else
						tw.WriteAttributeString ("rootType", "Delegate");
					
					if (type.BaseType != typeof(MarshalByRefObject))
						tw.WriteAttributeString ("extends", GetQualifiedXmlType (tw, type.BaseType, null));
						
					if (type.IsMarshalByRef) {
						foreach (Type interf in type.GetInterfaces ()) {
							tw.WriteStartElement ("suds","implements", MetaData.SudsNamespace);
							tw.WriteAttributeString ("type", GetQualifiedXmlType (tw, interf, null));
							tw.WriteEndElement ();
						}
					}
				}
				else if (typeof(ISerializable).IsAssignableFrom (type))
					tw.WriteAttributeString ("rootType", "ISerializable");
					
			}
			tw.WriteEndElement ();	// suds
		}
		
		void WriteMessageBindingBody (XmlTextWriter tw, Type t)
		{
			tw.WriteStartElement ("soap", "body", MetaData.SoapNamespace);
			tw.WriteAttributeString ("use", "encoded");
			tw.WriteAttributeString ("encodingStyle", MetaData.SoapEncodingNamespace);
			tw.WriteAttributeString ("namespace", GetXmlNamespace (t, null));
			tw.WriteEndElement ();
		}
		
		void WritePart (XmlTextWriter tw, string name, Type t, Type containerType)
		{
			tw.WriteStartElement ("part", MetaData.WsdlNamespace);
			tw.WriteAttributeString ("name", name);
			tw.WriteAttributeString ("type", GetQualifiedXmlType (tw, t, containerType));
			tw.WriteEndElement ();
		}
		
		void WriteDataTypeSchema (XmlTextWriter tw, Type type)
		{
			if (type.IsArray)
				WriteArraySchema (tw, type);
			else if (type.IsEnum)
				WriteEnumSchema (tw, type);
			else
				WriteClassSchema (tw, type);
		}
		
		void WriteArraySchema (XmlTextWriter tw, Type type)
		{
			tw.WriteStartElement ("complexType", MetaData.SchemaNamespace);
			tw.WriteAttributeString ("name", GetXmlType (type));
			tw.WriteStartElement ("complexContent", MetaData.SchemaNamespace);
			tw.WriteStartElement ("restriction", MetaData.SchemaNamespace);
			tw.WriteAttributeString ("base", GetQualifiedName (tw, MetaData.SoapEncodingNamespace, "Array"));
			tw.WriteStartElement ("attribute", MetaData.SchemaNamespace);
			tw.WriteAttributeString ("ref", GetQualifiedName (tw, MetaData.SoapEncodingNamespace, "arrayType"));
			
			string arrayType = "";
			
			while (type.IsArray)
			{
				arrayType = arrayType + "[" + new string (',', type.GetArrayRank()-1) + "]";
				type = type.GetElementType ();
			}
			arrayType = GetQualifiedXmlType (tw, type, null) + arrayType;
			
			tw.WriteAttributeString ("wsdl", "arrayType", MetaData.WsdlNamespace, arrayType);
			tw.WriteEndElement ();	// attribute
			tw.WriteEndElement ();  // restriction
			tw.WriteEndElement ();  // complexContent
			tw.WriteEndElement ();  // complexType
		}
		
		void WriteEnumSchema (XmlTextWriter tw, Type type)
		{
			tw.WriteStartElement ("simpleType", MetaData.SchemaNamespace);
			tw.WriteAttributeString ("name", GetXmlType (type));
			tw.WriteAttributeString ("suds", "enumType", MetaData.SudsNamespace, GetQualifiedXmlType (tw, EnumToUnderlying (type), null));
			tw.WriteStartElement ("restriction", MetaData.SchemaNamespace);
			tw.WriteAttributeString ("base", "xsd:string");
			
			foreach (string name in Enum.GetNames (type))
			{
				tw.WriteStartElement ("enumeration", MetaData.SchemaNamespace);
				tw.WriteAttributeString ("value", name);
				tw.WriteEndElement ();
			}
			tw.WriteEndElement ();  // restriction
			tw.WriteEndElement ();  // simpleType
		}
		
		void WriteClassSchema (XmlTextWriter tw, Type type)
		{
			tw.WriteStartElement ("element", MetaData.SchemaNamespace);
			tw.WriteAttributeString ("name", type.Name);
			tw.WriteAttributeString ("type", GetQualifiedXmlType (tw, type, null));
			tw.WriteEndElement ();
			
			tw.WriteStartElement ("complexType", MetaData.SchemaNamespace);
			tw.WriteAttributeString ("name", GetXmlType (type));
			if (type.BaseType != null && type.BaseType != typeof(object) && type.BaseType != typeof(ValueType))
				tw.WriteAttributeString ("base", GetQualifiedXmlType (tw, type.BaseType, null));
			
			FieldInfo[] fields = type.GetFields (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
			
			// Element fields
			
			bool elemsStart = false;
			foreach (FieldInfo fi in fields)
			{
				SoapFieldAttribute att = (SoapFieldAttribute) InternalRemotingServices.GetCachedSoapAttribute (fi);
				if (att.UseAttribute) continue;
			
				if (!elemsStart) { tw.WriteStartElement ("all", MetaData.SchemaNamespace); elemsStart = true; }
				tw.WriteStartElement ("element", MetaData.SchemaNamespace);
				tw.WriteAttributeString ("name", att.XmlElementName);
				tw.WriteAttributeString ("type", GetQualifiedXmlType (tw, fi.FieldType, type));
				tw.WriteEndElement ();
			}
			if (elemsStart) tw.WriteEndElement ();	// all
			
			// Attribute fields
			
			foreach (FieldInfo fi in fields)
			{
				SoapFieldAttribute att = (SoapFieldAttribute) InternalRemotingServices.GetCachedSoapAttribute (fi);
				if (!att.UseAttribute) continue;
			
				tw.WriteStartElement ("attribute", MetaData.SchemaNamespace);
				tw.WriteAttributeString ("name", att.XmlElementName);
				tw.WriteAttributeString ("type", GetQualifiedXmlType (tw, fi.FieldType, type));
				tw.WriteEndElement ();
			}
			
			tw.WriteEndElement ();	// complexType
			
		}
		
		ArrayList FindServices (ServiceType[] servicetypes)
		{
			ArrayList list = new ArrayList ();
			foreach (ServiceType st in servicetypes)
				if (IsService (st.ObjectType)) list.Add (st);
			return list;
		}
		
		string GetSoapAction (MethodInfo mb)
		{
			return SoapServices.GetSoapActionFromMethodBase (mb);
		}
		
		string GetXmlNamespace (Type t, Type containerType)
		{
			string name, ns;
			
			if (t.IsArray)
			{
				return GetXmlNamespace (containerType, null);
			}

			if (SoapServices.GetXmlTypeForInteropType (t, out name, out ns))
				return ns;
				
			SoapTypeAttribute att = (SoapTypeAttribute) InternalRemotingServices.GetCachedSoapAttribute (t);
			return att.XmlNamespace;
		}
		
		string GetQualifiedName (XmlTextWriter tw, string namspace, string localName)
		{
			return tw.LookupPrefix (namspace) + ":" + localName;
		}
		
		string GetQualifiedXmlType (XmlTextWriter tw, Type type, Type containerType)
		{
			string name, ns;
			
			if (type.IsArray)
			{
				name = GetXmlType (type);
				ns = GetXmlNamespace (type, containerType);
			}
			else
			{
				name = GetXsdType (type);					
				if (name != null) return "xsd:" + name;
				
				if (!SoapServices.GetXmlTypeForInteropType (type, out name, out ns))
				{
					SoapTypeAttribute att = (SoapTypeAttribute) InternalRemotingServices.GetCachedSoapAttribute (type);
					name = att.XmlTypeName;
					ns = att.XmlNamespace;
				}
			}
			
			return GetQualifiedName (tw, ns, name);
		}
		
		string GetXmlType (Type type)
		{
			if (type.IsArray)
			{
				string itemType = GetXmlType (type.GetElementType ());
				itemType = "ArrayOf" + char.ToUpper (itemType[0]) + itemType.Substring (1);
				if (type.GetArrayRank () > 1) itemType += type.GetArrayRank ();
				return itemType;
			}
			else
			{
				string name = null, ns;
				
				name = GetXsdType (type);
				if (name != null) return name;
				
				if (SoapServices.GetXmlTypeForInteropType (type, out name, out ns))
					return name;
					
				SoapTypeAttribute att = (SoapTypeAttribute) InternalRemotingServices.GetCachedSoapAttribute (type);
				return att.XmlTypeName;
			}
		}
		
		string GetServiceName (Type t)
		{
			return t.Name + "Service";
		}
		
		string GetPortName (Type t)
		{
			return t.Name + "Port";
		}
		
		string GetBindingName (Type t)
		{
			return t.Name + "Binding";
		}
		
		void FindTypes (ServiceType[] servicetypes, Hashtable dataTypes, ArrayList services)
		{
			ArrayList mbrTypes = new ArrayList();
			
			foreach (ServiceType st in servicetypes)
				FindDataTypes (st.ObjectType, null, dataTypes, mbrTypes);
				
			foreach (Type mbrType in mbrTypes)
			{
				ServiceType stFound = null;
				foreach (ServiceType st in servicetypes)
					if (mbrType == st.ObjectType) stFound = st;
					
				if (stFound != null) services.Add (stFound);
				else services.Add (new ServiceType (mbrType));
			}
		}
		
		void FindDataTypes (Type t, Type containerType, Hashtable types, ArrayList services)
		{
			if (IsSystemType (t))
			{
				string ns = GetXmlNamespace (t, null);
				types [ns] = null;
				return;
			}
			
			if (!IsService (t))
			{
				if (!t.IsSerializable) return;
				
				string ns = GetXmlNamespace (t, containerType);
				SchemaInfo sinfo = (SchemaInfo) types [ns];
				if (sinfo == null)
				{
					sinfo = new SchemaInfo ();
					types [ns] = sinfo;
				}
				
				if (sinfo.Types.Contains (t)) return;
				
				sinfo.Types.Add (t);
				if (t.IsArray) return;
				
				FieldInfo[] fields = t.GetFields (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
				foreach (FieldInfo fi in fields)
				{
					string fns = GetXmlNamespace (fi.FieldType, t);
					if (!sinfo.Imports.Contains (fns)) sinfo.Imports.Add (fns);
					FindDataTypes (fi.FieldType, t, types, services);
				}
			}
			else
			{
				if (services.Contains (t)) return;
				services.Add (t);
				
				foreach (MethodInfo met in t.GetMethods (BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
				{
					ParameterInfo[] pars = met.GetParameters ();
					foreach (ParameterInfo par in pars)
						FindDataTypes (par.ParameterType, t, types, services);
							
					FindDataTypes (met.ReturnType, t, types, services);
				}
			}
		}
		
		bool IsService (Type t)
		{
			return t.IsMarshalByRef || t.IsInterface || typeof(Delegate).IsAssignableFrom (t);
		}
		
		bool IsSystemType (Type t)
		{
			return t.FullName.StartsWith ("System.") && !t.IsArray;
		}
		
		static string GetXsdType (Type type)
		{
			if (type.IsEnum) return null;
			
			switch (Type.GetTypeCode (type))
			{
				case TypeCode.Boolean: return "boolean";
				case TypeCode.Byte: return "unsignedByte";
				case TypeCode.Char: return "char";
				case TypeCode.DateTime: return "dateTime";
				case TypeCode.Decimal: return "decimal";
				case TypeCode.Double: return "double";
				case TypeCode.Int16: return "short";
				case TypeCode.Int32: return "int";
				case TypeCode.Int64: return "long";
				case TypeCode.SByte: return "byte";
				case TypeCode.Single: return "float";
				case TypeCode.UInt16: return "unsignedShort";
				case TypeCode.UInt32: return "unsignedInt";
				case TypeCode.UInt64: return "unsignedLong";
				case TypeCode.String: return "string";
			}
			
			if (type == typeof (TimeSpan))
				return "duration";
			if (type == typeof (object))
				return "anyType";
				
			return null;
		}
		
		//
		// This is needed, because enumerations from assemblies
		// do not report their underlyingtype, but they report
		// themselves
		//
		public static Type EnumToUnderlying (Type t)
		{
			TypeCode tc = Type.GetTypeCode (t);
	
			switch (tc){
			case TypeCode.Boolean:
				return typeof (bool);
			case TypeCode.Byte:
				return typeof (byte);
			case TypeCode.SByte:
				return typeof (sbyte);
			case TypeCode.Int16:
				return typeof (short);
			case TypeCode.UInt16:
				return typeof (ushort);
			case TypeCode.Int32:
				return typeof (int);
			case TypeCode.UInt32:
				return typeof (uint);
			case TypeCode.Int64:
				return typeof (long);
			case TypeCode.UInt64:
				return typeof (ulong);
			}
			throw new Exception ("Unhandled typecode in enum " + tc + " from " + t.AssemblyQualifiedName);
		}
	}
	
	class SchemaInfo
	{
		public ArrayList Types = new ArrayList ();
		public ArrayList Imports = new ArrayList ();
		public bool SudsGenerated;
	}
}
