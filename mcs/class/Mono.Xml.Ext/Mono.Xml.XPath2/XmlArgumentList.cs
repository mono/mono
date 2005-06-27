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

#if NET_2_0

using System.Collections;
using System.Xml;

namespace System.Xml.Query 
{
	public sealed class XmlArgumentList
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
