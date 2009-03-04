//
// ClientAccessPolicy.cs
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
using System.Linq;
using System.Net;
using System.Xml;

/*
default namespace = ""

grammar {

start = access-policy

access-policy = element access-policy {
  element cross-domain-access {
    element policy { allow-from, grant-to }
  }
}

allow-from = element allow-from {
  attribute http-request-headers { text },
  element domain {
    attribute uri { text }
  }
}

grant-to = element grant-to {
  (resource | socket-resource)+
}

resource = element resource {
  attribute path { text },
  attribute include-subpaths { "true" | "false" }
}

socket-resource = element socket-resource {
  attribute port { text },
  attribute protocol { text }
}

}
*/

namespace System.ServiceModel
{
	class ClientAccessPolicy
	{
/*
public static void Main (string [] args)
{
	foreach (string s in args)
		using (var r = XmlReader.Create (s))
			ClientAccessPolicyReader.Read (r);
}
*/

		public static ClientAccessPolicy Read (XmlReader reader)
		{
			var r = new ClientAccessPolicyReader (reader);
			r.Read ();
			return r.Result;
		}

		public ClientAccessPolicy ()
		{
			AllowedServices = new List<AllowFrom> ();
			GrantedResources = new List<GrantTo> ();
		}

		public List<AllowFrom> AllowedServices { get; private set; }
		public List<GrantTo> GrantedResources { get; private set; }

		public class AllowFrom
		{
			public AllowFrom ()
			{
				Domains = new List<Uri> ();
				AllowAllHeaders = true;
			}

			string [] headers;

			public bool AllowAllHeaders { get; private set; }

			public bool AllowAnyDomain { get; set; }

			public List<Uri> Domains { get; private set; }

			public void SetHttpRequestHeaders (string raw)
			{
				if (raw == "*")
					AllowAllHeaders = true;
				else {
					headers = raw.Split (',');
					for (int i = 0; i < headers.Length; i++)
						headers [i] = headers [i].Trim ();
				}
			}

			public bool IsAllowed (Uri uri, string [] headerKeys)
			{
				// check headers
				if (!AllowAllHeaders && headerKeys.All (s => Array.IndexOf (headers, s) < 0))
					return false;
				// check domains
				if (!AllowAnyDomain && Domains.All (domain => domain.Host != uri.Host))
					return false;
				return true;
			}
		}

		public class GrantTo
		{
			public GrantTo ()
			{
				Resources = new List<Resource> ();
				SocketResources = new List<SocketResource> ();
			}

			public List<Resource> Resources { get; private set; }
			public List<SocketResource> SocketResources { get; private set; }

			public bool IsGranted (Uri uri)
			{
				foreach (var gr in Resources) {
					if (gr.IncludeSubpaths) {
						if (uri.LocalPath.StartsWith (gr.Path, StringComparison.Ordinal))
							return true;
					} else {
						if (uri.LocalPath == gr.Path)
							return true;
					}
				}
				foreach (var sr in SocketResources) {
					if (sr.Protocol != "*" && sr.Protocol != uri.Scheme) // FIXME: what is expected values in "protocol" attribute?
						continue;
					if (sr.Port != -1 && sr.Port != uri.Port)
						continue;
					return true;
				}
				return false;
			}
		}

		public class Resource
		{
			public string Path { get; set; }
			public bool IncludeSubpaths { get; set; }
		}

		public class SocketResource
		{
			public SocketResource ()
			{
				Port = -1;
			}

			public int Port { get; set; }
			public string Protocol { get; set; }
		}

		class ClientAccessPolicyReader
		{
			public ClientAccessPolicyReader (XmlReader reader)
			{
				this.reader = reader;
				cap = new ClientAccessPolicy ();
			}

			XmlReader reader;
			ClientAccessPolicy cap;

			public ClientAccessPolicy Result {
				get { return cap; }
			}

