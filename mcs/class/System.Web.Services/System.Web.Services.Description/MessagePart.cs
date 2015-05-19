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
	[XmlFormatExtensionPoint ("Extensions")]
	public sealed class MessagePart :
		NamedItem
	{

		#region Fields

		XmlQualifiedName element;
		Message message;
		XmlQualifiedName type;
		ServiceDescriptionFormatExtensionCollection extensions;

		#endregion // Fields

		#region Constructors
		
		public MessagePart ()
		{
			element = XmlQualifiedName.Empty;
			message = null;
			type = XmlQualifiedName.Empty;
			extensions = new ServiceDescriptionFormatExtensionCollection (this);
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

		[XmlIgnore]
		public override ServiceDescriptionFormatExtensionCollection Extensions {
			get { return extensions; }
		}

		#endregion // Properties

		#region Methods

		internal void SetParent (Message message)
		{
			this.message = message; 
		}

		#endregion // Methods

	}
}
