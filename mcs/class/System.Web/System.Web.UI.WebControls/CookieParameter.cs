//
// System.Web.UI.WebControls.CookieParameter
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
	public class CookieParameter : Parameter {

		public CookieParameter () : base ()
		{
		}

		protected CookieParameter (CookieParameter original) : base (original)
		{
			this.CookieName = original.CookieName;
		}
		
		public CookieParameter (string name, string cookieName) : base (name)
		{
			CookieName = cookieName;
		}
		
		public CookieParameter (string name, TypeCode type, string cookieName) : base (name, type)
		{
			CookieName = cookieName;
		}
		
		protected override Parameter Clone()
		{
			return new CookieParameter (this);
		}
		
		protected override object Evaluate (Control control)
		{
			if (control == null || control.Page == null || control.Page.Request == null)
				return null;
			
			HttpCookie c = control.Page.Request.Cookies [CookieName];
			if (c == null)
				return null;
			
			return c.Value;
		}
		
		public string CookieName {
			get {
				string s = ViewState ["CookieName"] as string;
				if (s != null)
					return s;
				
				return "";
			}
			set {
				if (CookieName != value) {
					ViewState ["CookieName"] = value;
					OnParameterChanged ();
				}
			}
		}
		
	
	}
}
#endif

