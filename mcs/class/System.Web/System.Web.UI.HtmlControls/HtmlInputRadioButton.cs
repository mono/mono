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
		
		public HtmlInputRadioButton(): base("radio"){}
		
		protected override void OnPreRender(EventArgs e){
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
		
		protected override void RenderAttributes(HtmlTextWriter writer){
			writer.WriteAttribute("value", Value);
			Attributes.Remove("value");
			base.RenderAttributes(writer);
		}
		
		public bool LoadPostData(string postDataKey, NameValueCollection postCollection){
			string postValue = postCollection[postDataKey];
			bool myBool = false;
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
		public override string Name
		{
			get {
				string attr = Attributes ["name"]; // Gotta use "name" to group radio buttons
				return (attr == null) ? String.Empty : attr;
			}
			set { Attributes ["name"] = value; }
		}
		
		protected override string RenderedName{
			get{
				string attr = base.RenderedName;
				string id = UniqueID;
				int indexOfX = id.LastIndexOf('X');
				if (indexOfX != 0 && indexOfX >= 0){
					attr = String.Concat(attr, id.Substring(0,indexOfX+1));
				}
				return attr;
			}
		}

		public override string Value
		{
			get {
				string v = Attributes ["value"];
				if (v != null && v != "")
					return v;
				v = ID;
				Attributes ["value"] = v;
				return v;
			}

			set { Attributes ["value"] = value; }
		}
		
	} // class HtmlInputRadioButton
} // namespace System.Web.UI.HtmlControls

