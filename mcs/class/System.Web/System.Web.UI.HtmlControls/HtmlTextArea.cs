/*	System.Web.UI.HtmlControls
*	Authors
*		Leen Toelen (toelen@hotmail.com)
*/

using System;
using System.Web;
using System.Web.UI;
using System.ComponentModel;
using System.Globalization;
using System.Collections.Specialized;

namespace System.Web.UI.HtmlControls{
	
	[DefaultEvent("ServerChange")]
	[ValidationProperty("Value")]
	public class HtmlTextArea : HtmlContainerControl, IPostBackDataHandler{
		
		private static readonly object EventServerChange = new object ();
		
		public HtmlTextArea(): base("textarea"){}
		
		[DefaultValue("")]
		[WebCategory("Appearance")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int Cols{
			get{
				string attr = Attributes["cols"];
				if (attr != null){
					return Int32.Parse(attr, CultureInfo.InvariantCulture);
				}
				return -1;
			}
			set{
				Attributes["cols"] = AttributeToString(value);
			}
		}
		
		[DefaultValue("")]
		[WebCategory("Appearance")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public int Rows{
			get{
				string attr = Attributes["rows"];
				if (attr != null){
					return Int32.Parse(attr, CultureInfo.InvariantCulture);;
				}
				return -1;
			}
			set{
				Attributes["rows"] = AttributeToString(value);
			}
		}
		
		[DefaultValue("")]
		[WebCategory("Appearance")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string Value{
			get{
				return InnerHtml;
			}
			set{
				InnerHtml = value;
			}
		}
			
		[DefaultValue("")]
		[WebCategory("Behavior")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public virtual string Name{
			get{
				if (UniqueID != null){
					return UniqueID;
				}
				return String.Empty;
			}
			set{}
		}
		
		[WebCategory("Action")]
		[WebSysDescription("Fires when the text within the control changes.")]
		public event EventHandler ServerChange{
			add{
				Events.AddHandler(EventServerChange, value);
			}
			remove{
				Events.RemoveHandler(EventServerChange, value);
			}
		}
		
		protected virtual void OnServerChange(EventArgs e){
			EventHandler handler;
			handler = (EventHandler) Events[EventServerChange];
			if(handler != null){
				handler (this, e);
			}
		}
		
		bool IPostBackDataHandler.LoadPostData (string postDataKey,
							NameValueCollection postCollection)
		{
			string currentValue = Value;
			string postedValue = postCollection[postDataKey];
			if (!currentValue.Equals(postedValue) && currentValue != null){
				Value = HttpUtility.HtmlEncode(postedValue);
				return true;
			}
			return false;
		}
		
		protected override void RenderAttributes(HtmlTextWriter writer){
			writer.WriteAttribute("name", Name);
			Attributes.Remove("name");
			base.RenderAttributes(writer);
		}
		
		void IPostBackDataHandler.RaisePostDataChangedEvent ()
		{
			OnServerChange (EventArgs.Empty);
		}
		
		protected override void OnPreRender(EventArgs e){
			if(Events[EventServerChange]==null || Disabled){
				ViewState.SetItemDirty("value",false);
			}
		}
		
		protected override void AddParsedSubObject(object obj){
			if (obj is LiteralControl || obj is DataBoundLiteralControl)
				AddParsedSubObject(obj);
			else
				throw new NotSupportedException("HtmlTextArea cannot have children of Type " + obj.GetType().Name);
		}
		
	} // class HtmlTextArea
} // namespace System.Web.UI.HtmlControls

