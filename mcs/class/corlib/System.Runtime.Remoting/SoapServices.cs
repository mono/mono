//
// System.Runtime.Remoting.SoapServices.cs
//
// Author: Jaime Anguiano Olarra (jaime@gnome.org)
//
// (c) 2002, Jaime Anguiano Olarra
// 
// FIXME: This is just a skeleton for practical purposes.


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
	
		[MonoTODO]
		public static string XmlNsForClrType { 
			get { 
				throw new NotImplementedException (); 
			}
		}
		
		[MonoTODO]
		public static string XmlNsForClrTypeWithAssembly { 	
			get { 
				throw new NotImplementedException (); 
			}
		}


		[MonoTODO]
		public static string XmlNsForClrTypeWithNs {
			get { 
				throw new NotImplementedException (); 
			}
		}

		[MonoTODO]
		public static string XmlNsForClrTypeWithMsAndAssembly {		
			get { 
				throw new NotImplementedException (); 
			}
		}

		
		// public methods

		[MonoTODO]
		public static string CodeXmlNamespaceForClrTypeNamespace (string typeNamespace, 
									string assemblyName) {
			
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static bool DecodeXmlNamespaceForClrTypeNamespace (string inNamespace, 
									out string typeNamespace, 
									out string assemblyName) {
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		public override bool Equals (object obj) {
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		public new static bool Equals (object objectA, object objectB) {
			throw new NotImplementedException (); 
		}
		
		[MonoTODO]
		public override int GetHashCode ( ) {
			throw new NotImplementedException (); 
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

		[MonoTODO]
		public static string GetSoapActionFromMethodBase (MethodBase mb) {
			throw new NotImplementedException (); 

		}

		[MonoTODO]
		public new Type GetType () {
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		public static bool GetTypeAndMethodNameFromSoapAction (string soapAction, 
									out string typeName, 
									out string methodName) {
			throw new NotImplementedException (); 

		}

		[MonoTODO]
		public static bool GetXmlElementForInteropType (Type type, 
								out string xmlElement, 
								out string xmlNamespace) {
			throw new NotImplementedException (); 

		}

		[MonoTODO]
		public static string GetXmlNamespaceForMethodCall (MethodBase mb) {
			throw new NotImplementedException (); 

		}

		[MonoTODO]
		public static string GetXmlNamespaceForMethodResponse (MethodBase mb) {
			throw new NotImplementedException (); 

		}

		[MonoTODO]
		public static bool GetXmlTypeForInteropType (Type type, 
							out string xmlType, 
							out string xmlTypeNamespace) {
			throw new NotImplementedException (); 

		}

		[MonoTODO]
		public static bool IsClrTypeNamespace (string namespaceString) {
			throw new NotImplementedException (); 

		}

		[MonoTODO]
		public static bool IsSoapActionValidForMethodBase (string soapAction, MethodBase mb) {
			throw new NotImplementedException (); 

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

		[MonoTODO]
		public override string ToString ( ) {
			throw new NotImplementedException (); 
		}

		// protected methods

		[MonoTODO]
		~SoapServices ( ) {
			throw new NotImplementedException (); 
		}
			
		[MonoTODO]
		protected object MemberWiseClone ( ) {	
			throw new NotImplementedException (); 
		}

	}
}

