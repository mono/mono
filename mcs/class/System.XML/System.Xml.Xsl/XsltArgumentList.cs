// System.Xml.Xsl.XsltArgumentList
//
// Author: Tim Coleman <tim@timcoleman.com>
// (C) Copyright 2002 Tim Coleman
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using System.Collections;
using System.Security.Permissions;
using System.Xml.XPath;

namespace System.Xml.Xsl
{
	public class XsltArgumentList
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

		#region Event

#pragma warning disable 67
		[MonoTODO]
		public event XsltMessageEncounteredEventHandler XsltMessageEncountered;
#pragma warning restore 67

		#endregion

		#region Methods

		[PermissionSet (SecurityAction.LinkDemand, Unrestricted = true)]
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

			if (parameter is Int16)			return (double) (Int16) parameter;
			if (parameter is UInt16)		return (double) (UInt16) parameter;
			if (parameter is Int32)			return (double) (Int32) parameter;
			if (parameter is Int64)			return (double) (Int64) parameter;
			if (parameter is UInt64)		return (double) (UInt64) parameter;
			if (parameter is Single)		return (double) (Single) parameter;
			if (parameter is decimal)		return (double) (decimal) parameter;

			return parameter.ToString ();
		}

		#endregion
	}
}
