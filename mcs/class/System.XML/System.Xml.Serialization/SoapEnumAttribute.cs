//
// SoapEnumAttribute.cs: 
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
	/// Summary description for SoapEnumAttribute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public class SoapEnumAttribute : Attribute
	{
		private string name;

		public SoapEnumAttribute ()
		{
		}

		public SoapEnumAttribute (string name)
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
		
		internal bool InternalEquals (SoapEnumAttribute other)
		{
			return (other == null && name == other.name);
		}
	}
}
