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
		
		protected virtual void OnServerChange(EventArgs e){
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
		
		bool IPostBackDataHandler.LoadPostData (string postDataKey,
							NameValueCollection postCollection)
		{
			string postValue = postCollection [postDataKey];
			bool myBool = false;
			if (postValue != null && postValue.Equals (Value)) {
				if (!Checked) {
					Checked = true;
					myBool = true;
				}
			} else {
				if (Checked) {
					Checked = false;
					myBool = false;
				}
			}
			return myBool;
		}
		
		void IPostBackDataHandler.RaisePostDataChangedEvent ()
		{
			OnServerChange (EventArgs.Empty);
		}
		
		[WebCategory("Action")]
		[WebSysDescription("Fires when the checked state of the control changes.")]
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
		
		internal override string RenderedName{
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

