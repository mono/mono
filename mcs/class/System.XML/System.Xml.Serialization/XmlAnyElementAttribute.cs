//
// filename.cs: 
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
	/// Summary description for XmlAnyElementAttribute.
	/// </summary>
	/// 
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field
		| AttributeTargets.Parameter | AttributeTargets.ReturnValue, AllowMultiple=true)]
	public class XmlAnyElementAttribute : Attribute
	{
		private string elementName;
		private string ns;
		private int order;

		public XmlAnyElementAttribute ()
		{
		}

		public XmlAnyElementAttribute (string name) 
		{
			elementName = name;
		}

		public XmlAnyElementAttribute (string name, string ns)
		{
			elementName = name;
			Namespace = ns;
		}

		public string Name {
			get {
				return elementName;
			} 
			set {
				elementName = value;
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
		/// <summary>
		/// Specifies Order in which Memberswill be serialized as Elements.
		/// </summary>
		internal int Order
		{
			get{ return  order; }
			set{ order = value; }
		}
	}
}
