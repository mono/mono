//
// filename.cs: 
//
// Author:
//   John Donagher (john@webmeta.com)
//
// (C) 2002 John Donagher
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
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
		private bool isNamespaceSpecified;
#if NET_2_0
		private int order = -1;
#endif		

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
			this.ns = ns;
		}

		public string Name {
			get {
				if (elementName == null) {
					return string.Empty;
				}
				return elementName;
			}
			set { elementName = value; }
		}

		public string Namespace {
			get { return ns; }
			set {
				isNamespaceSpecified = true;
				ns = value;
			}
		}

		internal bool NamespaceSpecified {
			get { return isNamespaceSpecified; }
		}

#if NET_2_0
		[MonoTODO]
		internal bool IsNullableSpecified { get; set; }

		[MonoTODO]
		public int Order {
			get { return order; }
			set { order = value; }
		}
#endif
		
		internal void AddKeyHash (System.Text.StringBuilder sb)
		{
			sb.Append ("XAEA ");
			KeyHelper.AddField (sb, 1, ns);
			KeyHelper.AddField (sb, 2, elementName);
			sb.Append ('|');
		}
	}
}
