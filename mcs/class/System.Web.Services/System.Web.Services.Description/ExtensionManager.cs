// 
// System.Web.Services.Description.ExtensionManager.cs
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) 2003 Ximian, Inc.
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
using System.Web.Services.Configuration;
using System.Xml.Serialization;
using System.Xml;

namespace System.Web.Services.Description 
{
	internal abstract class ExtensionManager 
	{
		static Hashtable extensionsByName;
		static Hashtable extensionsByType;
		static ArrayList maps = new ArrayList ();
		static ArrayList extensions = new ArrayList ();

		static ExtensionManager ()
		{
			extensionsByName = new Hashtable ();
			extensionsByType = new Hashtable ();

			RegisterExtensionType (typeof (HttpAddressBinding));
			RegisterExtensionType (typeof (HttpBinding));
			RegisterExtensionType (typeof (HttpOperationBinding));
			RegisterExtensionType (typeof (HttpUrlEncodedBinding));
			RegisterExtensionType (typeof (HttpUrlReplacementBinding));
			RegisterExtensionType (typeof (MimeContentBinding));
			RegisterExtensionType (typeof (MimeMultipartRelatedBinding));
			RegisterExtensionType (typeof (MimeTextBinding));
			RegisterExtensionType (typeof (MimeXmlBinding));
			RegisterExtensionType (typeof (SoapAddressBinding));
			RegisterExtensionType (typeof (SoapBinding));
			RegisterExtensionType (typeof (SoapBodyBinding));
			RegisterExtensionType (typeof (SoapFaultBinding));
			RegisterExtensionType (typeof (SoapHeaderBinding));
//			RegisterExtensionType (typeof (SoapHeaderFaultBinding));
			RegisterExtensionType (typeof (SoapOperationBinding));
#if NET_2_0
			RegisterExtensionType (typeof (Soap12AddressBinding));
			RegisterExtensionType (typeof (Soap12Binding));
			RegisterExtensionType (typeof (Soap12BodyBinding));
			RegisterExtensionType (typeof (Soap12FaultBinding));
			RegisterExtensionType (typeof (Soap12HeaderBinding));
			RegisterExtensionType (typeof (Soap12OperationBinding));
#endif

#if !MOBILE
			/*
			 * Currently, the mobile profile has not support for
			 * System.Configuration, so there are no external modules
			 * defined
			 */
#if NET_2_0 
			foreach (TypeElement el in WebServicesSection.Current.ServiceDescriptionFormatExtensionTypes)
				RegisterExtensionType (el.Type);
#else
			foreach (Type type in WSConfig.Instance.FormatExtensionTypes)
				RegisterExtensionType (type);
#endif
#endif
			CreateExtensionSerializers ();
		}
	
		static void RegisterExtensionType (Type type)
		{
			ExtensionInfo ext = new ExtensionInfo();
			ext.Type = type;
			
			object[] ats = type.GetCustomAttributes (typeof(XmlFormatExtensionPrefixAttribute), true);
			
			foreach (XmlFormatExtensionPrefixAttribute at in ats)
				ext.NamespaceDeclarations.Add (new XmlQualifiedName (at.Prefix, at.Namespace));
			
			ats = type.GetCustomAttributes (typeof(XmlFormatExtensionAttribute), true);
			if (ats.Length > 0)
			{
				XmlFormatExtensionAttribute at = (XmlFormatExtensionAttribute)ats[0];
				ext.ElementName = at.ElementName;
				if (at.Namespace != null) ext.Namespace = at.Namespace;
			}

			XmlRootAttribute root = new XmlRootAttribute ();
			root.ElementName = ext.ElementName;
			if (ext.Namespace != null) root.Namespace = ext.Namespace;

			XmlReflectionImporter ri = new XmlReflectionImporter ();
			XmlTypeMapping map = ri.ImportTypeMapping (type, root);
			
			if (ext.ElementName == null) throw new InvalidOperationException ("XmlFormatExtensionAttribute must be applied to type " + type);
			extensionsByName.Add (ext.Namespace + " " + ext.ElementName, ext);
			extensionsByType.Add (type, ext);
			
			maps.Add (map);
			extensions.Add (ext);
		}
		
		static void CreateExtensionSerializers ()
		{
			XmlSerializer[] sers = XmlSerializer.FromMappings ((XmlMapping[]) maps.ToArray (typeof(XmlMapping)));
			for (int n=0; n<sers.Length; n++)
				((ExtensionInfo)extensions[n]).Serializer = sers[n];
			
			maps = null;
			extensions = null;
		}
		
