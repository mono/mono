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
		private bool isFixed;
		private string val;

		protected XmlSchemaFacet()
		{
			val = string.Empty;
		}
		[DefaultValue(false)]
		[XmlAttribute]
		public virtual bool IsFixed 
		{
			get{ return  isFixed; }
			set{ isFixed = value; }
		}
		[XmlAttribute]
		public string Value 
		{
			get{ return  val; } 
			set{ val = value; }
		}
	}
}
