/*	System.Web.UI.HtmlControls
*	Authors
*		Leen Toelen (toelen@hotmail.com)
*/

using System;
using System.Web;
using System.Web.UI;
using System.Globalization;

namespace System.Web.UI.HtmlControls{
	
	public class HtmlInputButton : HtmlInputControl, IPostBackEventHandler{
		
		private static readonly object EventServerClick;
		
		public HtmlInputButton(string type): base(type){}
		
		protected void OnServerClick(EventArgs e){
			EventHandler handler = (EventHandler) Events[EventServerClick];
			if (handler != null){
				handler.Invoke(this, e);
			}
		}
		
		protected override void RenderAttributes(HtmlTextWriter writer){
			string attrType = Type;
			bool ofTypeSubmit = (String.Compare(attrType, "submit", true) == 0);
			bool events;
			if (ofTypeSubmit != true){
				events = (Events[EventServerClick] != null);
			}
			else{
				events = false;
			}
			if (Page != null){
				if (ofTypeSubmit != true){
					WriteOnClickAttribute(
					                           writer,
					                           false,
					                           true,
					                           CausesValidation == false? Page.Validators.Count > 0: false);
				}
				else{
					if (events != true && String.Compare(attrType,"button", true) != 0){
						WriteOnClickAttribute(
						                           writer,
						                           false,
						                           true,
						                           CausesValidation == false? Page.Validators.Count > 0: false);
					}
				}
			}
			base.RenderAttributes(writer);
		}
		
		public void RaisePostBackEvent(string eventArgument){
			if(CausesValidation == true){
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
				return true;
			}
			set{
				ViewState["CausesValidation"] = (Boolean) value;
			}
		}
		
	} // end of System.Web.UI.HtmlControls.HtmlInputButton
} // namespace System.Web.UI.HtmlControls

