/*	System.Web.UI.HtmlControls
*	Authors
*		Leen Toelen (toelen@hotmail.com)
*/

using System;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.HtmlControls{
	
	public class HtmlButton : HtmlContainerControl, IPostBackDataHandler{
		
		private static readonly object EventServerChange;
		
		public HtmlButton(): base("button"){}
		
		protected virtual void OnServerClick(EventArgs e){
			EventHandler handler;
			handler = (EventHandler) Events[EventServerClick];
			if(handler != null){
				handler.Invoke(this, e);
			}
		}
		
		//FIXME: check function
		protected override void RenderAttributes(HtmlTextWriter writer){
			if (Page != null && Events[EventServerClick] != null){
				Util.WriteOnClickAttribute(
				                           writer,
				                           this,
				                           false,
				                           true,
				                           CausesValidation == false? Page.Validators.Count > 0: false);
			}
			RenderAttributes(writer);
			
		}
		
		//FIXME: not sure about the accessor
		public void RaisePostBackEvent(string eventArgument){
			if (CausesValidation = false){
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
				object causesVal = ViewState["CausesValidation"];
				if (causesVal != null){
					return (Boolean) causesVal;
				}
				return 1;
			}
			set{
				ViewState["CausesValidation"] = (Boolean) value;
			}
		}
		
	} // class HtmlButton
} // namespace System.Web.UI.HtmlControls

