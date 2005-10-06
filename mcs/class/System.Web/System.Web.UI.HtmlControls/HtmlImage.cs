/*	System.Web.UI.HtmlControls
*	Authors
*		Leen Toelen (toelen@hotmail.com)
*/

using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI;
using System.Globalization;

namespace System.Web.UI.HtmlControls{
	[ControlBuilder (typeof (HtmlControlBuilder))]
	public class HtmlImage : HtmlControl{
		
		public HtmlImage(): base("img"){}
		
		protected override void RenderAttributes(HtmlTextWriter writer){
			PreProcessRelativeReference(writer,"src");
			base.RenderAttributes(writer);
			writer.Write(" /");
		}
		
		[DefaultValue("")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebCategory("Layout")]
		public string Align{
			get{
				string attr = Attributes["align"];
				if (attr != null){
					return attr;
				}
				return String.Empty;
			}
			set{
				Attributes["align"] = AttributeToString(value);
			}
		}
		
		[DefaultValue("")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebCategory("Appearance")]
		public string Alt{
			get{
				string attr = Attributes["alt"];
				if (attr != null){
					return attr;
				}
				return String.Empty;
			}
			set{
				Attributes["alt"] = AttributeToString(value);
			}
		}
		
		[DefaultValue(0)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebCategory("Appearance")]
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
		
		[DefaultValue(100)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebCategory("Layout")]
		public int Height
		{
			get {
				string attr = Attributes ["height"];
				if (attr != null)
					return Int32.Parse (attr, CultureInfo.InvariantCulture);

				return -1;
			}

			set { Attributes["height"] = AttributeToString (value); }
		}

		[DefaultValue("")]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebCategory("Behavior")]
		public string Src{
			get{
				string attr = Attributes["src"];
				if (attr != null){
					return attr;
				}
				return String.Empty;
			}
			set{
				Attributes["src"] = AttributeToString(value);
			}
		}
		
		[DefaultValue(100)]
		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[WebCategory("Layout")]
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

