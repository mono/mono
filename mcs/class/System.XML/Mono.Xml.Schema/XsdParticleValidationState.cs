//
// Mono.Xml.Schema.XsdParticleValidationState.cs
//
// Author:
//	Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//
//	(C)2003 Atsushi Enomoto
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
using System.Xml.Schema;
using Mono.Xml;

namespace Mono.Xml.Schema
{
	internal class XsdValidationStateManager
	{
		Hashtable table;
		XmlSchemaElement currentElement;
		XmlSchemaContentProcessing processContents;

		public XsdValidationStateManager ()
		{
			table = new Hashtable ();
			processContents = XmlSchemaContentProcessing.Strict; // not Lax
		}

		public XmlSchemaElement CurrentElement {
			get { return currentElement; }
			set { currentElement = value; }
		}

		internal void SetCurrentElement (XmlSchemaElement elt)
		{
			this.currentElement = elt;
		}

		public XmlSchemaContentProcessing ProcessContents {
			get { return processContents; }
		}

		internal void SetProcessContents (XmlSchemaContentProcessing value)
		{
			this.processContents = value;
		}

		public XsdValidationState Get (XmlSchemaParticle xsobj)
		{
			XsdValidationState got = table [xsobj] as XsdValidationState;
			if (got == null)
				got = Create (xsobj);
			return got;
		}

		public XsdValidationState Create (XmlSchemaObject xsobj)
		{
			string typeName = xsobj.GetType ().Name;
			switch (typeName) {
			case "XmlSchemaElement":
				return AddElement ((XmlSchemaElement) xsobj);
			case "XmlSchemaSequence":
				return AddSequence ((XmlSchemaSequence) xsobj);
			case "XmlSchemaChoice":
				return AddChoice ((XmlSchemaChoice) xsobj);
			case "XmlSchemaAll":
				return AddAll ((XmlSchemaAll) xsobj);
			case "XmlSchemaAny":
				return AddAny ((XmlSchemaAny) xsobj);
			case "EmptyParticle":
				return AddEmpty ();
			default:
				// GroupRef should not appear
				throw new InvalidOperationException ("Should not occur.");
			}
		}

		internal XsdValidationState MakeSequence (XsdValidationState head, XsdValidationState rest)
		{
			if (head is XsdEmptyValidationState)
				return rest;
			else
				return new XsdAppendedValidationState (this, head, rest);
		}

		private XsdElementValidationState AddElement (XmlSchemaElement element)
		{
			XsdElementValidationState got = new XsdElementValidationState (element, this);
			return got;
		}

		private XsdSequenceValidationState AddSequence (XmlSchemaSequence sequence)
		{
			XsdSequenceValidationState got = new XsdSequenceValidationState (sequence, this);
			return got;
		}

		private XsdChoiceValidationState AddChoice (XmlSchemaChoice choice)
		{
			XsdChoiceValidationState got = new XsdChoiceValidationState (choice, this);
			return got;
		}

		private XsdAllValidationState AddAll (XmlSchemaAll all)
		{
			XsdAllValidationState got = new XsdAllValidationState (all, this);
			return got;
		}

		private XsdAnyValidationState AddAny (XmlSchemaAny any)
		{
			XsdAnyValidationState got = new XsdAnyValidationState (any, this);
			return got;
		}

		private XsdEmptyValidationState AddEmpty ()
		{
			return new XsdEmptyValidationState (this);
		}
	}

	internal abstract class XsdValidationState
	{
		// Static members

		static XsdInvalidValidationState invalid;

		static XsdValidationState ()
		{
			invalid = new XsdInvalidValidationState (null);
		}

		public static XsdInvalidValidationState Invalid {
			get { return invalid; }
		}

		// Dynamic members

		int occured;
		string message;
		XsdValidationStateManager manager;

		public XsdValidationState (XsdValidationStateManager manager)
		{
			this.manager = manager;
		}

		// Normally checks Max Occurs boundary
		public abstract XsdValidationState EvaluateStartElement (string localName, string ns);

		// Normally checks Min Occurs boundary
		public abstract bool EvaluateEndElement ();

		internal abstract bool EvaluateIsEmptiable ();

		public XsdValidationStateManager Manager {
			get { return manager; }
		}

		public string Message {
			get { return message; }
		}

		public string MessageInternal {
			get { return message; }
			set { message = value; }
		}

		public int Occured {
			get { return occured; }
		}

		internal int OccuredInternal {
			get { return occured; }
			set { occured = value; }
		}
	}

	internal class XsdElementValidationState : XsdValidationState
	{
		public XsdElementValidationState (XmlSchemaElement element, XsdValidationStateManager manager)
			: base (manager)
		{
			this.element = element;
			name = element.QualifiedName.Name;
			ns = element.QualifiedName.Namespace;
		}

