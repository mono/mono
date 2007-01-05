// 
// System.Web.Services.Description.MimeContentBinding.cs
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
using System.Web.Services.Configuration;
using System.Xml.Serialization;

namespace System.Web.Services.Description {
	[XmlFormatExtensionPrefix ("mime", "http://schemas.xmlsoap.org/wsdl/mime/")]
	[XmlFormatExtension ("content", "http://schemas.xmlsoap.org/wsdl/mime/", typeof (InputBinding), typeof (OutputBinding))]
	public sealed class MimeContentBinding : ServiceDescriptionFormatExtension {

		#region Fields

		public const string Namespace = "http://schemas.xmlsoap.org/wsdl/mime/";
		string part;
		string type;

		#endregion // Fields

		#region Constructors
		
		public MimeContentBinding ()
		{
			part = String.Empty;
			type = String.Empty;
		}
		
		#endregion // Constructors

		#region Properties

		[XmlAttribute ("part")]
		public string Part {
			get { return part; }
			set { part = value; }
		}

		[XmlAttribute ("type")]
		public string Type {
			get { return type; }
			set { type = value; }
		}

		#endregion // Properties
	}
}
