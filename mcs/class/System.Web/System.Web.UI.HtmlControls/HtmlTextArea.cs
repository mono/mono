/* System.Web.Configuration
 * Authors:
 *   Leen Toelen (toelen@hotmail.com)
 *  Copyright (C) 2001 Leen Toelen
*/
using System;
using System.Web;
using System.Web.UI;
using System.ComponentModel;
using System.Globalization;
using System.Collections.Specialized;

namespace System.Web.UI.HtmlControls{
	
		public class HtmlTextArea : HtmlContainerControl, IPostBackDataHandler{
		
		private static object EventServerChange = new Object();

		public HtmlTextArea(): base("textarea"){}
		
		public int Cols{
			get{
				string attr = Attributes["cols"];
				if (attr != null){
					return Int32.Parse(attr, CultureInfo.InvariantCulture);
				}
				return -1;
			}
			set{
				//MapIntegerAttributeToString(value) accessible constraint is "assembly"
				Attributes["cols"] = MapIntegerAttributeToString(value);
			}
		}
		
		public int Rows{
			get{
				string attr = Attributes["rows"];
				if (attr != null){
					return Int32.Parse(attr, CultureInfo.InvariantCulture);;
				}
				return -1;
			}
			set{
				//MapIntegerAttributeToString(value) accessible constraint is "assembly"
				Attributes["rows"] = MapIntegerAttributeToString(value);
			}
		}
		
		public string Value{
			get{
				return InnerHtml;
			}
			set{
				InnerHtml = value;
			}
		}

		protected string RenderedNameAttribute{
			get{
				return Name;
			}
		}
		
		public virtual string Name{
			get{
				if (UniqueID != null){
					return UniqueID;
				}
				return String.Empty;
			}
			set{}
		}
		
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
				handler.Invoke(this, e);
			}
		}
		
		//FIXME: not sure about the accessor
		public bool LoadPostData(string postDataKey, NameValueCollection postCollection){
			string currentValue = Value;
			string postedValue = postCollection[postDataKey];
			if (!currentValue.Equals(postedValue) && currentValue != null){
				Value = HttpUtility.HtmlEncode(postedValue);
				return true;
			}
			return false;

		}
		
		protected override void RenderAttributes(HtmlTextWriter writer){
			writer.WriteAttribute("name", RenderedNameAttribute);
			base.Attributes.Remove("name");
			base.RenderAttributes(writer);
		}
		
		//FIXME: not sure about the accessor
		public void RaisePostDataChangedEvent(){
			OnServerChange(EventArgs.Empty);
		}

		protected override void OnPreRender(EventArgs e){
			if(Events[EventServerChange]==null || Disabled==true){
				ViewState.SetItemDirty("value",false);
			}
		}

		protected override void AddParsedSubObject(object obj){
			//TODO: implement "Is Instance Of"
//			if (obj of type LiteralControl || obj of type DataBoundLiteralControl){
				AddParsedSubObject(obj);
//				return;			
//			}
			//FormatResourceString accessible constraint is "assembly"
			throw new HttpException(HttpRuntime.FormatResourceString("Cannot_Have_Children_Of_Type","HtmlTextArea",obj.GetType().Name.ToString()));
		}
	
	} // class HtmlTextArea
} // namespace System.Web.UI.HtmlControls

