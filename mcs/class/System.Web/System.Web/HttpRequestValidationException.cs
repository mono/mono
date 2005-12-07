//
// System.Web.HttpRequestValidationException
//
// Authors:
//   	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2003,2005 Novell, Inc (http://www.novell.com)
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

#if NET_1_1

using System.Security.Permissions;

namespace System.Web {

	// CAS - no InheritanceDemand here as the class is sealed
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
#if NET_2_0
	[Serializable]
#endif
	public sealed class HttpRequestValidationException : HttpException {

#if NET_2_0
		public HttpRequestValidationException ()
		{
		}

		public HttpRequestValidationException (string message) 
			: base (message)
		{
		}

		public HttpRequestValidationException (string message, Exception innerException) 
			: base (message, innerException)
		{
		}
#else
		internal HttpRequestValidationException (string msg) : base (msg)
		{
		}
#endif
		internal override string Description {
			get {
				return  "Request validation detected a potentially dangerous input value " +
					"from the client and aborted the request. This might be an attemp of " +
					"using cross-site scripting to compromise the security of your site. " +
					"You can disable request validation using the 'validateRequest=false' " +
					"attribute in your page or setting it in your machine.config or web.config " +
					"configuration files. If you disable it, you're encouraged to properly " +
					"check the input values you get from the client.<br>\r\n" +
					"You can get more information on input validation " +
					"<a href=\"http://www.cert.org/tech_tips/malicious_code_mitigation.html\">" +
					"here</a>.";
			}
		}
	}
}
#endif
