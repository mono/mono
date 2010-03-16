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

#if !MOONLIGHT
using XsValidationFlags = System.Xml.Schema.XmlSchemaValidationFlags;
#endif

namespace System.Xml
{
	public sealed class XmlReaderSettings
	{
		private bool checkCharacters;
		private bool closeInput;
		private ConformanceLevel conformance;
		private bool ignoreComments;
		private bool ignoreProcessingInstructions;
		private bool ignoreWhitespace;
		private int lineNumberOffset;
		private int linePositionOffset;
		private bool prohibitDtd;
		private XmlNameTable nameTable;
#if !MOONLIGHT
		private XmlSchemaSet schemas;
		private bool schemasNeedsInitialization;
		private XsValidationFlags validationFlags;
		private ValidationType validationType;
#endif
		private XmlResolver xmlResolver;
#if MOONLIGHT
		private DtdProcessing dtdProcessing;
		private long maxCharactersFromEntities;
		private long maxCharactersInDocument;
#endif

		public XmlReaderSettings ()
		{
			Reset ();
		}

#if !MOONLIGHT
		public event ValidationEventHandler ValidationEventHandler;
#endif

		public XmlReaderSettings Clone ()
		{
			return (XmlReaderSettings) MemberwiseClone ();
		}

		public void Reset ()
		{
			checkCharacters = true;
			closeInput = false; // ? not documented
			conformance = ConformanceLevel.Document;
			ignoreComments = false;
			ignoreProcessingInstructions = false;
			ignoreWhitespace = false;
			lineNumberOffset = 0;
			linePositionOffset = 0;
			prohibitDtd = true;
#if MOONLIGHT
			xmlResolver = new XmlXapResolver ();
#else
			schemas = null;
			schemasNeedsInitialization = true;
			validationFlags =
				XsValidationFlags.ProcessIdentityConstraints |
				XsValidationFlags.AllowXmlAttributes;
			validationType = ValidationType.None;
			xmlResolver = new XmlUrlResolver ();
#endif
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
#if MOONLIGHT
		public DtdProcessing DtdProcessing {
			get { return dtdProcessing; }
			set {
				dtdProcessing = value;
				prohibitDtd = (value == DtdProcessing.Prohibit);
			}
		}

		public long MaxCharactersFromEntities {
			get { return maxCharactersFromEntities; }
			set { maxCharactersFromEntities = value; }
		}

		[MonoTODO ("not used yet")]
		public long MaxCharactersInDocument {
			get { return maxCharactersInDocument; }
			set { maxCharactersInDocument = value; }
		}
#endif

		public bool IgnoreComments {
			get { return ignoreComments; }
			set { ignoreComments = value; }
		}

		public bool IgnoreProcessingInstructions {
			get { return ignoreProcessingInstructions; }
			set { ignoreProcessingInstructions = value; }
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

#if !MOONLIGHT
		public XmlSchemaSet Schemas {
			get {
				if (schemasNeedsInitialization) {
					schemas = new XmlSchemaSet ();
					schemasNeedsInitialization = false;
				}
				return schemas;
			}
			set {
				schemas = value;
				schemasNeedsInitialization = false;
			}
		}

		internal void OnValidationError (object o, ValidationEventArgs e)
		{
			if (ValidationEventHandler != null)
				ValidationEventHandler (o, e);
			else if (e.Severity == XmlSeverityType.Error)
				throw e.Exception;
		}

		internal void SetSchemas (XmlSchemaSet schemas)
		{
			this.schemas = schemas;
		}

		public XsValidationFlags ValidationFlags {
			get { return validationFlags; }
			set { validationFlags = value; }
		}

		public ValidationType ValidationType {
			get { return validationType; }
			set { validationType = value; }
		}
#endif

		public XmlResolver XmlResolver {
			internal get { return xmlResolver; }
			set { xmlResolver = value; }
		}
	}
}

#endif
