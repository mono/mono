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

		private Hashtable _extensionObjects;
		private Hashtable _parameters;

		#endregion

		#region Constructors

		public XsltArgumentList()
		{
			_extensionObjects = new Hashtable();
			_parameters = new Hashtable();
		}

		#endregion

		#region Methods

		//--------------------------------------------------
		public void AddExtensionObject( 
			string namespaceUri, 
			object extension )
		{
			if (namespaceUri == null) 
				throw new ArgumentException ("The namespaceUri is a null reference.");
			if (namespaceUri == "http://www.w3.org/1999/XSL/Transform")
				throw new ArgumentException ("The namespaceUri is http://www.w3.org/1999/XSL/Transform.");
			if (_extensionObjects.Contains (namespaceUri))
				throw new ArgumentException ("The namespaceUri already has an extension object associated with it.");

			_extensionObjects [namespaceUri] = extension;
		}

		//--------------------------------------------------
		public void AddParam (
			string name, 
			string namespaceUri, 
			object parameter) 
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

			if (_parameters.Contains (qName))
				throw new ArgumentException ("The namespaceUri already has a parameter associated with it.");

			if( !(parameter is string)
			&&  !(parameter is bool) 
			&&  !(parameter is double)
			&&  !(parameter is XPathNavigator)
			&&  !(parameter is XPathNodeIterator) ) {
				if(parameter is Int16
				|| parameter is UInt16
				|| parameter is Int32
				|| parameter is Int64
				|| parameter is UInt64
				|| parameter is Single
				|| parameter is decimal ) {
					parameter = (double)parameter;
				} else {
					parameter = parameter.ToString();
				}
			}
			_parameters [qName] = parameter;
		}

		//--------------------------------------------------
		public void Clear()
		{
			_extensionObjects.Clear();
			_parameters.Clear();
		}

		//--------------------------------------------------
		public object GetExtensionObject(
			string namespaceUri )
		{
			return _extensionObjects [namespaceUri];
		}

		//--------------------------------------------------
		public object GetParam(
			string name,
			string namespaceUri )
		{
			if (name == null)
				throw (new ArgumentException ("The parameter name is a null reference."));
			if (namespaceUri == null) 
				throw (new ArgumentException ("The namespaceUri is a null reference."));
			XmlQualifiedName qName = new XmlQualifiedName (name, namespaceUri);
			return _parameters [qName];
		}
		
		//--------------------------------------------------
		public object RemoveExtensionObject(
			string namespaceUri )
		{
			object thisObject = this.GetExtensionObject (namespaceUri);
			_extensionObjects.Remove (namespaceUri);
			return thisObject;
		}

		//--------------------------------------------------
		public object RemoveParam(
			string name,
			string namespaceUri )
		{
			XmlQualifiedName qName = new XmlQualifiedName( name, namespaceUri );
			object thisObject = this.GetParam (name, namespaceUri);
			_parameters.Remove (qName);
			return thisObject;
		}

		#endregion
	}
}
