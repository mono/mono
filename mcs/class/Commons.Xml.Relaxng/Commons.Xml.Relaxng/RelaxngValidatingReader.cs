//
// Commons.Xml.Relaxng.RelaxngValidatingReader
//
// Author:
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// (C)2003 Atsushi Enomoto. "Some rights reserved."
//
using System;
using System.Collections;
using System.Xml;
using Commons.Xml.Relaxng.Derivative;

namespace Commons.Xml.Relaxng
{
	public class RelaxngValidatingReader : XmlDefaultReader
	{
		public RelaxngValidatingReader (XmlReader reader)
			: this (reader, (RngPattern) null)
		{
		}

		public RelaxngValidatingReader (XmlReader reader, XmlReader grammarXml)
			: this (reader, RngGrammar.Read (grammarXml))
		{
		}

		public RelaxngValidatingReader (XmlReader reader, RngPattern pattern)
			: base (reader)
		{
			this.reader = reader;
			this.pattern = pattern;
		}

		XmlReader reader;
		RngPattern pattern;
		RdpPattern vState;
		RdpPattern prevState;	// Mainly for debugging.
		ArrayList PredefinedAttributes = new ArrayList ();

		internal string CurrentStateXml {
			get { return RdpUtil.DebugRdpPattern (vState, new Hashtable ()); }
		}

		internal string PreviousStateXml {
			get { return RdpUtil.DebugRdpPattern (prevState, new Hashtable ()); }
		}

		private RngException createValidationError (string message)
		{
			IXmlLineInfo li = reader as IXmlLineInfo;
			string lineInfo = reader.BaseURI;
			if (li != null)
				lineInfo += String.Format (" line {0}, column {1}",
					li.LineNumber, li.LinePosition);
			return new RngException (message + lineInfo, prevState);
		}

		public override bool Read ()
		{
			if (!pattern.IsCompiled) {
				pattern.Compile ();
			}
			if (vState == null)
				vState = pattern.StartPattern;

			bool ret = reader.Read ();

			switch (reader.NodeType) {
			case XmlNodeType.Element:
				// StartTagOpenDeriv
				prevState = vState;
				vState = vState.StartTagOpenDeriv (
					reader.LocalName, reader.NamespaceURI);
				if (vState.PatternType == RngPatternType.NotAllowed)
					throw createValidationError (String.Format ("Invalid start tag found. LocalName = {0}, NS = {1}. ", reader.LocalName, reader.NamespaceURI));

				// AttsDeriv equals to for each AttDeriv
				if (reader.AttributeCount > 0) {
					string elementNS = reader.NamespaceURI;
					reader.MoveToFirstAttribute ();
					do {
						if (reader.Name.IndexOf ("xml:") == 0 || reader.Name.IndexOf ("xmlns:") == 0 || reader.Name == "xmlns")
							continue;

						prevState = vState;
						string attrNS = reader.NamespaceURI == "" ? elementNS : reader.NamespaceURI;
						vState = vState.AttDeriv (reader.LocalName, attrNS, reader.GetAttribute (reader.Name));
						if (vState.PatternType == RngPatternType.NotAllowed)
							throw createValidationError (String.Format ("Invalid attribute found. LocalName = {0}, NS = {1}. ", reader.LocalName, reader.NamespaceURI));
					} while (reader.MoveToNextAttribute ());
				}
				MoveToElement ();

				// StarTagCloseDeriv
				prevState = vState;
				vState = vState.StartTagCloseDeriv ();
				if (vState.PatternType == RngPatternType.NotAllowed)
					throw createValidationError (String.Format ("Invalid start tag closing found. LocalName = {0}, NS = {1}. ", reader.LocalName, reader.NamespaceURI));

				// if it is empty, then redirect to EndElement
				if (reader.IsEmptyElement)
					goto case XmlNodeType.EndElement;
				break;
			case XmlNodeType.EndElement:
				// EndTagDeriv
				prevState = vState;
				vState = vState.EndTagDeriv ();
				if (vState.PatternType == RngPatternType.NotAllowed)
					throw createValidationError (String.Format ("Invalid end tag found. LocalName = {0}, NS = {1}. ", reader.LocalName, reader.NamespaceURI));
				break;
			case XmlNodeType.CDATA:
			case XmlNodeType.Text:
				prevState = vState;
				vState = vState.TextDeriv (this.Value);
				if (vState.PatternType == RngPatternType.NotAllowed)
					throw createValidationError (String.Format ("Invalid text found. Text value = {0} ", reader.Value));
				break;
				
			}
			return ret;
		}
	}
}

