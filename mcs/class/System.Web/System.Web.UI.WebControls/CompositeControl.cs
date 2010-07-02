//
// System.Web.UI.WebControls.CompositeControl
//
// Authors: Ben Maurer <bmaurer@novell.com>
//          Chris Toshok <toshok@novell.com>
//
// Copyright (C) 2005-2010 Novell, Inc (http://www.novell.com)
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

using System.ComponentModel;
using System.Security.Permissions;

namespace System.Web.UI.WebControls
{
	// CAS
	[AspNetHostingPermissionAttribute (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[Designer ("System.Web.UI.Design.WebControls.CompositeControlDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	public abstract class CompositeControl : WebControl, INamingContainer, ICompositeControlDesignerAccessor
	{
#if NET_4_0
		public override bool SupportsDisabledAttribute {
			get { return RenderingCompatibilityLessThan40; }
		}
#endif
		protected CompositeControl ()
		{
		}

		public override void DataBind ()
		{
			/* make sure all the child controls have been created */
			EnsureChildControls ();
			/* and then... */
			base.DataBind();
		}

		protected internal override void Render (HtmlTextWriter w)
		{
			/* make sure all the child controls have been created */
			EnsureChildControls ();
			/* and then... */
			base.Render (w);
		}

		void ICompositeControlDesignerAccessor.RecreateChildControls ()
		{
			RecreateChildControls ();
		}

		[MonoTODO("not sure exactly what this one does..")]
		protected virtual void RecreateChildControls ()
		{
			/* for now just call CreateChildControls to force
			 * the recreation of our children. */
			CreateChildControls ();
		}
	
		public override ControlCollection Controls {
			get {
				/* make sure all the child controls have been created */
				EnsureChildControls ();
				/* and then... */
				return base.Controls;
			}
		}
	}
}

