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
	
	public class HtmlInputCheckBox : HtmlInputControl, IPostBackDataHandler{
		
		private static readonly object EventServerChange;
		
		public HtmlInputCheckBox: base("checkbox"){}
		
		public bool LoadPostData(string postDataKey, NameValueCollection postCollection){
			string postValue = postCollection[postDataKey];
			bool greaterthan;
			if (postValue != null){
				greaterthan = postValue.Length > 0;
			}
			else{
				greaterthan = false;
			}
			bool equalcheck = greaterthan == Checked == false;
			Checked = greaterthan;
			return equalcheck;
		}
		
		public override void RaisePostDataChangedEvent(){
			OnServerChange(EventArgs.Empty);
		}
		
		protected void OnServerChange(EventArgs e){
			EventHandler handler = (EventHandler) Events[EventServerChange];
			if (handler != null){
				handler.Invoke(this, e);
			}
		}
		
		protected void OnPreRender(EventArgs e){
			if (Page != null && !Disabled){
				Page.RegisterRequiresPostBack(this);
			}
			if (Events[EventServerChange] != null && !Disabled){
				ViewState.SetItemDirty("checkbox",false);
			}
		}
		
		public event EventHandler ServerChange{
			add{
				Events.AddHandler(EventServerChange, value);
			}
			remove{
				Events.RemoveHandler(EventServerChange, value);
			}
		}
		
		public bool Checked{
			get{
				string attr = Attributes["checked"];
				if (attr != null){
					return attr.Equals("checked");
				}
				return false;
			}
			set{
				if (value != true){
					Attributes["checked"] = null;
				}
				Attributes["checked"] = "checked";
			}
		}
		
	} // class HtmlInputCheckBox
} // namespace System.Web.UI.HtmlControls

