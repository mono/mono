/*	System.Web.UI.HtmlControls
*	Authors
*		Leen Toelen (toelen@hotmail.com)
*/

using System;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.HtmlControls{
	
	public class HtmlButton : HtmlContainerControl, IPostBackEventHandler{
		
		private static readonly object EventServerClick;
		
		public HtmlButton(): base("button"){}
		
		protected virtual void OnServerClick(EventArgs e){
			EventHandler handler;
			handler = (EventHandler) Events[EventServerClick];
			if(handler != null){
				handler.Invoke(this, e);
			}
		}
		
		protected new void RenderAttributes(HtmlTextWriter writer){
			if (Page != null && Events[EventServerClick] != null){
				System.Web.UI.Util.WriteOnClickAttribute(
				                           writer,
				                           this,
				                           false,
				                           true,
				                           CausesValidation == false? Page.Validators.Count > 0: false);
			}
			base.RenderAttributes(writer);
		}
		
		public void RaisePostBackEvent(string eventArgument){
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

