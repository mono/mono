// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaGroupBase.
	/// </summary>
	public abstract class XmlSchemaGroupBase : XmlSchemaParticle
	{
		private XmlSchemaObjectCollection compiledItems;

		protected XmlSchemaGroupBase()
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
			XmlSchemaChoice choice = other as XmlSchemaChoice;
			if (choice == null)
				return false;
			if (this.ValidatedMaxOccurs != choice.ValidatedMaxOccurs ||
				this.ValidatedMinOccurs != choice.ValidatedMinOccurs)
				return false;
			if (this.CompiledItems.Count != choice.CompiledItems.Count)
				return false;
			for (int i = 0; i < CompiledItems.Count; i++) {
				XmlSchemaParticle p1 = this.CompiledItems [i] as XmlSchemaParticle;
				XmlSchemaParticle p2 = choice.CompiledItems [i] as XmlSchemaParticle;
				if (!p1.ParticleEquals (p2))
					return false;
			}
			return true;
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
				XmlSchemaParticle pb = baseGroup.CompiledItems [i] as XmlSchemaParticle;
				if (pb.ActualParticle == XmlSchemaParticle.Empty)
					continue;
				XmlSchemaParticle pd = null;
				while (this.CompiledItems.Count > index) {
					pd = this.CompiledItems [index] as XmlSchemaParticle;
					index++;
					if (pd.ActualParticle != XmlSchemaParticle.Empty)
						break;
				}
				if (pd != null) {
					try {
						pd.ActualParticle.ValidateDerivationByRestriction (pb.ActualParticle, h, schema);
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
			if (index != this.CompiledItems.Count)
				error (h, "Invalid particle derivation by restriction was found. Extraneous derived particle was found.");
		}
	}
}
