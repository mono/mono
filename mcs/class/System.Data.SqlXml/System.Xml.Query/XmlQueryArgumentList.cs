//
// System.Xml.Query.XmlQueryArgumentList
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
//

#if NET_1_2

using System.Collections;
using System.Data.SqlXml;
using System.Xml;

namespace System.Xml.Query {
        public class XmlQueryArgumentList
        {
		#region Fields

		Hashtable extensionObjects;
		Hashtable parameters;

		#endregion // Fields

		#region Constructors

		[MonoTODO]
		public XmlQueryArgumentList ()
		{
			extensionObjects = new Hashtable ();
			parameters = new Hashtable ();
		}

		#endregion // Constructors

		#region Methods

		[MonoTODO]
		public void AddExtensionObject (string namespaceUri, object value)
		{
			extensionObjects [namespaceUri] = value;
		}

		[MonoTODO]
		public void AddParam (string localName, string namespaceUri, object value)
		{
			AddParam (new XmlQualifiedName (localName, namespaceUri), value);
		}

		[MonoTODO]
		private void AddParam (XmlQualifiedName qname, object value)
		{
			parameters [qname] = value;
		}

		[MonoTODO]
		public object GetExtensionObject (string namespaceUri)
		{
			return extensionObjects [namespaceUri];
		}

		[MonoTODO]
		public object GetParam (string localName, string namespaceUri)
		{
			return GetParam (new XmlQualifiedName (localName, namespaceUri));
		}

		[MonoTODO]
		private object GetParam (XmlQualifiedName qname)
		{
			return parameters [qname];
		}

		[MonoTODO]
		public void RemoveExtensionObject (string namespaceUri)
		{
			extensionObjects.Remove (namespaceUri);	
		}

		[MonoTODO]
		public void RemoveParam (string localName, string namespaceUri)
		{
			RemoveParam (new XmlQualifiedName (localName, namespaceUri));
		}

		[MonoTODO]
		private void RemoveParam (XmlQualifiedName qname)
		{
			parameters.Remove (qname);
		}

		#endregion // Methods
        }
}

#endif // NET_1_2
