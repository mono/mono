/*	System.Web.UI.HtmlControls
*	Authors
*		Leen Toelen (toelen@hotmail.com)
*/

using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.HtmlControls{
	
	[DefaultEvent("ServerClick")]
	public class HtmlButton : HtmlContainerControl, IPostBackEventHandler{
		
		private static readonly object EventServerClick = new object ();
		
		//Checked
		public HtmlButton(): base("button"){}
		
		protected override void OnPreRender (EventArgs e)
		{
			base.OnPreRender (e);
			if (Page != null && Events [EventServerClick] != null)
				Page.RequiresPostBackScript ();
		}

		//Checked
		protected virtual void OnServerClick(EventArgs e){
			EventHandler handler;
			handler = (EventHandler) Events[EventServerClick];
			if(handler != null){
				handler.Invoke(this, e);
			}
		}
		
		protected override void RenderAttributes (HtmlTextWriter writer)
		{
			if (Page != null && Events [EventServerClick] != null) {
				string script = Page.GetPostBackClientEvent (this, String.Empty);
				AttributeCollection coll = Attributes;
				if (coll ["language"] != null)
					coll.Remove ("language");
				writer.WriteAttribute ("language", "javascript");

				string onclick;
				if ((onclick = coll ["onclick"]) != null) {
					script = onclick + " " + script;
					coll.Remove ("onclick");
				}

				writer.WriteAttribute ("onclick", script);
			}
			base.RenderAttributes(writer);
		}
		
		void System.Web.UI.IPostBackEventHandler.RaisePostBackEvent(string eventArgument){
			if (CausesValidation){
				Page.Validate();
			}
			OnServerClick(EventArgs.Empty);
		}
		
		[WebCategory("Action")]
		[WebSysDescription("Fires when the control is clicked.")]
		public event EventHandler ServerClick{
			add{
				Events.AddHandler(EventServerClick, value);
			}
			remove{
				Events.RemoveHandler(EventServerClick, value);
			}
		}
		
		[DefaultValue(true)]
		[WebCategory("Behavior")]
		public bool CausesValidation{
			get{
				object attr = ViewState["CausesValidation"];
				if (attr != null){
					return (Boolean) attr;
				}
				return true;
			}
			set{
				ViewState["CausesValidation"] = (Boolean) value;
			}
		}
		
	} // class HtmlButton
} // namespace System.Web.UI.HtmlControls

