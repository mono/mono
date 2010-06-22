//
// System.Web.UI.WebControls.FormParameter
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

namespace System.Web.UI.WebControls
{
	[DefaultPropertyAttribute ("FormField")]
	public class FormParameter : Parameter
	{
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

		public FormParameter (string name, DbType dbType, string formField) : base (name, dbType)
		{
			FormField = formField;
		}
		
		protected override Parameter Clone ()
		{
			return new FormParameter (this);
		}
#if NET_4_0
		protected internal
#else
		protected
#endif
		override object Evaluate (HttpContext ctx, Control control)
		{
			HttpRequest req = ctx != null ? ctx.Request : null;
			if (req == null)
				return null;
			
			return req.Form [FormField];
		}
		
		[DefaultValueAttribute ("")]
		public string FormField {
			get {
				string s = ViewState ["FormField"] as string;
				if (s != null)
					return s;
				
				return String.Empty;
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


