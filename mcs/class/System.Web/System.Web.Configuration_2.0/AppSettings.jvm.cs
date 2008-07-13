//
// System.Web.Configuration.AppSettings.jvm.cs
//
// Authors:
// 	Konstantin Triger <kostat@mainsoft.com>
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
// Copyright (C) 2008 Mainsoft corp.  (http://www.mainsoft.com)
//

using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;
using javax.servlet;

namespace System.Web.Configuration
{
	sealed class KeyValueMergedCollection : NameValueCollection
	{
		readonly NameValueCollection _wrapped;
		public KeyValueMergedCollection (HttpContext hc, NameValueCollection wrapped)
			: base (wrapped) {
			_wrapped = wrapped;

			ServletConfig config = (ServletConfig) AppDomain.CurrentDomain.GetData (vmw.common.IAppDomainConfig.SERVLET_CONFIG);
			if (config != null) {
				
				ServletContext context = config.getServletContext ();

				for (java.util.Enumeration e = context.getInitParameterNames (); e.hasMoreElements (); ) {
					string key = (string) e.nextElement ();
					Set (key, context.getInitParameter (key));
				}

				for (java.util.Enumeration e = config.getInitParameterNames (); e.hasMoreElements (); ) {
					string key = (string) e.nextElement ();
					Set (key, config.getInitParameter (key));
				}
			}
		}

		public override void Add (string name, string val) {
			Set (name, val);
		}

		public override void Remove (string name) {
			_wrapped.Remove (name);
			base.Remove (name);
		}

		public override void Clear () {
			_wrapped.Clear ();
			base.Clear ();
		}

		public override void Set (string name, string value) {
			_wrapped.Set (name, value);
			base.Set (name, value);
		}
	}
}
