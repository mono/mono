/*	System.Web.UI.HtmlControls
*	Authors
*		Leen Toelen (toelen@hotmail.com)
*/

using System;
using System.Collections;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.Util;

namespace System.Web.UI.HtmlControls{
	
	public class HtmlForm : HtmlContainerControl{
		
		// Unused
		//private static string SmartNavIncludeScriptKey  = "SmartNavIncludeScript";
		
		public HtmlForm(): base("form"){}
				
		protected override void RenderAttributes(HtmlTextWriter writer){
			writer.WriteAttribute("name", Name);
			Attributes.Remove("name");
			writer.WriteAttribute("method", Method);
			Attributes.Remove("method");
			writer.WriteAttribute("action", Action, true);
			Attributes.Remove("action");
			if (Enctype != null){
				writer.WriteAttribute ("enctype", Enctype);
				Attributes.Remove ("enctype");
			}

			Hashtable onSubmit = Page.ClientScript.submitStatements;
 			if (onSubmit != null && onSubmit.Count > 0){
				StringBuilder sb = new StringBuilder ();
				string prev = Attributes ["onsubmit"];
 				if (prev != null){
					sb.Append (prev);
 					Attributes.Remove ("onsubmit");
 				}

				foreach (string s in onSubmit.Values)
					sb.Append (s);

 				writer.WriteAttribute ("language", "javascript");
 				writer.WriteAttribute ("onsubmit", sb.ToString ());
 			}

			if (ID == null)
				writer.WriteAttribute ("id", ClientID);

			base.RenderAttributes (writer);
		}
		
		protected override void Render(HtmlTextWriter output){
			/*** Disabled smart navigation. We have no scripts - Gonzalo
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
			*/
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
		
		internal string Action {
			get{
				string executionFilePath = Context.Request.CurrentExecutionFilePath;
				string filePath = Context.Request.FilePath;
				string attr;
				if (executionFilePath == filePath) {
					attr = filePath;
					int lastSlash = attr.LastIndexOf('/');
					if (lastSlash >= 0)
						attr = attr.Substring(lastSlash + 1);
				} else {
					attr = UrlUtils.MakeRelative (executionFilePath, UrlUtils.GetDirectory (filePath));
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
				return "";
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
				return UniqueID;
			}
			set {
			}
		}		
	} // class HtmlForm
} // namespace System.Web.UI.HtmlControls

