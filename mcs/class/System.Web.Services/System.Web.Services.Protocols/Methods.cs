//
// Methods.cs: Information about a method and its mapping to a SOAP web service.
//
// Author:
//   Miguel de Icaza
//   Lluis Sanchez Gual (lluis@ximian.com)
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
		
		internal WebMethodAttribute MethodAttribute;
		internal bool OneWay;
		internal SoapParameterStyle ParameterStyle;
		internal SoapBindingStyle SoapBindingStyle;
		internal SoapBindingUse Use;

		internal bool BufferResponse;
		internal int CacheDuration;
		internal string Description;
		internal bool EnableSession;

		internal XmlSerializer RequestSerializer;
		internal XmlSerializer ResponseSerializer;

		internal HeaderInfo[] Headers;

		internal SoapExtensionRuntimeConfig[] SoapExtensions;
		
		private XmlMembersMapping[] _membersMapping;
		
		internal XmlMembersMapping InputMembersMapping
		{
			get { return _membersMapping[0]; }
		}

		internal XmlMembersMapping OutputMembersMapping
		{
			get { return _membersMapping[1]; }
		}

		//
		// Constructor
		//
		MethodStubInfo (TypeStubInfo parent, LogicalMethodInfo source, object kind, XmlReflectionImporter xmlImporter, SoapReflectionImporter soapImporter)
		{
			MethodInfo = source;

			XmlElementAttribute optional_ns = null;

			if (kind == null) {
				Use = parent.Use;
				RequestName = "";
				RequestNamespace = parent.WebServiceNamespace;
				ResponseName = "";
				ResponseNamespace = parent.WebServiceNamespace;
				ParameterStyle = parent.ParameterStyle;
				SoapBindingStyle = parent.SoapBindingStyle;
				OneWay = false;
			}
			else if (kind is SoapDocumentMethodAttribute){
				SoapDocumentMethodAttribute dma = (SoapDocumentMethodAttribute) kind;
				
				Use = dma.Use;
				if (Use == SoapBindingUse.Default)
					Use = parent.Use;
				
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
				SoapBindingStyle = SoapBindingStyle.Document;
			} else {
				SoapRpcMethodAttribute rma = (SoapRpcMethodAttribute) kind;
				Use = SoapBindingUse.Encoded;	// RPC always use encoded

				Action = rma.Action;
				Binding = rma.Binding;
				RequestName = rma.RequestElementName;
				RequestNamespace = rma.RequestNamespace;
				ResponseNamespace = rma.ResponseNamespace;
				ResponseName = rma.ResponseElementName;
				ParameterStyle = SoapParameterStyle.Wrapped;
				OneWay = rma.OneWay;
				SoapBindingStyle = SoapBindingStyle.Rpc;

				// For RPC calls, make all arguments be part of the empty namespace
				optional_ns = new XmlElementAttribute ();
				optional_ns.Namespace = "";
			}

			if (OneWay){
				if (source.ReturnType != typeof (void))
					throw new Exception ("OneWay methods should not have a return value");
				if (source.OutParameters.Length != 0)
					throw new Exception ("OneWay methods should not have out/ref parameters");
			}
			
			object [] o = source.GetCustomAttributes (typeof (WebMethodAttribute));
			if (o.Length == 1){
				MethodAttribute = (WebMethodAttribute) o [0];
				BufferResponse = MethodAttribute.BufferResponse;
				CacheDuration = MethodAttribute.CacheDuration;
				Description = MethodAttribute.Description;
				EnableSession = MethodAttribute.EnableSession;
				Name = MethodAttribute.MessageName;

				if (Name == "")
					Name = source.Name;
			} else
				Name = source.Name;

			if (RequestNamespace == "") RequestNamespace = parent.WebServiceNamespace;
			if (ResponseNamespace == "") ResponseNamespace = parent.WebServiceNamespace;
			if (RequestName == "") RequestName = Name;
			if (ResponseName == "")	ResponseName = Name + "Response";
			if (Binding == null) Binding = parent.DefaultBinding;
			else if (parent.GetBinding (Binding) == null) throw new InvalidOperationException ("Type '" + parent.Type + "' is missing WebServiceBinding attribute that defines a binding named '" + Binding + "'");
				
			if (Action == null || Action == "")
				Action = RequestNamespace.EndsWith("/") ? (RequestNamespace + Name) : (RequestNamespace + "/" + Name);
			
			bool hasWrappingElem = (ParameterStyle == SoapParameterStyle.Wrapped);
			
			XmlReflectionMember [] in_members = BuildRequestReflectionMembers (optional_ns);
			XmlReflectionMember [] out_members = BuildResponseReflectionMembers (optional_ns);

			_membersMapping = new XmlMembersMapping [2];

			if (Use == SoapBindingUse.Literal) {
				_membersMapping [0] = xmlImporter.ImportMembersMapping (RequestName, RequestNamespace, in_members, hasWrappingElem);
				_membersMapping [1] = xmlImporter.ImportMembersMapping (ResponseName, ResponseNamespace, out_members, hasWrappingElem);
			}
			else {
				_membersMapping [0] = soapImporter.ImportMembersMapping (RequestName, RequestNamespace, in_members, hasWrappingElem, true);
				_membersMapping [1] = soapImporter.ImportMembersMapping (ResponseName, ResponseNamespace, out_members, hasWrappingElem, true);
			}

			XmlSerializer [] s = null;
			s = XmlSerializer.FromMappings (_membersMapping);
			RequestSerializer = s [0];
			ResponseSerializer = s [1];

			o = source.GetCustomAttributes (typeof (SoapHeaderAttribute));
			Headers = new HeaderInfo[o.Length];
			for (int i = 0; i < o.Length; i++) {
				SoapHeaderAttribute att = (SoapHeaderAttribute) o[i];
				MemberInfo[] mems = source.DeclaringType.GetMember (att.MemberName);
				if (mems.Length == 0) throw new InvalidOperationException ("Member " + att.MemberName + " not found in class " + source.DeclaringType.FullName);
				
				Type headerType = (mems[0] is FieldInfo) ? ((FieldInfo)mems[0]).FieldType : ((PropertyInfo)mems[0]).PropertyType;
				Headers [i] = new HeaderInfo (mems[0], att);
				parent.RegisterHeaderType (headerType);
			}

			SoapExtensions = SoapExtension.GetMethodExtensions (source);
		}

		static internal MethodStubInfo Create (TypeStubInfo parent, LogicalMethodInfo lmi, XmlReflectionImporter xmlImporter, SoapReflectionImporter soapImporter)
		{
			object [] o = lmi.GetCustomAttributes (typeof (SoapDocumentMethodAttribute));
			if (o.Length == 0) o = lmi.GetCustomAttributes (typeof (SoapRpcMethodAttribute));

			if (o.Length == 0)
			{
				if (lmi.GetCustomAttributes (typeof (WebMethodAttribute)).Length == 0) return null;
				return new MethodStubInfo (parent, lmi, null, xmlImporter, soapImporter);
			}
			else
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

		public HeaderInfo GetHeaderInfo (Type headerType)
		{
			foreach (HeaderInfo headerInfo in Headers)
				if (headerInfo.HeaderType == headerType) return headerInfo;
			return null;
		}
	}

	internal class HeaderInfo
	{
		internal MemberInfo Member;
		internal SoapHeaderAttribute AttributeInfo;
		internal Type HeaderType;

		public HeaderInfo (MemberInfo member, SoapHeaderAttribute attributeInfo)
		{
			Member = member;
			AttributeInfo = attributeInfo;
			if (Member is PropertyInfo) HeaderType = ((PropertyInfo)Member).PropertyType;
			else HeaderType = ((FieldInfo)Member).FieldType;
		}
		
		public object GetHeaderValue (object ob)
		{
			if (Member is PropertyInfo) return ((PropertyInfo)Member).GetValue (ob, null);
			else return ((FieldInfo)Member).GetValue (ob);
		}

		public void SetHeaderValue (object ob, object value)
		{
			if (Member is PropertyInfo) ((PropertyInfo)Member).SetValue (ob, value, null);
			else ((FieldInfo)Member).SetValue (ob, value);
		}

		public SoapHeaderDirection Direction
		{
			get { return AttributeInfo.Direction; }
		}
	}

	internal class Fault
	{
		public Fault () {}

		public Fault (SoapException ex) 
		{
			faultcode = ex.Code;
			faultstring = ex.Message;
			faultactor = ex.Actor;
			detail = ex.Detail;
		}

		public XmlQualifiedName faultcode;
		public string faultstring;
		public string faultactor;
		public XmlNode detail;
	}
	
	internal class BindingInfo
	{
		public BindingInfo (WebServiceBindingAttribute at, string ns)
		{
			Name = at.Name;
			Namespace = at.Namespace;
			if (Namespace == "") Namespace = ns;
			Location = at.Location;
		}
		
		public BindingInfo (string name, string ns)
		{
			Name = name;
			Namespace = ns;
		}
		
		public string Name;
		public string Namespace;
		public string Location;
	}

	//
	// Holds the metadata loaded from the type stub, as well as
	// the metadata for all the methods in the type
	//
	internal class TypeStubInfo {
		Hashtable name_to_method = new Hashtable ();
		Hashtable header_serializers = new Hashtable ();
		Hashtable header_serializers_byname = new Hashtable ();
		ArrayList bindings = new ArrayList ();
		ArrayList methods = new ArrayList ();

		// Precomputed
		internal SoapParameterStyle      ParameterStyle;
		internal SoapServiceRoutingStyle RoutingStyle;
		internal SoapBindingUse          Use;
		internal string                  WebServiceName;
		internal string                  WebServiceNamespace;
		internal string                  DefaultBinding;
		internal string                  Description;
		internal XmlSerializer           FaultSerializer;
		internal SoapExtensionRuntimeConfig[][] SoapExtensions;
		internal SoapBindingStyle SoapBindingStyle;
		internal XmlReflectionImporter 	XmlImporter;
		internal SoapReflectionImporter SoapImporter;
		internal Type                   Type;

		void GetTypeAttributes (Type t)
		{
			object [] o;

			o = t.GetCustomAttributes (typeof (WebServiceAttribute), false);
			if (o.Length == 1){
				WebServiceAttribute a = (WebServiceAttribute) o [0];
				WebServiceName = (a.Name != string.Empty) ? a.Name : t.Name;
				WebServiceNamespace = (a.Namespace != string.Empty) ? a.Namespace : WebServiceAttribute.DefaultNamespace;
				Description = a.Description;
			} else {
				WebServiceName = t.Name;
				WebServiceNamespace = WebServiceAttribute.DefaultNamespace;
			}
			
			DefaultBinding = WebServiceName + "Soap";
			BindingInfo binfo = new BindingInfo (DefaultBinding, WebServiceNamespace);
			bindings.Add (binfo);

			o = t.GetCustomAttributes (typeof (WebServiceBindingAttribute), false);
			foreach (WebServiceBindingAttribute at in o)
				bindings.Add (new BindingInfo (at, WebServiceNamespace));

			o = t.GetCustomAttributes (typeof (SoapDocumentServiceAttribute), false);
			if (o.Length == 1){
				SoapDocumentServiceAttribute a = (SoapDocumentServiceAttribute) o [0];

				ParameterStyle = a.ParameterStyle;
				RoutingStyle = a.RoutingStyle;
				Use = a.Use;
				SoapBindingStyle = SoapBindingStyle.Document;
			} else {
				o = t.GetCustomAttributes (typeof (SoapRpcServiceAttribute), false);
				if (o.Length == 1){
					SoapRpcServiceAttribute srs = (SoapRpcServiceAttribute) o [0];
					
					ParameterStyle = SoapParameterStyle.Wrapped;
					RoutingStyle = srs.RoutingStyle;
					Use = SoapBindingUse.Encoded;
					SoapBindingStyle = SoapBindingStyle.Rpc;
				} else {
					ParameterStyle = SoapParameterStyle.Wrapped;
					RoutingStyle = SoapServiceRoutingStyle.SoapAction;
					Use = SoapBindingUse.Literal;
					SoapBindingStyle = SoapBindingStyle.Document;
				}
			}
			
			if (ParameterStyle == SoapParameterStyle.Default) ParameterStyle = SoapParameterStyle.Wrapped;
			if (Use == SoapBindingUse.Default) Use = SoapBindingUse.Literal;

			FaultSerializer = new XmlSerializer (typeof(Fault));
			SoapExtensions = SoapExtension.GetTypeExtensions (t);
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
				this.methods.Add (msi);
			}
		}
		
		internal TypeStubInfo (Type t)
		{
			this.Type = t;
			GetTypeAttributes (t);

			XmlImporter = new XmlReflectionImporter ();
			SoapImporter = new SoapReflectionImporter ();
			GetTypeMethods (t, XmlImporter, SoapImporter);
		}

		internal MethodStubInfo GetMethod (string name)
		{
			return (MethodStubInfo) name_to_method [name];
		}

		internal void RegisterHeaderType (Type type)
		{
			XmlSerializer s = (XmlSerializer) header_serializers [type];
			if (s != null) return;

			XmlReflectionImporter ri = new XmlReflectionImporter ();
			XmlTypeMapping tm = ri.ImportTypeMapping (type, WebServiceAttribute.DefaultNamespace);
			s = new XmlSerializer (tm);

			header_serializers [type] = s;
			header_serializers_byname [new XmlQualifiedName (tm.ElementName, tm.Namespace)] = s;
		}

		internal XmlSerializer GetHeaderSerializer (Type type)
		{
			return (XmlSerializer) header_serializers [type];
		}
	
		internal XmlSerializer GetHeaderSerializer (XmlQualifiedName qname)
		{
			return (XmlSerializer) header_serializers_byname [qname];
		}
		
		internal ArrayList Methods
		{
			get { return methods; }
		}

		internal ArrayList Bindings
		{
			get { return bindings; }
		}
		
		internal BindingInfo GetBinding (string name)
		{
			for (int n=0; n<bindings.Count; n++)
				if (((BindingInfo)bindings[n]).Name == name) return (BindingInfo)bindings[n];
			return null;
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
