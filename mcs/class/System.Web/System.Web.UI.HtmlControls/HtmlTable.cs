/*	System.Web.UI.HtmlControls
*	Authors
*		Leen Toelen (toelen@hotmail.com)
*/

using System;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.HtmlControls{
	
	public class HtmlTable : HtmlContainerControl {
		private HtmlTableRowCollection _rows;

		public HtmlTable():base("table");
		
		protected override ControlCollection CreateControlCollection(){
			return new HtmlTableRowControlCollection;
		}
		
		protected override void RenderChildren(HtmlTextWriter writer){
			writer.WriteLine();
			writer.Indent = writer.Indent + 1;
			base.RenderChildren;
			writer.Indent = writer.Indent - 1;
		}
		
		protected override void RenderEndTag(HtmlTextWriter writer){
			base.RenderEndTag(writer);
			writer.WriteLine;
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
		
		public int Border {
			get{
				string attr = Attributes["border"];
				if (attr != null) return return Int32.Parse(attr, CultureInfo.InvariantCulture);
				return -1;
			}
			set{
				Attributes["border"] = MapIntegerAttributeToString(value);
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
		
		public int CellPadding {
			get{
				string attr = Attributes["cellpadding"];
				if (attr != null) return return Int32.Parse(attr, CultureInfo.InvariantCulture);
				return -1;
			}
			set{
				Attributes["cellpadding"] = MapIntegerAttributeToString(value);
			}
		}
		
		public int CellSpacing {
			get{
				string attr = Attributes["cellspacing"];
				if (attr != null) return return Int32.Parse(attr, CultureInfo.InvariantCulture);
				return -1;
			}
			set{
				Attributes["cellspacing"] = MapIntegerAttributeToString(value);
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

		public override string InnerHtml {
			get{
				throw new NotSupportedException(HttpRuntime.FormatResourceString("InnerHtml_Not_Supported", this.GetType.Name);
			}
			set{
				throw new NotSupportedException(HttpRuntime.FormatResourceString("InnerHtml_Not_Supported", this.GetType.Name);
			}
		}
		
		public override string InnerText {
			get{
				throw new NotSupportedException(HttpRuntime.FormatResourceString("InnerText_Not_Supported", this.GetType.Name);
			}
			set{
				throw new NotSupportedException(HttpRuntime.FormatResourceString("InnerText_Not_Supported", this.GetType.Name);
			}
		}

		public virtual HtmlTableRowCollection Rows {
			get{
				if (_rows == null) _rows = new HtmlTableRowCollection;
				return _rows;
			}
		}
		
		public string Height {
			get{
				string attr = Attributes["width"];
				if (attr != null) return attr;
				return "";
			}
			set{
				Attributes["width"] = MapStringAttributeToString(value);
			}
		}		
		private protected class HtmlTableRowControlCollection : ControlCollection {
			internal HtmlTableCellControlCollection(Control owner): base(owner);
			
			public override void Add(Control child){
				if (child Is HtmlTableCell){
					base.Add(child);
				}
				else{
					throw new ArgumentException(HttpRuntime.FormatResourceString("Cannot_Have_Children_Of_Type","HtmlTableRow",child.GetType.Name.ToString);
				}
			}
			
			public override void AddAt(int index, Control child){
				if (child Is HtmlTableCell){
					base.AddAt(index,child);
				}
				else{
					throw new ArgumentException(HttpRuntime.FormatResourceString("Cannot_Have_Children_Of_Type","HtmlTableRow",child.GetType.Name.ToString);
				}
			}
		} // end of HtmlTableRowControlCollection
	}
	// end of System.Web.UI.HtmlControl
}
