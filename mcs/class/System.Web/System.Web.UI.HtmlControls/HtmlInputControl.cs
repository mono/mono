/*	System.Web.UI.HtmlControls
*	Authors
*		Leen Toelen (toelen@hotmail.com)
*/

using System;
using System.Web;
using System.Web.UI;
using System.Globalization;

namespace System.Web.UI.HtmlControls{
	
	public class HtmlInputControl : HtmlControl{
		
		public HtmlInputControl(string type):base("type"){
			Attributes["type"] = type;
		}
		
		protected override void RenderAttributes(HtmlTextWriter writer){
			writer.WriteAttribute("name",RenderedNameAttribute);
			Attributes.Remove("name");
			RenderAttributes(writer);
			writer.Write(" /");
		}
		
		public string Name{
			get{
				return UniqueID;
			}
			set{}
		}
		
		protected string RenderedNameAttribute{
			get{
				return Name;
			}
		}
		
		public string Type{
			get{
				string attr = Attributes["type"];
				if (attr != null){
					return attr;
				}
				return "";
			}
		}
		
		public string Value{
			get{
				string attr = Attributes["value"];
				if (attr != null){
					return attr;
				}
				return "";
			}
			set{
				Attributes["value"] = MapStringAttributeToString(value);
			}
		}
	} // class HtmlInputControl
} // namespace System.Web.UI.HtmlControls


