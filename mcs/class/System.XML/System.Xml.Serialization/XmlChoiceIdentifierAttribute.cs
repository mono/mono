//
// XmlChoiceIdentifierAttribute.cs: 
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
	/// Summary description for XmlChoiceIdentifierAttribute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field
		 | AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
	public class XmlChoiceIdentifierAttribute : Attribute
	{
		private string memberName;

		public XmlChoiceIdentifierAttribute ()
		{
		}
		public XmlChoiceIdentifierAttribute (string name)
		{
			MemberName = name;
		}

		public string MemberName {
			get {
				return memberName;
			} 
			set {
				memberName = value;
			}
		}
		
		internal void AddKeyHash (System.Text.StringBuilder sb)
		{
			sb.Append ("XCA ");
			KeyHelper.AddField (sb, 1, memberName);
			sb.Append ('|');
		}
	}
}