		// final fields
		XmlSchemaElement element;
		string name;
		string ns;

		// Methods
		
		public override XsdValidationState EvaluateStartElement (string name, string ns)
		{
			if (this.name == name && this.ns == ns && !element.IsAbstract) {
				return this.CheckOccurence (element);
			} else {
				for (int i = 0; i < element.SubstitutingElements.Count; i++) {
					XmlSchemaElement subst = (XmlSchemaElement) element.SubstitutingElements [i];
					if (subst.QualifiedName.Name == name &&
						subst.QualifiedName.Namespace == ns) {
						return this.CheckOccurence (subst);
					}
				}
				return XsdValidationState.Invalid;
			}
		}

		private XsdValidationState CheckOccurence (XmlSchemaElement maybeSubstituted)
		{
			OccuredInternal++;
			Manager.SetCurrentElement (maybeSubstituted);
			if (Occured > element.ValidatedMaxOccurs) {
				MessageInternal = "Element occurence excess.";
				return XsdValidationState.Invalid;
			} else if (Occured == element.ValidatedMaxOccurs) {
				return Manager.Create (XmlSchemaParticle.Empty);
			} else {
				return this;
			}
		}

		public override bool EvaluateEndElement ()
		{
			return EvaluateIsEmptiable ();
		}

		internal override bool EvaluateIsEmptiable ()
		{
			return (element.ValidatedMinOccurs <= Occured &&
				element.ValidatedMaxOccurs >= Occured);
		}
	}

	internal class XsdSequenceValidationState : XsdValidationState
	{
		XmlSchemaSequence seq;
		int current;
		XsdValidationState currentAutomata;
		bool emptiable;
		decimal minOccurs;
		decimal maxOccurs;

		public XsdSequenceValidationState (XmlSchemaSequence sequence, XsdValidationStateManager manager)
			: this (sequence, manager, sequence.ValidatedMinOccurs, sequence.ValidatedMaxOccurs, -1)
		{
		}

		public XsdSequenceValidationState (XmlSchemaSequence sequence, XsdValidationStateManager manager,
			decimal minOccurs, decimal maxOccurs, int current)
			: base (manager)
		{
			seq = sequence;
			this.minOccurs = minOccurs;
			this.maxOccurs = maxOccurs;
			this.current = current;
		}

		public override XsdValidationState EvaluateStartElement (string name, string ns)
		{
			if (seq.CompiledItems.Count == 0)
				return XsdValidationState.Invalid;

			int idx = current < 0 ? 0 : current;
			XsdValidationState xa = currentAutomata;
			// If it is true and when matching particle was found, then
			// it will increment occurence.
			bool increment = false;

			while (true) {
//				if (current < 0 || current == seq.CompiledItems.Count) {
//					idx = current = 0;
//					increment = true;
//				}
				if (xa == null) {	// This code runs in case of a newiteration.
					xa = Manager.Create (seq.CompiledItems [idx] as XmlSchemaParticle);
					increment = true;
				}
				if (xa is XsdEmptyValidationState &&
						seq.CompiledItems.Count == idx + 1 &&
						Occured == maxOccurs) {
					return XsdValidationState.Invalid;
				} else {
					XsdValidationState result = xa.EvaluateStartElement (name, ns);
					if (result == XsdValidationState.Invalid) {
						if (!xa.EvaluateIsEmptiable ()) {
							emptiable = false;
							return XsdValidationState.Invalid;
						}
					} else {
						current = idx;
						currentAutomata = result;
						if (increment) {
							OccuredInternal++;
							if (Occured > maxOccurs)
								return XsdValidationState.Invalid;
						}
//						current++;
//						return Manager.MakeSequence (result, this);
						return this;
					// skip in other cases.
					}
				}
				idx++;
				if (idx > current && increment && current >= 0) {
					return XsdValidationState.Invalid;
				}
				if (seq.CompiledItems.Count > idx) {
					xa = Manager.Create (seq.CompiledItems [idx] as XmlSchemaParticle);
				}
				else if (current < 0) {	// started from top
					return XsdValidationState.Invalid;
				}
				else {		// started from middle
					idx = 0;
					xa = null;
				}
			}
		}

		public override bool EvaluateEndElement ()
		{
			if (minOccurs > Occured + 1)
				return false;
			if (seq.CompiledItems.Count == 0)
				return true;
			if (currentAutomata == null && minOccurs <= Occured)
				return true;

			int idx = current < 0 ? 0 : current;
			XsdValidationState xa = currentAutomata;
			if (xa == null)
				xa = Manager.Create (seq.CompiledItems [idx] as XmlSchemaParticle);
			while (xa != null) {
				if (!xa.EvaluateEndElement ())
					if (!xa.EvaluateIsEmptiable ())
						return false;	// cannot omit following items.
				idx++;
				if (seq.CompiledItems.Count > idx)
					xa = Manager.Create (seq.CompiledItems [idx] as XmlSchemaParticle);
				else
					xa = null;
			}
			if (current < 0)
				OccuredInternal++;

			return minOccurs <= Occured && maxOccurs >= Occured;
		}

