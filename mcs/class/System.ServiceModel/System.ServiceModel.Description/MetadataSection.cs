//
// MetadataSection.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//	Ankit Jain <jankit@novell.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
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
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Net.Security;
using System.ServiceModel.Channels;
using System.ServiceModel.Configuration;
using System.ServiceModel.Description;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

using WSServiceDescription = System.Web.Services.Description.ServiceDescription;

namespace System.ServiceModel.Description
{
	[XmlRoot ("MetadataSection", Namespace = "http://schemas.xmlsoap.org/ws/2004/09/mex")]
	public class MetadataSection
	{
		string dialect;
		string identifier;
		object metadata;
		Collection<XmlAttribute> attributes;
		static MetadataSectionSerializer serializer;

		static MetadataSection ()
		{
			serializer = new MetadataSectionSerializer ();
		}
		
		public MetadataSection ()
			: this (null, null, null)
		{
		}

		public MetadataSection (string dialect, string identifier, object metadata)
		{
			this.dialect = dialect;
			this.identifier = identifier;
			this.metadata = metadata;

			attributes = new Collection<XmlAttribute> ();
		}

		public static string MetadataExchangeDialect {
			get { return "http://schemas.xmlsoap.org/ws/2004/09/mex"; }
		}

		public static string PolicyDialect {
			get { return "http://schemas.xmlsoap.org/ws/2004/09/policy"; }
		}

		public static string ServiceDescriptionDialect {
			get { return "http://schemas.xmlsoap.org/wsdl/"; }
		}

		public static string XmlSchemaDialect {
			get { return "http://www.w3.org/2001/XMLSchema"; }
		}

		[XmlAttribute]
		public string Dialect {
			get { return dialect; }
			set { dialect = value; }
		}

		[XmlAttribute]
		public string Identifier {
			get { return identifier; }
			set { identifier = value; }
		}

		[XmlElement ("Location", Namespace = "http://schemas.xmlsoap.org/ws/2004/09/mex", Type = typeof (MetadataLocation), IsNullable = false)]
		[XmlElement ("Metadata", Namespace = "http://schemas.xmlsoap.org/ws/2004/09/mex", Type = typeof (MetadataSet), IsNullable = false)]
		[XmlElement ("schema", Namespace = "http://www.w3.org/2001/XMLSchema", Type = typeof (XmlSchema), IsNullable = false)]
		[XmlElement ("definitions", Namespace = "http://schemas.xmlsoap.org/wsdl/", Type = typeof (System.Web.Services.Description.ServiceDescription), IsNullable = false)]
		[XmlElement ("MetadataReference", Namespace = "http://schemas.xmlsoap.org/ws/2004/09/mex", Type = typeof (MetadataReference), IsNullable = false)]
		[XmlAnyElement]
		public object Metadata {
			get { return metadata; }
			set { metadata = value; }
		}

		[XmlAnyAttribute]
		public Collection<XmlAttribute> Attributes {
			get { return attributes; }
		}

		internal static XmlSerializer Serializer {
			get { return serializer; }
		}

		public static MetadataSection CreateFromSchema (XmlSchema schema)
		{
			return new MetadataSection (
				MetadataSection.XmlSchemaDialect,
				schema.TargetNamespace, schema);
		}

		public static MetadataSection CreateFromServiceDescription (
			WSServiceDescription serviceDescription)
		{
			return new MetadataSection (
				MetadataSection.ServiceDescriptionDialect,
				serviceDescription.TargetNamespace, serviceDescription);
		}
	}

}
