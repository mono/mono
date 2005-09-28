//
// Microsoft.Web.Services.JavaScriptConverter
//
// Author:
//   Chris Toshok (toshok@ximian.com)
//
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.Web;

namespace Microsoft.Web.Services
{
	public abstract class JavaScriptConverter
	{
		protected JavaScriptConverter ()
		{
		}

		public virtual object Deserialize (string s, Type t)
		{
			return null;
		}

		public object Deserialize (string s)
		{
			return Deserialize (s, null);
		}

		protected virtual string GetClientTypeName (Type serverType)
		{
			return null;
		}

		internal static JavaScriptConverter GetConverter (Type t)
		{
			return null;
		}

		public static string GetConverterScript (Type t)
		{
			return null;
		}

		protected virtual void Initialize ()
		{
		}

		protected void RegisterJavaScript (string javascript)
		{
		}

		public virtual string Serialize (object o)
		{
			return null;
		}

		public string JavaScript {
			get {
				return null;
			}

		}

		protected virtual Type[] SupportedTypes {
			get {
				return null;
			}
		}
	}
}

#endif
