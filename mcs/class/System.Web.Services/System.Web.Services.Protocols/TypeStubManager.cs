//
// Methods.cs: Information about a method and its mapping to a SOAP web service.
//
// Author:
//   Miguel de Icaza
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// (C) 2003 Ximian, Inc.
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
		
		internal string OperationName
		{
			get { return MethodInfo.Name; }
		}

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
	internal abstract class TypeStubInfo 
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

			object [] o = Type.GetCustomAttributes (typeof (WebServiceBindingAttribute), false);

			bool isClientSide = typeof (SoapHttpClientProtocol).IsAssignableFrom (Type);
			bool defaultAdded = false;

			string defaultBindingName = logicalType.WebServiceName + ProtocolName;
			if (o.Length > 0)
				foreach (WebServiceBindingAttribute at in o) {
					AddBinding (new BindingInfo (at, defaultBindingName, LogicalType.WebServiceNamespace));
					if ((at.Name == null || at.Name.Length == 0) || (at.Name == defaultBindingName))
						defaultAdded = true;
				}

			if (!defaultAdded && !isClientSide)
				AddBindingAt (0, new BindingInfo (null, defaultBindingName, logicalType.WebServiceNamespace));

#if NET_2_0
			foreach (Type ifaceType in Type.GetInterfaces ()) {
				o = ifaceType.GetCustomAttributes (typeof (WebServiceBindingAttribute), false);
				if (o.Length > 0) {
					defaultBindingName = ifaceType.Name + ProtocolName;
					foreach (WebServiceBindingAttribute at in o)
						AddBinding (new BindingInfo (at, defaultBindingName, LogicalType.WebServiceNamespace));
				}
			}
#endif
		}
		
#if NET_2_0
		public WsiProfiles WsiClaims {
			get {
				return (((BindingInfo) Bindings [0]).WebServiceBindingAttribute != null) ?
					((BindingInfo) Bindings [0]).WebServiceBindingAttribute.ConformsTo : WsiProfiles.None;
			}
		}
#endif
		
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
		
		protected abstract MethodStubInfo CreateMethodStubInfo (TypeStubInfo typeInfo, LogicalMethodInfo methodInfo, bool isClientProxy);
		
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
		
		internal void AddBinding (BindingInfo info)
		{
			bindings.Add (info);
		}

		internal void AddBindingAt (int pos, BindingInfo info)
		{
			bindings.Insert (pos, info);
		}
		
		internal BindingInfo GetBinding (string name)
		{
			if (name == null || name.Length == 0) return (BindingInfo) bindings[0];
			
			for (int n = 0; n < bindings.Count; n++)
				if (((BindingInfo)bindings[n]).Name == name) return (BindingInfo)bindings[n];
			return null;
		}
	}
	
	internal class BindingInfo
	{
		public BindingInfo (WebServiceBindingAttribute at, string name, string ns)
		{
			if (at != null) {
#if NET_1_1
				Name = at.Name;
#endif
				Namespace = at.Namespace;
				Location = at.Location;
				WebServiceBindingAttribute = at;
			}

			if (Name == null || Name.Length == 0)
				Name = name;

			if (Namespace == null || Namespace.Length == 0)
				Namespace = ns;
		}
		
		public readonly string Name;
		public readonly string Namespace;
		public readonly string Location;
		public readonly WebServiceBindingAttribute WebServiceBindingAttribute;
	}

	//
	// Manages type stubs
	//
	internal class TypeStubManager 
	{
#if !TARGET_JVM
		static Hashtable type_to_manager;
#else
		const string type_to_manager_key = "TypeStubManager.type_to_manager";
		static Hashtable type_to_manager {
			get {
				Hashtable hash = (Hashtable)AppDomain.CurrentDomain.GetData(type_to_manager_key);

				if (hash != null)
					return hash;

				lock(type_to_manager_key) {
					AppDomain.CurrentDomain.SetData(type_to_manager_key, new Hashtable());
				}

				return (Hashtable)AppDomain.CurrentDomain.GetData(type_to_manager_key);
			}
			set {
				//do nothing: we manage our type_to_manager per domain
			}
		}
#endif
		
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
			lock (type_to_manager)
			{
				LogicalTypeInfo tm = (LogicalTypeInfo) type_to_manager [t];
	
				if (tm != null)
					return tm;

				tm = new LogicalTypeInfo (t);
				type_to_manager [t] = tm;

				return tm;
			}
		}
	}
}
