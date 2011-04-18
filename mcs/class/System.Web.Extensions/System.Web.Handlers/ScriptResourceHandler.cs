//
// ScriptResourceHandler.cs
//
// Author:
//   Igor Zelmanovich <igorz@mainsoft.com>
//
// (C) 2007 Mainsoft, Inc.  http://www.mainsoft.com
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
using System.Collections.Generic;
using System.Text;

namespace System.Web.Handlers
{
	public partial class ScriptResourceHandler : IHttpHandler
	{
		protected virtual bool IsReusable {
			get { return true; }
		}

		#region IHttpHandler Members

		bool IHttpHandler.IsReusable {
			get { return IsReusable; }
		}

		void IHttpHandler.ProcessRequest (HttpContext context) {
			ProcessRequest (context);
		}

		#endregion

		// TODO: add value cache?
		static string GetScriptStringLiteral (string value)
		{
			if (String.IsNullOrEmpty (value))
				return "\"" + value + "\"";
			
			var sb = new StringBuilder ("\"");
			for (int i = 0; i < value.Length; i++) {
				char ch = value [i];
				switch (ch) {
					case '\'':
						sb.Append ("\\u0027");
						break;

					case '"':
						sb.Append ("\\\"");
						break;

					case '\\':
						sb.Append ("\\\\");
						break;

					case '\n':
						sb.Append ("\\n");
						break;

					case '\r':
						sb.Append ("\\r");
						break;

					default:
						sb.Append (ch);
						break;
				}
			}
			sb.Append ("\"");
			
			return sb.ToString ();
		}

		class ResourceKey
		{
			readonly string _resourceName;
			readonly bool _notifyScriptLoaded;

			public ResourceKey (string resourceName, bool notifyScriptLoaded) {
				_resourceName = resourceName;
				_notifyScriptLoaded = notifyScriptLoaded;
			}

			public override bool Equals (object obj) {
				if (!(obj is ResourceKey))
					return base.Equals (obj);

				ResourceKey resKey = (ResourceKey) obj;
				return resKey._resourceName == this._resourceName && resKey._notifyScriptLoaded == _notifyScriptLoaded;
			}

			public override int GetHashCode () {
				return _resourceName.GetHashCode () ^ _notifyScriptLoaded.GetHashCode ();
			}
		}
	}
}
