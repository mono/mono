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
				throw new HttpException (404, "File '" + fileName + "' does not exist");

			if ((fi.Attributes & FileAttributes.Directory) != 0) {
				response.Redirect (request.Path + '/');
				return;
			}

			DateTime lastWT = fi.LastWriteTime;
			try {
				response.WriteFile (fileName);
				response.ContentType = MimeTypes.GetMimeType (fileName);
			} catch (Exception e) {
				throw new HttpException (401, "Forbidden");
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

