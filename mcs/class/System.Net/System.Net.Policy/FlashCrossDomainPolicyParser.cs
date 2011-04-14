//
// FlashCrossDomainPolicyParser.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//	Moonlight List (moonlight-list@lists.ximian.com)
//
// Copyright (C) 2009-2010 Novell, Inc.  http://www.novell.com
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

#if NET_2_1

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

/*

Specification: http://www.adobe.com/devnet/articles/crossdomain_policy_file_spec.html

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

namespace System.Net.Policy {

	partial class FlashCrossDomainPolicy {

		static bool ReadBooleanAttribute (string attribute)
		{
			switch (attribute) {
			case null:
			case "true":
				return true;
			case "false":
				return false;
			default:
				throw new XmlException ();
			}
		}

		// only "domain" and "secure" attributes are allowed - anything else is considered invalid
		static AllowAccessFrom CreateAllowAccessFrom (XmlReader reader)
		{
			int n = reader.AttributeCount;
			string domain = reader.GetAttribute ("domain");
			if (domain != null)
				n--;
			string secure = reader.GetAttribute ("secure");
			if (secure != null)
				n--;
			if (n != 0)
				throw new XmlException ("unknown/unsupported attributes");

			return new AllowAccessFrom () { Domain = domain, Secure = ReadBooleanAttribute (secure) };
		}

		// only "domain", "secure" and "headers" attributes are allowed - anything else is considered invalid
		static AllowHttpRequestHeadersFrom CreateAllowHttpRequestHeadersFrom (XmlReader reader)
		{
			int n = reader.AttributeCount;
			string domain = reader.GetAttribute ("domain");
			if (domain != null)
				n--;
			string secure = reader.GetAttribute ("secure");
			if (secure != null)
				n--;
			string headers = reader.GetAttribute ("headers");
			if (headers != null)
				n--;
			if (n != 0)
				throw new XmlException ("unknown/unsupported attributes");

			var h = new AllowHttpRequestHeadersFrom () { Domain = domain, Secure = ReadBooleanAttribute (secure) };
			h.Headers.SetHeaders (headers);
			return h;
		}

		// only "permitted-cross-domain-policies" attribute is allowed - anything else is considered invalid
		static string GetSiteControl (XmlReader reader)
		{
			int n = reader.AttributeCount;
			string site = reader.GetAttribute ("permitted-cross-domain-policies");
			if (site != null)
				n--;
			if (n != 0)
				throw new XmlException ("unknown/unsupported attributes");
			return site;
		}

		static public ICrossDomainPolicy FromStream (Stream stream)
		{
			FlashCrossDomainPolicy cdp = new FlashCrossDomainPolicy ();

			// Silverlight accepts whitespaces before the XML - which is invalid XML
			StreamReader sr = new StreamReader (stream);
			while (Char.IsWhiteSpace ((char) sr.Peek ()))
				sr.Read ();

			XmlReaderSettings policy_settings = new XmlReaderSettings ();
			policy_settings.DtdProcessing = DtdProcessing.Ignore;
			using (XmlReader reader = XmlReader.Create (sr, policy_settings)) {

				reader.MoveToContent ();
				if (reader.HasAttributes || reader.IsEmptyElement) {
					reader.Skip ();
					return null;
				}

				while (!reader.EOF) {
					reader.ReadStartElement ("cross-domain-policy", String.Empty);
					for (reader.MoveToContent (); reader.NodeType != XmlNodeType.EndElement; reader.MoveToContent ()) {
						if (reader.NodeType != XmlNodeType.Element) {
							reader.Skip ();
							continue;
						}

						switch (reader.LocalName) {
						case "site-control":
							cdp.SiteControl = GetSiteControl (reader);
							reader.Skip ();
							break;
						case "allow-access-from":
							var a = CreateAllowAccessFrom (reader);
							cdp.AllowedAccesses.Add (a);
							reader.Skip ();
							break;
						case "allow-http-request-headers-from":
							var h = CreateAllowHttpRequestHeadersFrom (reader);
							cdp.AllowedHttpRequestHeaders.Add (h);
							reader.Skip ();
							break;
						default:
							reader.Skip ();
							return null;
						}
					}
					reader.ReadEndElement ();
					reader.MoveToContent ();
				}
			}

			// if none supplied set a default for headers
			if (cdp.AllowedHttpRequestHeaders.Count == 0) {
				var h = new AllowHttpRequestHeadersFrom () { Domain = "*", Secure = true };
				h.Headers.SetHeaders (null); // defaults
				cdp.AllowedHttpRequestHeaders.Add (h);
			}
			return cdp;
		}
	}
}

#endif