		internal override bool EvaluateIsEmptiable ()
		{
			if (minOccurs > Occured + 1)
				return false;
			if (minOccurs == 0 && currentAutomata == null)
				return true;

			if (emptiable)
				return true;
			if (seq.CompiledItems.Count == 0)
				return true;

			int idx = current < 0 ? 0 : current;
			XsdValidationState xa = currentAutomata;
			if (xa == null)
				xa = Manager.Create (seq.CompiledItems [idx] as XmlSchemaParticle);
			while (xa != null) {
				if (!xa.EvaluateIsEmptiable ())
					return false;
				idx++;
				if (seq.CompiledItems.Count > idx)
					xa = Manager.Create (seq.CompiledItems [idx] as XmlSchemaParticle);
				else
					xa = null;
			}
			emptiable = true;
			return true;
		}

	}

	internal class XsdChoiceValidationState : XsdValidationState
	{
		XmlSchemaChoice choice;
		bool emptiable;
		bool emptiableComputed;

		public XsdChoiceValidationState (XmlSchemaChoice choice, XsdValidationStateManager manager)
			: base (manager)
		{
			this.choice = choice;
		}

		public override XsdValidationState EvaluateStartElement (string localName, string ns)
		{
			emptiableComputed = false;

			for (int i = 0; i < choice.CompiledItems.Count; i++) {
				XmlSchemaParticle xsobj = (XmlSchemaParticle) choice.CompiledItems [i];
				XsdValidationState xa = Manager.Create (xsobj);
				XsdValidationState result = xa.EvaluateStartElement (localName, ns);
				if (result != XsdValidationState.Invalid) {
					OccuredInternal++;
					if (Occured > choice.ValidatedMaxOccurs)
						return XsdValidationState.Invalid;
					else if (Occured == choice.ValidatedMaxOccurs)
						return result;
					else
						return Manager.MakeSequence (result, this);
				}
			}
			emptiable = choice.ValidatedMinOccurs <= Occured;
			emptiableComputed = true;
			return XsdValidationState.Invalid;
		}

		public override bool EvaluateEndElement ()
		{
			emptiableComputed = false;

			if (choice.ValidatedMinOccurs > Occured + 1)
				return false;

			else if (choice.ValidatedMinOccurs <= Occured)
				return true;

			for (int i = 0; i < choice.CompiledItems.Count; i++) {
				XmlSchemaParticle p = (XmlSchemaParticle) choice.CompiledItems [i];
				if (Manager.Create (p).EvaluateIsEmptiable ())
					return true;
			}
			return false;
		}

		internal override bool EvaluateIsEmptiable ()
		{
			if (emptiableComputed)
				return emptiable;

			if (choice.ValidatedMaxOccurs < Occured)
				return false;
			else if (choice.ValidatedMinOccurs > Occured + 1)
				return false;

			for (int i = Occured; i < choice.ValidatedMinOccurs; i++) {
				bool next = false;
				for (int pi = 0; pi < choice.CompiledItems.Count; pi++) {
					XmlSchemaParticle p = (XmlSchemaParticle) choice.CompiledItems [pi];
					if (Manager.Create (p).EvaluateIsEmptiable ()) {
						next = true;
						break;
					}
				}
				if (!next)
					return false;
			}
			return true;
		}
	}

	internal class XsdAllValidationState : XsdValidationState
	{
		XmlSchemaAll all;
		ArrayList consumed = new ArrayList ();

		public XsdAllValidationState (XmlSchemaAll all, XsdValidationStateManager manager)
			: base (manager)
		{
			this.all = all;
		}

		public override XsdValidationState EvaluateStartElement (string localName, string ns)
		{
			if (all.CompiledItems.Count == 0)
				return XsdValidationState.Invalid;

			// We don't have to keep element validation state, since 
			// it must occur only 0 or 1.
			for (int i = 0; i < all.CompiledItems.Count; i++) {
				XmlSchemaElement xsElem = (XmlSchemaElement) all.CompiledItems [i];
				if (xsElem.QualifiedName.Name == localName &&
					xsElem.QualifiedName.Namespace == ns) {
					if (consumed.Contains (xsElem))
						return XsdValidationState.Invalid;
					consumed.Add (xsElem);
					Manager.SetCurrentElement (xsElem);
					OccuredInternal = 1;	// xs:all also occurs 0 or 1 always.
					return this;
				}
			}
			return XsdValidationState.Invalid;
		}

