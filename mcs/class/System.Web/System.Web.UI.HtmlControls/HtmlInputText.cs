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
		
		protected override void OnPreRender(EventArgs e){
			if (Events[EventServerChange] != null && !Disabled){
				ViewState.SetItemDirty("value",false);
			}
		}
		
		protected void OnServerChange(EventArgs e){
			EventHandler handler = (EventHandler) Events[EventServerChange];
			if (handler != null) handler.Invoke(this, e);
		}
		
		protected override void RenderAttributes(HtmlTextWriter writer){
			//hide value when password box
			if (String.Compare(Type, "password") != 0){
				ViewState.Remove("value");
			}
			RenderAttributes(writer);
		}
		
		public bool LoadPostData(string postDataKey, NameValueCollection postCollection){
			string currentValue = Value;
			string[] postedValue = postCollection.GetValues(postDataKey);
			if (!currentValue.Equals(postedValue)){
				Value = postedValue[0];
				return true;
			}
			return false;
		}
		
		public void RaisePostDataChangedEvent(){
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
				string attr = (String) ViewState["maxlength"];
				if (attr != null) return Int32.Parse(attr, CultureInfo.InvariantCulture);
				return -1;
			}
			set{
				Attributes["maxlength"] = AttributeToString(value);
			}
		}
		
		public int Size{
			get{
				string attr = (String) ViewState["size"];
				if (attr != null) return Int32.Parse(attr, CultureInfo.InvariantCulture);
				return -1;
			}
			set{
				Attributes["size"] = AttributeToString(value);
			}
		}
		
		public override string Value{
			get{
				string attr = Attributes["value"];
				if (attr != null) return attr;
				return "";
			}
			set{
				Attributes["value"] = AttributeToString(value);
			}
		}
		
	} // class HtmlInputText
} // namespace System.Web.UI.HtmlControls

