//
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
//
// Authors:
//	Vladimir Krasnov <vladimirk@mainsoft.com>
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

using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace System.Web.UI.WebControls
{
	public class ProfileParameter : Parameter
	{
		public ProfileParameter ()
			: base ()
		{
		}

		protected ProfileParameter (ProfileParameter original)
			: base (original)
		{
			this.PropertyName = original.PropertyName;
		}

		public ProfileParameter (string name, string propertyName)
			: base (name)
		{
			this.PropertyName = propertyName;
		}

		public ProfileParameter (string name, TypeCode type, string propertyName)
			: base (name, type)
		{
			this.PropertyName = propertyName;
		}

		public ProfileParameter (string name, DbType dbType, string propertyName)
			: base (name, dbType)
		{
			this.PropertyName = propertyName;
		}
		
		protected override Parameter Clone ()
		{
			return new ProfileParameter (this);
		}
#if NET_4_0
		protected internal
#else
		protected
#endif
		override object Evaluate (HttpContext context, Control control)
		{
			if (context == null || context.Profile == null)
				return null;

			if (string.IsNullOrEmpty (PropertyName))
				return null;

			return context.Profile [PropertyName];
		}

		public string PropertyName
		{
			get
			{
				object o = ViewState ["PropertyName"];
				return (o != null) ? (string) o : string.Empty;
			}
			set { ViewState ["PropertyName"] = value; }
		}

	}
}

#endif
