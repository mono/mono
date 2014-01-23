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

using System;
using System.IO;
using System.Net;
using System.Xml.Schema;

using XsValidationFlags = System.Xml.Schema.XmlSchemaValidationFlags;

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
		private XmlSchemaSet schemas;
		private bool schemasNeedsInitialization;
		private XsValidationFlags validationFlags;
		private ValidationType validationType;
		private XmlResolver xmlResolver;
#if NET_4_0
		private DtdProcessing dtdProcessing;
#endif
		private long maxCharactersFromEntities;
		private long maxCharactersInDocument;

#if NET_4_5
		private bool isReadOnly;
		private bool isAsync;
#endif

		public XmlReaderSettings ()
		{
			Reset ();
		}

		public event ValidationEventHandler ValidationEventHandler;

		public XmlReaderSettings Clone ()
		{
			var clone = (XmlReaderSettings) MemberwiseClone ();
#if NET_4_5
			clone.isReadOnly = false;
#endif
			return clone;
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
			schemas = null;
			schemasNeedsInitialization = true;
			validationFlags =
				XsValidationFlags.ProcessIdentityConstraints |
				XsValidationFlags.AllowXmlAttributes;
			validationType = ValidationType.None;
			xmlResolver = new XmlUrlResolver ();
#if NET_4_5
			isAsync = false;
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
#if NET_4_0
		public DtdProcessing DtdProcessing {
			get { return dtdProcessing; }
			set {
				dtdProcessing = value;
				prohibitDtd = (value == DtdProcessing.Prohibit);
			}
		}
#endif
		public long MaxCharactersFromEntities {
			get { return maxCharactersFromEntities; }
			set { maxCharactersFromEntities = value; }
		}

		[MonoTODO ("not used yet")]
		public long MaxCharactersInDocument {
			get { return maxCharactersInDocument; }
			set { maxCharactersInDocument = value; }
		}

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

#if NET_4_0
		[ObsoleteAttribute("Use DtdProcessing property instead")]
#endif
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

		public XmlResolver XmlResolver {
			internal get { return xmlResolver; }
			set { xmlResolver = value; }
		}

#if NET_4_5
		internal void SetReadOnly ()
		{
			isReadOnly = true;
		}

		/*
		 * FIXME: The .NET 4.5 runtime throws an exception when attempting to
		 *        modify any of the properties after the XmlReader has been constructed.
		 */
		void EnsureWritability ()
		{
			if (isReadOnly)
				throw new InvalidOperationException ("XmlReaderSettings in read-only");
		}

		public bool Async {
			get { return isAsync; }
			set {
				EnsureWritability ();
				isAsync = value;
			}
		}
#endif
	}
}
