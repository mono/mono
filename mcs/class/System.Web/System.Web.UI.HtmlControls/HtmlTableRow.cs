/*	System.Web.UI.HtmlControls
*	Authors
*		Leen Toelen (toelen@hotmail.com)
*/

using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.HtmlControls{
	[ParseChildren(true, "Cells")]
	public class HtmlTableRow : HtmlContainerControl {
		private HtmlTableCellCollection _cells;
		public HtmlTableRow():base("tr"){}
		
		protected override ControlCollection CreateControlCollection(){
			return new HtmlTableCellControlCollection(this);
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
		
		[DefaultValue("")]
		[WebCategory("Layout")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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
		
		[DefaultValue("")]
		[WebCategory("Appearance")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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
		
		[DefaultValue("")]
		[WebCategory("Appearance")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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
		
		[Browsable(false)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public virtual HtmlTableCellCollection Cells {
			get{
				if (_cells == null) _cells = new HtmlTableCellCollection(this);
				return _cells;
			}
		}
		
		[DefaultValue("")]
		[WebCategory("Layout")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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
				throw new NotSupportedException("InnerHtml is not supported by HtmlTableRow");
			}
			set{
				throw new NotSupportedException("InnerHtml is not supported by HtmlTableRow");
			}
		}
		
		public override string InnerText {
			get{
				throw new NotSupportedException("InnerText is not supported by HtmlTableRow");
			}
			set{
				throw new NotSupportedException("InnerText is not supported by HtmlTableRow");
			}
		}
		
		[DefaultValue("")]
		[WebCategory("Layout")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public string VAlign {
			get{
				string attr = Attributes["valign"];
				if (attr != null) return attr;
				return String.Empty;
			}
			set{
				Attributes["valign"] = AttributeToString(value);
			}
		}
		
		
		
		protected class HtmlTableCellControlCollection : ControlCollection {
			
			internal HtmlTableCellControlCollection(Control owner): base(owner){}
			
			public override void Add(Control child){
				if (child is HtmlTableCell){
					base.Add(child);
				}
				else{
					throw new ArgumentException("HtmlTableRow cannot have children of Type " + child.GetType().Name);
				}
			}
			
			public override void AddAt(int index, Control child){
				if (child is HtmlTableCell){
					base.AddAt(index,child);
				}
				else{
					throw new ArgumentException("HtmlTableRow cannot have children of Type " + child.GetType().Name);
				}
			}
		}
	}  // end of System.Web.UI.HtmlControls.HtmlTableRow+HtmlTableCellControlCollection
	// end of System.Web.UI.HtmlControls.HtmlTableRow
}
