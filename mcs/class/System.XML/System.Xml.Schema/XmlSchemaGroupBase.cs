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
	}
}
