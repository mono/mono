// System.Xml.Xsl.XsltArgumentList
//
// Author: Tim Coleman <tim@timcoleman.com>
// (C) Copyright 2002 Tim Coleman

using System;
using System.Collections;
using System.Xml;
using System.Xml.XPath;

namespace System.Xml.Xsl
{
	public sealed class XsltArgumentList
	{
		#region Fields

		internal Hashtable extensionObjects;
		internal Hashtable parameters;

		#endregion

		#region Constructors

		public XsltArgumentList ()
		{
			extensionObjects = new Hashtable ();
			parameters = new Hashtable ();
		}

		#endregion

		#region Methods

		public void AddExtensionObject (string namespaceUri, object extension)
		{
			if (namespaceUri == null)
				throw new ArgumentException ("The namespaceUri is a null reference.");
			if (namespaceUri == "http://www.w3.org/1999/XSL/Transform")
				throw new ArgumentException ("The namespaceUri is http://www.w3.org/1999/XSL/Transform.");
			if (extensionObjects.Contains (namespaceUri))
				throw new ArgumentException ("The namespaceUri already has an extension object associated with it.");

			extensionObjects [namespaceUri] = extension;
		}

		public void AddParam (string name, string namespaceUri, object parameter)
		{

			if (namespaceUri == null)
				throw new ArgumentException ("The namespaceUri is a null reference.");

			if (namespaceUri == "http://www.w3.org/1999/XSL/Transform")
				throw new ArgumentException ("The namespaceUri is http://www.w3.org/1999/XSL/Transform.");

			if (name == null)
				throw new ArgumentException ("The parameter name is a null reference.");


			// TODO:
			// verify that the name is a valid name according to
			// the W3C XML specification

			XmlQualifiedName qName = new XmlQualifiedName (name, namespaceUri);

			if (parameters.Contains (qName))
				throw new ArgumentException ("The namespaceUri already has a parameter associated with it.");

			parameter = ValidateParam (parameter);

			parameters [qName] = parameter;
		}

		public void Clear ()
		{
			extensionObjects.Clear ();
			parameters.Clear ();
		}

		public object GetExtensionObject (string namespaceUri)
		{
			return extensionObjects [namespaceUri];
		}

		public object GetParam (string name, string namespaceUri)
		{
			if (name == null)
				throw (new ArgumentException ("The parameter name is a null reference."));
			if (namespaceUri == null)
				throw (new ArgumentException ("The namespaceUri is a null reference."));

			XmlQualifiedName qName = new XmlQualifiedName (name, namespaceUri);
			return parameters [qName];
		}

		public object RemoveExtensionObject (string namespaceUri)
		{
			object extensionObject = this.GetExtensionObject (namespaceUri);
			extensionObjects.Remove (namespaceUri);
			return extensionObject;
		}

		public object RemoveParam (string name, string namespaceUri)
		{
			XmlQualifiedName qName = new XmlQualifiedName (name, namespaceUri);
			object parameter = this.GetParam (name, namespaceUri);
			parameters.Remove (qName);
			return parameter;
		}

		private object ValidateParam (object parameter)
		{
			if (parameter is string)		return parameter;
			if (parameter is bool)			return parameter;
			if (parameter is double)		return parameter;
			if (parameter is XPathNavigator)	return parameter;
			if (parameter is XPathNodeIterator)	return parameter;

			if (parameter is Int16)			return (double) parameter;
			if (parameter is UInt16)		return (double) parameter;
			if (parameter is Int32)			return (double) parameter;
			if (parameter is Int64)			return (double) parameter;
			if (parameter is UInt64)		return (double) parameter;
			if (parameter is Single)		return (double) parameter;
			if (parameter is decimal)		return (double) parameter;

			return parameter.ToString ();
		}

		#endregion
	}
}
