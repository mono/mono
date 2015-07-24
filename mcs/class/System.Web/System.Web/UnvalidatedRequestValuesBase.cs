//
// System.Web.UnvalidatedRequestValuesBase.cs
//
// Author:
//   Mike Morano <mmorano@mikeandwan.us>
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
using System.Collections.Specialized;


namespace System.Web {
	public abstract class UnvalidatedRequestValuesBase {
		void NotImplemented ()
		{
			throw new NotImplementedException ();
		}

		public virtual HttpCookieCollection Cookies 
		{ 
			get { NotImplemented (); return null; }
		}

		public virtual HttpFileCollection Files 
		{ 
			get { NotImplemented (); return null; }
		}

		public virtual NameValueCollection Form 
		{ 
			get { NotImplemented (); return null; }
		}

		public virtual NameValueCollection Headers 
		{ 
			get { NotImplemented (); return null; }
		}

		public virtual string this[string field] 
		{ 
			get { NotImplemented (); return null; }
		}

		public virtual string Path 
		{ 
			get { NotImplemented (); return null; }
		}

		public virtual string PathInfo 
		{ 
			get { NotImplemented (); return null; }
		}

		public virtual NameValueCollection QueryString 
		{ 
			get { NotImplemented (); return null; }
		}

		public virtual string RawUrl 
		{ 
			get { NotImplemented (); return null; }
		}

		public virtual Uri Url 
		{ 
			get { NotImplemented (); return null; }
		}
	}
}
