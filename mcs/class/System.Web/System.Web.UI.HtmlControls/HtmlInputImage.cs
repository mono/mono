/*	System.Web.UI.HtmlControls
*	Authors
*		Leen Toelen (toelen@hotmail.com)
*/

using System;
using System.Web;
using System.Web.UI;
using System.Globalization;

namespace System.Web.UI.HtmlControls{
	
	public class HtmlInputImage : HtmlInputControl, IPostBackEventHandler, IPostBackDataHandler{
		
		private static readonly object EventServerChange;
		private int _x, _y;
		
		public HtmlInputImage(): base("image"){}
		
		protected void OnPreRender(EventArgs e){
			if (Page != null && !Disabled){
				Page.RegisterRequiresPostBack(this);
			}
		}
		
		protected void OnServerClick(ImageClickEventArgs e){
			ImageClickEventHandler handler = (ImageClickEventHandler) Events[EventServerClick];
			if (handler != null){
				handler.Invoke(this, e);
			}
		}
		
		protected override void RenderAttributes(HtmlTextWriter writer){
			PreProcessRelativeReferenceAttribute(writer,"src");
			if (Page != null && !CausesValidation){
				Util.WriteOnClickAttribute(
				                           writer,
				                           this,
				                           false,
				                           true,
				                           CausesValidation == false? Page.Validators.Count > 0: false);
			}
			RenderAttributes(writer);
		}
		
		public bool LoadPostData(string postDataKey, NameValueCollection postCollection){
			string postX = postCollection[String.Concat(RenderedNameAttribute,".x")];
			string postY = postCollection[String.Concat(RenderedNameAttribute,".y")];
			if (postX != null && postY != null && postX.Length >= 0 && postY.Length >= 0){
				_x = Int32.Parse(postX, CultureInfo.InvariantCulture);
				_y = Int32.Parse(postY, CultureInfo.InvariantCulture);
				Page.RegisterRequiresRaiseEvent(this);
			}
			return false;
		}
		
		public override void RaisePostDataChangedEvent(){}
		
		public override IPostBackEventHandler RaisePostBackEvent(string eventArgument){
			if (CausesValidation != null){
				Page.Validate();
			}
			OnServerClick(new ImageClickEventArgs(_x, _y));
		}
		
		public event EventHandler ServerClick{
			add{
				Events.AddHandler(EventServerClick, value);
			}
			remove{
				Events.RemoveHandler(EventServerClick, value);
			}
		}
		
		public string Align{
			get{
				string attr = Attributes["align"];
				if (attr != null){
					return attr;
				}
				return "";
			}
			set{
				Attributes["align"] = MapStringAttributeToString(value);
			}
		}
		
		public string Alt{
			get{
				string attr = Attributes["alt"];
				if (attr != null){
					return attr;
				}
				return "";
			}
			set{
				Attributes["alt"] = MapStringAttributeToString(value);
			}
		}
		
		public int Border{
			get{
				string attr = Attributes["border"];
				if (attr != null){
					return Int32.Parse(attr,CultureInfo.InvariantCulture);
				}
				return -1;
			}
			set{
				Attributes["border"] = MapIntegerAttributeToString(value);
			}
		}
		
		public bool CausesValidation{
			get{
				object causesVal = ViewState["CausesValidation"];
				if (causesVal != null){
					return (Boolean) causesVal;
				}
				return true;
			}
			set{
				ViewState["CausesValidation"] = (Boolean) value;
			}
		}
		
		public string Src{
			get{
				string attr = Attributes["src"];
				if (attr != null){
					return attr;
				}
				return "";
			}
			set{
				Attributes["src"] = MapStringAttributeToString(value);
			}
		}
		
		public event ServerClick;
		
	} // class HtmlInputImage
} // namespace System.Web.UI.HtmlControls

