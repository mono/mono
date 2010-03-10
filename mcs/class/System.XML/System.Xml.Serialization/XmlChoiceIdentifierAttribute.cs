//
// XmlChoiceIdentifierAttribute.cs: 
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
using System.Reflection;

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
			memberName = name;
		}

		public string MemberName {
			get {
				if (memberName == null) {
					return string.Empty;
				}
				return memberName;
			}
			set { memberName = value; }
		}

#if NET_2_1
		MemberInfo member;
		// It is used only in 2.1 S.X.Serialization.dll in MS SDK.
		internal MemberInfo MemberInfo {
			get { return member; }
			set {
				MemberName = value != null ? value.Name : null;
				member = value;
			}
		}
#endif

		internal void AddKeyHash (System.Text.StringBuilder sb)
		{
			sb.Append ("XCA ");
			KeyHelper.AddField (sb, 1, memberName);
			sb.Append ('|');
		}
	}
}
