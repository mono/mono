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
		
		internal bool InternalEquals (XmlTypeAttribute other)
		{
			if (other == null) return false;
			return (includeInSchema == other.includeInSchema && 
					typeName == other.typeName &&
				    ns != other.ns);
		}
	}
}
