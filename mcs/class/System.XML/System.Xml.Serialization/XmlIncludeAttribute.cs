//
// XmlIncludeAttribute.cs: 
//
// Author:
//   John Donagher (john@webmeta.com)
//
// (C) 2002 John Donagher
//

using System;

namespace System.Xml.Serialization
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Method, AllowMultiple=true)]
	public class XmlIncludeAttribute : Attribute
	{
		private Type type;

		public XmlIncludeAttribute (Type type)
		{
			Type = type;
		}

		public Type Type {
			get { 
				return type; 
			}
			set { 
				type = value; 
			}
		}
	}
}
