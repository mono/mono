// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;

namespace System.Xml.Schema
{
			
	/// <summary>
	/// Summary description for XmlSchemaFacet.
	/// </summary>
	public abstract class XmlSchemaFacet : XmlSchemaAnnotated
	{
		[Flags]
		internal protected enum Facet {
			None = 0,
			length = 1 ,
			minLength = 2,
			maxLength = 4,
			pattern = 8,
			enumeration = 16,
			whiteSpace = 32,
			maxInclusive = 64,
			maxExclusive = 128,
			minExclusive = 256,
			minInclusive = 512, 
			totalDigits = 1024,
			fractionDigits = 2048
		};
 
		internal protected const Facet AllFacets = 
		                        Facet.length | Facet.minLength |  Facet.maxLength |
					Facet.minExclusive | Facet.maxExclusive |
					Facet.minInclusive | Facet.maxInclusive |
					Facet.pattern | Facet.enumeration | Facet.whiteSpace |
					Facet.totalDigits | Facet.fractionDigits;
		
		internal abstract Facet ThisFacet { get ; }
		
		private bool isFixed;
		private string val;

		protected XmlSchemaFacet()
		{
		}
		
		[System.Xml.Serialization.XmlAttribute("value")]
		public string Value
		{
			get{ return  val; } 
			set{ val = value; }
		}

		[DefaultValue(false)]
		[System.Xml.Serialization.XmlAttribute("fixed")]
		public virtual bool IsFixed 
		{
			get{ return  isFixed; }
			set{ isFixed = value; }
		}
	}
}
