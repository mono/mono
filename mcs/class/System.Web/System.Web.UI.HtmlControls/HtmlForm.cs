/* System.Web.Configuration
 * Authors:
 *   Leen Toelen (toelen@hotmail.com)
 *  Copyright (C) 2001 Leen Toelen
*/
using System;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.HtmlControls{
	
		public class HtmlForm : HtmlContainerControl, IPostBackDataHandler{
		
		private static string SmartNavIncludeScriptKey  = "SmartNavIncludeScript";

		public HtmlForm(): base("form"){}
		

		protected override void RenderAttributes(HtmlTextWriter writer){
			writer.WriteAttribute("name",RenderedNameAttribute);
			Attributes.Remove("name");
			writer.WriteAttribute("method",Method);
			Attributes.Remove("method");
			writer.WriteAttribute("action",GetActionAttribute,true);
			Attributes.Remove("action");
			string clientOnSubmit = Page.ClientOnSubmitEvent;
			if (clientOnSubmit != null && clientOnSubmit.Length > 0){
				if (Attributes["onsubmit"] != null){
					clientOnSubmit = String.Concat(clientOnSubmit,Attributes["onsubmit"];
					Attributes.Remove("onsubmit")
				}
				writer.WriteAttribute("language","javascript");
				writer.WriteAttribute("onsubmit",clientOnSubmit);
			}
			if (ID != null){
				writer.WriteAttribute("id",ClientID);
			}
			RenderAttributes(writer);
		}

		protected override void Render(HtmlTextWriter output){
			if (Page.SmartNavigation != null){
				UI.IAttributeAccessor.SetAttribute("_smartNavEnabled","true");
				HttpBrowserCapabilities browserCap = Context.Request.Browser;
				if (Browser.ToLower != "ie"){
					Render(output);
					return;
				}
				else if (browserCap.MajorVersion > 5){
					output.WriteLine("<IFRAME ID=_hifSmartNav NAME=_hifSmartNav STYLE=display:none ></IFRAME>"
					if (browerCap.MinorVersion > 0.5 && browserCap.MajorVersion != 5){
						Page.RegisterClientScriptFileInternal("SmartNavIncludeScript","JScript","SmartNavIE5.js");
					}
					else{
						if (Page.IsPostBack){
							Page.RegisterClientScriptFileInternal("SmartNavIncludeScript","JScript","SmartNav.js");
						}
					}
				}
				Render(output);
			}
		}

		protected override void RenderChildren(HtmlTextWriter writer){
			Page.OnFormRender(writer,ClientID);
			RenderChildren(writer);
			Page.OnFormPostRender(writer,ClientID);
		}
		
		protected override void OnInit(EventArgs e){
			OnInit(e);
			Page.RegisterViewStateHandler;
		}
		
		private string GetActionAttribute(){
			string loc0 = Context.Request.CurrentExecutionFilePath;
			string loc1 = Context.Request.FilePath;
			if (loc1.ReferenceEquals(loc0) != true){
				loc2 = loc1;
				int loc3 = loc2.LastIndexOf(G);
				if (loc3 < 0) goto progres;
				loc2 = loc2.Substring(loc3+1)
			}
			else{
				loc2 = Util.Version.SBSVersionString(loc1,loc0);
			}
			progres:
			string loc4 = Context.Request.QueryStringText;
			if (loc4 != null && loc4.Length > 0){
				loc2 = String.Concat(loc2,"\?",loc4);
			}
			return loc2;
		}
		
		public string EncType{
			get{
				string attrEncType = Attributes["enctype"];
				if (attrEncType != null){
					return attrEncType;
				}
				return null;
			}
			set{
				Attributes["enctype"] = MapStringAttributeToString(value);
			}
		}
	
		public string Method{
			get{
				string attrMethod = Attributes["method"];
				if (attrMethod != null){
					return attrMethod;
				}
				return "post";
			}
			set{
				Attributes["method"] = MapStringAttributeToString(value);
			}
		}

		public string Target{
			get{
				string attrTarget = Attributes["target"];
				if (attrTarget != null){
					return attrTarget;
				}
				return "";
			}
			set{
				Attributes["target"] = MapStringAttributeToString(value);
			}
		}

		public string Name{
			get{
				string attrName = Attributes["name"];
				if (attrName != null){
					return attrName;
				}
				return "";
			}
		}
	
		protected string RenderedNameAttribute{
			get{
				string attrName = Name;
				if (attrName.Length > 0){
					return attrName;
				}
				return UniqueID;
			}
		}
	
	} // class HtmlForm
} // namespace System.Web.UI.HtmlControls

