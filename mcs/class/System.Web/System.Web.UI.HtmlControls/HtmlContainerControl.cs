//
// System.Web.UI.HtmlControls.HtmlContainerControl.cs
//
// Author
//   Bob Smith <bob@thestuff.net>
//
// (C) Bob Smith
//

using System;
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
		
		private string _innerHtml = String.Empty;
		private string _innerText = String.Empty;
		private bool _doText = false;
		private bool _doChildren = true;
		
		public HtmlContainerControl() : base(){}
		
		public HtmlContainerControl(string tag) : base(tag) {}
		
		public virtual string InnerHtml
		{
			get { return _innerHtml; }
			set {
				_innerHtml = value;
				_doText = false;
				_doChildren = false;
			}
		}
		
		public virtual string InnerText
		{
			get { return _innerText; }
			set {
				_innerText = value;
				_doText = true;
				_doChildren = false;
			}
		}
		
		protected override void Render(HtmlTextWriter writer)
		{
			base.Render (writer);
			if (_doChildren)
				RenderChildren(writer);
			else if (_doText)
				writer.Write (HttpUtility.HtmlEncode (_innerText));
			else
				writer.Write (_innerHtml);

			RenderEndTag (writer);
		}

		protected virtual void RenderEndTag (HtmlTextWriter writer)
		{
			writer.WriteEndTag (TagName);
		}

		protected override ControlCollection CreateControlCollection ()
		{
			return new ControlCollection (this);
		}

	}
}
