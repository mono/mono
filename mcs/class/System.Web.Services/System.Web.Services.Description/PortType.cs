// 
// System.Web.Services.Description.PortType.cs
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

using System.Xml.Serialization;
using System.Web.Services.Configuration;

namespace System.Web.Services.Description
{
#if NET_2_0
	[XmlFormatExtensionPoint ("Extensions")]
#endif
	public sealed class PortType :
#if NET_2_0
		NamedItem
#else
		DocumentableItem 
#endif
	{
		#region Fields

#if !NET_2_0
		string name;
#endif
		OperationCollection operations;
		ServiceDescription serviceDescription;
#if NET_2_0
		ServiceDescriptionFormatExtensionCollection extensions;
#endif

		#endregion // Fields

		#region Constructors
		
		public PortType ()
		{
			operations = new OperationCollection (this);
			serviceDescription = null;
#if NET_2_0
			extensions = new ServiceDescriptionFormatExtensionCollection (this);
#endif
		}
		
		#endregion // Constructors

		#region Properties

#if !NET_2_0
		[XmlAttribute ("name", DataType = "NCName")]
		public string Name {
			get { return name; }
			set { name = value; }
		}
#endif

		[XmlElement ("operation")]
		public OperationCollection Operations {
			get { return operations; }
		}
	
//		[XmlIgnore]
		public ServiceDescription ServiceDescription {
			get { return serviceDescription; }
		}

#if NET_2_0
		[XmlIgnore]
		public override ServiceDescriptionFormatExtensionCollection Extensions {
			get { return extensions; }
		}
#endif

		#endregion // Properties

		#region Methods

		internal void SetParent (ServiceDescription serviceDescription)
		{
			this.serviceDescription = serviceDescription;
		}

		#endregion // Methods
	}
}
