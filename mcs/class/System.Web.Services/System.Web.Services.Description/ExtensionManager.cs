// 
// System.Web.Services.Description.ExtensionManager.cs
//
// Author:
//   Lluis Sanchez Gual (lluis@ximian.com)
//
// Copyright (C) 2003 Ximian, Inc.
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
			
			foreach (Type type in WSConfig.Instance.FormatExtensionTypes)
				RegisterExtensionType (type);
		}
	
		public static void RegisterExtensionType (Type type)
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
			
			// TODO: use array method to create the serializers
			ext.Serializer = new XmlSerializer (map);

			if (ext.ElementName == null) throw new InvalidOperationException ("XmlFormatExtensionAttribute must be applied to type " + type);
			extensionsByName.Add (ext.Namespace + " " + ext.ElementName, ext);
			extensionsByType.Add (type, ext);
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
		
		public static ArrayList BuildExtensionImporters ()
		{
			return BuildExtensionList (WSConfig.Instance.ExtensionImporterTypes);
		}
		
		public static ArrayList BuildExtensionReflectors ()
		{
			return BuildExtensionList (WSConfig.Instance.ExtensionReflectorTypes);
		}
		
		public static ArrayList BuildExtensionList (ArrayList exts)
		{
			ArrayList extensionTypes = new ArrayList ();
			
			if (exts != null)
			{
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
			}

			ArrayList extensions = new ArrayList (extensionTypes.Count);
			foreach (WSExtensionConfig econf in extensionTypes)
				extensions.Add (Activator.CreateInstance (econf.Type));
				
			return extensions;
		}
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
