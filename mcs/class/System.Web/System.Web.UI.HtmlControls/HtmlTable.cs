/*	System.Web.UI.HtmlControls
*	Authors
*		Leen Toelen (toelen@hotmail.com)
*/

using System;
using System.Web;
using System.Web.UI;
using System.Globalization;

namespace System.Web.UI.HtmlControls{
	
	[ParseChildren(true, "Rows")]
	public class HtmlTable : HtmlContainerControl {
		private HtmlTableRowCollection _rows;
		
		public HtmlTable():base("table"){}
		
		protected override ControlCollection CreateControlCollection(){
			return new HtmlTableRowControlCollection(this);
		}
		
		protected override void RenderChildren(HtmlTextWriter writer){
			writer.WriteLine();
			writer.Indent = writer.Indent + 1;
			base.RenderChildren(writer);
			writer.Indent = writer.Indent - 1;
		}
		
		protected override void RenderEndTag(HtmlTextWriter writer){
			base.RenderEndTag(writer);
			writer.WriteLine();
		}
		
		public string Align {
			get{
				string attr = Attributes["align"];
				if (attr != null) return attr;
				return String.Empty;
			}
			set{
				Attributes["align"] = AttributeToString(value);
			}
		}
		
		public string BgColor {
			get{
				string attr = Attributes["bgcolor"];
				if (attr != null) return attr;
				return String.Empty;
			}
			set{
				Attributes["bgcolor"] = AttributeToString(value);
			}
		}
		
		public int Border {
			get{
				string attr = Attributes["border"];
				if (attr != null) return Int32.Parse(attr, CultureInfo.InvariantCulture);
				return -1;
			}
			set{
				Attributes["border"] = AttributeToString(value);
			}
		}
		
		public string BorderColor {
			get{
				string attr = Attributes["bordercolor"];
				if (attr != null) return attr;
				return String.Empty;
			}
			set{
				Attributes["bordercolor"] = AttributeToString(value);
			}
		}
		
		public int CellPadding {
			get{
				string attr = Attributes["cellpadding"];
				if (attr != null) return Int32.Parse(attr, CultureInfo.InvariantCulture);
				return -1;
			}
			set{
				Attributes["cellpadding"] = AttributeToString(value);
			}
		}
		
		public int CellSpacing {
			get{
				string attr = Attributes["cellspacing"];
				if (attr != null) return Int32.Parse(attr, CultureInfo.InvariantCulture);
				return -1;
			}
			set{
				Attributes["cellspacing"] = AttributeToString(value);
			}
		}
		
		public string Height {
			get{
				string attr = Attributes["height"];
				if (attr != null) return attr;
				return String.Empty;
			}
			set{
				Attributes["height"] = AttributeToString(value);
			}
		}
		
		public override string InnerHtml {
			get{
				throw new NotSupportedException("InnerHtml property not supported by HtmlTable");
			}
			set{
				throw new NotSupportedException("InnerHtml property not supported by HtmlTable");
			}
		}
		
		public override string InnerText {
			get{
				throw new NotSupportedException("InnerText property not supported by HtmlTable");
			}
			set{
				throw new NotSupportedException("InnerText property not supported by HtmlTable");
			}
		}
		
		public virtual HtmlTableRowCollection Rows {
			get{
				if (_rows == null) _rows = new HtmlTableRowCollection(this);
				return _rows;
			}
		}
		
		public string Width {
			get{
				string attr = Attributes["width"];
				if (attr != null) return attr;
				return String.Empty;
			}
			set{
				Attributes["width"] = AttributeToString(value);
			}
		}
		
		protected class HtmlTableRowControlCollection : ControlCollection {
			
			internal HtmlTableRowControlCollection(Control owner): base(owner){}
			
			public override void Add(Control child){
				if ((child as HtmlTableRow) != null){
					base.Add(child);
				}
				else{
					throw new ArgumentException("HtmlTableRow cannot have children of type " + child.GetType().Name);
				}
			}
			
			public override void AddAt(int index, Control child){
				if ((child as HtmlTableRow) != null){
					base.AddAt(index,child);
				}
				else{
					throw new ArgumentException("HtmlTableRow cannot have children of type " + child.GetType().Name);
				}
			}
		} // end of HtmlTableRowControlCollection
	}
	// end of System.Web.UI.HtmlControl
}
