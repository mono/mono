//
// SoapAttributeAttribute.cs: 
//
// Author:
//   John Donagher (john@webmeta.com)
//
// (C) 2002 John Donagher
//

using System;
using System.Text;

namespace System.Xml.Serialization
{
	/// <summary>
	/// Summary description for SoapAttributeAttribute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field
		 | AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
	public class SoapAttributeAttribute : Attribute
	{
		private string attrName;
		private string dataType;
		private string ns;

		public SoapAttributeAttribute ()
		{
		}

		public SoapAttributeAttribute (string attrName) 
		{
			AttributeName = attrName;
		}

		public string AttributeName {
			get	{
				return attrName;
			} 
			set	{
				attrName = value;
			}
		}

		public string DataType {
			get {
				return dataType;
			} 
			set {
				dataType = value;
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
		
		internal void AddKeyHash (System.Text.StringBuilder sb)
		{
			sb.Append ("SAA ");
			KeyHelper.AddField (sb, 1, attrName);
			KeyHelper.AddField (sb, 2, dataType);
			KeyHelper.AddField (sb, 3, ns);
			sb.Append ("|");
		}
	}
}
