//
// System.Runtime.Remoting.SoapServices.cs
//
// Author: Jaime Anguiano Olarra (jaime@gnome.org)
//         Lluis Sanchez Gual (lluis@ximian.com)
//
// (c) 2002, Jaime Anguiano Olarra
// 

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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


using System;
using System.Collections;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Metadata;
using System.Reflection;
using System.Runtime.InteropServices;

namespace System.Runtime.Remoting {

#if NET_2_0
	[System.Runtime.InteropServices.ComVisible (true)]
#endif
	public class SoapServices
	{
		static Hashtable _xmlTypes = new Hashtable ();
		static Hashtable _xmlElements = new Hashtable ();
		static Hashtable _soapActions = new Hashtable ();
		static Hashtable _soapActionsMethods = new Hashtable ();
		static Hashtable _typeInfos = new Hashtable ();
		
		class TypeInfo
		{
			public Hashtable Attributes;
			public Hashtable Elements;
		}
		
		// Private constructor: nobody instantiates this class
		private SoapServices () {}
		
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
				return XmlNsForClrTypeWithNs + typeNamespace;
			else if (typeNamespace == string.Empty)
				return EncodeNs (XmlNsForClrTypeWithAssembly + assemblyName);
			else
				return EncodeNs (XmlNsForClrTypeWithNsAndAssembly + typeNamespace + "/" + assemblyName);
		}

		public static bool DecodeXmlNamespaceForClrTypeNamespace (string inNamespace, 
									out string typeNamespace, 
									out string assemblyName) {

			if (inNamespace == null) throw new ArgumentNullException ("inNamespace");

			inNamespace = DecodeNs (inNamespace);
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
			else if (inNamespace.StartsWith(XmlNsForClrTypeWithAssembly))
			{
				int typePos = XmlNsForClrTypeWithAssembly.Length;
				assemblyName = inNamespace.Substring (typePos);
				return true;
			}
			else
				return false;
		}

		public static void GetInteropFieldTypeAndNameFromXmlAttribute (Type containingType,
										string xmlAttribute, string xmlNamespace,
										out Type type, out string name) 
		{
			TypeInfo tf = (TypeInfo) _typeInfos [containingType];
			Hashtable ht = tf != null ? tf.Attributes : null;
			GetInteropFieldInfo (ht, xmlAttribute, xmlNamespace, out type, out name);
		}

		public static void GetInteropFieldTypeAndNameFromXmlElement (Type containingType,
										string xmlElement, string xmlNamespace,
										out Type type, out string name) 
		{
			TypeInfo tf = (TypeInfo) _typeInfos [containingType];
			Hashtable ht = tf != null ? tf.Elements : null;
			GetInteropFieldInfo (ht, xmlElement, xmlNamespace, out type, out name);
		}

		static void GetInteropFieldInfo (Hashtable fields, 
										string xmlName, string xmlNamespace,
										out Type type, out string name) 
		{
			if (fields != null)
			{
				FieldInfo field = (FieldInfo) fields [GetNameKey (xmlName, xmlNamespace)];
				if (field != null)
				{
					type = field.FieldType;
					name = field.Name;
					return;
				}
			}
			type = null;
			name = null;
		}
		
		static string GetNameKey (string name, string namspace)
		{
			if (namspace == null) return name;
			else return name + " " + namspace;
		}

		public static Type GetInteropTypeFromXmlElement (string xmlElement, string xmlNamespace) 
		{
			lock (_xmlElements.SyncRoot)
			{
				return (Type) _xmlElements [xmlElement + " " + xmlNamespace];
			}
		}
			
		public static Type GetInteropTypeFromXmlType (string xmlType, string xmlTypeNamespace) 
		{
			lock (_xmlTypes.SyncRoot)
			{
				return (Type) _xmlTypes [xmlType + " " + xmlTypeNamespace];
			}
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
			return InternalGetSoapAction (mb);
		}

