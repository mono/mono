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
	// in the WebClientProtocol derivative stub class
	//
	internal class MethodStubInfo 
	{
		internal LogicalMethodInfo MethodInfo;
		internal TypeStubInfo TypeStub;

		// The name used by the stub class to reference this method.
		internal string Name;
		internal WebMethodAttribute MethodAttribute;

		//
		// Constructor
		//
		public MethodStubInfo (TypeStubInfo parent, LogicalMethodInfo source)
		{
			TypeStub = parent;
			MethodInfo = source;

			object [] o = source.GetCustomAttributes (typeof (WebMethodAttribute));
			if (o.Length > 0)
			{
				MethodAttribute = (WebMethodAttribute) o [0];
				Name = MethodAttribute.MessageName;
				if (Name == "") Name = source.Name;
			}
			else
				Name = source.Name;
		}
	}

	//
	// Holds the metadata loaded from the type stub, as well as
	// the metadata for all the methods in the type
	//
	internal class TypeStubInfo 
	{
		Hashtable name_to_method = new Hashtable ();
		MethodStubInfo[] methods;
		ArrayList bindings = new ArrayList ();
		LogicalTypeInfo logicalType;
		string defaultBinding;
		ArrayList mappings;
		XmlSerializer[] serializers;

		public TypeStubInfo (LogicalTypeInfo logicalTypeInfo)
		{
			this.logicalType = logicalTypeInfo;

			defaultBinding = logicalType.WebServiceName + ProtocolName;
			BindingInfo binfo = new BindingInfo (defaultBinding, logicalType.WebServiceNamespace);
			Bindings.Add (binfo);
		}
		
		public LogicalTypeInfo LogicalType
		{
			get { return logicalType; }
		}
		
		public Type Type
		{
			get { return logicalType.Type; }
		}
		
		public string DefaultBinding
		{
			get { return defaultBinding; }
		}
		
		public virtual XmlReflectionImporter XmlImporter 
		{
			get { return null; }
		}

		public virtual SoapReflectionImporter SoapImporter 
		{
			get { return null; }
		}
		
		public virtual string ProtocolName
		{
			get { return null; }
		}
		
		public XmlSerializer GetSerializer (int n)
		{
			return serializers [n];
		}
		
		public int RegisterSerializer (XmlMapping map)
		{
			if (mappings == null) mappings = new ArrayList ();
			return mappings.Add (map);
		}

		public void Initialize ()
		{
			BuildTypeMethods ();
			
			if (mappings != null)
			{
				// Build all the serializers at once
				XmlMapping[] maps = (XmlMapping[]) mappings.ToArray(typeof(XmlMapping));
				serializers = XmlSerializer.FromMappings (maps);
			}
		}
		
		//
		// Extract all method information
		//
		protected virtual void BuildTypeMethods ()
		{
			bool isClientProxy = typeof(WebClientProtocol).IsAssignableFrom (Type);

			ArrayList metStubs = new ArrayList ();
			foreach (LogicalMethodInfo mi in logicalType.LogicalMethods)
			{
				if (!isClientProxy && mi.CustomAttributeProvider.GetCustomAttributes (typeof(WebMethodAttribute), true).Length == 0)
					continue;
					
				MethodStubInfo msi = CreateMethodStubInfo (this, mi, isClientProxy);

				if (msi == null)
					continue;

				if (name_to_method.ContainsKey (msi.Name)) {
					string msg = "Both " + msi.MethodInfo.ToString () + " and " + GetMethod (msi.Name).MethodInfo + " use the message name '" + msi.Name + "'. ";
					msg += "Use the MessageName property of WebMethod custom attribute to specify unique message names for the methods";
					throw new InvalidOperationException (msg);
				}
				
				name_to_method [msi.Name] = msi;
				metStubs.Add (msi);
			}
			methods = (MethodStubInfo[]) metStubs.ToArray (typeof (MethodStubInfo));
		}
		
		protected virtual MethodStubInfo CreateMethodStubInfo (TypeStubInfo typeInfo, LogicalMethodInfo methodInfo, bool isClientProxy)
		{
			return new MethodStubInfo (typeInfo, methodInfo);
		}
		
		public MethodStubInfo GetMethod (string name)
		{
			return (MethodStubInfo) name_to_method [name];
		}

		public MethodStubInfo[] Methods
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
	// This class has information abou a web service. Through providess
	// access to the TypeStubInfo instances for each protocol.
	//
	internal class LogicalTypeInfo
	{
		LogicalMethodInfo[] logicalMethods;

		internal string WebServiceName;
		internal string WebServiceNamespace;
		internal string WebServiceLiteralNamespace;
		internal string WebServiceEncodedNamespace;
		internal string WebServiceAbstractNamespace;
		internal string Description;
		internal Type Type;

		TypeStubInfo soapProtocol;
		TypeStubInfo httpGetProtocol;
		TypeStubInfo httpPostProtocol;
		
		public LogicalTypeInfo (Type t)
		{
			this.Type = t;

			object [] o = Type.GetCustomAttributes (typeof (WebServiceAttribute), false);
			if (o.Length == 1){
				WebServiceAttribute a = (WebServiceAttribute) o [0];
				WebServiceName = (a.Name != string.Empty) ? a.Name : Type.Name;
				WebServiceNamespace = (a.Namespace != string.Empty) ? a.Namespace : WebServiceAttribute.DefaultNamespace;
				Description = a.Description;
			} else {
				WebServiceName = Type.Name;
				WebServiceNamespace = WebServiceAttribute.DefaultNamespace;
			}
			
			// Determine the namespaces for literal and encoded schema types
			
			bool encoded = false;
			
			o = t.GetCustomAttributes (typeof(SoapDocumentServiceAttribute), true);
			if (o.Length > 0) {
				SoapDocumentServiceAttribute at = (SoapDocumentServiceAttribute) o[0];
				encoded = (at.Use == SoapBindingUse.Encoded);
			}
			else if (t.GetCustomAttributes (typeof(SoapRpcServiceAttribute), true).Length > 0)
				encoded = true;
			
			string sep = WebServiceNamespace.EndsWith ("/") ? "" : "/";
			
			if (encoded) {
				WebServiceEncodedNamespace = WebServiceNamespace;
				WebServiceLiteralNamespace = WebServiceNamespace + sep + "literalTypes";
			}
			else {
				WebServiceEncodedNamespace = WebServiceNamespace + sep + "encodedTypes";
				WebServiceLiteralNamespace = WebServiceNamespace;
			}
			
			WebServiceAbstractNamespace = WebServiceNamespace + sep + "AbstractTypes";
			
			MethodInfo [] type_methods = Type.GetMethods (BindingFlags.Instance | BindingFlags.Public);
			logicalMethods = LogicalMethodInfo.Create (type_methods, LogicalMethodTypes.Sync);
		}
		
		public LogicalMethodInfo[] LogicalMethods
		{
			get { return logicalMethods; }
		}
		
		public TypeStubInfo GetTypeStub (string protocolName)
		{
			lock (this)
			{
				switch (protocolName)
				{
					case "Soap": 
						if (soapProtocol == null) soapProtocol = CreateTypeStubInfo (typeof(SoapTypeStubInfo));
						return soapProtocol;
					case "HttpGet":
						if (httpGetProtocol == null) httpGetProtocol = CreateTypeStubInfo (typeof(HttpGetTypeStubInfo));
						return httpGetProtocol;
					case "HttpPost":
						if (httpPostProtocol == null) httpPostProtocol = CreateTypeStubInfo (typeof(HttpPostTypeStubInfo));
						return httpPostProtocol;
				}
			}
			throw new InvalidOperationException ("Protocol " + protocolName + " not supported");
		}
		
		TypeStubInfo CreateTypeStubInfo (Type type)
		{
			TypeStubInfo tsi = (TypeStubInfo) Activator.CreateInstance (type, new object[] {this});
			tsi.Initialize ();
			return tsi;
		}
		
		public string GetWebServiceNamespace (SoapBindingUse use)
		{
			if (use == SoapBindingUse.Literal) return WebServiceLiteralNamespace;
			else return WebServiceEncodedNamespace;
		}
		
	}

	//
	// Manages type stubs
	//
	internal class TypeStubManager 
	{
		static Hashtable type_to_manager;
		
		static TypeStubManager ()
		{
			type_to_manager = new Hashtable ();
		}

		static internal TypeStubInfo GetTypeStub (Type t, string protocolName)
		{
			LogicalTypeInfo tm = GetLogicalTypeInfo (t);
			return tm.GetTypeStub (protocolName);
		}
		
		//
		// This needs to be thread safe
		//
		static internal LogicalTypeInfo GetLogicalTypeInfo (Type t)
		{
			LogicalTypeInfo tm = (LogicalTypeInfo) type_to_manager [t];

			if (tm != null)
				return tm;

			lock (typeof (LogicalTypeInfo))
			{
				tm = (LogicalTypeInfo) type_to_manager [t];

				if (tm != null)
					return tm;
					
				tm = new LogicalTypeInfo (t);
				type_to_manager [t] = tm;

				return tm;
			}
		}
	}
}
