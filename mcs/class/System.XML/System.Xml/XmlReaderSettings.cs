//
// XmlReaderSettings.cs
//
// Author:
//   Atsushi Enomoto <atsushi@ximian.com>
//
// (C) 2004 Novell Inc.
//

using System;
using System.IO;
using System.Net;
using System.Xml.Schema;

namespace System.Xml
{
	public sealed class XmlReaderSettings : ICloneable
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
		}

		public event ValidationEventHandler ValidationEventHandler;

		public XmlReaderSettings Clone ()
		{
			return new XmlReaderSettings (this);
		}

		object ICloneable.Clone ()
		{
			return this.Clone ();
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

		[MonoTODO ("Can set null value?")]
		public XmlSchemaSet Schemas {
			get { return schemas; }
			set { schemas = value; }
		}

		public bool XsdValidate {
			get { return xsdValidate; }
			set { xsdValidate = value; }
		}
	}
}
