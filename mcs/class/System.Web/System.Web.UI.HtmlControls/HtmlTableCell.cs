/*	System.Web.UI.HtmlControls
*	Authors
*		Leen Toelen (toelen@hotmail.com)
*/

using System;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.HtmlControls{
	public class HtmlTableCell : HtmlContainerControl {
		public HtmlTableCell(): base("td");
		
		public HtmlTableCell(string tagName): base(tagName);
		
		protected override void RenderEndTag(HtmlTextWriter writer){
			base.RenderEndTag(writer);
			writer.WriteLine();
		}
		
		public string Align {
			get{
				string attr = Attributes["align"];
				if (attr != null) return attr;
				return "";
			}
			set{
				Attributes["align"] = MapStringAttributeToString(value);
			}
		}
		
		public string BgColor {
			get{
				string attr = Attributes["bgcolor"];
				if (attr != null) return attr;
				return "";
			}
			set{
				Attributes["bgcolor"] = MapStringAttributeToString(value);
			}
		}
		
		public string BorderColor {
			get{
				string attr = Attributes["bordercolor"];
				if (attr != null) return attr;
				return "";
			}
			set{
				Attributes["bordercolor"] = MapStringAttributeToString(value);
			}
		}
		
		public int ColSpan {
			get{
				string attr = Attributes["colspan"];
				if (attr != null) return return Int32.Parse(attr, CultureInfo.InvariantCulture);
				return -1;
			}
			set{
				Attributes["colspan"] = MapIntegerAttributeToString(value);
			}
		}
		
		public string Height {
			get{
				string attr = Attributes["height"];
				if (attr != null) return attr;
				return "";
			}
			set{
				Attributes["height"] = MapStringAttributeToString(value);
			}
		}
		
		public bool NoWrap {
			get{
				string attr = Attributes["colspan"];
				if (attr != null) return String.Equals("nowrap");
				return false;
			}
			set{
				if (value == true){
					Attributes["nowrap"] = "nowrap";
				}
				else{
					Attributes["nowrap"] = null;
				}
			}
		}
		
		public int RowSpan {
			get{
				string attr = Attributes["rowspan"];
				if (attr != null) return return Int32.Parse(attr, CultureInfo.InvariantCulture);
				return -1;
			}
			set{
				Attributes["rowspan"] = MapIntegerAttributeToString(value);
			}
		}
		
		public string VAlign {
			get{
				string attr = Attributes["valign"];
				if (attr != null) return attr;
				return "";
			}
			set{
				Attributes["valign"] = MapStringAttributeToString(value);
			}
		}
		
		public string Width {
			get{
				string attr = Attributes["width"];
				if (attr != null) return attr;
				return "";
			}
			set{
				Attributes["width"] = MapStringAttributeToString(value);
			}
		}
		
	}
	// System.Web.UI.HtmlControls.HtmlTableCell
	
}
