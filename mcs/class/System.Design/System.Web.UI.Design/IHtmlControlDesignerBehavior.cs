using System;
using System.ComponentModel.Design;
using System.Web.UI.WebControls;

namespace System.Web.UI.Design {
	public interface IHtmlControlDesignerBehavior {
		object GetAttribute (string attribute, bool ignoreCase);
		object GetStyleAttribute (string attribute, bool designTimeOnly, bool ignoreCase);
		void RemoveAttribute (string attribute, bool ignoreCase);
		void RemoveStyleAttribute (string attribute, bool designTimeOnly, bool ignoreCase);
		void SetAttribute (string attribute, object value, bool ignoreCase);
		void SetStyleAttribute (string attribute, bool designTimeOnly, object value, bool ignoreCase);
		HtmlControlDesigner Designer { get; set; }
		object DesignTimeElement { get; }
	}
}