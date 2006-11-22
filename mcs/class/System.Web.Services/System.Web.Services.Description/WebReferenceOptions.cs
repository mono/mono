// 
// WebReferenceOptions.cs
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

#if NET_2_0

using System;
using System.ComponentModel;
using System.IO;
using System.Collections.Specialized;
using System.Web.Services;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace System.Web.Services.Description
{
	[XmlRoot ("webReferenceOptions", Namespace = "http://microsoft.com/webReference/")]
	[XmlType ("webReferenceOptions", Namespace = "http://microsoft.com/webReference/")]
	public class WebReferenceOptions
	{
		#region Static members
		static XmlSchema schema;
		static XmlSerializerImplementation implementation =
			new WebReferenceOptionsSerializerImplementation ();

		public const string TargetNamespace = "http://microsoft.com/webReference/";

		public static XmlSchema Schema {
			get {
				if (schema == null) {
					schema = XmlSchema.Read (typeof (ServiceDescription).Assembly.GetManifestResourceStream ("web-reference.xsd"), null);
				}
				return schema;
			}
		}

		public static WebReferenceOptions Read (Stream stream, ValidationEventHandler validationEventHandler)
		{
			return Read (XmlReader.Create (stream), validationEventHandler);
		}

		public static WebReferenceOptions Read (TextReader reader, ValidationEventHandler validationEventHandler)
		{
			return Read (XmlReader.Create (reader), validationEventHandler);
		}

		public static WebReferenceOptions Read (XmlReader xmlReader,
			ValidationEventHandler validationEventHandler)
		{
			XmlReaderSettings s = new XmlReaderSettings ();
			s.ValidationType = ValidationType.Schema;
			s.Schemas.Add (Schema);
			if (validationEventHandler != null)
				s.ValidationEventHandler += validationEventHandler;
			using (XmlReader r = XmlReader.Create (xmlReader, s)) {
				XmlSerializer ser = implementation.GetSerializer (typeof (WebReferenceOptions));
				try {
					return (WebReferenceOptions) ser.Deserialize (r);
				} catch (XmlSchemaValidationException ex) {
					throw new InvalidOperationException ("There is an error in input webReference XML", ex);
				}
			}
		}

		#endregion

		#region Instance members

		CodeGenerationOptions code_options;
		StringCollection importer_extensions = new StringCollection ();
		ServiceDescriptionImportStyle style;
		bool verbose;

		[XmlElement ("codeGenerationOptions")]
		[DefaultValue (CodeGenerationOptions.GenerateOldAsync)]
		public CodeGenerationOptions CodeGenerationOptions {
			get { return code_options; }
			set { code_options = value; }
		}

		[XmlArray ("schemaImporterExtensions")]
		[XmlArrayItem ("type")]
		public StringCollection SchemaImporterExtensions {
			get { return importer_extensions; }
		}

		[XmlElement ("style")]
		[DefaultValue (ServiceDescriptionImportStyle.Client)]
		public ServiceDescriptionImportStyle Style {
			get { return style; }
			set { style = value; }
		}

		[XmlElement ("verbose")]
		public bool Verbose {
			get { return verbose; }
			set { verbose = value; }
		}

		#endregion
	}
}

#endif
