//
// System.Web.UI.WebControls.SessionParameter
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
	public class SessionParameter : Parameter {

		public SessionParameter () : base ()
		{
		}

		protected SessionParameter (SessionParameter original) : base (original)
		{
			this.SessionField = original.SessionField;
		}
		
		public SessionParameter (string name, string sessionField) : base (name)
		{
			SessionField = sessionField;
		}
		
		public SessionParameter (string name, TypeCode type, string sessionField) : base (name, type)
		{
			SessionField = sessionField;
		}
		
		protected override Parameter Clone ()
		{
			return new SessionParameter (this);
		}
		
		protected override object Evaluate (Control control)
		{
			if (control == null || control.Page == null || control.Page.Session == null)
				return null;
			
			return control.Page.Session [SessionField];
		}
		
		public string SessionField {
			get {
				string s = ViewState ["SessionField"] as string;
				if (s != null)
					return s;
				
				return "";
			}
			set {
				if (SessionField != value) {
					ViewState ["SessionField"] = value;
					OnParameterChanged ();
				}
			}
		}
	}
}
#endif

