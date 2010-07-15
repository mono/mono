//
// System.Web.UI.WebControls.Substitution.cs
//
// Authors:
//	Marek Habersack (mhabersack@novell.com)
//
// (C) 2008 Novell, Inc (http://www.novell.com)
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
using System.ComponentModel;
using System.Security.Permissions;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	[AspNetHostingPermissionAttribute(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[DefaultProperty ("MethodName")]
	[ParseChildren (true)]
	[PersistChildren (false)]
	[Designer ("System.Web.UI.Design.WebControls.SubstitutionDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	public class Substitution : Control
	{
		[DefaultValue ("")]
		[WebCategory ("Behavior")]
		public virtual string MethodName {
			get {
				string methodName = ViewState ["MethodName"] as string;
				if (String.IsNullOrEmpty (methodName))
					return String.Empty;

				return methodName;
			}
			
			set { ViewState ["MethodName"] = value; }
		}

		public Substitution ()
		{}

		protected override ControlCollection CreateControlCollection ()
		{
			return new EmptyControlCollection (this);
		}

		[MonoTODO ("Why override?")]
		protected internal override void OnPreRender (EventArgs e)
		{
			base.OnPreRender (e);
		}

		protected internal override void Render (HtmlTextWriter writer)
		{
			string method = MethodName;
			if (method.Length == 0)
				return;
			
			TemplateControl tc = TemplateControl;
			if (tc == null)
				return;
			
			HttpContext ctx = Context;
			HttpResponse resp = ctx != null ? ctx.Response : null;

			if (resp == null)
				return;

			resp.WriteSubstitution (CreateCallback (method, tc));
		}

		HttpResponseSubstitutionCallback CreateCallback (string method, TemplateControl tc)
		{
			try {
				return Delegate.CreateDelegate (typeof(HttpResponseSubstitutionCallback),
								tc.GetType (),
								method,
								true,
								true) as HttpResponseSubstitutionCallback;
			} catch (Exception ex) {
				throw new HttpException ("Cannot find static method '" + method + "' matching HttpResponseSubstitutionCallback", ex);
			}
		}
	}
}
#endif
