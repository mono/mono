//
// System.Web.UI.HtmlControls.HtmlContainerControl.cs
//
// Authors:
// 	Bob Smith <bob@thestuff.net>
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) Bob Smith
// (c) 2002 Ximian, Inc. (http://www.ximian.com)
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.Text;

//LAMESPEC: The dox talk about HttpException but are very ambigious.
//TODO: Check to see if Render really is overridden instead of a LiteralControl being added. It apears that this is the
//case due to testing. Anything inside the block is overwritten by the content of this control, so it doesnt apear
//to do anything with children.

//TODO: If Test.InnerText = Test.InnerHtml without ever assigning anything into InnerHtml, you get this:
// Exception Details: System.Web.HttpException: Cannot get inner content of Message because the contents are not literal.
//[HttpException (0x80004005): Cannot get inner content of Message because the contents are not literal.]
//  System.Web.UI.HtmlControls.HtmlContainerControl.get_InnerHtml() +278
//  ASP.test3_aspx.AnchorBtn_Click(Object Source, EventArgs E) in \\genfs2\www24\bobsmith11\test3.aspx:6
//  System.Web.UI.HtmlControls.HtmlAnchor.OnServerClick(EventArgs e) +108
//  System.Web.UI.HtmlControls.HtmlAnchor.System.Web.UI.IPostBackEventHandler.RaisePostBackEvent(String eventArgument) +26
//  System.Web.UI.Page.RaisePostBackEvent(IPostBackEventHandler sourceControl, String eventArgument) +18
//  System.Web.UI.Page.RaisePostBackEvent(NameValueCollection postData) +149
//  System.Web.UI.Page.ProcessRequestMain() +660


namespace System.Web.UI.HtmlControls
{
	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public abstract class HtmlContainerControl : HtmlControl {

#if NET_2_0
		protected
#else
		public
#endif
		HtmlContainerControl () : this ("span") {}
		
		public HtmlContainerControl (string tag) : base(tag) {}

		[HtmlControlPersistable (false)]
		[BrowsableAttribute(false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public virtual string InnerHtml
		{
			get {
				if (Controls.Count == 0)
					return String.Empty;
				
				if (Controls.Count == 1) {
					Control ctrl = Controls [0];
					LiteralControl lc = ctrl as LiteralControl;
					if (lc != null)
						return lc.Text;

					DataBoundLiteralControl dblc = ctrl as DataBoundLiteralControl;
					if (dblc != null)
						return dblc.Text;
				}
				
				throw new HttpException ("There is no literal content!");
			}

			set {
				Controls.Clear ();
				Controls.Add (new LiteralControl (value));
				if (value == null)
					ViewState.Remove ("innerhtml");
				else
					ViewState ["innerhtml"] = value;
			}
		}

		[HtmlControlPersistable (false)]
		[BrowsableAttribute(false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public virtual string InnerText
		{
			get {
				return HttpUtility.HtmlDecode (InnerHtml);
			}

			set {
				InnerHtml = HttpUtility.HtmlEncode (value);
			}
		}
		
#if NET_2_0
		protected internal
#else
		protected
#endif		
		override void Render (HtmlTextWriter writer)
		{
			RenderBeginTag (writer);
			RenderChildren (writer);
			RenderEndTag (writer);
		}

		protected virtual void RenderEndTag (HtmlTextWriter writer)
		{
			writer.WriteEndTag (TagName);
		}

		protected override void RenderAttributes (HtmlTextWriter writer)
		{
			ViewState.Remove ("innerhtml");
			base.RenderAttributes (writer);
		}

		/* we need to override this because our base class
		 * (HtmlControl) returns an instance of
		 * EmptyControlCollection. */
		protected override ControlCollection CreateControlCollection ()
		{
			return new ControlCollection (this);
		}

		protected override void LoadViewState (object savedState)
		{
			if (savedState != null) {
				base.LoadViewState (savedState);
				string inner = ViewState ["innerhtml"] as string;
				if (inner != null)
					InnerHtml = inner;
			}
		}
	}
}
