//
// XmlRootAttribute.cs: 
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
using System.Text;

namespace System.Xml.Serialization
{
	/// <summary>
	/// Summary description for XmlRootAttribute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct |
		AttributeTargets.Enum | AttributeTargets.Interface |
		AttributeTargets.ReturnValue)]
	public class XmlRootAttribute : Attribute
	{
		private string dataType;
		private string elementName;
		private bool isNullable = true;
		private string ns;

		public XmlRootAttribute ()
		{
		}

		public XmlRootAttribute (string elementName)
		{
			this.elementName = elementName;
		}

		public string DataType {
			get {
				if (dataType == null) {
					return string.Empty;
				}
				return dataType;
			}
			set { dataType = value; }
		}

		public string ElementName {
			get {
				if (elementName == null) {
					return string.Empty;
				}
				return elementName;
			}
			set { elementName = value; }
		}

		public bool IsNullable {
			get { return isNullable; }
			set {
				isNullable = value;
			}
		}
		
		public string Namespace {
			get { return ns; } 
			set { ns = value; }
		}

		internal void AddKeyHash (System.Text.StringBuilder sb)
		{
			sb.Append ("XRA ");
			KeyHelper.AddField (sb, 1, ns);
			KeyHelper.AddField (sb, 2, elementName);
			KeyHelper.AddField (sb, 3, dataType);
			KeyHelper.AddField (sb, 4, isNullable);
			sb.Append ('|');
		}

		internal string Key {
			get {
				var sb = new StringBuilder ();
				AddKeyHash (sb);
				return sb.ToString ();
			}
		}
	}
}
