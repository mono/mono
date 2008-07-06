//
// System.ComponentModel.Design.DesignerActionMethodItem.cs
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
#if NET_2_0
using System.Windows.Forms;
using System.Collections;

namespace System.ComponentModel.Design
{
	public class DesignerActionMethodItem : DesignerActionItem
	{
		string member_name;
		bool designer_verb;
		IComponent related_component;
		DesignerActionList action_list;
		
		public DesignerActionMethodItem (DesignerActionList actionList, string memberName, string displayName)
			: this (actionList, memberName, displayName, null, false)
		{
		}
		
		public DesignerActionMethodItem (DesignerActionList actionList, string memberName,
						 string displayName, bool includeAsDesignerVerb)
			: this (actionList, memberName, displayName, null, includeAsDesignerVerb)
		{
		}
		
		public DesignerActionMethodItem (DesignerActionList actionList, string memberName,
						 string displayName, string category)
			: this (actionList, memberName, displayName, category, false)
		{
		}
		
		public DesignerActionMethodItem (DesignerActionList actionList, string memberName,
						 string displayName, string category, bool includeAsDesignerVerb)
			: this (actionList, memberName, displayName, category, null, includeAsDesignerVerb)
		{
		}
		
		public DesignerActionMethodItem (DesignerActionList actionList, string memberName,
						 string displayName, string category, string description)
			: this (actionList, memberName, displayName, category, description, false)
		{
		}
		
		public DesignerActionMethodItem (DesignerActionList actionList, string memberName,
						 string displayName, string category, string description,
						 bool includeAsDesignerVerb)
			: base (displayName, category, description)
		{
			this.action_list = actionList;
			this.member_name = memberName;
			this.designer_verb = includeAsDesignerVerb;
		}
		
		public virtual bool IncludeAsDesignerVerb {
			get {
				return designer_verb;
			}
		}
		
		public virtual string MemberName {
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

		//
		// I have no clue *where* this would look at to invoke the command
		//
		public virtual void Invoke ()
		{
			throw new NotImplementedException ();
		}
	}
}
#endif