			public void Read ()
			{
				reader.MoveToContent ();
				if (reader.IsEmptyElement) {
					reader.Skip ();
					return;
				}
				reader.ReadStartElement ("access-policy", String.Empty);
				for (reader.MoveToContent (); reader.NodeType != XmlNodeType.EndElement; reader.MoveToContent ()) {
					if (reader.NodeType != XmlNodeType.Element)
						throw new XmlException (String.Format ("Unexpected access-policy content: {0}", reader.NodeType));
					if (reader.IsEmptyElement) {
						reader.Skip ();
						continue;
					}
					reader.ReadStartElement ("cross-domain-access", String.Empty);
					for (reader.MoveToContent (); reader.NodeType != XmlNodeType.EndElement; reader.MoveToContent ()) {
						if (reader.NodeType != XmlNodeType.Element)
							throw new XmlException (String.Format ("Unexpected access-policy content: {0}", reader.NodeType));
						ReadPolicyElement ();
					}
					reader.ReadEndElement ();
				}
				reader.ReadEndElement ();
			}

			void ReadPolicyElement ()
			{
				if (reader.IsEmptyElement) {
					reader.Skip ();
					return;
				}

				reader.ReadStartElement ("policy", String.Empty);
				for (reader.MoveToContent (); reader.NodeType != XmlNodeType.EndElement; reader.MoveToContent ()) {
					if (reader.NodeType != XmlNodeType.Element)
						throw new XmlException (String.Format ("Unexpected policy content: {0}", reader.NodeType));
					if (reader.IsEmptyElement || reader.NamespaceURI != String.Empty) {
						reader.Skip ();
						continue;
					}
					switch (reader.LocalName) {
					case "allow-from":
						ReadAllowFromElement ();
						break;
					case "grant-to":
						ReadGrantToElement ();
						break;
					default:
						reader.Skip ();
						continue;
					}
				}
				reader.ReadEndElement ();
			}

			void ReadAllowFromElement ()
			{
				var v = new AllowFrom ();
				cap.AllowedServices.Add (v);

				if (reader.IsEmptyElement) {
					reader.Skip ();
					return;
				}

				v.SetHttpRequestHeaders (reader.GetAttribute ("http-request-headers"));
				reader.ReadStartElement ("allow-from", String.Empty);
				for (reader.MoveToContent (); reader.NodeType != XmlNodeType.EndElement; reader.MoveToContent ()) {
					if (reader.NodeType != XmlNodeType.Element)
						throw new XmlException (String.Format ("Unexpected allow-from content: {0}", reader.NodeType));
					if (reader.NamespaceURI != String.Empty) {
						reader.Skip ();
						continue;
					}
					switch (reader.LocalName) {
					case "domain":
						var d = reader.GetAttribute ("uri");
						if (d == "*")
							v.AllowAnyDomain = true;
						else
							v.Domains.Add (new Uri (d));
						reader.Skip ();
						break;
					default:
						reader.Skip ();
						continue;
					}
				}
				reader.ReadEndElement ();
			}

			void ReadGrantToElement ()
			{
				var v = new GrantTo ();
				cap.GrantedResources.Add (v);

				if (reader.IsEmptyElement) {
					reader.Skip ();
					return;
				}

				reader.ReadStartElement ("grant-to", String.Empty);
				for (reader.MoveToContent (); reader.NodeType != XmlNodeType.EndElement; reader.MoveToContent ()) {
					if (reader.NodeType != XmlNodeType.Element)
						throw new XmlException (String.Format ("Unexpected grant-to content: {0}", reader.NodeType));
					if (reader.NamespaceURI != String.Empty) {
						reader.Skip ();
						continue;
					}
					switch (reader.LocalName) {
					case "resource":
						var r = new Resource ();
						v.Resources.Add (r);
						r.Path = reader.GetAttribute ("path");
						if (reader.MoveToAttribute ("include-subpaths")) {
							r.IncludeSubpaths = XmlConvert.ToBoolean (reader.Value);
							reader.MoveToElement ();
						}
						break;
					case "socket-resource":
						var sr = new SocketResource ();
						v.SocketResources.Add (sr);
						if (reader.MoveToAttribute ("port")) {
							var p = reader.GetAttribute ("port");
							sr.Port = p == "*" ? -1 : XmlConvert.ToInt32 (p);
							reader.MoveToElement ();
						}
						sr.Protocol = reader.GetAttribute ("protocol");
						break;
					}
					reader.Skip ();
				}
				reader.ReadEndElement ();
			}
		}
	}
}