		public static ExtensionInfo GetFormatExtensionInfo (string elementName, string namesp)
		{
			return (ExtensionInfo) extensionsByName [namesp + " " + elementName];
		}
		
		public static ExtensionInfo GetFormatExtensionInfo (Type extType)
		{
			return (ExtensionInfo) extensionsByType [extType];
		}
		
		public static ICollection GetFormatExtensions ()
		{
			return extensionsByName.Values;
		}

		public static ServiceDescriptionFormatExtensionCollection GetExtensionPoint (object ob)
		{
			Type type = ob.GetType ();
			object[] ats = type.GetCustomAttributes (typeof(XmlFormatExtensionPointAttribute), true);
			if (ats.Length == 0) return null;

			XmlFormatExtensionPointAttribute at = (XmlFormatExtensionPointAttribute)ats[0];
			
			PropertyInfo prop = type.GetProperty (at.MemberName);
			if (prop != null)
				return prop.GetValue (ob, null) as ServiceDescriptionFormatExtensionCollection;
			else {
				FieldInfo field = type.GetField (at.MemberName);
				if (field != null)
					return field.GetValue (ob) as ServiceDescriptionFormatExtensionCollection;
				else
					throw new InvalidOperationException ("XmlFormatExtensionPointAttribute: Member " + at.MemberName + " not found");
			}
		}

		/*
		 * The mobile profile lacks support for configuration
		 */
#if MOBILE
		public static ArrayList BuildExtensionImporters ()
		{
			return new ArrayList (0);
		}
		
		public static ArrayList BuildExtensionReflectors ()
		{
			return new ArrayList (0);
		}

#else
		public static ArrayList BuildExtensionImporters ()
		{
#if NET_2_0
			return BuildExtensionList (WebServicesSection.Current.SoapExtensionImporterTypes);
#else
			return BuildExtensionList (WSConfig.Instance.ExtensionImporterTypes);
#endif
		}
		
		public static ArrayList BuildExtensionReflectors ()
		{
#if NET_2_0
			return BuildExtensionList (WebServicesSection.Current.SoapExtensionReflectorTypes);
#else
			return BuildExtensionList (WSConfig.Instance.ExtensionReflectorTypes);
#endif
		}

#if NET_2_0
		public static ArrayList BuildExtensionList (TypeElementCollection exts)
#else
		public static ArrayList BuildExtensionList (ArrayList exts)
#endif
		{
			ArrayList extensionTypes = new ArrayList ();
			
			if (exts != null)
			{
#if NET_2_0 
				foreach (TypeElement econf in exts)
				{
					extensionTypes.Add (econf);
				}
#else
				foreach (WSExtensionConfig econf in exts)
				{
					bool added = false;
					for (int n=0; n<extensionTypes.Count && !added; n++)
					{
						WSExtensionConfig cureconf = (WSExtensionConfig) extensionTypes [n];
	
						if ((econf.Group < cureconf.Group) || ((econf.Group == cureconf.Group) && (econf.Priority < cureconf.Priority))) {
							extensionTypes.Insert (n, econf);
							added = true;
						}
					}
					if (!added) extensionTypes.Add (econf);
				}
#endif
			}

			ArrayList extensions = new ArrayList (extensionTypes.Count);
#if NET_2_0
			foreach (TypeElement econf in extensionTypes)
#else
			foreach (WSExtensionConfig econf in extensionTypes)
#endif
				extensions.Add (Activator.CreateInstance (econf.Type));
				
			return extensions;
		}
#endif
	}
	
	internal class ExtensionInfo
	{
		ArrayList _namespaceDeclarations;
		string _namespace;
		string _elementName;
		Type _type;
		XmlSerializer _serializer;

		public ArrayList NamespaceDeclarations
		{
			get { 
				if (_namespaceDeclarations == null) _namespaceDeclarations = new ArrayList ();
				return _namespaceDeclarations; 
			}
		}
		
		public string Namespace
		{
			get { return _namespace; }
			set { _namespace = value; }
		}
		
		public string ElementName
		{
			get { return _elementName; }
			set { _elementName = value; }
		}
		
		public Type Type
		{
			get { return _type; }
			set { _type = value; }
		}
		
		public XmlSerializer Serializer
		{
			get { return _serializer; }
			set { _serializer = value; }
		}		
	}
}
