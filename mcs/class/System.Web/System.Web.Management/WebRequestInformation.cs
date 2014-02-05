//
// WebRequestInformation.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Principal;

namespace System.Web.Management {
	public sealed class WebRequestInformation {
		private string requestUrl;
		private string requestPath;
		private IPrincipal principal;
		private string userHostAddress;
		private string threadAccountName;

		public string RequestUrl
		{
			get
			{
				return this.requestUrl;
			}
		}

		public string RequestPath
		{
			get
			{
				return this.requestPath;
			}
		}

		public IPrincipal Principal
		{
			get
			{
				return this.principal;
			}
		}

		public string UserHostAddress
		{
			get
			{
				return this.userHostAddress;
			}
		}

		public string ThreadAccountName
		{
			get
			{
				return this.threadAccountName;
			}
		}

		internal WebRequestInformation ()
		{
			HttpContext current = HttpContext.Current;
			HttpRequest httpRequest = null;
			if (current != null) {
				httpRequest = current.Request;
				this.principal = current.User;
			} else
				this.principal = (IPrincipal) null;
			if (httpRequest == null) {
				this.requestUrl = string.Empty;
				this.requestPath = string.Empty;
				this.userHostAddress = string.Empty;
			} else {
				this.requestUrl = httpRequest.RawUrlUnvalidated;
				this.requestPath = httpRequest.Path;
				this.userHostAddress = httpRequest.UserHostAddress;
			}
			this.threadAccountName = WindowsIdentity.GetCurrent ().Name;
		}

		[MonoTODO]
		public void FormatToString (WebEventFormatter formatter)
		{
			throw new NotImplementedException ();
		}
	}
}
