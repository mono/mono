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
		
		public HtmlAnchor(): base("a"){}
		
		protected override void OnPreRender (EventArgs e)
		{
			base.OnPreRender (e);
			if (Page != null && Events [EventServerClick] != null)
				Page.RequiresPostBackScript ();
		}

		protected virtual void OnServerClick(EventArgs e){
			EventHandler handler;
			handler = (EventHandler) Events[EventServerClick];
			if (handler != null)
                                handler (this, e);
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

		[WebCategory("Action")]
		[WebSysDescription("Fires when the control is clicked.")]
		public event EventHandler ServerClick{
			add{
				Events.AddHandler(EventServerClick, value);
			}
			remove{
				Events.RemoveHandler(EventServerClick, value);
			}
		}
		
		[DefaultValue("")]
		[WebCategory("Action")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
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
		
		[DefaultValue("")]
		[WebCategory("Navigation")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
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
		
		[DefaultValue("")]
		[WebCategory("Navigation")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
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
		
		[DefaultValue("")]
		[WebCategory("Appearance")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
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

