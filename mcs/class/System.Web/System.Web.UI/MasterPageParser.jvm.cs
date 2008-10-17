//
// System.Web.UI.MasterPageParser
//
// Authors:
//   Lluis Sanchez Gual (lluis@novell.com)
//
// (C) 2005 Novell, Inc.
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

using System;
using System.Collections;
using System.IO;
using System.Web;
using System.Web.Compilation;
using System.Web.Util;
using System.Web.J2EE;

namespace System.Web.UI
{
	internal sealed class MasterPageParser
	{
		internal MasterPageParser (string virtualPath, string inputFile, HttpContext context)
		{
		}
		
		public static MasterPage GetCompiledMasterInstance (string virtualPath, string inputFile, HttpContext context)
		{
			string resolvedUrl;
			if (VirtualPathUtility.IsAbsolute (virtualPath))
				resolvedUrl = virtualPath;
			else if (VirtualPathUtility.IsAppRelative (virtualPath))
				resolvedUrl = System.Web.Util.UrlUtils.ResolveVirtualPathFromAppAbsolute (virtualPath);
			else
				resolvedUrl = VirtualPathUtility.Combine (VirtualPathUtility.GetDirectory (context.Request.FilePath, false), virtualPath);
			Type tmpType = PageMapper.GetObjectType (context, resolvedUrl);
			if (tmpType == null)
				throw new InvalidOperationException ("Master page '" + virtualPath + "' not found");

			Object obj = Activator.CreateInstance (tmpType);
			return (MasterPage) obj;
		}

		public static Type GetCompiledMasterType (string virtualPath, string inputFile, HttpContext context)
		{
			return null;
		}
	}
}

#endif
