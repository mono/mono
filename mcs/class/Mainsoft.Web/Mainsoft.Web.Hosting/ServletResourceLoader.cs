using System;
using System.Collections.Generic;
using System.Text;
using vmw.@internal.io;
using javax.servlet;
using java.io;
using java.net;

namespace Mainsoft.Web.Hosting
{
	class ServletResourceLoader : IResourceLoader
	{
		ServletContext _context;

		public ServletResourceLoader (ServletContext context) {
			_context = context;
		}

		public URL getResource (String resourceName) {
			if (resourceName == null)
				throw new ArgumentNullException ("resourceName");

			if (!resourceName.StartsWith ("/"))
				resourceName = "/" + resourceName;

			return _context.getResource (resourceName);
		}

		public InputStream getResourceAsStream (String resourceName) {
			if (resourceName == null)
				throw new ArgumentNullException ("resourceName");

			if (!resourceName.StartsWith ("/"))
				resourceName = "/" + resourceName;

			return _context.getResourceAsStream (resourceName);
		}

		public java.util.Set getResourcePaths (String path) {
			if (path == null)
				throw new ArgumentNullException ("path");

			if (!path.StartsWith ("/"))
				path = "/" + path;

			return _context.getResourcePaths (path);
		}
	}

}
