/*	System.Web.UI.HtmlControls
*	Authors
*		Leen Toelen (toelen@hotmail.com)
*/

using System;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.HtmlControls{
	
	public class HtmlAnchor : HtmlContainerControl, IPostBackEventHandler{
		
		private static readonly object EventServerClick;
		
		public HtmlAnchor(): base("a"){}
		
		protected virtual void OnServerClick(EventArgs e){
			EventHandler handler;
			handler = (EventHandler) Events[EventServerClick];
			if(handler != null) handler.Invoke(this, e);
		}
		
		protected new void RenderAttributes(HtmlTextWriter writer){
			if ( (EventHandler) Events[EventServerClick] != null){
				Attributes.Remove("href");
				RenderAttributes(writer);
				writer.WriteAttribute(Page.GetPostBackClientHyperlink(this,""),"href");
			}
			else{
				PreProcessRelativeReference(writer,"href");
				RenderAttributes(writer);
			}
		}
		
		public void RaisePostBackEvent(string eventArgument){
			OnServerClick(EventArgs.Empty);
		}
		
		public event EventHandler ServerClick{
			add{
				Events.AddHandler(EventServerClick, value);
			}
			remove{
				Events.RemoveHandler(EventServerClick, value);
			}
		}
		
		public string HRef{
			get{
				string attr = Attributes["href"];
				if (attr != null) return attr;
				return "";
			}
			set{
				Attributes["href"] = AttributeToString(value);
			}
		}
		
		public string Name{
			get{
				string attr = Attributes["name"];
				if (attr != null) return attr;
				return "";
			}
			set{
				Attributes["name"] = AttributeToString(value);
			}
		}
		
		public string Target{
			get{
				string attr = Attributes["target"];
				if (attr != null) return attr;
				return "";
			}
			set{
				Attributes["target"] = AttributeToString(value);
			}
		}
		
		public string Title{
			get{
				string attr = Attributes["title"];
				if (attr != null) return attr;
				return "";
			}
			set{
				Attributes["title"] = AttributeToString(value);
			}
		}
		
	} // class HtmlAnchor
} // namespace System.Web.UI.HtmlControls

