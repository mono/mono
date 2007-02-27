// 
// System.Web.Services.Description.DocumentableItem.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
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

using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Xml.Serialization;
using System.Xml;

namespace System.Web.Services.Description {
	public abstract class DocumentableItem {

		#region Fields

		XmlElement docElement;

#if NET_2_0
		XmlAttribute [] extAttributes;
		XmlSerializerNamespaces namespaces;
#endif

		#endregion // Fields

		#region Constructors

		protected DocumentableItem ()
		{
		}
		
		#endregion // Constructors

		#region Properties

		[XmlIgnore]
		public string Documentation {
			get { 
				return docElement != null ? docElement.InnerText : ""; 
			}
			
			set {
				if (value == null || value.Length == 0)
					docElement = null;
				else {
					XmlDocument doc = new XmlDocument ();
					docElement = doc.CreateElement ("wsdl", "documentation", "http://schemas.xmlsoap.org/wsdl/");
					docElement.InnerText = value;
				}
			}
		}

		[ComVisible (false)]
		[XmlAnyElement (Name="documentation", Namespace="http://schemas.xmlsoap.org/wsdl/")]
		public XmlElement DocumentationElement
		{
			get { return docElement; }
			set { docElement = value; }
		}

#if NET_2_0
		[XmlAnyAttribute]
		public XmlAttribute [] ExtensibleAttributes {
			get { return extAttributes; }
			set { extAttributes = value; }
		}

		[XmlIgnore]
		public abstract ServiceDescriptionFormatExtensionCollection Extensions {
			get;
		}

		[XmlNamespaceDeclarations]
		public XmlSerializerNamespaces Namespaces {
			get { 
				if (namespaces == null)
					namespaces = new XmlSerializerNamespaces ();
				return namespaces;
			}
			set { namespaces = value; }
		}
#endif

		#endregion // Properties
	}
}
