//
// XmlSchemaGroupBase.cs
//
// Authors:
//	Dwivedi, Ajay kumar Adwiv@Yahoo.com
//	Atsushi Enomoto atsushi@ximian.com
//
using System;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	public abstract class XmlSchemaGroupBase : XmlSchemaParticle
	{
		private XmlSchemaObjectCollection compiledItems;

		protected XmlSchemaGroupBase ()
		{
			compiledItems = new XmlSchemaObjectCollection ();
		}

		[XmlIgnore]
		public abstract XmlSchemaObjectCollection Items { get; }

		internal XmlSchemaObjectCollection CompiledItems 
		{
			get{ return compiledItems; }
		}

		internal override bool ParticleEquals (XmlSchemaParticle other)
		{
			XmlSchemaGroupBase gb = other as XmlSchemaGroupBase;
			if (gb == null)
				return false;
			if (this.GetType () != gb.GetType ())
				return false;

			if (this.ValidatedMaxOccurs != gb.ValidatedMaxOccurs ||
				this.ValidatedMinOccurs != gb.ValidatedMinOccurs)
				return false;
			if (this.CompiledItems.Count != gb.CompiledItems.Count)
				return false;
			for (int i = 0; i < CompiledItems.Count; i++) {
				XmlSchemaParticle p1 = this.CompiledItems [i] as XmlSchemaParticle;
				XmlSchemaParticle p2 = gb.CompiledItems [i] as XmlSchemaParticle;
				if (!p1.ParticleEquals (p2))
					return false;
			}
			return true;
		}

		internal override void CheckRecursion (int depth, ValidationEventHandler h, XmlSchema schema)
		{
			foreach (XmlSchemaParticle p in this.Items)
				p.CheckRecursion (depth, h, schema);
		}

		internal void ValidateNSRecurseCheckCardinality (XmlSchemaAny any,
			ValidationEventHandler h, XmlSchema schema)
		{
			foreach (XmlSchemaParticle p in Items)
				p.ValidateDerivationByRestriction (any, h, schema);
			ValidateOccurenceRangeOK (any, h, schema);
		}

		internal void ValidateRecurse (XmlSchemaGroupBase baseGroup,
			ValidationEventHandler h, XmlSchema schema)
		{
			int index = 0;
			for (int i = 0; i < baseGroup.CompiledItems.Count; i++) {
				XmlSchemaParticle pb = ((XmlSchemaParticle) baseGroup.CompiledItems [i]).ActualParticle;
				if (pb == XmlSchemaParticle.Empty)
					continue;
				XmlSchemaParticle pd = null;
				while (this.CompiledItems.Count > index) {
					pd = ((XmlSchemaParticle) this.CompiledItems [index]).ActualParticle;
					index++;
					if (pd != XmlSchemaParticle.Empty)
						break;
				}
				ValidateParticleSection (ref index, pd, pb, h, schema);
			}
			if (this.compiledItems.Count > 0 && index != this.CompiledItems.Count)
				error (h, "Invalid particle derivation by restriction was found. Extraneous derived particle was found.");
		}

		private void ValidateParticleSection (ref int index, XmlSchemaParticle pd, XmlSchemaParticle pb, ValidationEventHandler h, XmlSchema schema)
		{
			if (pd == pb) // they are same particle
				return;

			if (pd != null) {
				try {
					XmlSchemaElement el = pd as XmlSchemaElement;
					XmlSchemaParticle pdx = pd;
					if (el != null && el.SubstitutingElements.Count > 0)
						pdx = el.SubstitutingChoice;

					pdx.ValidateDerivationByRestriction (pb, h, schema);
				} catch (XmlSchemaException ex) {
					if (!pb.ValidateIsEmptiable ())
						error (h, "Invalid particle derivation by restriction was found. Invalid sub-particle derivation was found.", ex);
					else
						index--; // try the same derived particle and next base particle.
				}
			} else if (!pb.ValidateIsEmptiable ()) {
				error (h, "Invalid particle derivation by restriction was found. Base schema particle has non-emptiable sub particle that is not mapped to the derived particle.");
				return;
			}
		}
	}
}
