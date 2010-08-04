//
// Author: Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2009 Novell, Inc (http://www.novell.com)
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
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace System.ServiceModel.Discovery
{
	public sealed class DiscoveryVersion
	{
		static DiscoveryVersion ()
		{
			v11 = new DiscoveryVersion ("WSDiscovery11", "http://docs.oasis-open.org/ws-dd/ns/discovery/2009/01", "urn:docs-oasis-open-org:ws-dd:ns:discovery:2009:01", MessageVersion.Soap12WSAddressing10);
			april2005 = new DiscoveryVersion ("WSDiscoveryApril2005", "http://schemas.xmlsoap.org/ws/2005/04/discovery", "urn:schemas-xmlsoap-org:ws:2005:04:discovery", MessageVersion.Soap12WSAddressingAugust2004);
			cd1 = new DiscoveryVersion ("WSDiscoveryCD1", "http://docs.oasis-open.org/ws-dd/ns/discovery/2008/09", "urn:docs-oasis-open-org:ws-dd:discovery:2008:09", MessageVersion.Soap12WSAddressingAugust2004);
		}

		static readonly DiscoveryVersion v11, april2005, cd1;

		public static DiscoveryVersion WSDiscovery11 {
			get { return v11; }
		}

		public static DiscoveryVersion WSDiscoveryApril2005 {
			get { return april2005; }
		}

		public static DiscoveryVersion WSDiscoveryCD1 {
			get { return cd1; }
		}

		public static DiscoveryVersion FromName (string name)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			switch (name) {
			case "WSDiscovery11":
				return v11;
			case "WSDiscoveryApril2005":
				return april2005;
			case "WSDiscoveryCD1":
				return cd1;
			default:
				throw new ArgumentNullException (String.Format ("Invalid version name: {0}", name));
			}
		}

		internal DiscoveryVersion (string name, string ns, string adhoc, MessageVersion version)
		{
			this.Name = name;
			this.Namespace = ns;
			AdhocAddress = new Uri (adhoc);
			MessageVersion = version;
		}

		public Uri AdhocAddress { get; private set; }
		public MessageVersion MessageVersion { get; private set; }
		public string Name { get; private set; }
		public string Namespace { get; private set; }

		public override string ToString ()
		{
			return Name;
		}
	}
}
