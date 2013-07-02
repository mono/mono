// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com

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
using System.Globalization;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaParticle.
	/// </summary>
	public abstract class XmlSchemaParticle : XmlSchemaAnnotated
	{
		internal static XmlSchemaParticle Empty {
			get {
				if (empty == null) {
					empty = new EmptyParticle ();
				}
				return empty;
			}
		}

		decimal minOccurs, maxOccurs;
		string  minstr, maxstr;
		static XmlSchemaParticle empty;
		decimal validatedMinOccurs = 1, validatedMaxOccurs = 1;
		internal int recursionDepth = -1;
		private decimal minEffectiveTotalRange = -1;
		internal bool parentIsGroupDefinition;

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

				decimal val = decimal.Parse (value, CultureInfo.InvariantCulture);
				if(val >= 0 && (val == Decimal.Truncate(val)))
				{
					minOccurs = val;
					minstr	 = val.ToString (CultureInfo.InvariantCulture);
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
					decimal val = decimal.Parse (value, CultureInfo.InvariantCulture);
					if(val >= 0 && (val == Decimal.Truncate(val)))
					{
						maxOccurs = val;
						maxstr = val.ToString (CultureInfo.InvariantCulture);
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
				MinOccursString = value.ToString (CultureInfo.InvariantCulture);
			}
		}

		[XmlIgnore]
		public decimal MaxOccurs 
		{
			get{ return  maxOccurs; } 
			set
			{
				if (value == decimal.MaxValue)
					MaxOccursString = "unbounded";
				else
					MaxOccursString = value.ToString (CultureInfo.InvariantCulture);
			}
		}

		internal decimal ValidatedMinOccurs
		{
			get { return validatedMinOccurs; }
		}

		internal decimal ValidatedMaxOccurs
		{
			get { return validatedMaxOccurs; }
//			set { validatedMaxOccurs = value; }
		}
		#endregion

		internal XmlSchemaParticle OptimizedParticle;

		internal virtual XmlSchemaParticle GetOptimizedParticle (bool isTop)
		{
			return null;
		}

		internal XmlSchemaParticle GetShallowClone ()
		{
			return (XmlSchemaParticle) MemberwiseClone ();
		}

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

		internal override void CopyInfo (XmlSchemaParticle obj)
		{
			base.CopyInfo (obj);
			if (MaxOccursString == "unbounded")
				obj.maxOccurs = obj.validatedMaxOccurs = decimal.MaxValue;
			else 
				obj.maxOccurs = obj.validatedMaxOccurs = this.ValidatedMaxOccurs;
			if (MaxOccurs == 0)
				obj.minOccurs = obj.validatedMinOccurs = 0;
			else
				obj.minOccurs = obj.validatedMinOccurs = this.ValidatedMinOccurs;
			if (MinOccursString != null)
				obj.MinOccursString = MinOccursString;
			if (MaxOccursString != null)
				obj.MaxOccursString = MaxOccursString;
		}

		internal virtual bool ValidateOccurenceRangeOK (XmlSchemaParticle other,
			ValidationEventHandler h, XmlSchema schema, bool raiseError)
		{
			if ((this.ValidatedMinOccurs < other.ValidatedMinOccurs) ||
				(other.ValidatedMaxOccurs != decimal.MaxValue &&
				this.ValidatedMaxOccurs > other.ValidatedMaxOccurs)) {
				if (raiseError)
					error (h, "Invalid derivation occurence range was found.");
				return false;
			}
			return true;
		}

		internal virtual decimal GetMinEffectiveTotalRange ()
		{
			return ValidatedMinOccurs;
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

		// 3.9.6 Particle Emptiable
		internal virtual bool ValidateIsEmptiable ()
		{
			return this.validatedMinOccurs == 0 || this.GetMinEffectiveTotalRange () == 0;
		}

		internal virtual bool ValidateDerivationByRestriction (XmlSchemaParticle baseParticle,
			ValidationEventHandler h, XmlSchema schema, bool raiseError)
		{
			return false;
		}

		internal virtual void ValidateUniqueParticleAttribution (
			XmlSchemaObjectTable qnames, ArrayList nsNames,
			ValidationEventHandler h, XmlSchema schema)
		{
		}

		internal virtual void ValidateUniqueTypeAttribution (XmlSchemaObjectTable labels,
			ValidationEventHandler h, XmlSchema schema)
		{
		}

		internal virtual void CheckRecursion (Stack stack, ValidationEventHandler h, XmlSchema schema)
		{
		}

		internal virtual bool ParticleEquals (XmlSchemaParticle other)
		{
			return false;
		}

		#region Internal Class
		internal class EmptyParticle : XmlSchemaParticle
		{
			internal EmptyParticle ()
			{
			}

			internal override XmlSchemaParticle GetOptimizedParticle (bool isTop)
			{
				return this;
			}

			internal override bool ParticleEquals (XmlSchemaParticle other)
			{
				return other == this || other == XmlSchemaParticle.Empty;
			}

			internal override bool ValidateDerivationByRestriction (XmlSchemaParticle baseParticle,
				ValidationEventHandler h, XmlSchema schema, bool raiseError)
			{
				return true;
			}

			internal override void CheckRecursion (Stack stack, 
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
