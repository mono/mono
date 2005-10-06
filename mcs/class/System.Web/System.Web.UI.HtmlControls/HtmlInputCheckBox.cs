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
	public class HtmlInputCheckBox : HtmlInputControl, IPostBackDataHandler{
		
		private static readonly object EventServerChange = new object ();
		
		public HtmlInputCheckBox(): base("checkbox"){}
		
		bool IPostBackDataHandler.LoadPostData (string postDataKey, 
							       NameValueCollection postCollection)
		{
			string postValue = postCollection [postDataKey];
			bool postChecked = false;
			bool retval = false;

			if (postValue != null)
				postChecked = postValue.Length > 0;

			if (Checked != postChecked){
				retval = true;
				Checked = postChecked;
			}

			return retval;
		}
		
		void IPostBackDataHandler.RaisePostDataChangedEvent ()
		{
			OnServerChange (EventArgs.Empty);
		}
		
		protected virtual void OnServerChange(EventArgs e){
			EventHandler handler = (EventHandler) Events[EventServerChange];
			if (handler != null)
                                handler (this, e);
		}
		
		protected override void OnPreRender(EventArgs e){
			if (Page != null && !Disabled)
				Page.RegisterRequiresPostBack(this);
			if (Events[EventServerChange] != null && !Disabled)
				ViewState.SetItemDirty("checkbox",false);
		}
		
		[WebCategory("Action")]
		[WebSysDescription("Fires when the checked satte of the control is changed.")]
		public event EventHandler ServerChange{
			add{
				Events.AddHandler(EventServerChange, value);
			}
			remove{
				Events.RemoveHandler(EventServerChange, value);
			}
		}
		
		[DefaultValue("")]
		[WebCategory("Misc")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool Checked{
			get{
				string attr = Attributes["checked"];
				if (attr != null)
					return attr.Equals("checked");
				return false;
			}
			set{
				Attributes["checked"] = (value == true)? "checked": null;
			}
		}
		
	} // class HtmlInputCheckBox
} // namespace System.Web.UI.HtmlControls

