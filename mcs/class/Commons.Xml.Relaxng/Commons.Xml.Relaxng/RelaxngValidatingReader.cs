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
		bool labelsComputed;
		Hashtable elementLabels = new Hashtable ();
		Hashtable attributeLabels = new Hashtable ();
		bool isEmptiable;
		bool roughLabelCheck;
		ArrayList strictCheckCache;

		internal string CurrentStateXml {
			get { return RdpUtil.DebugRdpPattern (vState, new Hashtable ()); }
		}

		internal string PreviousStateXml {
			get { return RdpUtil.DebugRdpPattern (prevState, new Hashtable ()); }
		}

		public bool RoughLabelCheck {
			get { return roughLabelCheck; }
			set { roughLabelCheck = value; }
		}

		#region These members will be removed soon!
		[Obsolete ("Use GetElementLabels(object) instead.")]
		public ICollection ExpectedElements {
			get {
				if (!labelsComputed)
					GetLabels (elementLabels, attributeLabels);
				return elementLabels.Values;
			}
		}

		[Obsolete ("Use GetAttributeLabels(object) instead.")]
		public ICollection ExpectedAttributes {
			get {
				if (!labelsComputed)
					GetLabels (elementLabels, attributeLabels);
				return attributeLabels.Values;
			}
		}

		[Obsolete ("Use GetElementLabels(object) and GetAttributeLabels(object) instead.")]
		public void GetLabels (Hashtable elements, Hashtable attributes)
		{
			if (elements == null)
				throw new ArgumentNullException ("elements");
			if (attributes == null)
				throw new ArgumentNullException ("attributes");
			PrepareState ();
			vState.GetLabels (elements, attributes);

			if (roughLabelCheck)
				return;

			// Strict check that tries actual validation that will
			// cover rejection by notAllowed.
			if (strictCheckCache == null)
				strictCheckCache = new ArrayList ();
			else
				strictCheckCache.Clear ();
			foreach (XmlQualifiedName qname in attributes.Values)
				if (vState.AttDeriv (qname.Name, qname.Namespace,null, this) is RdpNotAllowed)
					strictCheckCache.Add (qname);
			foreach (XmlQualifiedName qname in strictCheckCache)
				attributes.Remove (qname);
			strictCheckCache.Clear ();
			foreach (XmlQualifiedName qname in elements.Values)
				if (vState.StartTagOpenDeriv (qname.Name, qname.Namespace) is RdpNotAllowed)
					strictCheckCache.Add (qname);
			foreach (XmlQualifiedName qname in strictCheckCache)
				elements.Remove (qname);
		}

		[Obsolete ("Use Emptiable(object) instead.")]
		public bool Emptiable ()
		{
			if (!labelsComputed) {
				PrepareState ();
				isEmptiable = !(vState.EndTagDeriv () is RdpNotAllowed);
			}
			return isEmptiable;
		}
		#endregion

		#region Validation State support


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

			labelsComputed = false;
			elementLabels.Clear ();
			attributeLabels.Clear ();

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

