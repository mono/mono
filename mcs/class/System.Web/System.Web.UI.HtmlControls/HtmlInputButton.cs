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
		
		private static readonly object EventServerChange;
		
		public HtmlInputButton(string type): base(type){}
		
		protected void OnServerClick(ImageClickEventArgs e){
			EventHandler handler = (EventHandler) Events[EventServerClick];
			if (handler != null){
				handler.Invoke(this, e);
			}
		}
		
		protected override void RenderAttributes(HtmlTextWriter writer){
			string attrType = Type;
			bool ofTypeSubmit = String.Compare(attrType, "submit", true) == false;
			bool events;
			if (ofTypeSubmit != true){
				events = (Events[EventServerClick] != null);
			}
			else{
				events = false;
			}
			if (Page != null){
				if (ofTypeSubmit != true){
					Util.WriteOnClickAttribute(
					                           writer,
					                           this,
					                           false,
					                           true,
					                           CausesValidation == false? Page.Validators.Count > 0: false);
				}
				else{
					if (events != true && String.Compare(attrType,"button", true) != null){
						Util.WriteOnClickAttribute(
						                           writer,
						                           this,
						                           false,
						                           true,
						                           CausesValidation == false? Page.Validators.Count > 0: false);
					}
				}
			}
			RenderAttributes(writer);
		}
		
		public override IPostBackEventHandler RaisePostBackEvent(string eventArgument){
			if (CausesValidation != null){
				Page.Validate();
			}
			OnServerClick(new ImageClickEventArgs(_x, _y));
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

