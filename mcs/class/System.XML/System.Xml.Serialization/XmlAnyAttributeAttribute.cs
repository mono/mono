//
// XmlAnyAttribute.cs: 
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
	/// Summary description for XmlAnyAttributeAttribute.
	/// </summary>
	/// 
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field
		| AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
	public class XmlAnyAttributeAttribute : Attribute
	{
		
		public XmlAnyAttributeAttribute()
		{
		
		}
	}
}
