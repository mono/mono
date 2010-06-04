//
// System.Web.Compilation.BuildManager
//
// Authors:
//      Marek Habersack (mhabersack@novell.com)
//
// (C) 2009-2010 Novell, Inc (http://www.novell.com)
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.Compilation;

namespace System.Web.Routing
{
	public class PageRouteHandler : IRouteHandler
	{
		public bool CheckPhysicalUrlAccess { get; private set; }
		
		public string VirtualPath { get; private set; }
		
		public PageRouteHandler (string virtualPath)
			: this (virtualPath, true)
		{
		}

		public PageRouteHandler (string virtualPath, bool checkPhysicalUrlAccess)
		{
			if (String.IsNullOrEmpty (virtualPath) || !virtualPath.StartsWith ("~/"))
				throw new ArgumentException ("VirtualPath must be a non empty string starting with ~/", "virtualPath");
			
			VirtualPath = virtualPath;
			CheckPhysicalUrlAccess = checkPhysicalUrlAccess;
		}

		[MonoTODO ("Implement checking physical URL access")]
		public virtual IHttpHandler GetHttpHandler (RequestContext requestContext)
		{
			if (requestContext == null)
				throw new ArgumentNullException ("requestContext");

			string vpath = GetSubstitutedVirtualPath (requestContext);
			int idx = vpath.IndexOf ('?');
			if (idx > -1)
				vpath = vpath.Substring (0, idx);

			if (String.IsNullOrEmpty (vpath))
				return null;
			
			return BuildManager.CreateInstanceFromVirtualPath (vpath, typeof (Page)) as IHttpHandler;
		}

		public string GetSubstitutedVirtualPath (RequestContext requestContext)
		{
			if (requestContext == null)
				throw new ArgumentNullException ("requestContext");

			RouteData rd = requestContext.RouteData;
			Route route = rd != null ? rd.Route as Route: null;
			if (route == null)
				return VirtualPath;
			
			VirtualPathData vpd = new Route (VirtualPath.Substring (2), this).GetVirtualPath (requestContext, rd.Values);
			if (vpd == null)
				return VirtualPath;

			return "~/" + vpd.VirtualPath;
		}
	}
}
