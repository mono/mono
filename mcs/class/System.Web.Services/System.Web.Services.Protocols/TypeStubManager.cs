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

		// The name used by the stub class to reference this method.
		internal string Name;
		internal WebMethodAttribute MethodAttribute;

		//
		// Constructor
		//
		public MethodStubInfo (TypeStubInfo parent, LogicalMethodInfo source)
		{
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
		LogicalMethodInfo[] logicalMethods;

		// Precomputed
		internal string WebServiceName;
		internal string WebServiceNamespace;
		internal string Description;
		internal Type Type;

		public TypeStubInfo (Type t)
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
		}

		//
		// Extract all method information
		//
		public virtual void BuildTypeMethods ()
		{
			MethodInfo [] type_methods = Type.GetMethods (BindingFlags.Instance | BindingFlags.Public);
			logicalMethods = LogicalMethodInfo.Create (type_methods, LogicalMethodTypes.Sync);
			bool isClientProxy = typeof(WebClientProtocol).IsAssignableFrom (Type);

			ArrayList metStubs = new ArrayList ();
			foreach (LogicalMethodInfo mi in logicalMethods)
			{
				if (!isClientProxy && mi.CustomAttributeProvider.GetCustomAttributes (typeof(WebMethodAttribute), true).Length == 0)
					continue;
					
				MethodStubInfo msi = CreateMethodStubInfo (this, mi, isClientProxy);

				if (msi == null)
					continue;

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
		
		public LogicalMethodInfo[] LogicalMethods
		{
			get { return logicalMethods; }
		}
	}
	
	//
	// Manages 
	//
	internal class TypeStubManager 
	{
		static Hashtable type_to_manager;
		
		static TypeStubManager ()
		{
			type_to_manager = new Hashtable ();
		}

		//
		// This needs to be thread safe
		//
		static internal TypeStubInfo GetTypeStub (Type t, Type protocolStubType)
		{
			string key = t.FullName + " : " + protocolStubType.FullName;
			TypeStubInfo tm = (TypeStubInfo) type_to_manager [key];

			if (tm != null)
				return tm;

			lock (typeof (TypeStubInfo))
			{
				tm = (TypeStubInfo) type_to_manager [key];

				if (tm != null)
					return tm;

				tm = (TypeStubInfo) Activator.CreateInstance (protocolStubType, new object[] {t});
				tm.BuildTypeMethods ();
				type_to_manager [t] = tm;

				return tm;
			}
		}
	}
}
