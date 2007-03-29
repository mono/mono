//
// System.Web.Compilation.AppResourceFilesCollection
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
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Web;
using System.Web.Caching;
using System.Web.Configuration;
using System.Web.Util;

namespace System.Web.Compilation
{
	internal class AppResourcesCompiler
	{
		public AppResourcesCompiler (string path) { }
		public Assembly Compile () { return null; }
		internal static Assembly GetCachedLocalResourcesAssembly (string path)
		{
			string localResourcesPath = UrlUtils.Combine (path, "App_LocalResources");
			return System.Web.J2EE.PageMapper.GetObjectAssembly (HttpContext.Current, localResourcesPath);
		}
	}
}
#endif
