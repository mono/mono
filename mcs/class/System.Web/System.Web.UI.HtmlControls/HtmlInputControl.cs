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
		
		protected virtual new void RenderAttributes(HtmlTextWriter writer){
			writer.WriteAttribute("name",RenderedName);
			Attributes.Remove("name");
			base.RenderAttributes(writer);
			writer.Write(" /");
		}
		
		public string Name{
			get{
				return UniqueID;
			}
			set{}
		}
		
		protected virtual string RenderedName{
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
		
		public virtual string Value{
			get{
				string attr = Attributes["value"];
				if (attr != null){
					return attr;
				}
				return "";
			}
			set{
				Attributes["value"] = AttributeToString(value);
			}
		}
	} // class HtmlInputControl
} // namespace System.Web.UI.HtmlControls

