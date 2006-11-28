// 
// System.Web.Services.Description.MessagePart.cs
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

using System.Xml;
using System.Xml.Serialization;
using System.Web.Services.Configuration;

namespace System.Web.Services.Description 
{
#if NET_2_0
	[XmlFormatExtensionPoint ("Extensions")]
#endif
	public sealed class MessagePart :
#if NET_2_0
		NamedItem
#else
		DocumentableItem 
#endif
	{

		#region Fields

		XmlQualifiedName element;
		Message message;
#if !NET_2_0
		string name;
#endif
		XmlQualifiedName type;
#if NET_2_0
		ServiceDescriptionFormatExtensionCollection extensions;
#endif

		#endregion // Fields

		#region Constructors
		
		public MessagePart ()
		{
			element = XmlQualifiedName.Empty;
			message = null;
#if !NET_2_0
			name = String.Empty;
#endif
			type = XmlQualifiedName.Empty;
#if NET_2_0
			extensions = new ServiceDescriptionFormatExtensionCollection (this);
#endif
		}
		
		#endregion // Constructors

		#region Properties

		[XmlAttribute ("element")]
		public XmlQualifiedName Element {
			get { return element; }
			set { element = value; }
		}
		
//		[XmlIgnore]
		public Message Message {
			get { return message; }
		}
	
#if !NET_2_0
		[XmlAttribute ("name", DataType = "NMTOKEN")]
		public string Name {
			get { return name; }
			set { name = value; }
		}
#endif

		[XmlAttribute ("type")]
		public XmlQualifiedName Type {
			get { return type; }
			set { type = value; }
		}
		
		internal bool DefinedByType {
			get { return type != null && type != XmlQualifiedName.Empty; }
		}

		internal bool DefinedByElement {
			get { return element != null && element != XmlQualifiedName.Empty; }
		}

#if NET_2_0
		[XmlIgnore]
		public override ServiceDescriptionFormatExtensionCollection Extensions {
			get { return extensions; }
		}
#endif

		#endregion // Properties

		#region Methods

		internal void SetParent (Message message)
		{
			this.message = message; 
		}

		#endregion // Methods

	}
}
