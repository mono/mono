// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaGroupRef.
	/// </summary>
	public class XmlSchemaGroupRef : XmlSchemaParticle
	{
		private XmlSchemaGroupBase particle;
		private XmlQualifiedName refName;
		public XmlSchemaGroupRef()
		{
		}
		[XmlIgnore]
		public XmlSchemaGroupBase Particle 
		{
			get{ return particle; }
		}
		[XmlAttribute]
		public XmlQualifiedName RefName 
		{
			get{ return  refName; } 
			set{ refName = value; }
		}
	}
}
