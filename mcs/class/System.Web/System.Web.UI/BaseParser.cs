//
// System.Web.UI.BaseParser.cs
//
// Authors:
// 	Duncan Mak  (duncan@ximian.com)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc. (http://www.ximian.com)
//

using System.IO;
using System.Web;
using System.Web.Util;

namespace System.Web.UI
{
	public class BaseParser
	{
		private HttpContext context;
		private string baseDir;
		private string baseVDir;
		private string vPath;

		internal string MapPath (string path)
		{
			return MapPath (path, true);
		}

		internal string MapPath (string path, bool allowCrossAppMapping)
		{
			if (context == null)
				throw new HttpException ("context is null!!");

			return context.Request.MapPath (path, context.Request.ApplicationPath, allowCrossAppMapping);
		}

		internal string PhysicalPath (string path)
		{
			if (Path.DirectorySeparatorChar != '/')
				path = path.Replace ('/', '\\');
				
			return Path.Combine (BaseDir, path);
		}

		internal HttpContext Context
		{
			get {
				return context;
			}
			set {
				context = value;
			}
		}

		internal string BaseDir
		{
			get {
				if (baseDir == null)
					baseDir = MapPath (BaseVirtualDir, false);

				return baseDir;
			}
		}

		internal string BaseVirtualDir
		{
			get {
				if (baseVDir == null)
					baseVDir = UrlUtils.GetDirectory (context.Request.FilePath);

				return baseVDir;
			}
		}

		internal string CurrentVirtualPath
		{
			get {
				return vPath;
			}
		}
	}

}

