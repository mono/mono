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
	
	public class HtmlInputText : HtmlInputControl, IPostBackDataHandler{
		
		private static readonly object EventServerChange;
		
		public HtmlInputText(string type):base(type){}
		public HtmlInputText():base("text"){}
		
		protected void OnPreRender(EventArgs e){
			if (Events[EventServerChange] != null && !Disabled){
				ViewState.SetItemDirty("value",false);
			}
		}
		
		protected void OnServerChange(EventArgs e){
			EventHandler handler = (EventHandler) Events[EventServerChange];
			if (handler != null){
				handler.Invoke(this, e);
			}
		}
		
		protected void RenderAttributes(HtmlTextWriter writer){
			if (!String.Compare(Type, "password", true)){
				ViewState.Remove("value");
			}
			RenderAttributes(writer);
		}
		
		public bool LoadPostData(string postDataKey, NameValueCollection postCollection){
			string currentValue = Value;
			string postValue = postCollection.GetValues[];
			if (!currentValue.Equals(postValue){
				Value = postValue;
				return true;
			}
			return false;
		}
		
		public override void RaisePostDataChangedEvent(){
			OnServerChange(EventArgs.Empty);
		}
		
		public event EventHandler ServerChange{
			add{
				Events.AddHandler(EventServerChange, value);
			}
			remove{
				Events.RemoveHandler(EventServerChange, value);
			}
		}
		
		public int MaxLength{
			get{
				string currentMaxLength = (String) ViewState["maxlength"];
				if (currentMaxLength != null){
					return Int32.Parse(currentMaxLength, CultureInfo.InvariantCulture);
				}
				return -1;
			}
			set{
				Attributes["maxlength"] = MapIntegerAttributeToString(value);
			}
		}
		
		public int Size{
			get{
				string currentMaxLength = (String) ViewState["size"];
				if (currentMaxLength != null){
					return Int32.Parse(currentMaxLength, CultureInfo.InvariantCulture);
				}
				return -1;
			}
			set{
				Attributes["size"] = MapIntegerAttributeToString(value);
			}
		}
		
		public string Value{
			get{
				string attr = Attributes["value"];
				if (attr != null){
					return attr;
				}
				return "";
			}
			set{
				Attributes["value"] = MapStringAttributeToString(value);
			}
		}
		
	} // class HtmlInputText
} // namespace System.Web.UI.HtmlControls

