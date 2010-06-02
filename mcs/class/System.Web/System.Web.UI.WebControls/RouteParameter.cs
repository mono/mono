//
// Authors:
//   Marek Habersack <mhabersack@novell.com>
//
// (C) 2010 Novell, Inc (http://novell.com/)
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
using System;
using System.ComponentModel;
using System.Data;
using System.Web.Routing;

namespace System.Web.UI.WebControls
{
	[DefaultProperty ("RouteKey")]
	public class RouteParameter : Parameter
	{
		string routeKey;
		
		[DefaultValue ("")]
		public string RouteKey {
			get { return routeKey; }
			set { routeKey = value ?? String.Empty; }
		}

		public RouteParameter ()
		{
			this.RouteKey = String.Empty;
			this.Name = String.Empty;
			this.Type = TypeCode.Empty;
			this.Direction = ParameterDirection.Input;
			this.DefaultValue = null;
		}

		protected RouteParameter (RouteParameter original)
			: base (original)
		{
			this.RouteKey = original.RouteKey;
		}

		public RouteParameter (string name, string routeKey)
			: base (name)
		{
			this.RouteKey = routeKey;
		}

		public RouteParameter (string name, DbType dbType, string routeKey)
			: base (name, dbType)
		{
			this.RouteKey = routeKey;
		}

		public RouteParameter (string name, TypeCode type, string routeKey)
			: base (name, type)
		{
			this.RouteKey = routeKey;
		}
		
		protected override Parameter Clone ()
		{
			return new RouteParameter (this);
		}

		protected internal override object Evaluate (HttpContext context, Control control)
		{
			if (context == null || control == null)
				return null;

			Page p = control.Page;
			if (p == null)
				throw new NullReferenceException (".NET emulation");

			RouteData rd = p.RouteData;
			if (rd == null)
				return null;
			
			return rd.Values [RouteKey];
		}
	}
}
