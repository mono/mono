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

		protected XmlSchemaParticle()
		{
			minOccurs = decimal.One;
			maxOccurs = decimal.One;
		}
		[XmlIgnore]
		public decimal MaxOccurs 
		{
			get{ return  maxOccurs; } 
			set
			{
				if(value >= 0 && (value == Decimal.Truncate(value)))
					maxOccurs = value;
				else
					throw new XmlSchemaException("MaxOccurs must be a non-negative number",null);
			}
		}
		[XmlAttribute]
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
					//Setting through the property
					MaxOccurs = val;
					maxstr = value;
				}
			}
		}
		[XmlIgnore]
		public decimal MinOccurs
		{
			get{ return  minOccurs; }
			set
			{
				if(value >= 0 && (value == Decimal.Truncate(value)))
					minOccurs = value;
				else
					throw new XmlSchemaException("MinOccursString must be a non-negative number",null); 					
			}
		}
		[XmlAttribute]
		public string MinOccursString
		{
			get{ return minstr; }
			set
			{
				MinOccurs = decimal.Parse(value);
				minstr = value;
			}
		}
	}
}