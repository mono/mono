// 
// Fault12.cs
//
// Author:
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.
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

//
// This file is used to generate Fault12Serializer.cs with fault-12.genxs.
// To generate the file, do:
//	- replace _internal_ class in this file to _public_ class
//	- optionally you might have to remove Fault12Serializer.cs from
//	  System.Web.Services.dll.sources.
//	- Build System.Web.Services.dll with make PROFILE=net_2_0.
//	- run genxs.exe with 2.0 libraries (the easiest way would be
//	  to build genxs under 2.0 profile i.e. make PROFILE=net_2_0)
//	- Edit Fault12Serializer.cs to rename "FaultSerializer" to
//	  "Fault12Serializer" as the name is a duplicate, and
//	  wrap the entire code with #if NET_2_0.
//	- revert _public_ class in this file to _internal_ class back.
//

using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Text;
using System.Collections;
using System.Globalization;

namespace System.Web.Services.Protocols
{
	[XmlRoot ("Fault", Namespace = "http://www.w3.org/2003/05/soap-envelope")]
	[XmlType ("Fault", Namespace = "http://www.w3.org/2003/05/soap-envelope")]
	internal class Soap12Fault
	{
		// dummy constructor to not be rejected by genxs.
		public Soap12Fault ()
		{
		}

		public static XmlSerializer Serializer =
#if NET_2_0
			new Fault12Serializer ();
#else
			null;
#endif

#if NET_2_0

		public Soap12Fault (SoapException ex) 
		{
			Code = new Soap12FaultCode ();
			Code.Value = ex.Code;
			if (ex.SubCode != null)
				Code.Subcode = CreateFaultCode (ex.SubCode);
			Node = ex.Node;
			Role = ex.Role;
			Reason = new Soap12FaultReason ();
			Soap12FaultReasonText text =
				new Soap12FaultReasonText ();
			text.XmlLang = ex.Lang;
			text.Value = ex.Message;
			Reason.Texts = new Soap12FaultReasonText [] {text};
			if (ex.Detail != null) {
				Detail = new Soap12FaultDetail ();
				if (ex.Detail.NodeType == XmlNodeType.Attribute)
					Detail.Attributes = new XmlAttribute [] {
						(XmlAttribute) ex.Detail};
				else if (ex.Detail.NodeType == XmlNodeType.Element)
					Detail.Children = new XmlElement [] {
						(XmlElement) ex.Detail};
				else
					Detail.Text = ex.Detail.Value;
			}
		}

		static Soap12FaultCode CreateFaultCode (SoapFaultSubCode code)
		{
			if (code == null)
				throw new ArgumentNullException ("code");
			Soap12FaultCode ret = new Soap12FaultCode ();
			ret.Value = code.Code;
			if (code.SubCode != null)
				ret.Subcode = CreateFaultCode (code.SubCode);
			return ret;
		}

		public static SoapFaultSubCode GetSoapFaultSubCode (Soap12FaultCode src)
		{
			return (src == null) ? null :
				new SoapFaultSubCode (src.Value, GetSoapFaultSubCode (src.Subcode));
		}
#endif

		public Soap12FaultCode Code;

		public Soap12FaultReason Reason;

		[XmlElement (DataType = "anyURI")]
		public string Node;

		[XmlElement (DataType = "anyURI")]
		public string Role;

		public Soap12FaultDetail Detail;
	}

	[XmlType ("Code", Namespace = "http://www.w3.org/2003/05/soap-envelope")]
	internal class Soap12FaultCode
	{
		public XmlQualifiedName Value;

		public Soap12FaultCode Subcode;
	}

	[XmlType ("Reason", Namespace = "http://www.w3.org/2003/05/soap-envelope")]
	internal class Soap12FaultReason
	{
		[XmlElement ("Text", Namespace = "http://www.w3.org/2003/05/soap-envelope")]
		public Soap12FaultReasonText [] Texts;
	}

	[XmlType ("Text", Namespace = "http://www.w3.org/2003/05/soap-envelope")]
	internal class Soap12FaultReasonText
	{
		[XmlAttribute ("lang", Namespace = "http://www.w3.org/XML/1998/namespace")]
		public string XmlLang;

		[XmlText]
		public string Value;
	}

	[XmlType ("Detail", Namespace = "http://www.w3.org/2003/05/soap-envelope")]
	internal class Soap12FaultDetail
	{
		[XmlAnyAttribute]
		public XmlAttribute [] Attributes;

		[XmlAnyElement]
		public XmlElement [] Children;

		[XmlText]
		public string Text;
	}
}
