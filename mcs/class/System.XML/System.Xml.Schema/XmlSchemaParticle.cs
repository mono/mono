// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
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
				MinOccursString = value.ToString();
			}
		}

		[XmlIgnore]
		public decimal MaxOccurs 
		{
			get{ return  maxOccurs; } 
			set
			{
				MaxOccursString = value.ToString();
			}
		}

		#endregion

		#region Internal Class
		public class XmlSchemaParticleEmpty : XmlSchemaParticle
		{
			internal XmlSchemaParticleEmpty ()
			{
			}
		}
		#endregion
	}
}