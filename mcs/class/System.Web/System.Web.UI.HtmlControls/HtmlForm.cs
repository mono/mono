/*	System.Web.UI.HtmlControls
*	Authors
*		Leen Toelen (toelen@hotmail.com)
*/

using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.HtmlControls{
	
	public class HtmlForm : HtmlContainerControl{
		
		private static string SmartNavIncludeScriptKey  = "SmartNavIncludeScript";
		
		public HtmlForm(): base("form"){}
				
		protected override void RenderAttributes(HtmlTextWriter writer){
			writer.WriteAttribute("name", RenderedName);
			Attributes.Remove("name");
			writer.WriteAttribute("method", Method);
			Attributes.Remove("method");
			writer.WriteAttribute("action", Action, true);
			Attributes.Remove("action");
			if (Enctype != null){
				writer.WriteAttribute ("enctype", Enctype);
				Attributes.Remove ("enctype");
			}
				

//			string clientOnSubmit = Page.ClientOnSubmitEvent;
// 			if (clientOnSubmit != null && clientOnSubmit.Length > 0){
// 				if (Attributes["onsubmit"] != null){
// 					clientOnSubmit = String.Concat(clientOnSubmit,Attributes["onsubmit"]);
// 					Attributes.Remove("onsubmit");
// 				}
// 				writer.WriteAttribute("language","javascript");
// 				writer.WriteAttribute("onsubmit",clientOnSubmit);
// 			}
			if (ID == null){
				writer.WriteAttribute("id",ClientID);
			}
			base.RenderAttributes(writer);
		}
		
		protected override void Render(HtmlTextWriter output){
			if (Page.SmartNavigation == false){
				base.Render (output);
				return;
			}

			((IAttributeAccessor) this).SetAttribute("_smartNavigation","true");
			HttpBrowserCapabilities browserCap = Context.Request.Browser;
			if (browserCap.Browser.ToLower() != "ie" && browserCap.MajorVersion < 5){
				base.Render(output);
				return;
			}
			output.WriteLine("<IFRAME ID=_hifSmartNav NAME=_hifSmartNav STYLE=display:none ></IFRAME>");
				
			if (browserCap.MinorVersion < 0.5 && browserCap.MajorVersion != 5)
				Page.RegisterClientScriptFile("SmartNavIncludeScript","JScript","SmartNavIE5.js");
			else if (Page.IsPostBack) Page.RegisterClientScriptFile("SmartNavIncludeScript","JScript","SmartNav.js");
			base.Render(output);
		}
		
		protected override void RenderChildren (HtmlTextWriter writer)
		{
			Page.OnFormRender (writer,ClientID);
			base.RenderChildren (writer);
			Page.OnFormPostRender (writer,ClientID);
		}
		
		protected override void OnInit(EventArgs e){
			base.OnInit(e);
			Page.RegisterViewStateHandler();
		}
		
		internal string Action{
			get{
				string executionFilePath = Context.Request.CurrentExecutionFilePath;
				string filePath = Context.Request.FilePath;
				string attr;
				if (String.ReferenceEquals(executionFilePath, filePath) == true){
					attr = filePath;
					int lastSlash = attr.LastIndexOf('/');
					if (lastSlash >= 0)
						attr = attr.Substring(lastSlash + 1);
				}
				else{
					attr = System.Web.Util.UrlUtils.MakeRelative(filePath,executionFilePath);
				}
				string queryString = Context.Request.QueryStringRaw;
				if (queryString != null && queryString.Length > 0)
					attr = String.Concat(attr, '?', queryString);
				return attr;
			}
		}
		
		[DefaultValue("")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebCategory("Behavior")]
		public string Enctype{
			get{
				string attr = Attributes["enctype"];
				if (attr != null){
					return attr;
				}
				return null;
			}
			set{
				Attributes["enctype"] = AttributeToString(value);
			}
		}
		
		[DefaultValue("")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebCategory("Behavior")]
		public string Method{
			get{
				string attr = Attributes["method"];
				if (attr != null){
					return attr;
				}
				return "post";
			}
			set{
				Attributes["method"] = AttributeToString(value);
			}
		}
		
		[DefaultValue("")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebCategory("Behavior")]
		public string Target{
			get{
				string attr = Attributes["target"];
				if (attr != null){
					return attr;
				}
				return String.Empty;
			}
			set{
				Attributes["target"] = AttributeToString(value);
			}
		}
		
		[DefaultValue("")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebCategory("Appearance")]
		public virtual string Name{
			get{
				string attr = Attributes["name"];
				if (attr != null){
					return attr;
				}
				return String.Empty;
			}
			set {
				Attributes ["Name"] = value;
			}
		}
		
		internal string RenderedName{
			get{
				string attr = Name;
				if (attr.Length > 0){
					return attr;
				}
				return UniqueID;
			}
		}
		
	} // class HtmlForm
} // namespace System.Web.UI.HtmlControls

