//
// System.Web.UI.WebControls.QueryStringParameter
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
	public class QueryStringParameter : Parameter {
		protected QueryStringParameter (QueryStringParameter original) : base (original)
		{
			this.QueryStringField = original.QueryStringField;
			
		}
		
		public QueryStringParameter (string name, string queryStringField) : base (name)
		{
			QueryStringField = queryStringField;
		}
		
		public QueryStringParameter (string name, TypeCode type, string queryStringField) : base (name, type)
		{
			QueryStringField = queryStringField;
		}
		
		protected override Parameter Clone ()
		{
			return new QueryStringParameter (this);
		}
		
		protected override object Evaluate (Control control)
		{
			if (control == null || control.Page == null || control.Page.Request == null)
				return null;
			
			return control.Page.Request.QueryString [QueryStringField];
		}
		
		public string QueryStringField {
			get {
				string s = ViewState ["QueryStringField"] as string;
				if (s != null)
					return s;
				
				return "";
			}
			set {
				if (QueryStringField != value) {
					ViewState ["QueryStringField"] = value;
					OnParameterChanged ();
				}
			}
		}
	}
}
#endif

