//
// System.ComponentModel.Design.DesignerActionPropertyItem.cs
//
// Authors:
//      Miguel de Icaza (miguel@novell.com)
//
// Copyright 2006 Novell, Inc
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
using System.Windows.Forms;
using System.Collections;

namespace System.ComponentModel.Design
{
	public sealed class DesignerActionPropertyItem : DesignerActionItem
	{
		string member_name;
		IComponent related_component;
		
		public DesignerActionPropertyItem (string memberName, string displayName)
			: this (memberName, displayName, null)
		{
		}
		
		public DesignerActionPropertyItem (string memberName, string displayName, string category)
			: this (memberName, displayName, category, null)
		{
		}
		
		public DesignerActionPropertyItem (string memberName, string displayName, string category, string description)
			: base (displayName, category, description)
		{
			this.member_name = memberName;
		}
		
		public string MemberName {
			get {
				return member_name;
			}
		}

		public IComponent RelatedComponent {
			get {
				return related_component;
			}

			set {
				related_component = value;
			}
		}
	}
}
