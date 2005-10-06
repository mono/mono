/*	System.Web.UI.HtmlControls
*	Authors
*		Leen Toelen (toelen@hotmail.com)
*/

using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.HtmlControls{
	
	[DefaultEvent("ServerChange")]
	public class HtmlInputHidden : HtmlInputControl, IPostBackDataHandler{
		
		private static readonly object EventServerChange = new object ();
		
		public HtmlInputHidden () : base ("hidden")
		{
		}
		
		bool IPostBackDataHandler.LoadPostData (string postDataKey,
						       NameValueCollection postCollection)
		{
			string postValue = postCollection [postDataKey];
			if (postValue != null)
				Value = postValue;
			return false;
		}
		
		void IPostBackDataHandler.RaisePostDataChangedEvent ()
		{
			// don't need anything. LoadPostData always returns false.
		}
		
		protected virtual void OnServerChange (EventArgs e)
		{
			EventHandler handler = (EventHandler) Events [EventServerChange];
			if (handler != null)
                                handler (this, e);
		}
		
		protected override void OnPreRender(EventArgs e){
			if (Events[EventServerChange] != null && !Disabled)
				ViewState.SetItemDirty("value",false);
		}
		
		[WebCategory("Action")]
		[WebSysDescription("Fires when the value of the control changes.")]
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

