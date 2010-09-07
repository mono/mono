//
// Author: Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2009,2010 Novell, Inc (http://www.novell.com)
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
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace System.ServiceModel.Discovery.Version11
{
	[XmlSchemaProvider ("GetSchema")]
	public class DiscoveryMessageSequence11 : IXmlSerializable
	{
		public static DiscoveryMessageSequence11 FromDiscoveryMessageSequence (DiscoveryMessageSequence discoveryMessageSequence)
		{
			return new DiscoveryMessageSequence11 (discoveryMessageSequence);
		}

		static readonly DiscoveryVersion version = DiscoveryVersion.WSDiscovery11;
		static XmlSchema schema;
		
		static XmlSchema Schema {
			get {
				if (schema == null)
					schema = DiscoveryMessageSequence.BuildSchema (version);
				return schema;
			}
		}

		public static XmlQualifiedName GetSchema (XmlSchemaSet schemaSet)
		{
			schemaSet.Add (Schema);
			return new XmlQualifiedName ("AppSequenceType", version.Namespace);
		}

		// for deserialization
		DiscoveryMessageSequence11 ()
		{
		}

		internal DiscoveryMessageSequence11 (DiscoveryMessageSequence source)
		{
			this.source = source;
		}

		DiscoveryMessageSequence source;

		public XmlSchema GetSchema ()
		{
			return Schema;
		}

		public void ReadXml (XmlReader reader)
		{
			source = DiscoveryMessageSequence.ReadXml (reader, version);
		}

		public DiscoveryMessageSequence ToDiscoveryMessageSequence ()
		{
			if (source == null)
				throw new InvalidOperationException ("Call ReadXml method first to fill its contents");
			return source;
		}

		public void WriteXml (XmlWriter writer)
		{
			source.WriteXml (writer);
		}
	}
}
