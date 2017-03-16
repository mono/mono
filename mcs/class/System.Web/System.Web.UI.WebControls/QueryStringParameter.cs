//
// System.Web.UI.WebControls.QueryStringParameter
//
// Authors:
//	Ben Maurer (bmaurer@users.sourceforge.net)
//
// (C) 2003 Ben Maurer
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Collections;
using System.Collections.Specialized;
using System.Data;
using System.Text;
using System.ComponentModel;

namespace System.Web.UI.WebControls {

	[DefaultPropertyAttribute ("QueryStringField")]
	public class QueryStringParameter : Parameter {

		public QueryStringParameter () : base ()
		{
		}

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

		public QueryStringParameter (string name, DbType dbType, string queryStringField) : base (name, dbType)
		{
			QueryStringField = queryStringField;
		}
		
		protected override Parameter Clone ()
		{
			return new QueryStringParameter (this);
		}
		protected internal
		override object Evaluate (HttpContext context, Control control)
		{
			if (context == null || context.Request == null)
				return null;
			
			return context.Request.QueryString [QueryStringField];
		}
		
		[DefaultValueAttribute ("")]
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


