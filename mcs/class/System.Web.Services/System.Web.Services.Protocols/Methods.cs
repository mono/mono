//
// Methods.cs: Information about a method and its mapping to a SOAP web service.
//
// Author:
//   Miguel de Icaza
//
// (C) 2003 Ximian, Inc.
//
// TODO:
//    
//

using System.Reflection;
using System.Collections;
using System.Xml;
using System.Xml.Serialization;
using System.Web.Services;
using System.Web.Services.Description;

namespace System.Web.Services.Protocols {

	//
	// This class represents all the information we extract from a MethodInfo
	// in the SoapHttpClientProtocol derivative stub class
	//
	internal class MethodStubInfo {
		internal LogicalMethodInfo MethodInfo;

		// The name used bythe stub class to reference this method.
		internal string Name;
		
		internal string Action;
		internal string Binding;

		// The name/namespace of the request 
		internal string RequestName;
		internal string RequestNamespace;

		// The name/namespace of the response.
		internal string ResponseName;
		internal string ResponseNamespace;
		
		internal bool   OneWay;
		internal SoapParameterStyle ParameterStyle;

		internal XmlSerializer RequestSerializer;
		internal XmlSerializer ResponseSerializer;

		internal HeaderInfo[] Headers;

		//
		// Constructor
		//
		MethodStubInfo (TypeStubInfo parent, LogicalMethodInfo source, object kind, XmlReflectionImporter xmlImporter, SoapReflectionImporter soapImporter)
		{
			MethodInfo = source;

			XmlElementAttribute optional_ns = null;
			SoapBindingUse use;
			
			if (kind is SoapDocumentMethodAttribute){
				SoapDocumentMethodAttribute dma = (SoapDocumentMethodAttribute) kind;
				
				use = dma.Use;
				if (use == SoapBindingUse.Default)
					use = parent.Use;
				
				Action = dma.Action;
				Binding = dma.Binding;
				RequestName = dma.RequestElementName;
				RequestNamespace = dma.RequestNamespace;
				ResponseName = dma.ResponseElementName;
				ResponseNamespace = dma.ResponseNamespace;
				ParameterStyle = dma.ParameterStyle;
				if (ParameterStyle == SoapParameterStyle.Default)
					ParameterStyle = parent.ParameterStyle;
				OneWay = dma.OneWay;
			} else {
				SoapRpcMethodAttribute rma = (SoapRpcMethodAttribute) kind;
				use = SoapBindingUse.Encoded;	// RPC always use encoded

				Action = rma.Action;
				Binding = rma.Binding;
				RequestName = rma.RequestElementName;
				RequestNamespace = rma.RequestNamespace;
				ResponseNamespace = rma.ResponseNamespace;
				ResponseName = rma.ResponseElementName;
				OneWay = rma.OneWay;

				// For RPC calls, make all arguments be part of the empty namespace
				optional_ns = new XmlElementAttribute ();
				optional_ns.Namespace = "";
			}
			if (Binding == "")
				Binding = parent.BindingName;
			if (RequestName == "")
				RequestName = source.Name;
			
			if (OneWay){
				if (source.ReturnType != typeof (void))
					throw new Exception ("OneWay methods should not have a return value");
				if (source.OutParameters.Length != 0)
					throw new Exception ("OneWay methods should not have out/ref parameters");
			}
			
			object [] o = source.GetCustomAttributes (typeof (WebMethodAttribute));
			if (o.Length == 1){
				WebMethodAttribute wma = (WebMethodAttribute) o [0];

				Name = wma.MessageName;
				if (Name == "")
					Name = source.Name;
			} else
				Name = source.Name;

			if (ResponseName == "")
				ResponseName = Name + "Response";

			XmlReflectionMember [] in_members = BuildRequestReflectionMembers (optional_ns);
			XmlReflectionMember [] out_members = BuildResponseReflectionMembers (optional_ns);

			XmlMembersMapping [] members = new XmlMembersMapping [2];
			try {
				if (use == SoapBindingUse.Literal) {
					members [0] = xmlImporter.ImportMembersMapping (RequestName, RequestNamespace, in_members, true);
					members [1] = xmlImporter.ImportMembersMapping (ResponseName, ResponseNamespace, out_members, true);
				}
				else {
					members [0] = soapImporter.ImportMembersMapping (RequestName, RequestNamespace, in_members, true, true);
					members [1] = soapImporter.ImportMembersMapping (ResponseName, ResponseNamespace, out_members, true, true);
				}

				XmlSerializer [] s = null;
				s = XmlSerializer.FromMappings (members);
				RequestSerializer = s [0];
				ResponseSerializer = s [1];
			} catch {
				Console.WriteLine ("Got exception while creating serializer");
				Console.WriteLine ("Method name: " + RequestName + " parameters are:");

				for (int i = 0; i < in_members.Length; i++) {
					Console.WriteLine ("    {0}: {1} {2}", i, in_members [i].MemberName, in_members [i].MemberType);
				}

				Console.WriteLine ("Output parameters are:");
				for (int i = 0; i < out_members.Length; i++) {
					Console.WriteLine ("    {0}: {1} {2}", i, out_members [i].MemberName, out_members [i].MemberType);
				}
				throw;
			}

			o = source.GetCustomAttributes (typeof (SoapHeaderAttribute));
			Headers = new HeaderInfo[o.Length];
			for (int i = 0; i < o.Length; i++) {
				SoapHeaderAttribute att = (SoapHeaderAttribute) o[i];
				MemberInfo[] mems = source.DeclaringType.GetMember (att.MemberName);
				if (mems.Length == 0) throw new InvalidOperationException ("Member " + att.MemberName + " not found in class " + source.DeclaringType.FullName);
				
				Type headerType = (mems[0] is FieldInfo) ? ((FieldInfo)mems[0]).FieldType : ((PropertyInfo)mems[0]).PropertyType;
				Headers [i] = new HeaderInfo (parent.GetCommonSerializer (headerType), mems[0], att);
			}
		}

