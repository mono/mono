//
// XmlEnumAttribute.cs: 
//
// Author:
//   John Donagher (john@webmeta.com)
//
// (C) 2002 John Donagher
//

using System;

namespace System.Xml.Serialization
{
	/// <summary>
	/// Summary description for XmlEnumAttribute.
	/// </summary>\
	[AttributeUsage(AttributeTargets.Field)]
	public class XmlEnumAttribute : Attribute
	{
		private string name;

		public XmlEnumAttribute ()
		{
		}

		public XmlEnumAttribute (string name) 
		{
			Name = name;
		}

		public string Name {
			get { 
				return name; 
			}
			set { 
				name = value; 
			}
		}

		internal void AddKeyHash (System.Text.StringBuilder sb)
		{
			sb.Append ("XENA ");
			KeyHelper.AddField (sb, 1, name);
			sb.Append ('|');
		}
	}
}
