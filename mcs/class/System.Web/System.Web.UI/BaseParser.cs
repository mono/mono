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
			return context.Request.MapPath (path, baseVDir, allowCrossAppMapping);
		}

		internal string PhysicalPath (string path)
		{
			if (Path.DirectorySeparatorChar != '/')
				path = path.Replace ('/', '\\');
				
			return Path.GetFullPath (Path.Combine (baseVDir, path));
		}

		internal HttpContext Context
		{
			get {
				return context;
			}
		}

		internal string BaseDir
		{
			get {
				if (baseDir == null)
					baseDir = MapPath (baseVDir, false);

				return baseDir;
			}
		}

		internal string BaseVirtualDir
		{
			get {
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

