//
// UnvalidatedRequestValuesWrapper.cs
//
// Authors:
//	Matthias Dittrich <matthi.d@gmail.com>
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
	public class UnvalidatedRequestValuesWrapper : UnvalidatedRequestValuesBase {
		private readonly UnvalidatedRequestValues u;

		public override NameValueCollection Form
		{
			get { return this.u.Form; }
		}

		public override NameValueCollection QueryString
		{
			get { return this.u.QueryString; }
		}

		public override NameValueCollection Headers
		{
			get { return this.u.Headers; }
		}

		public override HttpCookieCollection Cookies
		{
			get { return this.u.Cookies; }
		}

		public override HttpFileCollectionBase Files
		{
			get { return (HttpFileCollectionBase) new HttpFileCollectionWrapper (this.u.Files); }
		}

		public override string RawUrl
		{
			get { return this.u.RawUrl; }
		}

		public override string Path
		{
			get { return this.u.Path; }
		}

		public override string PathInfo
		{
			get { return this.u.PathInfo; }
		}

		public override string this [string field]
		{
			get { return this.u [field]; }
		}

		public override Uri Url
		{
			get { return this.u.Url; }
		}

		public UnvalidatedRequestValuesWrapper (UnvalidatedRequestValues requestValues)
		{
			if (requestValues == null)
				throw new ArgumentNullException ("requestValues");
			this.u = requestValues;
		}
	}
}
