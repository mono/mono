/*	System.Web.UI.HtmlControls
*	Authors
*		Leen Toelen (toelen@hotmail.com)
*/

using System;
using System.Web;
using System.Web.UI;
using System.Globalization;

namespace System.Web.UI.HtmlControls{
	
	public class HtmlImage : HtmlControl{
		
		public HtmlImage(): base("img"){}
		
		protected new void RenderAttributes(HtmlTextWriter writer){
			PreProcessRelativeReference(writer,"src");
			RenderAttributes(writer);
			writer.Write(" /");
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
				Attributes["align"] = AttributeToString(value);
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
				Attributes["alt"] = AttributeToString(value);
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
				Attributes["border"] = AttributeToString(value);
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
				Attributes["src"] = AttributeToString(value);
			}
		}
		
		public int Width{
			get{
				string attr = Attributes["width"];
				if (attr != null){
					return Int32.Parse(attr,CultureInfo.InvariantCulture);
				}
				return -1;
			}
			set{
				Attributes["width"] = AttributeToString(value);
			}
		}
		
	} // class HtmlImage
} // namespace System.Web.UI.HtmlControls

