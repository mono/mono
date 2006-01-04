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
			if (reader.NodeType == XmlNodeType.Attribute)
				throw new RelaxngException ("RELAX NG does not support standalone attribute validation (it is prohibited due to the specification section 7.1.5");
			this.reader = reader;
			this.pattern = pattern;
		}

		XmlReader reader;
		RelaxngPattern pattern;
		RdpPattern vState;
		RdpPattern prevState;	// Mainly for debugging.
		ArrayList PredefinedAttributes = new ArrayList ();
		bool labelsComputed;
		Hashtable elementLabels = new Hashtable ();
		Hashtable attributeLabels = new Hashtable ();
		bool isEmptiable;
		bool roughLabelCheck;
		ArrayList strictCheckCache;
		bool reportDetails;
		string cachedValue;
		int startElementDepth = -1;
		bool inContent;

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

			labelsComputed = false;
			elementLabels.Clear ();
			attributeLabels.Clear ();

			bool ret = reader.Read ();

			// Process pending text node validation if required.
			if (cachedValue != null)
				ValidateText (ret);

			switch (reader.NodeType) {
			case XmlNodeType.Element:
				inContent = true;
				// StartTagOpenDeriv
				prevState = vState;
				vState = StartTagOpenDeriv (vState,
					reader.LocalName, reader.NamespaceURI);
				if (vState.PatternType == RelaxngPatternType.NotAllowed)
					throw CreateValidationError (String.Format ("Invalid start tag found. LocalName = {0}, NS = {1}.", reader.LocalName, reader.NamespaceURI), true);

				// AttsDeriv equals to for each AttDeriv
				string elementNS = reader.NamespaceURI;
				if (reader.MoveToFirstAttribute ()) {
					do {
						if (reader.Name.IndexOf ("xmlns:") == 0 || reader.Name == "xmlns")
							continue;

						prevState = vState;
						string attrNS = reader.NamespaceURI;
						vState = vState.AttDeriv (reader.LocalName, attrNS, reader.GetAttribute (reader.LocalName, attrNS), this);
						if (vState.PatternType == RelaxngPatternType.NotAllowed)
							throw CreateValidationError (String.Format ("Invalid attribute found. LocalName = {0}, NS = {1}.", reader.LocalName, reader.NamespaceURI), false);
					} while (reader.MoveToNextAttribute ());
					MoveToElement ();
				}

				// StarTagCloseDeriv
				prevState = vState;
				vState = StartTagCloseDeriv (vState);
				if (vState.PatternType == RelaxngPatternType.NotAllowed)
					throw CreateValidationError (String.Format ("Invalid start tag closing found. LocalName = {0}, NS = {1}.", reader.LocalName, reader.NamespaceURI), false);

				// if it is empty, then redirect to EndElement
				if (reader.IsEmptyElement)
					goto case XmlNodeType.EndElement;
				break;
			case XmlNodeType.EndElement:
				if (reader.Depth == 0)
					inContent = false;
				// EndTagDeriv
				prevState = vState;
				vState = EndTagDeriv (vState);
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

			if (reader.NodeType == XmlNodeType.Element)
				startElementDepth = reader.Depth;
			else if (reader.NodeType == XmlNodeType.EndElement)
				startElementDepth = -1;

			return ret;
		}

		void ValidateText (bool remain)
		{
			RdpPattern ts = vState;
			switch (reader.NodeType) {
			case XmlNodeType.EndElement:
				if (startElementDepth != reader.Depth)
					goto case XmlNodeType.Element;
				ts = TextOnlyDeriv (ts);
				ts = ts.TextDeriv (cachedValue, reader);
				// FIXME: shouldn't it be done?
//				if (Util.IsWhitespace (cachedValue))
//					ts = vState.MakeChoice (ts, vState);
				break;
			case XmlNodeType.Element:
				startElementDepth = -1;
				if (!Util.IsWhitespace (cachedValue)) {
					ts = MixedTextDeriv (ts, cachedValue);
					ts = ts.TextDeriv (cachedValue, reader);
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

		#region Memoization support

		ArrayList memo = new ArrayList ();

		enum DerivativeType {
			StartTagOpen,
			StartTagClose,
			EndTag,
			Mixed,
			TextOnly
		}

		class Memoization
		{
			public Memoization (DerivativeType type, RdpPattern input, RdpPattern output)
			{
				Type = type;
				Input = input;
				Output = output;
			}

			public readonly DerivativeType Type;
			public readonly RdpPattern Input;
			public readonly RdpPattern Output;
		}

		class MemoizationStartTagOpen : Memoization
		{
			public MemoizationStartTagOpen (string name, string ns, RdpPattern input, RdpPattern output)
				: base (DerivativeType.StartTagOpen, input, output)
			{
				Name = name;
				NS = ns;
			}

			public readonly string Name;
			public readonly string NS;
		}

		RdpPattern StartTagOpenDeriv (RdpPattern p, string local, string ns)
		{
			for (int i = 0; i < memo.Count; i++) {
				Memoization tag = (Memoization) memo [i];
				if (tag.Type != DerivativeType.StartTagOpen)
					continue;
				MemoizationStartTagOpen sto =
					tag as MemoizationStartTagOpen;
				if (sto.Input == p &&
				    object.ReferenceEquals (sto.Name, local) &&
				    object.ReferenceEquals (sto.NS, ns))
					return tag.Output;
			}

			RdpPattern m = p.StartTagOpenDeriv (local, ns);
			memo.Add (new MemoizationStartTagOpen (local, ns, p, m));
			return m;
		}

		RdpPattern StartTagCloseDeriv (RdpPattern p)
		{
			for (int i = 0; i < memo.Count; i++) {
				Memoization tag = (Memoization) memo [i];
				if (tag.Type == DerivativeType.StartTagClose && tag.Input == p)
					return tag.Output;
			}

			RdpPattern m = p.StartTagCloseDeriv ();
			memo.Add (new Memoization (
					DerivativeType.StartTagClose, p, m));
			return m;
		}

		RdpPattern EndTagDeriv (RdpPattern p)
		{
			for (int i = 0; i < memo.Count; i++) {
				Memoization tag = (Memoization) memo [i];
				if (tag.Type == DerivativeType.EndTag && tag.Input == p)
					return tag.Output;
			}

			RdpPattern m = p.EndTagDeriv ();
			memo.Add (new Memoization (DerivativeType.EndTag, p, m));
			return m;
		}

		RdpPattern MixedTextDeriv (RdpPattern p, string s)
		{
			for (int i = 0; i < memo.Count; i++) {
				Memoization tag = (Memoization) memo [i];
				if (tag.Type == DerivativeType.Mixed && tag.Input == p)
					return tag.Output;
			}

			RdpPattern m = p.MixedTextDeriv (s);
			memo.Add (new Memoization (DerivativeType.Mixed, p, m));
			return m;
		}

		RdpPattern TextOnlyDeriv (RdpPattern p)
		{
			for (int i = 0; i < memo.Count; i++) {
				Memoization tag = (Memoization) memo [i];
				if (tag.Type == DerivativeType.TextOnly && tag.Input == p)
					return tag.Output;
			}

			RdpPattern m = p.TextOnlyDeriv ();
			memo.Add (new Memoization (DerivativeType.TextOnly, p, m));
			return m;
		}

		#endif
	}
}

