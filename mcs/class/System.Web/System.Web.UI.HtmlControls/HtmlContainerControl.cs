//
// System.Web.UI.HtmlControls.HtmlContainerControl.cs
//
// Authors:
// 	Bob Smith <bob@thestuff.net>
// 	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) Bob Smith
// (c) 2002 Ximian, Inc. (http://www.ximian.com)
//

using System;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;

//LAMESPEC: The dox talk about HttpException but are very ambigious.
//TODO: Check to see if Render really is overridden instead of a LiteralControl being added. It apears that this is the
//case due to testing. Anything inside the block is overwritten by the content of this control, so it doesnt apear
//to do anything with children.
// a doc references this. add? protected override ControlCollection CreateControlCollection();

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
	public abstract class HtmlContainerControl : HtmlControl{
		
		public HtmlContainerControl () : this ("span") {}
		
		public HtmlContainerControl (string tag) : base(tag) {}

		[HtmlControlPersistable (false)]
		[BrowsableAttribute(false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public virtual string InnerHtml
		{
			get {
				if (Controls.Count == 0)
					return String.Empty;

				bool is_literal = true;
				StringBuilder text = new StringBuilder ();
				foreach (Control ctrl in Controls) {
					LiteralControl lc = ctrl as LiteralControl;
					if (lc == null) {
						is_literal = false;
						break;
					}
					text.Append (lc.Text);
				}
					
				if (!is_literal)
					throw new HttpException ("There is no literal content!");

				return text.ToString ();
			}

			set {
				Controls.Clear ();
				Controls.Add (new LiteralControl (value));
				ViewState ["innerhtml"] = value;
			}
		}

		[HtmlControlPersistable (false)]
		[BrowsableAttribute(false)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		public virtual string InnerText
		{
			get {
				return InnerHtml; //FIXME: decode it
			}

			set {
				InnerHtml = value; //FIXME: encode it
			}
		}
		
		protected override void Render (HtmlTextWriter writer)
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
