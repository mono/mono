//
// Methods.cs: Information about a method and its mapping to a SOAP web service.
//
// Author:
//   Miguel de Icaza
//
// (C) 2003 Ximian, Inc.
//
//

using System.Reflection;
using System.Collections;
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
		internal SoapBindingUse Use;

		internal XmlSerializer RequestSerializer;
		internal XmlSerializer ResponseSerializer;
		
		//
		// Constructor
		//
		MethodStubInfo (TypeStubInfo parent, LogicalMethodInfo source, SoapDocumentMethodAttribute dma, XmlReflectionImporter importer)
		{
			MethodInfo = source;

			Use = dma.Use;
			if (Use == SoapBindingUse.Default)
				Use = parent.Use;
			if (Use != SoapBindingUse.Literal)
				throw new Exception ("Only SoapBindingUse.Literal supported");
			
			OneWay = dma.OneWay;
			if (OneWay){
				if (source.ReturnType != typeof (void))
					throw new Exception ("OneWay methods should not have a return value");
				if (source.OutParameters.Length != 0)
					throw new Exception ("OneWay methods should not have out/ref parameters");
			}
			
			Action = dma.Action;
			Binding = (dma.Binding == "" ? parent.BindingName : dma.Binding);
			RequestName = dma.RequestElementName;
			if (RequestName == "")
				RequestName = source.Name;
			
			RequestNamespace = dma.RequestNamespace;
			ResponseName = dma.ResponseElementName;
			ResponseNamespace = dma.ResponseNamespace;
			ParameterStyle = dma.ParameterStyle;
			if (ParameterStyle == SoapParameterStyle.Default)
				ParameterStyle = parent.ParameterStyle;

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
			
			MakeRequestSerializer (importer);
			MakeResponseSerializer (importer);
		}

		static internal MethodStubInfo Create (TypeStubInfo parent, LogicalMethodInfo lmi, XmlReflectionImporter importer)
		{
			object [] o = lmi.GetCustomAttributes (typeof (SoapDocumentMethodAttribute));
			if (o.Length == 0){
				lmi.GetCustomAttributes (typeof (SoapRpcMethodAttribute));
				if (o.Length != 0)
					throw new Exception ("SoapRpcMethod not supported, only SoapDocumentMethod");
				return null;
			}

			return new MethodStubInfo (parent, lmi, (SoapDocumentMethodAttribute) o [0], importer);
		}

		void MakeRequestSerializer (XmlReflectionImporter importer)
		{
			ParameterInfo [] input = MethodInfo.InParameters;
			XmlReflectionMember [] in_members = new XmlReflectionMember [input.Length];
			
			for (int i = 0; i < input.Length; i++){
				XmlReflectionMember m = new XmlReflectionMember ();
				m.IsReturnValue = false;
				m.MemberName = input [i].Name;
				m.MemberType = input [i].ParameterType;

				in_members [i] = m;
			}

			TypeStubInfo.SingleMembersMapping [0] =
				importer.ImportMembersMapping (RequestName, RequestNamespace, in_members, true);
			XmlSerializer [] s = XmlSerializer.FromMappings (TypeStubInfo.SingleMembersMapping);
			RequestSerializer = s [0];
		}

		void MakeResponseSerializer (XmlReflectionImporter importer)
		{
			ParameterInfo [] output = MethodInfo.OutParameters;
			XmlReflectionMember [] out_members = new XmlReflectionMember [(OneWay ? 0 : 1) + output.Length];
			XmlReflectionMember m;
			int idx = 0;

			if (!OneWay){
				m = new XmlReflectionMember ();
				m.IsReturnValue = true;
				m.MemberName = RequestName + "Result";
				m.MemberType = MethodInfo.ReturnType;
				idx++;
				out_members [0] = m;
			}
			
			for (int i = 0; i < output.Length; i++){
				m = new XmlReflectionMember ();
				m.IsReturnValue = false;
				m.MemberName = output [i].Name;
				m.MemberType = output [i].ParameterType;

				if (m.MemberType.IsByRef)
					m.MemberType = m.MemberType.GetElementType ();
				out_members [i + idx] = m;
			}

			TypeStubInfo.SingleMembersMapping [0] =
				importer.ImportMembersMapping (RequestName, RequestNamespace, out_members, true);
			XmlSerializer [] s = XmlSerializer.FromMappings (TypeStubInfo.SingleMembersMapping);
			ResponseSerializer = s [0];
		}
	}

	//
	// Holds the metadata loaded from the type stub, as well as
	// the metadata for all the methods in the type
	//
	internal class TypeStubInfo {
		Hashtable name_to_method = new Hashtable ();

		// Precomputed
		internal SoapParameterStyle      ParameterStyle;
		internal SoapServiceRoutingStyle RoutingStyle;
		internal SoapBindingUse          Use;
		internal string                  WebServiceName;
		internal string                  WebServiceNamespace;
		internal string                  BindingLocation;
		internal string                  BindingName;
		internal string                  BindingNamespace;

		// We only use this to avoid creating tons of arrays that are barely used.
		internal static XmlMembersMapping [] SingleMembersMapping = new XmlMembersMapping [1];
		
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
				ParameterStyle = SoapParameterStyle.Wrapped;
				RoutingStyle = SoapServiceRoutingStyle.SoapAction;
				Use = SoapBindingUse.Literal;
			}

			//
			// Some validation
			//
			o = t.GetCustomAttributes (typeof (SoapRpcServiceAttribute), false);
			if (o.Length != 0)
				throw new Exception ("We do not support SoapRpcService encoding, Type=" + t.Name);
		}

		//
		// Extract all method information
		//
		void GetTypeMethods (Type t, XmlReflectionImporter importer)
		{
			MethodInfo [] type_methods = t.GetMethods (BindingFlags.Instance | BindingFlags.Public);
			LogicalMethodInfo [] methods = LogicalMethodInfo.Create (type_methods, LogicalMethodTypes.Sync);

			foreach (LogicalMethodInfo mi in methods){
				MethodStubInfo msi = MethodStubInfo.Create (this, mi, importer);

				if (msi == null)
					continue;
				
				name_to_method [msi.Name] = msi;
			}
		}
		
		internal TypeStubInfo (Type t)
		{
			GetTypeAttributes (t);

			XmlReflectionImporter importer = new XmlReflectionImporter ();
			GetTypeMethods (t, importer);
		}

		internal MethodStubInfo GetMethod (string name)
		{
			return (MethodStubInfo) name_to_method [name];
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
