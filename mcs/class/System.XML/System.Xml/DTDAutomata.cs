//
// Mono.Xml.DTDAutomata
//
// Author:
//	Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//
//	(C)2003 Atsushi Enomoto
//
using System;
using System.Collections;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using Mono.Xml.Schema;

namespace Mono.Xml
{
	public class DTDAutomataFactory
	{
		public DTDAutomataFactory (DTDObjectModel root)
		{
			this.root = root;
		}

		DTDObjectModel root;
		Hashtable choiceTable = new Hashtable ();
		Hashtable sequenceTable = new Hashtable ();

		public DTDChoiceAutomata Choice (DTDAutomata left, DTDAutomata right)
		{
			Hashtable rightPool = choiceTable [left] as Hashtable;
			if (rightPool == null) {
				rightPool = new Hashtable ();
				choiceTable [left] = rightPool;
			}
			DTDChoiceAutomata result = rightPool [right] as DTDChoiceAutomata;
			if (result == null) {
				result = new DTDChoiceAutomata (root, left, right);
				rightPool [right] = result;
			}
			return result;
		}

		public DTDSequenceAutomata Sequence (DTDAutomata left, DTDAutomata right)
		{
			Hashtable rightPool = sequenceTable [left] as Hashtable;
			if (rightPool == null) {
				rightPool = new Hashtable ();
				sequenceTable [left] = rightPool;
			}
			DTDSequenceAutomata result = rightPool [right] as DTDSequenceAutomata;
			if (result == null) {
				result = new DTDSequenceAutomata (root, left, right);
				rightPool [right] = result;
			}
			return result;
		}
	}

	public abstract class DTDAutomata
	{
		public DTDAutomata (DTDObjectModel root)
		{
			this.root = root;
		}

		private DTDObjectModel root;

		public DTDObjectModel Root {
			get { return root; }
		}

		public DTDAutomata MakeChoice (DTDAutomata other)
		{
			if (this == Root.Invalid)
				return other;
			if (other == Root.Invalid)
				return this;
			if (this == Root.Empty && other == Root.Empty)
				return this;
			if (this == Root.Any && other == Root.Any)
				return this;
			else if (other == Root.Empty)
				return Root.Factory.Choice (other, this);
			else
				return Root.Factory.Choice (this, other);
		}

		public DTDAutomata MakeSequence (DTDAutomata other)
		{
			if (this == Root.Invalid || other == Root.Invalid)
				return Root.Invalid;
			if (this == Root.Empty)
				return other;
			if (other == Root.Empty)
				return this;
			else
				return Root.Factory.Sequence (this, other);
		}

		public abstract DTDAutomata TryStartElement (string name);
		public virtual DTDAutomata TryEndElement ()
		{
			return Root.Invalid;
		}

		public virtual bool Emptiable {
			get { return false; }
		}
	}

	public class DTDElementAutomata : DTDAutomata
	{
		public DTDElementAutomata (DTDObjectModel root, string name)
			: base (root)
		{
			this.name = name;
		}

		private string name;

		public string Name {
			get { return name; }
		}

		public override DTDAutomata TryStartElement (string name)
		{
			if (name == Name)
				return Root.Empty;
			else
				return Root.Invalid;
		}
	}

	public class DTDChoiceAutomata : DTDAutomata
	{
		public DTDChoiceAutomata (DTDObjectModel root,
			DTDAutomata left, DTDAutomata right)
			: base (root)
		{
			this.left = left;
			this.right = right;
		}

		private DTDAutomata left;
		private DTDAutomata right;

		public DTDAutomata Left {
			get { return left; }
		}

		public DTDAutomata Right {
			get { return right; }
		}

		public override DTDAutomata TryStartElement (string name)
		{
			return left.TryStartElement (name).MakeChoice (
				right.TryStartElement (name));
		}

		public override DTDAutomata TryEndElement ()
		{
			return left.TryEndElement ().MakeChoice (right.TryEndElement ());
		}

		bool hasComputedEmptiable;
		bool cachedEmptiable;
		public override bool Emptiable {
			get {
				if (!hasComputedEmptiable) {
					cachedEmptiable = left.Emptiable || 
						right.Emptiable;
					hasComputedEmptiable = true;
				}
				return cachedEmptiable;
			}
		}
	}

	public class DTDSequenceAutomata : DTDAutomata
	{
		public DTDSequenceAutomata (DTDObjectModel root,
			DTDAutomata left, DTDAutomata right)
			: base (root)
		{
			this.left = left;
			this.right = right;
		}

		private DTDAutomata left;
		private DTDAutomata right;

		public DTDAutomata Left {
			get { return left; }
		}

		public DTDAutomata Right {
			get { return right; }
		}

		public override DTDAutomata TryStartElement (string name)
		{
			DTDAutomata afterL = left.TryStartElement (name);
			DTDAutomata afterR = right.TryStartElement (name);
			if (afterL == Root.Invalid)
				return (left.Emptiable) ? afterR : afterL;
			// else
			DTDAutomata whenLeftConsumed = afterL.MakeSequence (right);
			if (left.Emptiable)
				return afterR.MakeChoice (whenLeftConsumed);
			else
				return whenLeftConsumed;
		}

		public override DTDAutomata TryEndElement ()
		{
			return left.Emptiable ? right : Root.Invalid;
		}

		bool hasComputedEmptiable;
		bool cachedEmptiable;
		public override bool Emptiable {
			get {
				if (!hasComputedEmptiable) {
					cachedEmptiable = left.Emptiable &&
						right.Emptiable;
					hasComputedEmptiable = true;
				}
				return cachedEmptiable;
			}
		}
	}

	public class DTDOneOrMoreAutomata : DTDAutomata
	{
		public DTDOneOrMoreAutomata (DTDObjectModel root,
			DTDAutomata children)
			: base (root)
		{
			this.children = children;
		}

		private DTDAutomata children;

		public DTDAutomata Children {
			get { return children; }
		}

		public override DTDAutomata TryStartElement (string name)
		{
			DTDAutomata afterC = children.TryStartElement (name);
			if (afterC != Root.Invalid)
				return afterC.MakeSequence (
					Root.Empty.MakeChoice (this));
			else
				return Root.Invalid;
		}

		public override DTDAutomata TryEndElement ()
		{
			return Emptiable ? children.TryEndElement () : Root.Invalid;
		}
	}

	public class DTDEmptyAutomata : DTDAutomata
	{
		public DTDEmptyAutomata (DTDObjectModel root)
			: base (root)
		{
		}

		public override DTDAutomata TryEndElement ()
		{
			return this;
		}

		public override DTDAutomata TryStartElement (string name)
		{
			return Root.Invalid;
		}

		public override bool Emptiable {
			get { return true; }
		}
	}

	public class DTDAnyAutomata : DTDAutomata
	{
		public DTDAnyAutomata (DTDObjectModel root)
			: base (root)
		{
		}

		public override DTDAutomata TryEndElement ()
		{
			return this;
		}

		public override DTDAutomata TryStartElement (string name)
		{
			return this;
		}

		public override bool Emptiable {
			get { return true; }
		}
	}

	public class DTDInvalidAutomata : DTDAutomata
	{
		public DTDInvalidAutomata (DTDObjectModel root)
			: base (root)
		{
		}

		public override DTDAutomata TryEndElement ()
		{
			return this;
		}

		public override DTDAutomata TryStartElement (string name)
		{
			return this;
		}
	}
}