		static internal MethodStubInfo Create (TypeStubInfo parent, LogicalMethodInfo lmi, XmlReflectionImporter xmlImporter, SoapReflectionImporter soapImporter)
		{
			object [] o = lmi.GetCustomAttributes (typeof (SoapDocumentMethodAttribute));
			if (o.Length == 0){
				o = lmi.GetCustomAttributes (typeof (SoapRpcMethodAttribute));
				if (o.Length == 0)
					return null;
				return new MethodStubInfo (parent, lmi, o [0], xmlImporter, soapImporter);
			} else 
				return new MethodStubInfo (parent, lmi, o [0], xmlImporter, soapImporter);
		}

		XmlReflectionMember [] BuildRequestReflectionMembers (XmlElementAttribute optional_ns)
		{
			ParameterInfo [] input = MethodInfo.InParameters;
			XmlReflectionMember [] in_members = new XmlReflectionMember [input.Length];

			for (int i = 0; i < input.Length; i++)
			{
				XmlReflectionMember m = new XmlReflectionMember ();
				m.IsReturnValue = false;
				m.MemberName = input [i].Name;
				m.MemberType = input [i].ParameterType;

				m.XmlAttributes = new XmlAttributes (input[i]);
				m.SoapAttributes = new SoapAttributes (input[i]);

				if (m.MemberType.IsByRef)
					m.MemberType = m.MemberType.GetElementType ();
				if (optional_ns != null)
					m.XmlAttributes.XmlElements.Add (optional_ns);
				in_members [i] = m;
			}
			return in_members;
		}
		
		XmlReflectionMember [] BuildResponseReflectionMembers (XmlElementAttribute optional_ns)
		{
			ParameterInfo [] output = MethodInfo.OutParameters;
			bool has_return_value = !(OneWay || MethodInfo.ReturnType == typeof (void));
			XmlReflectionMember [] out_members = new XmlReflectionMember [(has_return_value ? 1 : 0) + output.Length];
			XmlReflectionMember m;
			int idx = 0;

			if (has_return_value)
			{
				m = new XmlReflectionMember ();
				m.IsReturnValue = true;
				m.MemberName = RequestName + "Result";
				m.MemberType = MethodInfo.ReturnType;

				m.XmlAttributes = new XmlAttributes (MethodInfo.ReturnTypeCustomAttributeProvider);
				m.SoapAttributes = new SoapAttributes (MethodInfo.ReturnTypeCustomAttributeProvider);

				if (optional_ns != null)
					m.XmlAttributes.XmlElements.Add (optional_ns);
				idx++;
				out_members [0] = m;
			}
			
			for (int i = 0; i < output.Length; i++)
			{
				m = new XmlReflectionMember ();
				m.IsReturnValue = false;
				m.MemberName = output [i].Name;
				m.MemberType = output [i].ParameterType;
				m.XmlAttributes = new XmlAttributes (output[i]);
				m.SoapAttributes = new SoapAttributes (output[i]);

				if (m.MemberType.IsByRef)
					m.MemberType = m.MemberType.GetElementType ();
				if (optional_ns != null)
					m.XmlAttributes.XmlElements.Add (optional_ns);
				out_members [i + idx] = m;
			}
			return out_members;
		}
	}

	internal class HeaderInfo
	{
		internal XmlSerializer Serializer;
		internal MemberInfo Member;
		internal SoapHeaderAttribute AttributeInfo;

		public HeaderInfo (XmlSerializer serializer, MemberInfo member, SoapHeaderAttribute attributeInfo)
		{
			Serializer = serializer;
			Member = member;
			AttributeInfo = attributeInfo;
		}
		
		public object GetHeaderValue (object ob)
		{
			if (Member is PropertyInfo) return ((PropertyInfo)Member).GetValue (ob, null);
			else return ((FieldInfo)Member).GetValue (ob);
		}
	}

