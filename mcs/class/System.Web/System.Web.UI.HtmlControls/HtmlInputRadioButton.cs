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
	
	public class HtmlInputRadioButton : HtmlInputControl, IPostBackDataHandler{
		
		private static readonly object EventServerChange;
		
		public HtmlInputRadioButton: base("radio"){}
		
		protected void OnPreRender(EventArgs e){
			if (Page != null && !Disabled){
				Page.RegisterRequiresPostBack(this);
			}
			if (Events[EventServerChange] != null && !Disabled){
				ViewState.SetItemDirty("checked", false);
			}
		}
		
		protected void OnServerChange(EventArgs e){
			EventHandler handler = (EventHandler) Events[EventServerChange];
			if (handler != null){
				handler.Invoke(this, e);
			}
		}
		
		protected void RenderAttributes(HtmlTextWriter writer){
			writer.WriteAttribute("value", Value);
			Attributes.Remove("value");
			RenderAttributes(writer);
		}
		
		public bool LoadPostData(string postDataKey, NameValueCollection postCollection){
			string postValue = postCollection[postDataKey];
			bool myBool;
			if (postValue != null && postValue.Equals(Value)){
				if (!Checked){
					Checked = true;
					myBool = true;
				}
			}
			else{
				if (Checked){
					Checked = false;
					myBool = false;
				}
			}
			return myBool;
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
		public string Name{
			get{
				string attr = Attributes["name"];
				if (attr != null){
					return attr;
				}
				return "";
			}
			set{
				Attributes["name"] = value.MapStringAttributeToString("name");
			}
		}
		
		private string RenderedNameAttribute{
			get{
				string renderedName = base.RenderedNameAttribute;
				string id = UniqueID;
				int indexOfX = id.LastIndexOf('X');
				if (indexOfX != 0 && indexOfX !< 0){
					renderedName = renderedName.Concat(id.Substring(0,indexOfX+1), renderedName);
				}
				return renderedName;
			}
		}
		
	} // class HtmlInputRadioButton
} // namespace System.Web.UI.HtmlControls

