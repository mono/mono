//
// SoapTypeAttribute.cs: 
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
	/// Summary description for SoapTypeAttribute.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct |
		 AttributeTargets.Enum | AttributeTargets.Interface)]
	public class SoapTypeAttribute : Attribute
	{
		private string ns;
		private string typeName;
		private bool includeInSchema = true;

		public SoapTypeAttribute ()
		{
		}
		public SoapTypeAttribute (string typeName)
		{
			TypeName = typeName;
		}
		public SoapTypeAttribute (string typeName, string ns)
		{
			TypeName = typeName;
			Namespace = ns;
		}
		
		public bool IncludeInSchema 
		{
			get { return  includeInSchema; }
			set { includeInSchema = value; }
		}

		public string Namespace {
			get { return ns;
			}
			set { ns = value;
			}
		}
		public string TypeName {
			get { return typeName;
			}
			set { typeName = value;
			}
		}
		
		internal void AddKeyHash (System.Text.StringBuilder sb)
		{
			sb.Append ("STA ");
			KeyHelper.AddField (sb, 1, ns);
			KeyHelper.AddField (sb, 2, typeName);
			KeyHelper.AddField (sb, 3, includeInSchema);
			sb.Append ('|');
		}
	}
}
