//
// System.ComponentModel.Design.DesignerActionList.cs
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
using System;
using System.Collections;

namespace System.ComponentModel.Design
{
	public class DesignerActionList
	{
		IComponent component;
		bool auto_show;
		DesignerActionItemCollection action_items;
		
		public DesignerActionList (IComponent component)
		{
			this.component = component;
			action_items = new DesignerActionItemCollection ();
		}

		public virtual bool AutoShow {
			get {
				return auto_show;
			}
			
			set {
				auto_show = value;    
			}
		}

		public IComponent Component {
			get {
				return component;
			}
		}

		public object GetService (Type serviceType)
		{
			return null;
			throw new NotImplementedException ();
		}
		
		public virtual DesignerActionItemCollection GetSortedActionItems ()
		{
			return action_items;
		}
	}
}
#endif