		public static bool GetTypeAndMethodNameFromSoapAction (string soapAction, 
									out string typeName, 
									out string methodName) 
		{
			lock (_soapActions.SyncRoot)
			{
				MethodBase mb = (MethodBase) _soapActionsMethods [soapAction];
				if (mb != null)
				{
					typeName = mb.DeclaringType.AssemblyQualifiedName;
					methodName = mb.Name;
					return true;
				}
			}
			
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

		public static bool GetXmlElementForInteropType (Type type, out string xmlElement, out string xmlNamespace)
		{
			SoapTypeAttribute att = (SoapTypeAttribute) InternalRemotingServices.GetCachedSoapAttribute (type);
			if (!att.IsInteropXmlElement)
			{
				xmlElement = null;
				xmlNamespace = null;
				return false;
			}
			
			xmlElement = att.XmlElementName;
			xmlNamespace = att.XmlNamespace;				
			return true;
		}

		public static string GetXmlNamespaceForMethodCall (MethodBase mb) 
		{
			return CodeXmlNamespaceForClrTypeNamespace (mb.DeclaringType.FullName, GetAssemblyName(mb));
		}

		public static string GetXmlNamespaceForMethodResponse (MethodBase mb) 
		{
			return CodeXmlNamespaceForClrTypeNamespace (mb.DeclaringType.FullName, GetAssemblyName(mb));
		}

		public static bool GetXmlTypeForInteropType (Type type, out string xmlType, out string xmlTypeNamespace) 
		{
			SoapTypeAttribute att = (SoapTypeAttribute) InternalRemotingServices.GetCachedSoapAttribute (type);
			
			if (!att.IsInteropXmlType)
			{
				xmlType = null;
				xmlTypeNamespace = null;
				return false;
			}
			
			xmlType = att.XmlTypeName;
			xmlTypeNamespace = att.XmlTypeNamespace;
			return true;
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

			string methodClassType = mb.DeclaringType.AssemblyQualifiedName;
			return typeName == methodClassType;
		}

		public static void PreLoad (Assembly assembly) 
		{
			foreach (Type t in assembly.GetTypes ())
				PreLoad (t);
		}

		public static void PreLoad (Type type) 
		{
			string name, namspace;
			TypeInfo tf = _typeInfos [type] as TypeInfo;
			if (tf != null) return;
			
			if (GetXmlTypeForInteropType (type, out name, out namspace))
				RegisterInteropXmlType (name, namspace, type);
				
			if (GetXmlElementForInteropType (type, out name, out namspace))
				RegisterInteropXmlElement (name, namspace, type);
				
			lock (_typeInfos.SyncRoot)
			{
				tf = new TypeInfo ();
				FieldInfo[] fields = type.GetFields (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
				
				foreach (FieldInfo field in fields)
				{
					SoapFieldAttribute att = (SoapFieldAttribute) InternalRemotingServices.GetCachedSoapAttribute (field);
					if (!att.IsInteropXmlElement ()) continue;
					
					string key = GetNameKey (att.XmlElementName, att.XmlNamespace);
					if (att.UseAttribute)
					{
						if (tf.Attributes == null) tf.Attributes = new Hashtable ();
						tf.Attributes [key] = field;
					}
					else
					{
						if (tf.Elements == null) tf.Elements = new Hashtable ();
						tf.Elements [key] = field;
					}
				}
				_typeInfos [type] = tf;
			}			
		}
		
		public static void RegisterInteropXmlElement (string xmlElement, string xmlNamespace, Type type) 
		{
			lock (_xmlElements.SyncRoot)
			{
				_xmlElements [xmlElement + " " + xmlNamespace] = type;
			}
		}

		public static void RegisterInteropXmlType (string xmlType, string xmlTypeNamespace, Type type) 
		{
			lock (_xmlTypes.SyncRoot)
			{
				_xmlTypes [xmlType + " " + xmlTypeNamespace] = type;
			}
		}

		public static void RegisterSoapActionForMethodBase (MethodBase mb) 
		{
			InternalGetSoapAction (mb);
		}
		
		static string InternalGetSoapAction (MethodBase mb)
		{
			lock (_soapActions.SyncRoot)
			{
				string action = (string) _soapActions [mb];
				if (action == null)
				{
					SoapMethodAttribute att = (SoapMethodAttribute) InternalRemotingServices.GetCachedSoapAttribute (mb);
					action = att.SoapAction;
					_soapActions [mb] = action;
					_soapActionsMethods [action] = mb;
				}
				return action;
			}
		}

		public static void RegisterSoapActionForMethodBase (MethodBase mb, string soapAction) 
		{
			lock (_soapActions.SyncRoot)
			{
				_soapActions [mb] = soapAction;
				_soapActionsMethods [soapAction] = mb;
			}
		}
		
		static string EncodeNs (string ns)
		{	
			// Simple url encoding for namespaces
			
			ns = ns.Replace (",","%2C");
			ns = ns.Replace (" ","%20");
			return ns.Replace ("=","%3D");
		}
		
		static string DecodeNs (string ns)
		{
			ns = ns.Replace ("%2C",",");
			ns = ns.Replace ("%20"," ");
			return ns.Replace ("%3D","=");
		}
	}
}

