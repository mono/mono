//
// XmlTypeAttribute.cs: 
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
	/// Summary description for XmlTypeAttribute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct |
		AttributeTargets.Enum | AttributeTargets.Interface)]
	public class XmlTypeAttribute : Attribute
	{
		private bool includeInSchema = true;
		private string ns;
		private string typeName;

		public XmlTypeAttribute ()
		{
		}

		public XmlTypeAttribute (string typeName)
		{
			TypeName = typeName;
		}

		public bool IncludeInSchema {
			get { 
				return includeInSchema; 
			}
			set { 
				includeInSchema = value; 
			}
		}

		public string Namespace {
			get { 
				return ns; 
			} 
			set { 
				ns = value; 
			}
		}
		public string TypeName {
			get { 
				return typeName; 
			} 
			set { 
				typeName = value; 
			}
		}
		
		internal void AddKeyHash (System.Text.StringBuilder sb)
		{
			sb.Append ("XTA ");
			KeyHelper.AddField (sb, 1, ns);
			KeyHelper.AddField (sb, 2, typeName);
			KeyHelper.AddField (sb, 4, includeInSchema);
			sb.Append ('|');
		}			
	}
}
