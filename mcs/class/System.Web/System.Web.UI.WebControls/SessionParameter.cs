//
// System.Web.UI.WebControls.SessionParameter
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

#if NET_2_0
using System.Collections;
using System.Collections.Specialized;
using System.Data;
using System.Text;
using System.ComponentModel;

namespace System.Web.UI.WebControls {

	[DefaultPropertyAttribute ("SessionField")]
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

		public SessionParameter (string name, DbType dbType, string sessionField) : base (name, dbType)
		{
			SessionField = sessionField;
		}
		
		protected override Parameter Clone ()
		{
			return new SessionParameter (this);
		}
#if NET_4_0
		protected internal
#else
		protected
#endif
		override object Evaluate (HttpContext ctx, Control control)
		{
			if (ctx == null || ctx.Session == null)
				return null;
			
			return ctx.Session [SessionField];
		}
		
		[DefaultValueAttribute ("")]
		[WebCategoryAttribute ("Parameter")]
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


