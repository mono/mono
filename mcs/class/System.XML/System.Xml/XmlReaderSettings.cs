//
// XmlReaderSettings.cs
//
// Author:
//   Atsushi Enomoto <atsushi@ximian.com>
//
// (C) 2004 Novell Inc.
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
using System.IO;
using System.Net;
using System.Xml.Schema;

namespace System.Xml
{
	public class XmlReaderSettings
	{
		private bool checkCharacters;
		private bool closeInput;
		private ConformanceLevel conformance;
		private bool dtdValidate;
		private bool ignoreComments;
		private bool ignoreIdentityConstraints;
		private bool ignoreInlineSchema;
		private bool ignoreProcessingInstructions;
		private bool ignoreSchemaLocation;
		private bool ignoreValidationWarnings;
		private bool ignoreWhitespace;
		private int lineNumberOffset;
		private int linePositionOffset;
		private bool prohibitDtd;
		private XmlNameTable nameTable;
		private XmlSchemaSet schemas;
		private bool xsdValidate;

		public XmlReaderSettings ()
		{
			Reset ();
		}

		private XmlReaderSettings (XmlReaderSettings org)
		{
			checkCharacters = org.checkCharacters;
			closeInput = org.closeInput;
			conformance = org.conformance;
			dtdValidate = org.dtdValidate;
			ignoreComments = org.ignoreComments;
			ignoreIdentityConstraints = 
				org.ignoreIdentityConstraints;
			ignoreInlineSchema = org.ignoreInlineSchema;
			ignoreProcessingInstructions = 
				org.ignoreProcessingInstructions;
			ignoreSchemaLocation = org.ignoreSchemaLocation;
			ignoreValidationWarnings = org.ignoreValidationWarnings;
			ignoreWhitespace = org.ignoreWhitespace;
			lineNumberOffset = org.lineNumberOffset;
			linePositionOffset = org.linePositionOffset;
			prohibitDtd = org.prohibitDtd;
			schemas = org.schemas;
			xsdValidate = org.xsdValidate;
			nameTable = org.NameTable;
		}

		public event ValidationEventHandler ValidationEventHandler;

		public XmlReaderSettings Clone ()
		{
			return new XmlReaderSettings (this);
		}

		public void Reset ()
		{
			checkCharacters = true;
			closeInput = false; // ? not documented
			conformance = ConformanceLevel.Document;
			dtdValidate = false;
			ignoreComments = false;
			ignoreIdentityConstraints = false; // ? not documented
			ignoreInlineSchema = true;
			ignoreProcessingInstructions = false;
			ignoreSchemaLocation = true;
			ignoreValidationWarnings = true;
			ignoreWhitespace = false;
			lineNumberOffset = 0;
			linePositionOffset = 0;
			prohibitDtd = false; // ? not documented
			schemas = new XmlSchemaSet ();
			xsdValidate = false;
		}

		public bool CheckCharacters {
			get { return checkCharacters; }
			set { checkCharacters = value; }
		}

		public bool CloseInput {
			get { return closeInput; }
			set { closeInput = value; }
		}

		public ConformanceLevel ConformanceLevel {
			get { return conformance; }
			set { conformance = value; }
		}

		public bool DtdValidate {
			get { return dtdValidate; }
			set { dtdValidate = value; }
		}

		public bool IgnoreComments {
			get { return ignoreComments; }
			set { ignoreComments = value; }
		}

		public bool IgnoreIdentityConstraints {
			get { return ignoreIdentityConstraints ; }
			set { ignoreIdentityConstraints = value; }
		}

		public bool IgnoreInlineSchema {
			get { return ignoreInlineSchema; }
			set { ignoreInlineSchema = value; }
		}

		public bool IgnoreProcessingInstructions {
			get { return ignoreProcessingInstructions; }
			set { ignoreProcessingInstructions = value; }
		}

		public bool IgnoreSchemaLocation {
			get { return ignoreSchemaLocation; }
			set { ignoreSchemaLocation = value; }
		}

		public bool IgnoreValidationWarnings {
			get { return ignoreValidationWarnings; }
			set { ignoreValidationWarnings = value; }
		}

		public bool IgnoreWhitespace {
			get { return ignoreWhitespace; }
			set { ignoreWhitespace = value; }
		}

		public int LineNumberOffset {
			get { return lineNumberOffset; }
			set { lineNumberOffset = value; }
		}

		public int LinePositionOffset {
			get { return linePositionOffset; }
			set { linePositionOffset = value; }
		}

		public bool ProhibitDtd {
			get { return prohibitDtd; }
			set { prohibitDtd = value; }
		}

		// LAMESPEC: MSDN documentation says "An empty XmlNameTable
		// object" for default value, but XmlNameTable cannot be
		// instantiate. It actually returns null by default.
		public XmlNameTable NameTable {
			get { return nameTable; }
			set { nameTable = value; }
		}

		// LAMESPEC: Apparently, this property should not have a setter.
		public XmlSchemaSet Schemas {
			get { return schemas; }
			set {
				throw new XmlException ("XmlReaderSettings.Schemas is read-only and cannot be set.");
			}
		}

		public bool XsdValidate {
			get { return xsdValidate; }
			set { xsdValidate = value; }
		}
	}
}

#endif
