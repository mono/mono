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
		static HtmlButton(){
			EventServerClick = new Object();
		}
		//Checked
		public HtmlButton(): base("button"){}
		
		//Checked
		protected virtual void OnServerClick(EventArgs e){
			EventHandler handler;
			handler = (EventHandler) Events[EventServerClick];
			if(handler != null){
				handler.Invoke(this, e);
			}
		}
		
		protected override void RenderAttributes(HtmlTextWriter writer){
			if (Page != null && Events[EventServerClick] != null){
				/* Got to figure out how to manage events */
			}
			base.RenderAttributes(writer);
		}
		
		void System.Web.UI.IPostBackEventHandler.RaisePostBackEvent(string eventArgument){
			if (CausesValidation){
				Page.Validate();
			}
			OnServerClick(EventArgs.Empty);
		}
		
		public event EventHandler ServerClick{
			add{
				Events.AddHandler(EventServerClick, value);
			}
			remove{
				Events.RemoveHandler(EventServerClick, value);
			}
		}
		
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

