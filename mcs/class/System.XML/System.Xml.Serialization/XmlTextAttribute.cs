//
// XmlTextAttribute.cs: 
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
	/// Summary description for XmlTextAttribute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field
		 | AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
	public class XmlTextAttribute : Attribute
	{
		private string dataType = "";
		private Type type;

		public XmlTextAttribute ()
		{
		}

		public XmlTextAttribute (Type type)
		{
			Type = type;
		}
		
		public string DataType {
			get { 
				return dataType; 
			}
			set { 
				dataType = value; 
			}
		}
		public Type Type 
		{
			get { 
				return type; 
			}
			set { 
				type = value; 
			}
		}
		
		internal bool InternalEquals (XmlTextAttribute ob)
		{
			if (ob == null) return false;
			return (dataType == ob.dataType && type == ob.type);
		}
	}
}
