// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Collections;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaParticle.
	/// </summary>
	public abstract class XmlSchemaParticle : XmlSchemaAnnotated
	{
		decimal minOccurs, maxOccurs;
		string  minstr, maxstr;
		static XmlSchemaParticle empty;
		decimal validatedMinOccurs, validatedMaxOccurs;
		internal int recursionDepth = -1;
		private decimal minEffectiveTotalRange = -1;
		internal bool parentIsGroupDefinition;

		internal static XmlSchemaParticle Empty {
			get {
				if (empty == null) {
					empty = new XmlSchemaParticleEmpty ();
				}
				return empty;
			}
		}

		protected XmlSchemaParticle()
		{
			minOccurs = decimal.One;
			maxOccurs = decimal.One;
		}

		#region Attributes

		[System.Xml.Serialization.XmlAttribute("minOccurs")]
		public string MinOccursString
		{
			get{ return minstr; }
			set
			{
				if (value == null) {
					minOccurs = decimal.One;
					minstr = value;
					return;
				}

				decimal val = decimal.Parse(value);
				if(val >= 0 && (val == Decimal.Truncate(val)))
				{
					minOccurs = val;
					minstr	 = val.ToString();
				}
				else
				{
					throw new XmlSchemaException
						("MinOccursString must be a non-negative number",null); 					
				}
			}
		}

		[System.Xml.Serialization.XmlAttribute("maxOccurs")]
		public string MaxOccursString
		{
			get{ return maxstr; }
			set
			{
				if(value == "unbounded")
				{
					maxstr = value;
					maxOccurs = decimal.MaxValue;
				}
				else
				{
					decimal val = decimal.Parse(value);
					if(val >= 0 && (val == Decimal.Truncate(val)))
					{
						maxOccurs = val;
						maxstr = val.ToString();
					}
					else
					{
						throw new XmlSchemaException
							("MaxOccurs must be a non-negative integer",null);
					}
					if (val == 0 && minstr == null)
						minOccurs = 0;
				}
			}
		}

		#endregion

		#region XmlIgnore

		[XmlIgnore]
		public decimal MinOccurs
		{
			get{ return  minOccurs; }
			set
			{
				MinOccursString = value.ToString ();
			}
		}

		[XmlIgnore]
		public decimal MaxOccurs 
		{
			get{ return  maxOccurs; } 
			set
			{
				MaxOccursString = value.ToString ();
			}
		}

		internal decimal ValidatedMinOccurs
		{
			get { return validatedMinOccurs; }
		}

		internal decimal ValidatedMaxOccurs
		{
			get { return validatedMaxOccurs; }
		}

		internal virtual XmlSchemaParticle ActualParticle
		{
			get { return this; }
		}
		#endregion

		internal void CompileOccurence (ValidationEventHandler h, XmlSchema schema)
		{
			if (MinOccurs > MaxOccurs && !(MaxOccurs == 0 && MinOccursString == null))
				error(h,"minOccurs must be less than or equal to maxOccurs");
			else {
				if (MaxOccursString == "unbounded")
					this.validatedMaxOccurs = decimal.MaxValue;
				else
					this.validatedMaxOccurs = maxOccurs;
				if (this.validatedMaxOccurs == 0)
					this.validatedMinOccurs = 0;
				else
					this.validatedMinOccurs = minOccurs;
			}
		}

		internal virtual void ValidateOccurenceRangeOK (XmlSchemaParticle other,
			ValidationEventHandler h, XmlSchema schema)
		{
			if ((this.ValidatedMinOccurs < other.ValidatedMinOccurs) ||
				(other.ValidatedMaxOccurs != decimal.MaxValue &&
				this.ValidatedMaxOccurs > other.ValidatedMaxOccurs))
				error (h, "Invalid derivation occurence range was found.");
		}

		internal virtual decimal GetMinEffectiveTotalRange ()
		{
			return 0;
		}

		internal decimal GetMinEffectiveTotalRangeAllAndSequence ()
		{
			if (minEffectiveTotalRange >= 0)
				return minEffectiveTotalRange;

			decimal product = 0; //this.ValidatedMinOccurs;
			XmlSchemaObjectCollection col = null;
			if (this is XmlSchemaAll)
				col = ((XmlSchemaAll) this).Items;
			else
				col = ((XmlSchemaSequence) this).Items;
			foreach (XmlSchemaParticle p in col)
				product += p.GetMinEffectiveTotalRange ();

			minEffectiveTotalRange = product;
			return product;
		}

		internal virtual bool ValidateIsEmptiable ()
		{
			return this.validatedMinOccurs == 0 || this.GetMinEffectiveTotalRange () == 0;
		}

		internal abstract void ValidateDerivationByRestriction (XmlSchemaParticle baseParticle,
			ValidationEventHandler h, XmlSchema schema);

		internal abstract void ValidateUniqueParticleAttribution (
			XmlSchemaObjectTable qnames, ArrayList nsNames,
			ValidationEventHandler h, XmlSchema schema);

		internal abstract void ValidateUniqueTypeAttribution (XmlSchemaObjectTable labels,
			ValidationEventHandler h, XmlSchema schema);

		// See http://www.thaiopensource.com/relaxng/simplify.html
		internal abstract void CheckRecursion (int depth, ValidationEventHandler h, XmlSchema schema);

		internal abstract bool ParticleEquals (XmlSchemaParticle other);

		#region Internal Class
		public class XmlSchemaParticleEmpty : XmlSchemaParticle
		{
			internal XmlSchemaParticleEmpty ()
			{
			}

			internal override bool ParticleEquals (XmlSchemaParticle other)
			{
				return other == this || other is XmlSchemaParticleEmpty;
			}


			internal override void ValidateDerivationByRestriction (XmlSchemaParticle baseParticle,
				ValidationEventHandler h, XmlSchema schema)
			{
				// TODO
			}

			internal override void CheckRecursion (int depth, 
				ValidationEventHandler h, XmlSchema schema)
			{
				// do nothing
			}

			internal override void ValidateUniqueParticleAttribution (XmlSchemaObjectTable qnames,
				ArrayList nsNames, ValidationEventHandler h, XmlSchema schema)
			{
				// do nothing
			}

			internal override void ValidateUniqueTypeAttribution (XmlSchemaObjectTable labels,
				ValidationEventHandler h, XmlSchema schema)
			{
				// do nothing
			}

		}
		#endregion
	}
}