/*	System.Web.UI.HtmlControls
*	Authors
*		Leen Toelen (toelen@hotmail.com)
*/

using System;
using System.Web;
using System.Web.UI;
using System.Globalization;
using System.Collections.Specialized;

namespace System.Web.UI.HtmlControls{
	
	public class HtmlInputHidden : HtmlInputControl, IPostBackDataHandler{
		
		private static readonly object EventServerChange;
		
		public HtmlInputHidden () : base ("hidden")
		{
		}
		
		public bool LoadPostData(string postDataKey, NameValueCollection postCollection){
			string postValue = postCollection[postDataKey];
			if (postValue != null)
				Value = postValue;
			return false;
		}
		
		public virtual void RaisePostDataChangedEvent(){
			OnServerChange(EventArgs.Empty);
		}
		
		protected void OnServerChange(EventArgs e){
			EventHandler handler = (EventHandler) Events[EventServerChange];
			if (handler != null) handler.Invoke(this, e);
		}
		
		protected override void OnPreRender(EventArgs e){
			if (Events[EventServerChange] != null && !Disabled)
				ViewState.SetItemDirty("value",false);
		}
		
		public event EventHandler ServerChange{
			add{
				Events.AddHandler(EventServerChange, value);
			}
			remove{
				Events.RemoveHandler(EventServerChange, value);
			}
		}
		
	} // class HtmlInputFile
} // namespace System.Web.UI.HtmlControls

