//
// System.Runtime.Remoting.SoapServices.cs
//
// Author: Jaime Anguiano Olarra (jaime@gnome.org)
//         Lluis Sanchez Gual (lsg@ctv.es)
//
// (c) 2002, Jaime Anguiano Olarra
// 


using System;
using System.Runtime.Remoting;
using System.Reflection;
using System.Runtime.InteropServices;

namespace System.Runtime.Remoting {

	[Serializable]
	[ClassInterface (ClassInterfaceType.AutoDual)]
	public class SoapServices
	{
		// properties
	
		public static string XmlNsForClrType 
		{ 
			get { return "http://schemas.microsoft.com/clr/"; }
		}
		
		public static string XmlNsForClrTypeWithAssembly 
		{ 	
			get { return "http://schemas.microsoft.com/clr/assem/"; }
		}

		public static string XmlNsForClrTypeWithNs 
		{
			get { return "http://schemas.microsoft.com/clr/ns/"; }
		}

		public static string XmlNsForClrTypeWithNsAndAssembly
		{
			get { return "http://schemas.microsoft.com/clr/nsassem/"; }
		}

		
		// public methods

		public static string CodeXmlNamespaceForClrTypeNamespace (string typeNamespace, 
									string assemblyName) 
		{
			// If assemblyName is empty, then use the corlib namespace

			if (assemblyName == string.Empty)
				return XmlNsForClrTypeWithNs + typeNamespace + "/" + assemblyName;
			else
				return XmlNsForClrTypeWithNsAndAssembly + typeNamespace + "/" + assemblyName;
		}

		public static bool DecodeXmlNamespaceForClrTypeNamespace (string inNamespace, 
									out string typeNamespace, 
									out string assemblyName) {

			if (inNamespace == null) throw new ArgumentNullException ("inNamespace");

			typeNamespace = null;
			assemblyName = null;

			if (inNamespace.StartsWith(XmlNsForClrTypeWithNsAndAssembly))
			{
				int typePos = XmlNsForClrTypeWithNsAndAssembly.Length;
				if (typePos >= inNamespace.Length) return false;
				int assemPos = inNamespace.IndexOf ('/', typePos+1);
				if (assemPos == -1) return false;
				typeNamespace = inNamespace.Substring (typePos, assemPos - typePos);
				assemblyName = inNamespace.Substring (assemPos+1);
				return true;
			}
			else if (inNamespace.StartsWith(XmlNsForClrTypeWithNs))
			{
				int typePos = XmlNsForClrTypeWithNs.Length;
				typeNamespace = inNamespace.Substring (typePos);
				return true;
			}
			else
				return false;
		}

		[MonoTODO]
		public static void GetInteropFieldTypeAndNameFromXmlAttribute (Type containingType,
										string xmlAttribute,
										string xmlNamespace,
										out Type type,
										out string name) {
			throw new NotImplementedException (); 
		
		}

		[MonoTODO]
		public static void GetInteropFieldTypeAndNameFromXmlElement (Type containingType,
										string xmlElement,
										string xmlNamespace,
										out Type type,
										out string name) {
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		public static Type GetInteropTypeFromXmlElement (string xmlElement, string xmlNamespace) {
			throw new NotImplementedException (); 
		}
			
		[MonoTODO]
		public static Type GetInteropTypeFromXmlType (string xmlType, string xmlTypeNamespace) {
			throw new NotImplementedException (); 
		}

		private static string GetAssemblyName(MethodBase mb)
		{
			if (mb.DeclaringType.Assembly == typeof (object).Assembly)
				return string.Empty;
			else
				return mb.DeclaringType.Assembly.GetName().Name;
		}

		public static string GetSoapActionFromMethodBase (MethodBase mb) 
		{
			string ns = CodeXmlNamespaceForClrTypeNamespace (mb.DeclaringType.Name, GetAssemblyName(mb));
			return ns + "#" + mb.Name;
		}

		[MonoTODO]
		public new Type GetType () {
			throw new NotImplementedException (); 
		}

		public static bool GetTypeAndMethodNameFromSoapAction (string soapAction, 
									out string typeName, 
									out string methodName) {
			string type;
			string assembly;

			typeName = null;
			methodName = null;

			int i = soapAction.LastIndexOf ('#');
			if (i == -1) return false;

			methodName = soapAction.Substring (i+1);

			if (!DecodeXmlNamespaceForClrTypeNamespace (soapAction.Substring (0,i), out type, out assembly) )
				return false;

			if (assembly == null) 
				typeName = type + ", " + typeof (object).Assembly.GetName().Name;
			else
				typeName = type + ", " + assembly;

			return true;
		}

		[MonoTODO]
		public static bool GetXmlElementForInteropType (Type type, 
								out string xmlElement, 
								out string xmlNamespace) {
			throw new NotImplementedException (); 

		}

		public static string GetXmlNamespaceForMethodCall (MethodBase mb) 
		{
			return CodeXmlNamespaceForClrTypeNamespace (mb.DeclaringType.Name, GetAssemblyName(mb));
		}

		public static string GetXmlNamespaceForMethodResponse (MethodBase mb) 
		{
			return CodeXmlNamespaceForClrTypeNamespace (mb.DeclaringType.Name, GetAssemblyName(mb));
		}

		[MonoTODO]
		public static bool GetXmlTypeForInteropType (Type type, 
							out string xmlType, 
							out string xmlTypeNamespace) {
			throw new NotImplementedException (); 

		}

		public static bool IsClrTypeNamespace (string namespaceString) 
		{
			return namespaceString.StartsWith (XmlNsForClrType);
		}

		public static bool IsSoapActionValidForMethodBase (string soapAction, MethodBase mb) 
		{
			string typeName;
			string methodName;
			GetTypeAndMethodNameFromSoapAction (soapAction, out typeName, out methodName);

			if (methodName != mb.Name) return false;

			string methodClassType = mb.DeclaringType.FullName + ", " + mb.DeclaringType.Assembly.GetName().Name;
			return typeName == methodClassType;
		}

		[MonoTODO]
		public static void PreLoad (Assembly assembly) {
			throw new NotImplementedException (); 

		}

		[MonoTODO]
		public static void PreLoad (Type type) {
			throw new NotImplementedException (); 

		}
		
		[MonoTODO]
		public static void RegisterInteropXmlElement (string xmlElement, 
								string xmlNamespace, 
								Type type) {
			throw new NotImplementedException (); 

		}

		[MonoTODO]
		public static void RegisterInteropXmlType (string xmlType, 
							string xmlTypeNamespace, 
							Type type) {
			throw new NotImplementedException (); 
	
		}

		[MonoTODO]
		public static void RegisterSoapActionForMethodBase (MethodBase mb) {
			throw new NotImplementedException (); 
	
		}

		[MonoTODO]
		public static void RegisterSoapActionForMethodBase (MethodBase mb, string soapAction) {
			throw new NotImplementedException (); 
		}
	}
}

