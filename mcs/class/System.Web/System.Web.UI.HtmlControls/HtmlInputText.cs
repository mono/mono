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
	[ValidationProperty("Value")]
	public class HtmlInputText : HtmlInputControl, IPostBackDataHandler{
		
		private static readonly object EventServerChange = new object ();
		
		public HtmlInputText(string type):base(type){}
		public HtmlInputText():base("text"){}
		
		protected override void OnPreRender (EventArgs e)
		{
			if (Events [EventServerChange] == null && !Disabled)
				ViewState.SetItemDirty("value",false);
		}
		
		protected virtual void OnServerChange (EventArgs e)
		{
			EventHandler handler = (EventHandler) Events [EventServerChange];
			if (handler != null) handler (this, e);
		}
		
		protected override void RenderAttributes(HtmlTextWriter writer){
			//hide value when password box
			if (String.Compare (Type, "password",true) == 0)
				ViewState.Remove ("value");

			base.RenderAttributes(writer);
		}
		
		bool IPostBackDataHandler.LoadPostData (string postDataKey,
							NameValueCollection postCollection)
		{
			string currentValue = Value;
			string[] postedValue = postCollection.GetValues (postDataKey);
			if (!currentValue.Equals (postedValue)){
				Value = postedValue [0];
				return true;
			}
			return false;
		}
		
		void IPostBackDataHandler.RaisePostDataChangedEvent ()
		{
			OnServerChange (EventArgs.Empty);
		}
		
		[WebCategory("Action")]
		[WebSysDescription("Fires when the the text within the control changes.")]
		public event EventHandler ServerChange{
			add{
				Events.AddHandler(EventServerChange, value);
			}
			remove{
				Events.RemoveHandler(EventServerChange, value);
			}
		}
		
		[DefaultValue("")]
		[WebCategory("Behavior")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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
		
		[DefaultValue("")]
		[WebCategory("Appearance")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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
				return String.Empty;
			}
			set{
				Attributes["value"] = AttributeToString(value);
			}
		}
		
	} // class HtmlInputText
} // namespace System.Web.UI.HtmlControls

