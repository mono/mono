//
// System.Web.UI.BaseParser.cs
//
// Authors:
// 	Duncan Mak  (duncan@ximian.com)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2002 Ximian, Inc. (http://www.ximian.com)
//

using System;
using System.Collections;
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

		internal bool GetBool (Hashtable hash, string key, bool deflt)
		{
			string val = hash [key] as string;
			if (val == null)
				return deflt;

			hash.Remove (key);

			bool result;
			if (String.Compare (val, "true", true) == 0)
				result = true;
			else if (String.Compare (val, "false", true) == 0)
				result = false;
			else
				throw new HttpException ("Invalid value for " + key);

			return result;
		}

		internal string GetString (Hashtable hash, string key, string deflt)
		{
			string val = hash [key] as string;
			if (val == null)
				return deflt;

			hash.Remove (key);
			return val;
		}
		
		//
		
		internal HttpContext Context {
			get { return context; }
			set { context = value; }
		}

		internal string BaseDir {
			get {
				if (baseDir == null)
					baseDir = MapPath (BaseVirtualDir, false);

				return baseDir;
			}
		}

		internal virtual string BaseVirtualDir {
			get {
				if (baseVDir == null)
					baseVDir = UrlUtils.GetDirectory (context.Request.FilePath);

				return baseVDir;
			}
		}
	}
}

