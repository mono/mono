//
// CrossDomainPolicy.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2009 Novell, Inc.  http://www.novell.com
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
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

/*

# This grammar is based on the xsd from Adobe, but the schema is wrong.
# It should have used interleave (all). Some crossdomain.xml are invalidated.
# (For example, try mono-xmltool --validate-xsd http://www.adobe.com/xml/schemas/PolicyFile.xsd http://twitter.com/crossdomain.xml)

default namespace = ""

grammar {

start = cross-domain-policy

cross-domain-policy = element cross-domain-policy {
  element site-control {
    attribute permitted-cross-domain-policies {
      "all" | "by-contract-type" | "by-ftp-filename" | "master-only" | "none"
    }
  }?,
  element allow-access-from {
    attribute domain { text },
    attribute to-ports { text }?,
    attribute secure { xs:boolean }?
  }*,
  element allow-http-request-headers-from {
    attribute domain { text },
    attribute headers { text },
    attribute secure { xs:boolean }?
  }*,
  element allow-access-from-identity {
    element signatory {
      element certificate {
        attribute fingerprint { text },
        attribute fingerprint-algorithm { text }
      }
    }
  }*
}

}

*/

namespace System.ServiceModel
{
	class CrossDomainPolicy
	{
/*
public static void Main (string [] args)
{
	foreach (string s in args)
		using (var r = XmlReader.Create (s))
			CrossDomainPolicyReader.Read (r);
}
*/
		public static CrossDomainPolicy Read (XmlReader reader)
		{
			var r = new CrossDomainPolicyReader (reader);
			r.Read ();
			return r.Result;
		}

		public CrossDomainPolicy ()
		{
			AllowedAccesses = new List<AllowAccessFrom> ();
			AllowedHttpRequestHeaders = new List<AllowHttpRequestHeadersFrom> ();
			AllowedIdentities = new List<AllowAccessFromIdentity> ();
		}

		public string SiteControl { get; set; }
		public List<AllowAccessFrom> AllowedAccesses { get; private set; }
		public List<AllowHttpRequestHeadersFrom> AllowedHttpRequestHeaders { get; private set; }
		public List<AllowAccessFromIdentity> AllowedIdentities { get; private set; }

		public class AllowAccessFrom
		{
			public AllowAccessFrom ()
			{
				throw new XmlException ("Silverlight does not support allow-access-from specification in cross-domain.xml");
			}

			public string Domain { get; set; }
			public string ToPorts { get; set; }
			public bool Secure { get; set; }
		}

		public class AllowHttpRequestHeadersFrom
		{
			public string Domain { get; set; }
			public string Headers { get; set; }
			public bool Secure { get; set; }
		}

		public class AllowAccessFromIdentity
		{
			public AllowAccessFromIdentity ()
			{
				throw new XmlException ("Silverlight does not support allow-access-from-identity specification in cross-domain.xml");
			}

			public string Fingerprint { get; set; }
			public string FingerprintAlgorithm { get; set; }
		}

		class CrossDomainPolicyReader
		{
			public CrossDomainPolicyReader (XmlReader reader)
			{
				this.reader = reader;
				cdp = new CrossDomainPolicy ();
			}

			XmlReader reader;
			CrossDomainPolicy cdp;

			public CrossDomainPolicy Result {
				get { return cdp; }
			}

			public void Read ()
			{
				reader.MoveToContent ();
				if (reader.IsEmptyElement) {
					reader.Skip ();
					return;
				}
				reader.ReadStartElement ("cross-domain-policy", String.Empty);
				for (reader.MoveToContent (); reader.NodeType != XmlNodeType.EndElement; reader.MoveToContent ()) {
					if (reader.NodeType != XmlNodeType.Element)
						throw new XmlException (String.Format ("Unexpected cross-domain-policy content: {0}", reader.NodeType));
					switch (reader.LocalName) {
					case "site-control":
						cdp.SiteControl = reader.GetAttribute ("permitted-cross-domain-policies");
						reader.Skip ();
						break;
					case "allow-access-from":
						var a = new AllowAccessFrom () {
							Domain = reader.GetAttribute ("domain"),
							ToPorts = reader.GetAttribute ("to-ports"),
							Secure = reader.GetAttribute ("secure") == "true" };
						cdp.AllowedAccesses.Add (a);
						reader.Skip ();
						break;
					case "allow-http-request-headers-from":
						var h = new AllowHttpRequestHeadersFrom () {
							Domain = reader.GetAttribute ("domain"),
							Headers = reader.GetAttribute ("headers"),
							Secure = reader.GetAttribute ("secure") == "true" };
						cdp.AllowedHttpRequestHeaders.Add (h);
						reader.Skip ();
						break;
					case "allow-access-from-identity":
						if (reader.IsEmptyElement)
							throw new XmlException ("non-empty element 'allow-access-from-identity' is expected");
						reader.ReadStartElement ();
						reader.MoveToContent ();
						if (reader.IsEmptyElement)
							throw new XmlException ("non-empty element 'signatory' is expected");
						reader.ReadStartElement ("signatory", String.Empty);
						reader.MoveToContent ();
						if (reader.LocalName != "certificate" || reader.NamespaceURI != String.Empty)
							throw new XmlException ("element 'certificate' is expected");
						var i = new AllowAccessFromIdentity () {
							Fingerprint = reader.GetAttribute ("fingerprint"),
							FingerprintAlgorithm = reader.GetAttribute ("fingerprint-algorithm") };
						cdp.AllowedIdentities.Add (i);
						reader.Skip ();
						break;
					default:
						reader.Skip ();
						continue;
					}
				}
				reader.ReadEndElement ();
			}
		}
	}
}
