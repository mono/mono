// Author: Dwivedi, Ajay kumar
//            Adwiv@Yahoo.com
using System;
using System.Xml.Serialization;

namespace System.Xml.Schema
{
	/// <summary>
	/// Summary description for XmlSchemaGroup.
	/// </summary>
	public class XmlSchemaGroup : XmlSchemaAnnotated
	{
		private string name;
		private XmlSchemaGroupBase particle;

		public XmlSchemaGroup()
		{
			name = string.Empty;
		}
		[XmlAttribute]
		public string Name 
		{
			get{ return  name; } 
			set{ name = value; }
		}
		[XmlElement]
		public XmlSchemaGroupBase Particle
		{
			get{ return  particle; }
			set{ particle = value; }
		}
	}
}
