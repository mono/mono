//
// Mono.Xml.Schema.XsdParticleValidationState.cs
//
// Author:
//	Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//
//	(C)2003 Atsushi Enomoto
//
using System;
using System.Collections;
using System.Xml;
using System.Xml.Schema;


namespace Mono.Xml
{
	public class XsdValidationStateFactory
	{
		Hashtable table;

		public XsdValidationStateFactory ()
		{
			table = new Hashtable ();
		}

		public XsdValidationState Get (XmlSchemaObject xsobj)
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
			case "EmptyParticle":	// Microsoft.NET
			case "XmlSchemaParticleEmpty":	// Mono
				return AddEmpty ();
			default:
				throw new NotImplementedException ();
			}
		}

		private XsdElementValidationState AddElement (XmlSchemaElement element)
		{
			XsdElementValidationState got = new XsdElementValidationState (element, this);
			table [element] = got;
			return got;
		}

		private XsdSequenceValidationState AddSequence (XmlSchemaSequence sequence)
		{
			XsdSequenceValidationState got = new XsdSequenceValidationState (sequence, this);
			table [sequence] = got;
			return got;
		}

		private XsdChoiceValidationState AddChoice (XmlSchemaChoice choice)
		{
			XsdChoiceValidationState got = new XsdChoiceValidationState (choice, this);
			table [choice] = got;
			return got;
		}

		private XsdAllValidationState AddAll (XmlSchemaAll all)
		{
			XsdAllValidationState got = new XsdAllValidationState (all, this);
			table [all] = got;
			return got;
		}

		private XsdAnyValidationState AddAny (XmlSchemaAny any)
		{
			XsdAnyValidationState got = new XsdAnyValidationState (any, this);
			table [any] = got;
			return got;
		}

		private XsdEmptyValidationState AddEmpty ()
		{
			return new XsdEmptyValidationState (this);
		}
	}

	public abstract class XsdValidationState
	{
		int occured;
		XsdValidationStateFactory factory;
		XmlSchemaElement currentElement;

		public XsdValidationState (XsdValidationStateFactory factory)
		{
			this.factory = factory;
		}

		// Normally checks MaxOccurs boundary
		public abstract bool EvaluateStartElement (string localName, string ns);

		// Normally checks MinOccurs boundary
		public abstract bool EvaluateEndElement ();

		public abstract bool Emptiable { get; }

		public XmlSchemaDatatype Datatype {
			get {
				XmlSchemaDatatype dt = currentElement.ElementType as XmlSchemaDatatype;
				return dt != null ? dt : ((XmlSchemaType) currentElement.ElementType).Datatype;
			}
		}

		public XmlSchemaElement Element {
			get { return currentElement; }
		}

		public XsdValidationStateFactory Factory {
			get { return factory; }
		}

		public int Occured {
			get { return occured; }
		}

		internal void IncrementOccurence ()
		{
			occured++;
		}

		internal void SetCurrentElement (XmlSchemaElement element)
		{
			currentElement = element;
		}

		internal void SetValidResult (XsdValidationState matched)
		{
			this.currentElement = matched.currentElement;
		}
	}

	public class XsdElementValidationState : XsdValidationState
	{
		public XsdElementValidationState (XmlSchemaElement element, XsdValidationStateFactory factory)
			: base (factory)
		{
			this.element = element;
			this.schemaType = element.ElementType as XmlSchemaType;
			this.dataType = element.ElementType as XmlSchemaDatatype;
			name = element.QualifiedName.Name;
			ns = element.QualifiedName.Namespace;
		}

		// final fields
		XmlSchemaElement element;
		XmlSchemaType schemaType;
		XmlSchemaDatatype dataType;
		string name;
		string ns;

		// Methods
		
		public override bool EvaluateStartElement (string name, string ns)
		{
			if (this.name == name && this.ns == ns) {
				if (Occured >= element.MaxOccurs)
					return false;

				IncrementOccurence ();
				SetCurrentElement (element);
				return true;
			} else {
				return false;
			}
		}

		public override bool EvaluateEndElement ()
		{
			return (element.MinOccurs <= Occured);
		}

		public override bool Emptiable {
			get { return element.MinOccurs <= Occured; }
		}
	}

	public class XsdSequenceValidationState : XsdValidationState
	{
		XmlSchemaSequence seq;
		int current = -1;
		XsdValidationState currentAutomata;

		public XsdSequenceValidationState (XmlSchemaSequence sequence, XsdValidationStateFactory factory)
			: base (factory)
		{
			seq = sequence;
		}

		public override bool EvaluateStartElement (string localName, string ns)
		{
			if (Occured > seq.MaxOccurs)
				return false;

			int idx = current < 0 ? 0 : current;
			XsdValidationState xa = currentAutomata;
			if (xa == null)
				xa = Factory.Create (seq.Items [idx]);
			bool increment = false;

			while (xa != null) {
				if (!xa.EvaluateStartElement (localName, ns)) {
					if (!xa.Emptiable)
						return false;
				} else {
					if (increment) {
						if (Occured + 1 >= seq.MaxOccurs)
							return false;
						IncrementOccurence ();
					}
					current = idx;
					currentAutomata = xa;
					SetValidResult (currentAutomata);
					return true;
				}

				idx++;
				if (seq.Items.Count > idx)
					xa = Factory.Create (seq.Items [idx]);
				else if (current < 0)	// started from top
					xa = null;
				else {		// started from middle
					idx = 0;
					increment = true;
					xa = (idx < current) ?
						Factory.Create (seq.Items [idx])
						: null;
				}
			}
			return false;
		}

		public override bool EvaluateEndElement ()
		{
			if (current < 0)
				return (seq.MinOccurs <= Occured);

			// Then we are in the middle of the sequence.
			// Check all following emptiable items.
			if (!Emptiable)
				return false;

			return seq.MinOccurs <= Occured + 1;
		}

		public override bool Emptiable {
			get {
				if (seq.MinOccurs > Occured + 1)
					return false;

				int idx = current;
				XsdValidationState xa = currentAutomata;
				if (xa == null)
					xa = Factory.Create (seq.Items [idx]);
				while (xa != null) {
					if (!xa.Emptiable)
						return false;	// cannot omit following items.
					idx++;
					if (seq.Items.Count > idx)
						xa = Factory.Create (seq.Items [idx]);
					else
						xa = null;
				}
				return true;
			}
		}
	}

	public class XsdChoiceValidationState : XsdValidationState
	{
		XmlSchemaChoice choice;
		XsdValidationState incomplete;

		public XsdChoiceValidationState (XmlSchemaChoice choice, XsdValidationStateFactory factory)
			: base (factory)
		{
			this.choice = choice;
		}

		public override bool EvaluateStartElement (string localName, string ns)
		{
			if (incomplete != null) {
				if (incomplete.EvaluateStartElement (localName, ns))
					return true;
				else {
					if (!incomplete.Emptiable)
						return false;
					else
						incomplete = null;
				}
			}

			if (Occured >= choice.MaxOccurs)
				return false;

			foreach (XmlSchemaObject xsobj in choice.Items) {
				XsdValidationState xa = Factory.Create (xsobj);
				if (xa.EvaluateStartElement (localName, ns)) {
					incomplete = xa;
					this.SetValidResult (xa);
					IncrementOccurence ();
					return true;
				}
			}
			
			return false;
		}

		public override bool EvaluateEndElement ()
		{
			return (choice.MinOccurs <= Occured) && 
				(incomplete != null ? incomplete.Emptiable : true);
		}

		public override bool Emptiable {
			get {
				return (choice.MinOccurs <= Occured) &&
					(incomplete != null ? incomplete.Emptiable : true);
			}
		}
	}

	public class XsdAllValidationState : XsdValidationState
	{
		XmlSchemaAll all;
		ArrayList consumed = new ArrayList ();
		XsdValidationState incomplete;

		public XsdAllValidationState (XmlSchemaAll all, XsdValidationStateFactory factory)
			: base (factory)
		{
			this.all = all;
		}

		public override bool EvaluateStartElement (string localName, string ns)
		{
			if (Occured > all.MaxOccurs)
				return false;

			if (incomplete != null) {
				if (incomplete.EvaluateStartElement (localName, ns))
					return true;
				else {
					if (!incomplete.Emptiable)
						return false;
					else
						incomplete = null;
				}
			}

			foreach (XmlSchemaObject xsobj in all.Items) {
				if (consumed.Contains (xsobj))
					return false;

				XsdValidationState xa = Factory.Create (xsobj);
				if (xa.EvaluateStartElement (localName, ns)) {
					consumed.Add (xsobj);
					this.SetValidResult (xa);
					return true;
				}
			}
			return false;
		}

		public override bool EvaluateEndElement ()
		{
			if (consumed.Count == 0 && all.MinOccurs > 0)
				return false;

			// quick check
			if (all.Items.Count == consumed.Count)
				return true;

			if (incomplete != null && !incomplete.Emptiable)
				return false;

			foreach (XmlSchemaParticle xsobj in all.Items) {
				if (xsobj.MinOccurs > 0 && !consumed.Contains (xsobj))
					return false;	// missing item was found
			}
			return false;
		}

		public override bool Emptiable {
			get {
				return (all.MinOccurs <= Occured) &&
					(incomplete != null ? incomplete.Emptiable : true);
			}
		}
	}

	public class XsdAnyValidationState : XsdValidationState
	{
		public XsdAnyValidationState (XmlSchemaAny any, XsdValidationStateFactory factory)
			: base (factory)
		{
			this.any = any;
		}

		// final fields
		XmlSchemaAny any;

		// Methods
		
		public override bool EvaluateStartElement (string name, string ns)
		{
			throw new NotImplementedException ();
		}

		public override bool EvaluateEndElement ()
		{
			return (any.MinOccurs > Occured);
		}

		public override bool Emptiable {
			get { return any.MinOccurs <= Occured; }
		}
	}

	public class XsdEmptyValidationState : XsdValidationState
	{
		public XsdEmptyValidationState (XsdValidationStateFactory factory)
			: base (factory)
		{
		}

		// Methods
		
		public override bool EvaluateStartElement (string name, string ns)
		{
			return false;
		}

		public override bool EvaluateEndElement ()
		{
			return true;
		}

		public override bool Emptiable {
			get { return true; }
		}
	}

}
