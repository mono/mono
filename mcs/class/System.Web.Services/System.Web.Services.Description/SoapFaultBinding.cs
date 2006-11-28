// 
// System.Web.Services.Description.SoapFaultBinding.cs
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
	[XmlFormatExtension ("fault", "http://schemas.xmlsoap.org/wsdl/soap/", typeof (FaultBinding))]
	public class SoapFaultBinding : ServiceDescriptionFormatExtension {

		#region Fields

		string encoding;
		string ns;
		SoapBindingUse use;
		string name;

		#endregion // Fields

		#region Constructors
		
		public SoapFaultBinding ()
		{
			encoding = String.Empty;
			ns = String.Empty;
			use = SoapBindingUse.Default;
		}
		
		#endregion // Constructors

		#region Properties

#if NET_2_0
		[DefaultValue ("")]
#endif
		[XmlAttribute ("encodingStyle")]
		public string Encoding {
			get { return encoding; }
			set { encoding = value; }
		}
	
		[XmlAttribute ("namespace")]	
		public string Namespace {
			get { return ns; }
			set { ns = value; }
		}

		[DefaultValue (SoapBindingUse.Default)]
		[XmlAttribute ("use")]
		public SoapBindingUse Use {
			get { return use; }
			set { use = value; }
		}

#if NET_2_0
		[XmlAttribute ("name")]
		public string Name {
			get { return name; }
			set { name = value; }
		}
#endif
		#endregion // Properties
	}
}
