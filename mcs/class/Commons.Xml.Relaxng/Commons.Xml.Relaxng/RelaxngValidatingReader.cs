//
// Commons.Xml.Relaxng.RelaxngValidatingReader
//
// Author:
//	Atsushi Enomoto <ginga@kit.hi-ho.ne.jp>
//	Alexandre Alapetite <http://alexandre.alapetite.net/cv/>
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
using System.Text;
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
			if (pattern == null)
				throw new ArgumentNullException ("pattern");

			if (reader.NodeType == XmlNodeType.Attribute)
				throw new RelaxngException ("RELAX NG does not support standalone attribute validation (it is prohibited due to the specification section 7.1.5");
			this.reader = reader;
			this.pattern = pattern;
		}

		XmlReader reader;
		RelaxngPattern pattern;
		RdpPattern vState;
		RdpPattern prevState;	// Mainly for debugging.
		bool roughLabelCheck;
		ArrayList strictCheckCache;
		bool reportDetails;
		string cachedValue;
		int startElementDepth = -1;
		bool inContent;
		bool firstRead = true;

		internal string CurrentStateXml {
			get { return RdpUtil.DebugRdpPattern (vState, new Hashtable ()); }
		}

		internal string PreviousStateXml {
			get { return RdpUtil.DebugRdpPattern (prevState, new Hashtable ()); }
		}

		#region Validation State support

		public bool ReportDetails {
			get { return reportDetails; }
			set { reportDetails = value; }
		}

		public bool RoughLabelCheck {
			get { return roughLabelCheck; }
			set { roughLabelCheck = value; }
		}

		// It is used to disclose its validation feature to public
		class ValidationState
		{
			RdpPattern state;

			internal ValidationState (RdpPattern startState)
			{
				this.state = startState;
			}

			public RdpPattern Pattern {
				get { return state; }
			}

			public ValidationState AfterOpenStartTag (
				string localName, string ns)
			{
				RdpPattern p = state.StartTagOpenDeriv (
					localName, ns);
				return p is RdpNotAllowed ?
					null : new ValidationState (p);
			}

			public bool OpenStartTag (string localName, string ns)
			{
				RdpPattern p = state.StartTagOpenDeriv (
					localName, ns);
				if (p is RdpNotAllowed)
					return false;
				state = p;
				return true;
			}

			public ValidationState AfterCloseStartTag ()
			{
				RdpPattern p = state.StartTagCloseDeriv ();
				return p is RdpNotAllowed ?
					null : new ValidationState (p);
			}

			public bool CloseStartTag ()
			{
				RdpPattern p = state.StartTagCloseDeriv ();
				if (p is RdpNotAllowed)
					return false;
				state = p;
				return true;
			}

			public ValidationState AfterEndTag ()
			{
				RdpPattern p = state.EndTagDeriv ();
				if (p is RdpNotAllowed)
					return null;
				return new ValidationState (p);
			}

			public bool EndTag ()
			{
				RdpPattern p = state.EndTagDeriv ();
				if (p is RdpNotAllowed)
					return false;
				state = p;
				return true;
			}

			public ValidationState AfterAttribute (
				string localName, string ns, XmlReader reader)
			{
				RdpPattern p = state.AttDeriv (
					localName, ns, null, reader);
				if (p is RdpNotAllowed)
					return null;
				return new ValidationState (p);
			}

			public bool Attribute (
				string localName, string ns, XmlReader reader)
			{
				RdpPattern p = state.AttDeriv (
					localName, ns, null, reader);
				if (p is RdpNotAllowed)
					return false;
				state = p;
				return true;
			}
		}

		public object GetCurrentState ()
		{
			PrepareState ();
			return new ValidationState (vState);
		}

		private ValidationState ToState (object stateObject)
		{
			if (stateObject == null)
				throw new ArgumentNullException ("stateObject");
			ValidationState state = stateObject as ValidationState;
			if (state == null)
				throw new ArgumentException ("Argument stateObject is not of expected type.");
			return state;
		}

		public object AfterOpenStartTag (object stateObject,
			string localName, string ns)
		{
			ValidationState state = ToState (stateObject);
			return state.AfterOpenStartTag (localName, ns);
		}

		public bool OpenStartTag (object stateObject,
			string localName, string ns)
		{
			ValidationState state = ToState (stateObject);
			return state.OpenStartTag (localName, ns);
		}

		public object AfterAttribute (object stateObject,
			string localName, string ns)
		{
			ValidationState state = ToState (stateObject);
			return state.AfterAttribute (localName, ns, this);
		}

		public bool Attribute (object stateObject,
			string localName, string ns)
		{
			ValidationState state = ToState (stateObject);
			return state.Attribute (localName, ns, this);
		}

		public object AfterCloseStartTag (object stateObject)
		{
			ValidationState state = ToState (stateObject);
			return state.AfterCloseStartTag ();
		}

		public bool CloseStartTag (object stateObject)
		{
			ValidationState state = ToState (stateObject);
			return state.CloseStartTag ();
		}

		public object AfterEndTag (object stateObject)
		{
			ValidationState state = ToState (stateObject);
			return state.AfterEndTag ();
		}

		public bool EndTag (object stateObject)
		{
			ValidationState state = ToState (stateObject);
			return state.EndTag ();
		}

		public ICollection GetElementLabels (object stateObject)
		{
			ValidationState state = ToState (stateObject);
			RdpPattern p = state.Pattern;
			Hashtable elements = new Hashtable ();
			Hashtable attributes = new Hashtable ();
			p.GetLabels (elements, attributes);

			if (roughLabelCheck)
				return elements.Values;

			// Strict check that tries actual validation that will
			// cover rejection by notAllowed.
			if (strictCheckCache == null)
				strictCheckCache = new ArrayList ();
			else
				strictCheckCache.Clear ();
			foreach (XmlQualifiedName qname in elements.Values)
				if (p.StartTagOpenDeriv (qname.Name, qname.Namespace) is RdpNotAllowed)
					strictCheckCache.Add (qname);
			foreach (XmlQualifiedName qname in strictCheckCache)
				elements.Remove (qname);
			strictCheckCache.Clear ();

			return elements.Values;
		}

		public ICollection GetAttributeLabels (object stateObject)
		{
			ValidationState state = ToState (stateObject);
			RdpPattern p = state.Pattern;
			Hashtable elements = new Hashtable ();
			Hashtable attributes = new Hashtable ();
			p.GetLabels (elements, attributes);

			if (roughLabelCheck)
				return attributes.Values;

			// Strict check that tries actual validation that will
			// cover rejection by notAllowed.
			if (strictCheckCache == null)
				strictCheckCache = new ArrayList ();
			else
				strictCheckCache.Clear ();
			foreach (XmlQualifiedName qname in attributes.Values)
				if (p.AttDeriv (qname.Name, qname.Namespace,null, this) is RdpNotAllowed)
					strictCheckCache.Add (qname);
			foreach (XmlQualifiedName qname in strictCheckCache)
				attributes.Remove (qname);
			strictCheckCache.Clear ();

			return attributes.Values;
		}

		public bool Emptiable (object stateObject)
		{
			ValidationState state = ToState (stateObject);
			RdpPattern p = state.Pattern;
			return !(p.EndTagDeriv () is RdpNotAllowed);
		}
		#endregion

		private RelaxngException CreateValidationError (string message,
			bool elements)
		{
			if (ReportDetails)
				return CreateValidationError (String.Concat (message,
					" Expected ",
					elements ? "elements are: " : "attributes are: ",
					BuildLabels (elements),
					"."));
			return CreateValidationError (message);
		}

		private RelaxngException CreateValidationError (string message)
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
			if (vState != null)
				return;
			if (!pattern.IsCompiled) {
				pattern.Compile ();
			}
			if (vState == null)
				vState = pattern.StartPattern;
		}

		private string BuildLabels (bool elements)
		{
			StringBuilder sb = new StringBuilder ();
			ValidationState s = new ValidationState (prevState);
			ICollection col = elements ?
				GetElementLabels (s) : GetAttributeLabels (s);
			foreach (XmlQualifiedName qname in col) {
				sb.Append (qname.ToString ());
				sb.Append (' ');
			}
			return sb.ToString ();
		}

		public override bool Read ()
		{
			PrepareState ();

			// If the input XmlReader is already positioned on
			// the first node to validate, skip Read() here
			// (idea by Alex).
			bool ret;
			if (firstRead) {
				firstRead = false;
				if (reader.ReadState == ReadState.Initial)
					ret = reader.Read ();
				else
					ret = !((reader.ReadState == ReadState.Closed) || (reader.ReadState == ReadState.EndOfFile));
			}
			else
				ret = reader.Read ();

			// Process pending text node validation if required.
			if (cachedValue != null)
				ValidateText (ret);
			else if (cachedValue == null &&
				reader.NodeType == XmlNodeType.EndElement && 
				startElementDepth == reader.Depth)
				ValidateWeakMatch3 ();

			switch (reader.NodeType) {
			case XmlNodeType.Element:
				inContent = true;
				// StartTagOpenDeriv
				prevState = vState;
				vState = memo.StartTagOpenDeriv (vState,
					reader.LocalName, reader.NamespaceURI);
				if (vState.PatternType == RelaxngPatternType.NotAllowed)
					throw CreateValidationError (String.Format ("Invalid start tag found. LocalName = {0}, NS = {1}.", reader.LocalName, reader.NamespaceURI), true);

				// AttsDeriv equals to for each AttDeriv
				string elementNS = reader.NamespaceURI;
				if (reader.MoveToFirstAttribute ()) {
					do {
						if (reader.NamespaceURI == "http://www.w3.org/2000/xmlns/")
							continue;

						prevState = vState;
						string attrNS = reader.NamespaceURI;

#if false // old code

						vState = vState.AttDeriv (reader.LocalName, attrNS, reader.GetAttribute (reader.LocalName, attrNS), this);
						if (vState == RdpNotAllowed.Instance)
							throw CreateValidationError (String.Format ("Invalid attribute found. LocalName = {0}, NS = {1}.", reader.LocalName, reader.NamespaceURI), false);

#else

						prevState = vState;
						vState = memo.StartAttDeriv (vState, reader.LocalName, attrNS);
						if (vState == RdpNotAllowed.Instance)
							throw CreateValidationError (String.Format ("Invalid attribute found. LocalName = {0}, NS = {1}.", reader.LocalName, reader.NamespaceURI), false);
						prevState = vState;
						vState = memo.TextOnlyDeriv (vState);
						vState = TextDeriv (vState, reader.Value, reader);
						if (Util.IsWhitespace (reader.Value))
							vState = vState.Choice (prevState);
						vState = memo.EndAttDeriv (vState);
						if (vState == RdpNotAllowed.Instance)
							throw CreateValidationError (String.Format ("Invalid attribute value is found. Value = '{0}'", reader.Value), false);

#endif
					} while (reader.MoveToNextAttribute ());
					MoveToElement ();
				}

				// StarTagCloseDeriv
				prevState = vState;
				vState = memo.StartTagCloseDeriv (vState);
				if (vState.PatternType == RelaxngPatternType.NotAllowed)
					throw CreateValidationError (String.Format ("Invalid start tag closing found. LocalName = {0}, NS = {1}.", reader.LocalName, reader.NamespaceURI), false);

				// if it is empty, then redirect to EndElement
				if (reader.IsEmptyElement) {
					ValidateWeakMatch3 ();
					goto case XmlNodeType.EndElement;
				}
				break;
			case XmlNodeType.EndElement:
				if (reader.Depth == 0)
					inContent = false;
				// EndTagDeriv
				prevState = vState;
				vState = memo.EndTagDeriv (vState);
				if (vState.PatternType == RelaxngPatternType.NotAllowed)
					throw CreateValidationError (String.Format ("Invalid end tag found. LocalName = {0}, NS = {1}.", reader.LocalName, reader.NamespaceURI), true);
				break;
			case XmlNodeType.Whitespace:
				if (inContent)
					goto case XmlNodeType.Text;
				break;
			case XmlNodeType.CDATA:
			case XmlNodeType.Text:
			case XmlNodeType.SignificantWhitespace:
				// Whitespace cannot be skipped because data and
				// value types are required to validate whitespaces.
				cachedValue += Value;
				break;
			}

			if (reader.NodeType == XmlNodeType.Element && !reader.IsEmptyElement)
				startElementDepth = reader.Depth;
			else if (reader.NodeType == XmlNodeType.EndElement)
				startElementDepth = -1;

			return ret;
		}

		RdpPattern TextDeriv (RdpPattern p, string value, XmlReader context)
		{
			if (value.Length > 0 && p.IsTextValueDependent)
				return memo.TextDeriv (p, value, context);
			else
				return memo.EmptyTextDeriv (p);
		}

		void ValidateText (bool remain)
		{
			RdpPattern ts = vState;
			switch (reader.NodeType) {
			case XmlNodeType.EndElement:
				if (startElementDepth != reader.Depth)
					goto case XmlNodeType.Element;
				ts = ValidateTextOnlyCore ();
				break;
			case XmlNodeType.Element:
				startElementDepth = -1;
				if (!Util.IsWhitespace (cachedValue)) {
					ts = memo.MixedTextDeriv (ts);
					ts = TextDeriv (ts, cachedValue, reader);
				}
				break;
			default:
				if (!remain)
					goto case XmlNodeType.Element;
				return;
			}

			prevState = vState;
			vState = ts;

			if (vState.PatternType == RelaxngPatternType.NotAllowed)
				throw CreateValidationError (String.Format ("Invalid text found. Text value = {0} ", cachedValue), true);
			cachedValue = null;
			return;
		}

		// section 6.2.7 weak match 3
		// childrenDeriv cx p [] = childrenDeriv cx p [(TextNode "")]
		void ValidateWeakMatch3 ()
		{
			cachedValue = String.Empty;
			RdpPattern ts = ValidateTextOnlyCore ();

			prevState = vState;
			vState = ts;

			if (vState.PatternType == RelaxngPatternType.NotAllowed)
				throw CreateValidationError (String.Format ("Invalid text found. Text value = {0} ", cachedValue), true);
			cachedValue = null;
			startElementDepth = -1;
		}

		RdpPattern ValidateTextOnlyCore ()
		{
			RdpPattern ts = memo.TextOnlyDeriv (vState);
			ts = TextDeriv (ts, cachedValue, reader);
			if (Util.IsWhitespace (cachedValue))
				ts = vState.Choice (ts);
			return ts;
		}

		MemoizationStore memo = new MemoizationStore ();
	}

	#region Memoization support
	internal class MemoizationStore
	{
		Hashtable startOpen = new Hashtable ();
		Hashtable startClose = new Hashtable ();
		Hashtable startAtt = new Hashtable ();
		Hashtable endTag = new Hashtable ();
		Hashtable endAtt = new Hashtable ();
		Hashtable textOnly = new Hashtable ();
		Hashtable mixedText = new Hashtable ();
		Hashtable emptyText = new Hashtable ();
		Hashtable text = new Hashtable ();
		Hashtable text_value = new Hashtable ();
		Hashtable qnames = new Hashtable ();

		enum DerivativeType {
			StartTagOpen,
			StartAtt,
			StartTagClose,
			EndTag,
			EndAtt,
			Mixed,
			TextOnly
		}

		XmlQualifiedName GetQName (string local, string ns)
		{
			Hashtable nst = qnames [ns] as Hashtable;
			if (nst == null) {
				nst = new Hashtable ();
				qnames [ns] = nst;
			}
			XmlQualifiedName qn = nst [local] as XmlQualifiedName;
			if (qn == null) {
				qn = new XmlQualifiedName (local, ns);
				nst [local] = qn;
			}
			return qn;
		}

		public RdpPattern StartTagOpenDeriv (RdpPattern p, string local, string ns)
		{
			Hashtable h = startOpen [p] as Hashtable;
			if (h == null) {
				h = new Hashtable ();
				startOpen [p] = h;
			}
			XmlQualifiedName qn = GetQName (local, ns);
			RdpPattern m = h [qn] as RdpPattern;
			if (m == null) {
				m = p.StartTagOpenDeriv (local, ns, this);
				h [qn] = m;
			}
			return m;
		}

		public RdpPattern StartAttDeriv (RdpPattern p, string local, string ns)
		{
			Hashtable h = startAtt [p] as Hashtable;
			if (h == null) {
				h = new Hashtable ();
				startAtt [p] = h;
			}
			XmlQualifiedName qn = GetQName (local, ns);
			RdpPattern m = h [qn] as RdpPattern;
			if (m == null) {
				m = p.StartAttDeriv (local, ns, this);
				h [qn] = m;
			}
			return m;
		}

		public RdpPattern StartTagCloseDeriv (RdpPattern p)
		{
			RdpPattern m = startClose [p] as RdpPattern;
			if (m != null)
				return m;

			m = p.StartTagCloseDeriv (this);
			startClose [p] = m;
			return m;
		}

		public RdpPattern EndTagDeriv (RdpPattern p)
		{
			RdpPattern m = endTag [p] as RdpPattern;
			if (m != null)
				return m;

			m = p.EndTagDeriv (this);
			endTag [p] = m;
			return m;
		}

		public RdpPattern EndAttDeriv (RdpPattern p)
		{
			RdpPattern m = endAtt [p] as RdpPattern;
			if (m != null)
				return m;

			m = p.EndAttDeriv (this);
			endAtt [p] = m;
			return m;
		}

		public RdpPattern MixedTextDeriv (RdpPattern p)
		{
			RdpPattern m = mixedText [p] as RdpPattern;
			if (m != null)
				return m;

			m = p.MixedTextDeriv (this);
			mixedText [p] = m;
			return m;
		}

		public RdpPattern TextOnlyDeriv (RdpPattern p)
		{
			RdpPattern m = textOnly [p] as RdpPattern;
			if (m != null)
				return m;

			m = p.TextOnlyDeriv (this);
			textOnly [p] = m;
			return m;
		}

		public RdpPattern TextDeriv (RdpPattern p, string value, XmlReader context)
		{
			if (p.IsContextDependent)
				return p.TextDeriv (value, context);

			if (Object.ReferenceEquals (text_value [p], value))
				return text [p] as RdpPattern;
			RdpPattern m = p.TextDeriv (value, context, this);
			text_value [p] = value;
			text [p] = m;
			return m;
		}

		public RdpPattern EmptyTextDeriv (RdpPattern p)
		{
			RdpPattern m = emptyText [p] as RdpPattern;
			if (m != null)
				return m;

			m = p.EmptyTextDeriv (this);
			emptyText [p] = m;
			return m;
		}
	}
	#endregion
}

