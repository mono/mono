//
// System.Web.StaticFileHandler
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc (http://www.ximian.com)
//

using System;
using System.IO;

namespace System.Web
{
	class StaticFileHandler : IHttpHandler
	{
		public void ProcessRequest (HttpContext context)
		{
			HttpRequest request = context.Request;
			HttpResponse response = context.Response;
			string fileName = request.PhysicalPath;
			FileInfo fi = new FileInfo (fileName);
			if (!fi.Exists)
				throw new HttpException (404, "File '" + request.FilePath + "' not found.");

			if ((fi.Attributes & FileAttributes.Directory) != 0) {
				response.Redirect (request.Path + '/');
				return;
			}

			string strHeader = request.Headers ["If-Modified-Since"];
			try {
				if (strHeader != null) {
					DateTime dtIfModifiedSince = DateTime.ParseExact (strHeader, "r", null);
					DateTime ftime = fi.LastWriteTime.ToUniversalTime ();
					if (ftime <= dtIfModifiedSince) {
						response.StatusCode = 304;
						return;
					}
				}
			} catch { } 

			try {
				DateTime lastWT = fi.LastWriteTime.ToUniversalTime ();
				response.AddHeader ("Last-Modified", lastWT.ToString ("r"));

				response.WriteFile (fileName);
				response.ContentType = MimeTypes.GetMimeType (fileName);
			} catch (Exception e) {
				throw new HttpException (403, "Forbidden.");
			}
		}

		public bool IsReusable
		{
			get {
				return true;
			}
		}
	}
}

