//
// Copyright (C) 2010 Novell Inc. http://novell.com
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

namespace System.Xaml
{
	public class XamlXmlReader : XamlReader, IXamlLineInfo
	{
		public XamlXmlReader (Stream stream)
			: this (stream, (XamlXmlReaderSettings) null)
		{
		}

		public XamlXmlReader (string fileName)
			: this (fileName, (XamlXmlReaderSettings) null)
		{
		}

		public XamlXmlReader (TextReader textReader)
			: this (textReader, (XamlXmlReaderSettings) null)
		{
		}

		public XamlXmlReader (XmlReader xmlReader)
			: this (xmlReader, (XamlXmlReaderSettings) null)
		{
		}

		public XamlXmlReader (Stream stream, XamlSchemaContext schemaContext)
			: this (stream, schemaContext, null)
		{
		}

		public XamlXmlReader (Stream stream, XamlXmlReaderSettings settings)
			: this (stream, new XamlSchemaContext (null, null), settings)
		{
		}

		public XamlXmlReader (string fileName, XamlSchemaContext schemaContext)
			: this (fileName, schemaContext, null)
		{
		}

		public XamlXmlReader (string fileName, XamlXmlReaderSettings settings)
			: this (fileName, new XamlSchemaContext (null, null), settings)
		{
		}

		public XamlXmlReader (TextReader textReader, XamlSchemaContext schemaContext)
			: this (textReader, schemaContext, null)
		{
		}

		public XamlXmlReader (TextReader textReader, XamlXmlReaderSettings settings)
			: this (textReader, new XamlSchemaContext (null, null), settings)
		{
		}

		public XamlXmlReader (XmlReader xmlReader, XamlSchemaContext schemaContext)
			: this (xmlReader, schemaContext, null)
		{
		}

		public XamlXmlReader (XmlReader xmlReader, XamlXmlReaderSettings settings)
			: this (xmlReader, new XamlSchemaContext (null, null), settings)
		{
		}

		public XamlXmlReader (Stream stream, XamlSchemaContext schemaContext, XamlXmlReaderSettings settings)
			: this (XmlReader.Create (stream), schemaContext, settings)
		{
		}

		public XamlXmlReader (string fileName, XamlSchemaContext schemaContext, XamlXmlReaderSettings settings)
			: this (XmlReader.Create (fileName), schemaContext, settings)
		{
			close_reader = true;
		}

		public XamlXmlReader (TextReader textReader, XamlSchemaContext schemaContext, XamlXmlReaderSettings settings)
			: this (XmlReader.Create (textReader), schemaContext, settings)
		{
		}

		public XamlXmlReader (XmlReader xmlReader, XamlSchemaContext schemaContext, XamlXmlReaderSettings settings)
		{
			if (xmlReader == null)
				throw new ArgumentNullException ("xmlReader");
			if (schemaContext == null)
				throw new ArgumentNullException ("schemaContext");
			r = xmlReader;
			line_info = r as IXmlLineInfo;
			sctx = schemaContext;
			this.settings = settings ?? new XamlXmlReaderSettings ();
			close_reader = this.settings.CloseInput;
		}

		XmlReader r;
		IXmlLineInfo line_info;
		XamlSchemaContext sctx;
		XamlXmlReaderSettings settings;
		bool close_reader;

		public bool HasLineInfo {
			get { return line_info != null && line_info.HasLineInfo (); }
		}

		public override bool IsEof {
			get { throw new NotImplementedException (); }
		}

		public int LineNumber {
			get { return line_info != null ? line_info.LineNumber : 0; }
		}

		public int LinePosition {
			get { return line_info != null ? line_info.LinePosition : 0; }
		}

		public override XamlMember Member {
			get { throw new NotImplementedException (); }
		}
		public override NamespaceDeclaration Namespace {
			get { throw new NotImplementedException (); }
		}
		public override XamlNodeType NodeType {
			get { throw new NotImplementedException (); }
		}

		public override XamlSchemaContext SchemaContext {
			get { return sctx; }
		}

		public override XamlType Type {
			get { throw new NotImplementedException (); }
		}
		public override object Value {
			get { throw new NotImplementedException (); }
		}

		protected override void Dispose (bool disposing)
		{
			if (!disposing)
				return;
			if (close_reader)
				r.Close ();
		}
		
		public override bool Read ()
		{
			throw new NotImplementedException ();
		}
	}
}
