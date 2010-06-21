//
// System.Web.UI.WebControls.Content.cs
//
// Authors:
//   Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2005-2010 Novell, Inc.
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
using System.ComponentModel;

namespace System.Web.UI.WebControls
{
	[ToolboxItem (false)]
	[DesignerAttribute ("System.Web.UI.Design.WebControls.ContentDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
#if !TARGET_JVM
	[ControlBuilder(typeof(ContentBuilderInternal))]
#endif
	public class Content: Control, INamingContainer, INonBindingContainer
	{
		[ThemeableAttribute (false)]
		[DefaultValueAttribute ("")]
		[WebCategoryAttribute ("Behavior")]
		[IDReferencePropertyAttribute (typeof(ContentPlaceHolder))]
		public string ContentPlaceHolderID {
			get { return String.Empty; }
			set { throw new NotSupportedException (); }
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public new event EventHandler DataBinding {
			add { base.DataBinding += value; }
			remove { base.DataBinding -= value; }
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public new event EventHandler Disposed {
			add { base.Disposed += value; }
			remove { base.Disposed -= value; }
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public new event EventHandler Init {
			add { base.Init += value; }
			remove { base.Init -= value; }
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public new event EventHandler Load {
			add { base.Load += value; }
			remove { base.Load -= value; }
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public new event EventHandler PreRender {
			add { base.PreRender += value; }
			remove { base.PreRender -= value; }
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		public new event EventHandler Unload {
			add { base.Unload += value; ; }
			remove { base.Unload -= value; }
		}
	}
}