		public override bool EvaluateEndElement ()
		{
			if (all.Emptiable || all.ValidatedMinOccurs == 0)
				return true;
			if (all.ValidatedMinOccurs > 0 && consumed.Count == 0)
				return false;
			if (all.CompiledItems.Count == consumed.Count)
				return true;
			for (int i = 0; i < all.CompiledItems.Count; i++) {
				XmlSchemaElement el = (XmlSchemaElement) all.CompiledItems [i];
				if (el.ValidatedMinOccurs > 0 && !consumed.Contains (el))
					return false;
			}
			return true;
		}

		internal override bool EvaluateIsEmptiable ()
		{
			if (all.Emptiable || all.ValidatedMinOccurs == 0)
				return true;
			for (int i = 0; i < all.CompiledItems.Count; i++) {
				XmlSchemaElement el = (XmlSchemaElement) all.CompiledItems [i];
				if (el.ValidatedMinOccurs > 0 && !consumed.Contains (el))
					return false;
			}
			return true;
		}
	}

	internal class XsdAnyValidationState : XsdValidationState
	{
		XmlSchemaAny any;

		public XsdAnyValidationState (XmlSchemaAny any, XsdValidationStateManager manager)
			: base (manager)
		{
			this.any = any;
		}

		// Methods
		public override XsdValidationState EvaluateStartElement (string name, string ns)
		{
			if (!MatchesNamespace (ns))
				return XsdValidationState.Invalid;

			OccuredInternal++;
			Manager.SetProcessContents (any.ResolvedProcessContents);
			if (Occured > any.ValidatedMaxOccurs)
				return XsdValidationState.Invalid;
			else if (Occured == any.ValidatedMaxOccurs)
				return Manager.Create (XmlSchemaParticle.Empty);
			else
				return this;
		}

		private bool MatchesNamespace (string ns)
		{
			if (any.HasValueAny)
				return true;
			if (any.HasValueLocal && ns == String.Empty)
				return true;
			if (any.HasValueOther && (any.TargetNamespace == "" || any.TargetNamespace != ns))
				return true;
			if (any.HasValueTargetNamespace && any.TargetNamespace == ns)
				return true;
			for (int i = 0; i < any.ResolvedNamespaces.Count; i++)
				if (any.ResolvedNamespaces [i] == ns)
					return true;
			return false;
		}

		public override bool EvaluateEndElement ()
		{
			return EvaluateIsEmptiable ();
		}

		internal override bool EvaluateIsEmptiable ()
		{
			return any.ValidatedMinOccurs <= Occured &&
				any.ValidatedMaxOccurs >= Occured;
		}
	}

	internal class XsdAppendedValidationState : XsdValidationState
	{
		public XsdAppendedValidationState (XsdValidationStateManager manager,
			XsdValidationState head, XsdValidationState rest)
			: base (manager)
		{
			this.head = head;
			this.rest = rest;
		}

		XsdValidationState head;
		XsdValidationState rest;

		// Methods
		public override XsdValidationState EvaluateStartElement (string name, string ns)
		{
			XsdValidationState afterHead = head.EvaluateStartElement (name, ns);
			if (afterHead != XsdValidationState.Invalid) {
				head = afterHead;
				return afterHead is XsdEmptyValidationState ? rest : this;
			} else if (!head.EvaluateIsEmptiable ()) {
				return XsdValidationState.Invalid;
			}

			return rest.EvaluateStartElement (name, ns);
		}

		public override bool EvaluateEndElement ()
		{
			if (head.EvaluateEndElement ())
//				return true;
				return rest.EvaluateIsEmptiable ();
			if (!head.EvaluateIsEmptiable ())
				return false;
			return rest.EvaluateEndElement ();
		}

		internal override bool EvaluateIsEmptiable ()
		{
			if (head.EvaluateIsEmptiable ())
				return rest.EvaluateIsEmptiable ();
			else
				return false;
		}
	}

	internal class XsdEmptyValidationState : XsdValidationState
	{
		public XsdEmptyValidationState (XsdValidationStateManager manager)
			: base (manager)
		{
		}

		// Methods
		public override XsdValidationState EvaluateStartElement (string name, string ns)
		{
			return XsdValidationState.Invalid;
		}

		public override bool EvaluateEndElement ()
		{
			return true;
		}

		internal override bool EvaluateIsEmptiable ()
		{
			return true;
		}

	}

	internal class XsdInvalidValidationState : XsdValidationState
	{
		internal XsdInvalidValidationState (XsdValidationStateManager manager)
			: base (manager)
		{
		}

		// Methods
		public override XsdValidationState EvaluateStartElement (string name, string ns)
		{
			return this;
		}

		public override bool EvaluateEndElement ()
		{
			return false;
		}

		internal override bool EvaluateIsEmptiable ()
		{
			return false;
		}

	}
}
