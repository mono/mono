//
// System.Web.Compilation.SessionStateItemCollection
//
// Authors:
//   Marek Habersack (grendello@gmail.com)
//
// (C) 2006 Marek Habersack
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
#if NET_2_0
using System.Web;
using System.Web.Configuration;
using System.Web.Util;

namespace System.Web.SessionState 
{
	public class SessionIDManager : ISessionIDManager
	{
		SessionStateSection config;
		
		public SessionIDManager ()
		{
		}

		public static int SessionIDMaxLength {
			get { return 80; }
		}

		// Todo: find use for the context parameter?
		public virtual string CreateSessionID (HttpContext context)
		{
			return SessionId.Create ();
		}

		public virtual string Decode (string id)
		{
			return HttpUtility.UrlDecode (id);
		}

		public virtual string Encode (string id)
		{
			return HttpUtility.UrlEncode (id);
		}
		
		public string GetSessionID (HttpContext context)
		{
			string ret = null;

			if (SessionStateModule.IsCookieLess (context, config)) {
				string tmp = context.Request.Headers [SessionStateModule.HeaderName];
				if (tmp != null)
					ret = Decode (tmp);
			} else {
				HttpCookie cookie = context.Request.Cookies [config.CookieName];
				if (cookie != null)
					ret = Decode (cookie.Value);
			}
			
			if (ret != null && ret.Length > SessionIDMaxLength)
				throw new HttpException ("The length of the session-identifier value retrieved from the HTTP request exceeds the SessionIDMaxLength value.");
			if (!Validate (ret))
				throw new HttpException ("Invalid session ID");
			
			return ret;
		}

		public void Initialize ()
		{
			config = WebConfigurationManager.GetSection ("system.web/sessionState") as SessionStateSection;
		}

		public bool InitializeRequest (HttpContext context, bool suppressAutoDetectRedirect, out bool supportSessionIDReissue)
		{
			// TODO: Implement AutoDetect handling
			if (config.CookieLess) {
				supportSessionIDReissue = true;
				return false;
			} else {
				supportSessionIDReissue = false;
				return false;
			}
		}

		public void RemoveSessionID (HttpContext context)
		{
			context.Response.Cookies.Remove(config.CookieName);
		}

		// TODO: add code to check whether the response has already been sent
		public void SaveSessionID (HttpContext context, string id, out bool redirected, out bool cookieAdded)
		{
			if (!Validate (id))
				throw new HttpException ("Invalid session ID");

			HttpRequest request = context.Request;
			if (!SessionStateModule.IsCookieLess (context, config)) {
				HttpCookie cookie = new HttpCookie (config.CookieName, id);
				cookie.Path = request.ApplicationPath;
				context.Response.AppendCookie (cookie);
				cookieAdded = true;
				redirected = false;
			} else {
				request.SetHeader (SessionStateModule.HeaderName, id);
				cookieAdded = false;
				redirected = true;
				UriBuilder newUri = new UriBuilder (request.Url);
				newUri.Path = UrlUtils.InsertSessionId (id, request.FilePath);
				context.Response.Redirect (newUri.Uri.PathAndQuery, false);
			}
		}

		public virtual bool Validate (string id)
		{
			return true;
		}
	}
}
#endif
