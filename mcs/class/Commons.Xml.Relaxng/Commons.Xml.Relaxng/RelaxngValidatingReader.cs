//
// Commons.Xml.Relaxng.RelaxngValidatingReader
//
// Author:
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//
// 2003 Atsushi Enomoto. "No rights reserved."
//
// Copyright (c) 2004 Novell Inc.
// All rights reserved
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
using System.Collections;
using System.Xml;
using Commons.Xml.Relaxng.Derivative;

namespace Commons.Xml.Relaxng
{
	public class RelaxngValidatingReader : XmlDefaultReader
	{
		public RelaxngValidatingReader (XmlReader reader)
			: this (reader, (RelaxngPattern) null)
		{
		}

		public RelaxngValidatingReader (XmlReader reader, XmlReader grammarXml)
			: this (reader, grammarXml, null)
		{
		}

		public RelaxngValidatingReader (XmlReader reader, XmlReader grammarXml, RelaxngDatatypeProvider provider)
			: this (reader, RelaxngGrammar.Read (grammarXml, provider))
		{
		}

		public RelaxngValidatingReader (XmlReader reader, RelaxngPattern pattern)
			: base (reader)
		{
			this.reader = reader;
			this.pattern = pattern;
		}

		XmlReader reader;
		RelaxngPattern pattern;
		RdpPattern vState;
		RdpPattern prevState;	// Mainly for debugging.
		ArrayList PredefinedAttributes = new ArrayList ();

		internal string CurrentStateXml {
			get { return RdpUtil.DebugRdpPattern (vState, new Hashtable ()); }
		}

		internal string PreviousStateXml {
			get { return RdpUtil.DebugRdpPattern (prevState, new Hashtable ()); }
		}

		public void GetLabels (Hashtable elements, Hashtable attributes)
		{
			if (elements == null)
				throw new ArgumentNullException ("elements");
			if (attributes == null)
				throw new ArgumentNullException ("attributes");
			PrepareState ();
			vState.GetLabels (elements, attributes);
		}

		private RelaxngException createValidationError (string message)
		{
			IXmlLineInfo li = reader as IXmlLineInfo;
			string lineInfo = reader.BaseURI;
			if (li != null)
				lineInfo += String.Format (" line {0}, column {1}",
					li.LineNumber, li.LinePosition);
			return new RelaxngException (message + lineInfo, prevState);
		}

		private void PrepareState ()
		{
			if (!pattern.IsCompiled) {
				pattern.Compile ();
			}
			if (vState == null)
				vState = pattern.StartPattern;
		}

		public override bool Read ()
		{
			PrepareState ();

			bool ret = reader.Read ();

			switch (reader.NodeType) {
			case XmlNodeType.Element:
				// StartTagOpenDeriv
				prevState = vState;
				vState = vState.StartTagOpenDeriv (
					reader.LocalName, reader.NamespaceURI);
				if (vState.PatternType == RelaxngPatternType.NotAllowed)
					throw createValidationError (String.Format ("Invalid start tag found. LocalName = {0}, NS = {1}. ", reader.LocalName, reader.NamespaceURI));

				// AttsDeriv equals to for each AttDeriv
//				if (reader.AttributeCount > 0) {
				string elementNS = reader.NamespaceURI;
				if (reader.MoveToFirstAttribute ()) {
					do {
						if (reader.Name.IndexOf ("xmlns:") == 0 || reader.Name == "xmlns")
							continue;

						prevState = vState;
						string attrNS = /*reader.NamespaceURI == "" ? elementNS :*/ reader.NamespaceURI;
						vState = vState.AttDeriv (reader.LocalName, attrNS, reader.GetAttribute (reader.Name), this);
						if (vState.PatternType == RelaxngPatternType.NotAllowed)
							throw createValidationError (String.Format ("Invalid attribute found. LocalName = {0}, NS = {1}. ", reader.LocalName, reader.NamespaceURI));
					} while (reader.MoveToNextAttribute ());
					MoveToElement ();
				}

				// StarTagCloseDeriv
				prevState = vState;
				vState = vState.StartTagCloseDeriv ();
				if (vState.PatternType == RelaxngPatternType.NotAllowed)
					throw createValidationError (String.Format ("Invalid start tag closing found. LocalName = {0}, NS = {1}. ", reader.LocalName, reader.NamespaceURI));

				// if it is empty, then redirect to EndElement
				if (reader.IsEmptyElement)
					goto case XmlNodeType.EndElement;
				break;
			case XmlNodeType.EndElement:
				// EndTagDeriv
				prevState = vState;
				vState = vState.EndTagDeriv ();
				if (vState.PatternType == RelaxngPatternType.NotAllowed)
					throw createValidationError (String.Format ("Invalid end tag found. LocalName = {0}, NS = {1}. ", reader.LocalName, reader.NamespaceURI));
				break;
			case XmlNodeType.CDATA:
			case XmlNodeType.Text:
			case XmlNodeType.SignificantWhitespace:
				// Whitespace cannot be skipped because data and
				// value types are required to validate whitespaces.
//				if (Util.IsWhitespace (Value))
//					break;
				prevState = vState;
				vState = vState.TextDeriv (this.Value, reader);
				if (vState.PatternType == RelaxngPatternType.NotAllowed)
					throw createValidationError (String.Format ("Invalid text found. Text value = {0} ", reader.Value));
				break;
				
			}
			return ret;
		}
	}
}

