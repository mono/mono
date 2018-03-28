//
// System.Web.UI.WebControls.CookieParameter
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
	[DefaultPropertyAttribute ("CookieName")]
	public class CookieParameter : Parameter
	{
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

		public CookieParameter (string name, DbType dbType, string cookieName) : base (name, dbType)
		{
			CookieName = cookieName;
		}
		
		protected override Parameter Clone()
		{
			return new CookieParameter (this);
		}
		protected internal
		override object Evaluate (HttpContext context, Control control)
		{
			if (context == null || context.Request == null)
				return null;
			
			HttpCookie c = context.Request.Cookies [CookieName];
			if (c == null)
				return null;
			
			return c.Value;
		}
		
		[DefaultValueAttribute ("")]
		public string CookieName {
			get { return ViewState.GetString ("CookieName", String.Empty); }
			set {
				if (CookieName != value) {
					ViewState ["CookieName"] = value;
					OnParameterChanged ();
				}
			}
		}
		
	
	}
}



