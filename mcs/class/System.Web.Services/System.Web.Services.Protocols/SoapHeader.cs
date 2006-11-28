// 
// System.Web.Services.Protocols.SoapHeader.cs
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
using System.Xml.Serialization;
using System.Xml;

namespace System.Web.Services.Protocols {
	[SoapType (IncludeInSchema = false)]
	[XmlType (IncludeInSchema = false)]
	public abstract class SoapHeader {

		#region Fields

		string actor;
		bool didUnderstand;
		bool mustUnderstand;
		
#if NET_2_0
		string role;
		bool relay;
#endif

		#endregion // Fields

		#region Constructors

		protected SoapHeader ()
		{
			actor = String.Empty; 
			didUnderstand = false;
			mustUnderstand = false;
		}

		internal SoapHeader (XmlElement elem)
		{
			actor = elem.GetAttribute ("actor", WebServiceHelper.SoapEnvelopeNamespace);
			string me = elem.GetAttribute ("mustUnderstand", WebServiceHelper.SoapEnvelopeNamespace);
			if (me != "") EncodedMustUnderstand = me;
#if NET_2_0
			role = elem.GetAttribute ("role", WebServiceHelper.Soap12EnvelopeNamespace);
			me = elem.GetAttribute ("mustUnderstand", WebServiceHelper.Soap12EnvelopeNamespace);
			if (me != "") EncodedMustUnderstand12 = me;
#endif
		}

		#endregion // Constructors

		#region Properties

		[DefaultValue ("")]
		[SoapAttribute ("actor", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
		[XmlAttribute ("actor", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
		public string Actor {	
			get { return actor; }
			set { actor = value; }
		}

		[SoapIgnore]
		[XmlIgnore]
		public bool DidUnderstand {
			get { return didUnderstand; }
			set { didUnderstand = value; }
		}

		[DefaultValue ("0")]
		[SoapAttribute ("mustUnderstand", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
		[XmlAttribute ("mustUnderstand", Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
		public string EncodedMustUnderstand {
			get { return (MustUnderstand ? "1" : "0"); }
			set {	
				if (value == "true" || value == "1") 
					MustUnderstand = true;
				else if (value == "false" || value == "0")
					MustUnderstand = false;
				else
					throw new ArgumentException ();
			}
		}

		[SoapIgnore]
		[XmlIgnore]
		public bool MustUnderstand {
			get { return mustUnderstand; }
			set { mustUnderstand = value; }
		}
		
#if NET_2_0

		[DefaultValue ("0")]
		[SoapAttribute ("mustUnderstand", Namespace = "http://www.w3.org/2003/05/soap-envelope")]
		[XmlAttribute ("mustUnderstand", Namespace = "http://www.w3.org/2003/05/soap-envelope")]
		[System.Runtime.InteropServices.ComVisible(false)]
		public string EncodedMustUnderstand12 {
			get { return (MustUnderstand ? "1" : "0"); }
			set {	
				if (value == "true" || value == "1") 
					MustUnderstand = true;
				else if (value == "false" || value == "0")
					MustUnderstand = false;
				else
					throw new ArgumentException ();
			}
		}

		[DefaultValue ("0")]
		[SoapAttribute ("relay", Namespace = "http://www.w3.org/2003/05/soap-envelope")]
		[XmlAttribute ("relay", Namespace = "http://www.w3.org/2003/05/soap-envelope")]
		[System.Runtime.InteropServices.ComVisible(false)]
		public string EncodedRelay
		{
			get { return (Relay ? "1" : "0"); }
			set {	
				if (value == "true" || value == "1") 
					Relay = true;
				else if (value == "false" || value == "0")
					Relay = false;
				else
					throw new ArgumentException ();
			}
		}
		
		[SoapIgnore]
		[XmlIgnore]
		[System.Runtime.InteropServices.ComVisible(false)]
		public bool Relay {
			get { return relay; }
			set { relay = value; }
		}
		
		[DefaultValue ("")]
		[SoapAttribute ("role", Namespace = "http://www.w3.org/2003/05/soap-envelope")]
		[XmlAttribute ("role", Namespace = "http://www.w3.org/2003/05/soap-envelope")]
		[System.Runtime.InteropServices.ComVisible(false)]
		public string Role {
			get { return role; }
			set { role = value; }
		}
		
#endif

		#endregion // Properties
	}
}