	internal class Fault
	{
		public XmlQualifiedName faultcode;
		public string faultstring;
		public string faultactor;
		public XmlNode detail;
	}

	//
	// Holds the metadata loaded from the type stub, as well as
	// the metadata for all the methods in the type
	//
	internal class TypeStubInfo {
		Hashtable name_to_method = new Hashtable ();
		Hashtable common_serializers = new Hashtable ();

		// Precomputed
		internal SoapParameterStyle      ParameterStyle;
		internal SoapServiceRoutingStyle RoutingStyle;
		internal SoapBindingUse          Use;
		internal string                  WebServiceName;
		internal string                  WebServiceNamespace;
		internal string                  BindingLocation;
		internal string                  BindingName;
		internal string                  BindingNamespace;
		internal XmlSerializer           FaultSerializer;

		void GetTypeAttributes (Type t)
		{
			object [] o;

			o = t.GetCustomAttributes (typeof (WebServiceBindingAttribute), false);
			if (o.Length != 1)
				throw new Exception ("Expected WebServiceBindingAttribute on "+ t.Name);
			WebServiceBindingAttribute b = (WebServiceBindingAttribute) o [0];
			BindingLocation = b.Location;
			BindingName = b.Name;
			BindingNamespace = b.Namespace;

			o = t.GetCustomAttributes (typeof (WebService), false);
			if (o.Length == 1){
				WebServiceAttribute a = (WebServiceAttribute) o [0];

				WebServiceName = a.Name;
				WebServiceNamespace = a.Namespace;
			} else {
				WebServiceName = t.Name;
				WebServiceNamespace = WebServiceAttribute.DefaultNamespace;
			}

			o = t.GetCustomAttributes (typeof (SoapDocumentServiceAttribute), false);
			if (o.Length == 1){
				SoapDocumentServiceAttribute a = (SoapDocumentServiceAttribute) o [0];

				ParameterStyle = a.ParameterStyle;
				RoutingStyle = a.RoutingStyle;
				Use = a.Use;
			} else {
				o = t.GetCustomAttributes (typeof (SoapRpcServiceAttribute), false);
				if (o.Length == 1){
					SoapRpcServiceAttribute srs = (SoapRpcServiceAttribute) o [0];
					
					ParameterStyle = SoapParameterStyle.Wrapped;
					RoutingStyle = srs.RoutingStyle;
					Use = SoapBindingUse.Literal;
				} else {
					ParameterStyle = SoapParameterStyle.Wrapped;
					RoutingStyle = SoapServiceRoutingStyle.SoapAction;
					Use = SoapBindingUse.Literal;
				}
			}
			FaultSerializer = GetCommonSerializer (typeof(Fault));
		}

		//
		// Extract all method information
		//
		void GetTypeMethods (Type t, XmlReflectionImporter xmlImporter, SoapReflectionImporter soapImporter)
		{
			MethodInfo [] type_methods = t.GetMethods (BindingFlags.Instance | BindingFlags.Public);
			LogicalMethodInfo [] methods = LogicalMethodInfo.Create (type_methods, LogicalMethodTypes.Sync);

			foreach (LogicalMethodInfo mi in methods){
				MethodStubInfo msi = MethodStubInfo.Create (this, mi, xmlImporter, soapImporter);

				if (msi == null)
					continue;

				name_to_method [msi.Name] = msi;
			}
		}
		
		internal TypeStubInfo (Type t)
		{
			GetTypeAttributes (t);

			XmlReflectionImporter xmlImporter = new XmlReflectionImporter ();
			SoapReflectionImporter soapImporter = new SoapReflectionImporter ();
			GetTypeMethods (t, xmlImporter, soapImporter);
		}

		internal MethodStubInfo GetMethod (string name)
		{
			return (MethodStubInfo) name_to_method [name];
		}

		internal XmlSerializer GetCommonSerializer (Type type)
		{
			XmlSerializer s = (XmlSerializer) common_serializers [type];
			if (s != null) return s;
			s = new XmlSerializer (type);
			common_serializers [type] = s;
			return s;
		}
	}
	
	//
	// Manages 
	//
	internal class TypeStubManager {
		static Hashtable type_to_manager;
		
		static TypeStubManager ()
		{
			type_to_manager = new Hashtable ();
		}

		//
		// This needs to be thread safe
		//
		static internal TypeStubInfo GetTypeStub (Type t)
		{
			TypeStubInfo tm = (TypeStubInfo) type_to_manager [t];

			if (tm != null)
				return tm;

			lock (typeof (TypeStubInfo)){
				tm = (TypeStubInfo) type_to_manager [t];

				if (tm != null)
					return tm;
				
				tm = new TypeStubInfo (t);
				type_to_manager [t] = tm;

				return tm;
			}
		}
	}
}
