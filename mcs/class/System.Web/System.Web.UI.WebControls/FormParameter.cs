//
// System.Web.UI.WebControls.FormParameter
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
//

#if NET_1_2
using System.Collections;
using System.Collections.Specialized;
using System.Text;

namespace System.Web.UI.WebControls {
	public class FormParameter : Parameter {

		public FormParameter () : base ()
		{
		}

		protected FormParameter (FormParameter original) : base (original)
		{
			this.FormField = original.FormField;
		}
		
		public FormParameter (string name, string formField) : base (name)
		{
			FormField = formField;
		}
		
		public FormParameter (string name, TypeCode type, string formField) : base (name, type)
		{
			FormField = formField;
		}
		
		protected override Parameter Clone ()
		{
			return new FormParameter (this);
		}
		
		protected override object Evaluate (Control control)
		{
			if (control == null || control.Page == null || control.Page.Request == null)
				return null;
			
			return control.Page.Request.Form [FormField];
		}
		
		public string FormField {
			get {
				string s = ViewState ["FormField"] as string;
				if (s != null)
					return s;
				
				return "";
			}
			set {
				if (FormField != value) {
					ViewState ["FormField"] = value;
					OnParameterChanged ();
				}
			}
		}
	}
}
#endif

