/*	System.Web.UI.HtmlControls
*	Authors
*		Leen Toelen (toelen@hotmail.com)
*/

using System;
using System.ComponentModel;
using System.Globalization;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.HtmlControls{
	
	[DefaultEvent("ServerClick")]
	public class HtmlInputButton : HtmlInputControl, IPostBackEventHandler{
		
		private static readonly object EventServerClick = new object ();
		
		public HtmlInputButton(): base ("button")
		{
		}
		
		public HtmlInputButton(string type): base(type){}
		
		protected override void OnPreRender (EventArgs e)
		{
			base.OnPreRender(e);
		}

		protected override void RenderAttributes (HtmlTextWriter writer)
		{
			base.RenderAttributes (writer);
		}

		protected virtual void OnServerClick(EventArgs e){
			EventHandler handler = (EventHandler) Events[EventServerClick];
			if (handler != null){
				handler.Invoke(this, e);
			}
		}
		
		void IPostBackEventHandler.RaisePostBackEvent (string eventArgument)
		{
			if(CausesValidation == true){
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

