/*	System.Web.UI.HtmlControls
*	Authors
*		Leen Toelen (toelen@hotmail.com)
*/

using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.HtmlControls{
	
	[DefaultEvent("ServerClick")]
	public class HtmlAnchor : HtmlContainerControl, IPostBackEventHandler{
		
		private static readonly object EventServerClick = new object ();
		
		static HtmlAnchor(){
			EventServerClick = new Object();
		}
		
		public HtmlAnchor(): base("a"){}
		
		protected override void OnPreRender (EventArgs e)
		{
			base.OnPreRender (e);
		}

		protected virtual void OnServerClick(EventArgs e){
			EventHandler handler;
			handler = (EventHandler) Events[EventServerClick];
			if(handler != null) handler.Invoke(this, e);
		}
		
		protected override void RenderAttributes(HtmlTextWriter writer){
			if ( Events[EventServerClick] != null){
				Attributes.Remove("href");
				base.RenderAttributes(writer);
				writer.WriteAttribute("href", Page.GetPostBackClientHyperlink(this,String.Empty));
			}
			else{
				PreProcessRelativeReference(writer,"href");
				base.RenderAttributes(writer);
			}
		}
		
		void System.Web.UI.IPostBackEventHandler.RaisePostBackEvent(string eventArgument){
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
				return String.Empty;
			}
			set{
				Attributes["href"] = AttributeToString(value);
			}
		}
		
		public string Name{
			get{
				string attr = Attributes["name"];
				if (attr != null) return attr;
				return String.Empty;
			}
			set{
				Attributes["name"] = AttributeToString(value);
			}
		}
		
		public string Target{
			get{
				string attr = Attributes["target"];
				if (attr != null) return attr;
				return String.Empty;
			}
			set{
				Attributes["target"] = AttributeToString(value);
			}
		}
		
		public string Title{
			get{
				string attr = Attributes["title"];
				if (attr != null) return attr;
				return String.Empty;
			}
			set{
				Attributes["title"] = AttributeToString(value);
			}
		}
		
	} // class HtmlAnchor
} // namespace System.Web.UI.HtmlControls

