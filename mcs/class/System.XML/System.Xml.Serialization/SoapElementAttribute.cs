//
// SoapElementAttribute.cs: 
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
	/// Summary description for SoapElementAttribute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field
		 | AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
	public class SoapElementAttribute : Attribute
	{
		private string dataType;
		private string elementName;
		private bool isNullable;

		public SoapElementAttribute ()
		{
		}
		public SoapElementAttribute (string elementName) 
		{
			ElementName = elementName;
		}

		public string DataType {
			get { 
				return dataType; 
			}
			set { 
				dataType = value; 
			}
		}

		public string ElementName {
			get { 
				return elementName; 
			}
			set { 
				elementName = value; 
			}
		}

		public bool IsNullable {
			get { 
				return isNullable; 
			} 
			set { 
				isNullable = value; 
			}
		}
		
		internal void AddKeyHash (System.Text.StringBuilder sb)
		{
			sb.Append ("SEA ");
			KeyHelper.AddField (sb, 1, elementName);
			KeyHelper.AddField (sb, 2, dataType);
			KeyHelper.AddField (sb, 3, isNullable);
			sb.Append ('|');
		}
	}
}
