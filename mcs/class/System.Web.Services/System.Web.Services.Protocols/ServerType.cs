// 
// ServerType.cs
//
// Author:
//   Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.
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
using System.Web.Services;
using System.Web.Services.Description;
using System.Collections;

namespace System.Web.Services.Protocols
{
#if NET_2_0
	public
#else
	internal
#endif
	class ServerType
	{
		LogicalTypeInfo type;

		public ServerType (Type type)
		{
			this.type = TypeStubManager.GetLogicalTypeInfo (type);
		}

		internal LogicalTypeInfo LogicalType {
			get { return type; }
		}
	}

	//
	// This class has information about a web service. Through providess
	// access to the TypeStubInfo instances for each protocol.
	//
	internal class LogicalTypeInfo
	{
		LogicalMethodInfo[] logicalMethods;

		internal string WebServiceName;
		internal string WebServiceNamespace;
		internal string WebServiceAbstractNamespace;
		internal string Description;
		internal Type Type;
		SoapBindingUse bindingUse;
		SoapServiceRoutingStyle routingStyle;

		TypeStubInfo soapProtocol;
#if NET_2_0
		TypeStubInfo soap12Protocol;
#endif
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
			
			bindingUse = SoapBindingUse.Literal;
			
			o = t.GetCustomAttributes (typeof(SoapDocumentServiceAttribute), true);
			if (o.Length > 0) {
				SoapDocumentServiceAttribute at = (SoapDocumentServiceAttribute) o[0];
				bindingUse = at.Use;
				if (bindingUse == SoapBindingUse.Default)
					bindingUse = SoapBindingUse.Literal;
				routingStyle = at.RoutingStyle;
			}
			else if (t.GetCustomAttributes (typeof(SoapRpcServiceAttribute), true).Length > 0) {
				o = t.GetCustomAttributes (typeof(SoapRpcServiceAttribute), true);
				SoapRpcServiceAttribute at = (SoapRpcServiceAttribute) o[0];
#if NET_2_0
				bindingUse = at.Use;
#else
				bindingUse = SoapBindingUse.Encoded;
#endif
				routingStyle = at.RoutingStyle;
				if (bindingUse == SoapBindingUse.Default)
					bindingUse = SoapBindingUse.Encoded;
			}
			else
				routingStyle = SoapServiceRoutingStyle.SoapAction;
			string sep = WebServiceNamespace.EndsWith ("/") ? "" : "/";

			WebServiceAbstractNamespace = WebServiceNamespace + sep + "AbstractTypes";
#if NET_2_0
			MethodInfo [] type_methods;
			if (typeof (WebClientProtocol).IsAssignableFrom (Type))
				type_methods = Type.GetMethods (BindingFlags.Instance | BindingFlags.Public);
			else {
				MethodInfo [] all_type_methods = Type.GetMethods (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				ArrayList list = new ArrayList (all_type_methods.Length);
				foreach (MethodInfo mi in all_type_methods) {
					if (mi.IsPublic && mi.GetCustomAttributes (typeof (WebMethodAttribute), false).Length > 0)
						list.Add (mi);
					else {
						foreach (Type ifaceType in Type.GetInterfaces ()) {
							if (ifaceType.GetCustomAttributes (typeof (WebServiceBindingAttribute), false).Length > 0) {
								MethodInfo found = FindInInterface (ifaceType, mi);
								if (found != null) {
									if (found.GetCustomAttributes (typeof (WebMethodAttribute), false).Length > 0)
										list.Add (found);

									break;
								}
							}
						}
					}
				}
				type_methods = (MethodInfo []) list.ToArray (typeof (MethodInfo));
			}
#else
			MethodInfo [] type_methods = Type.GetMethods (BindingFlags.Instance | BindingFlags.Public);
#endif
			logicalMethods = LogicalMethodInfo.Create (type_methods, LogicalMethodTypes.Sync);
		}

		static MethodInfo FindInInterface (Type ifaceType, MethodInfo method) {
			int nameStartIndex = 0;
			if (method.IsPrivate) {
				nameStartIndex = method.Name.LastIndexOf ('.');
				if (nameStartIndex < 0)
					nameStartIndex = 0;
				else {
					if (String.CompareOrdinal (
						ifaceType.FullName.Replace ('+', '.'), 0, method.Name, 0, nameStartIndex) != 0)
						return null;

					nameStartIndex++;
				}
			}
			foreach (MethodInfo mi in ifaceType.GetMembers ()) {
				if (method.ReturnType == mi.ReturnType &&
					String.CompareOrdinal(method.Name, nameStartIndex, mi.Name, 0, mi.Name.Length) == 0) {
					ParameterInfo [] rpi = method.GetParameters ();
					ParameterInfo [] lpi = mi.GetParameters ();
					if (rpi.Length == lpi.Length) {
						bool match = true;
						for (int i = 0; i < rpi.Length; i++) {
							if (rpi [i].ParameterType != lpi [i].ParameterType) {
								match = false;
								break;
							}
						}

						if (match)
							return mi;
					}
				}
			}

			return null;
		}

		internal SoapBindingUse BindingUse {
			get { return bindingUse; }
		}

		internal SoapServiceRoutingStyle RoutingStyle {
			get { return routingStyle; }
		}

		internal LogicalMethodInfo[] LogicalMethods
		{
			get { return logicalMethods; }
		}

		internal TypeStubInfo GetTypeStub (string protocolName)
		{
			lock (this)
			{
				switch (protocolName)
				{
				case "Soap": 
					if (soapProtocol == null){
						soapProtocol = new SoapTypeStubInfo (this);
						soapProtocol.Initialize ();
					}
					return soapProtocol;
					
				case "Soap12": 
					if (soap12Protocol == null){
						soap12Protocol = new Soap12TypeStubInfo (this);
						soap12Protocol.Initialize ();
					}
					return soap12Protocol;
#if !MOBILE
				case "HttpGet":
					if (httpGetProtocol == null){
						httpGetProtocol = new HttpGetTypeStubInfo (this);
						httpGetProtocol.Initialize ();
					}
					return httpGetProtocol;
				case "HttpPost":
					if (httpPostProtocol == null){
						httpPostProtocol = new HttpPostTypeStubInfo (this);
						httpPostProtocol.Initialize ();
					}
					return httpPostProtocol;
#endif
				}
				throw new InvalidOperationException ("Protocol " + protocolName + " not supported");
			}
		}
		
		internal string GetWebServiceLiteralNamespace (string baseNamespace)
		{
			if (BindingUse == SoapBindingUse.Encoded) {
				string sep = baseNamespace.EndsWith ("/") ? "" : "/";
				return baseNamespace + sep + "literalTypes";
			}
			else
				return baseNamespace;
		}

		internal string GetWebServiceEncodedNamespace (string baseNamespace)
		{
			if (BindingUse == SoapBindingUse.Encoded)
				return baseNamespace;
			else {
				string sep = baseNamespace.EndsWith ("/") ? "" : "/";
				return baseNamespace + sep + "encodedTypes";
			}
		}

		internal string GetWebServiceNamespace (string baseNamespace, SoapBindingUse use)
		{
			if (use == SoapBindingUse.Literal) return GetWebServiceLiteralNamespace (baseNamespace);
			else return GetWebServiceEncodedNamespace (baseNamespace);
		}
		
	}
}
