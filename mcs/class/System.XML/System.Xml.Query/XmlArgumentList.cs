//
// System.Xml.Query.XmlArgumentList
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) Tim Coleman, 2003
// Copyright (C) Novell Inc, 2004
//

#if NET_2_0

using System.Collections;
using System.Xml;

namespace System.Xml.Query 
{
	public class XmlArgumentList
	{
		#region	Fields

		Hashtable extensionObjects;
		Hashtable parameters;

		#endregion // Fields

		#region	Constructors

		[MonoTODO]
		public XmlArgumentList ()
		{
			extensionObjects = new Hashtable ();
			parameters = new Hashtable ();
		}

		#endregion // Constructors

		#region	Methods

		[MonoTODO ("Confirm name conflicts")]
		public void AddExtensionObject (string namespaceUri, object value)
		{
			extensionObjects.Add (namespaceUri, value);
		}

		[MonoTODO ("Confirm name conflicts")]
		public void AddParameter (string localName,	string namespaceUri, object value)
		{
			parameters.Add (new XmlQualifiedName (localName, namespaceUri), value);
		}

		public void ClearExtensionObjects ()
		{
			extensionObjects.Clear ();
		}

		public void ClearParameters ()
		{
			parameters.Clear ();
		}

		public object GetExtensionObject (string namespaceUri)
		{
			return extensionObjects [namespaceUri];
		}

		[MonoTODO ("Performance")]
		public object GetParameter (string localName, string namespaceUri)
		{
			return parameters [new XmlQualifiedName (localName, namespaceUri)];
		}

		[MonoTODO]
		public void RemoveExtensionObject (string namespaceUri)
		{
			extensionObjects.Remove (namespaceUri);
		}

		[MonoTODO]
		public void RemoveParameter (string localName, string namespaceUri)
		{
			parameters.Remove (new XmlQualifiedName (localName, namespaceUri));
		}

		[MonoTODO]
		public void SetExtensionObject (string namespaceUri, object value)
		{
			extensionObjects [namespaceUri] = value;
		}

		[MonoTODO]
		public void SetParameter (string localName, string namespaceUri, object value)
		{
			parameters [new XmlQualifiedName (localName, namespaceUri)] = value;
		}

		#endregion // Methods
	}
}

#endif // NET_2_0
