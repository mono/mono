//
// System.Web.Configuration.HttpRuntimeConfig
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Novell, Inc. (http://www.novell.com)
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
#if !TARGET_J2EE
using System.CodeDom.Compiler;
#endif
using System.Collections;
using System.Configuration;
using System.IO;
using System.Web;
using System.Xml;

namespace System.Web.Configuration
{
	sealed class HttpRuntimeConfig
	{
		public int ExecutionTimeout = 90; // seconds
		public int MaxRequestLength = 4096; // KB
		public int RequestLengthDiskThreshold = 256; // KB
		public bool UseFullyQualifiedRedirectUrl = false;
		public int MinFreeThreads = 8;
		public int MinLocalRequestFreeThreads = 4;
		public int AppRequestQueueLimit = 100;
		public bool EnableKernelOutputCache = true;
		public bool EnableVersionHeader = true;
		public bool RequireRootSaveAsPath = true;
		public int IdleTimeout = 20; // minutes
		public bool Enable = true;
		public string VersionHeader;

		/* Only the config. handler should create instances of this. Use GetInstance (context) */
		public HttpRuntimeConfig (object p)
		{
			HttpRuntimeConfig parent = p as HttpRuntimeConfig;
			if (parent != null)
				Init (parent);
		}

		static public HttpRuntimeConfig GetInstance (HttpContext context)
		{
			HttpRuntimeConfig config;
			if (context == null)
				context = HttpContext.Current;

			config = context.GetConfig ("system.web/httpRuntime") as HttpRuntimeConfig;

			if (config == null)
				throw new Exception ("Configuration error.");

			return config;
		}
		
		void Init (HttpRuntimeConfig parent)
		{
			ExecutionTimeout = parent.ExecutionTimeout;
			MaxRequestLength = parent.MaxRequestLength;
			RequestLengthDiskThreshold = parent.RequestLengthDiskThreshold;
			UseFullyQualifiedRedirectUrl = parent.UseFullyQualifiedRedirectUrl;
			MinFreeThreads = parent.MinFreeThreads;
			MinLocalRequestFreeThreads = parent.MinLocalRequestFreeThreads;
			AppRequestQueueLimit = parent.AppRequestQueueLimit;
			EnableKernelOutputCache = parent.EnableKernelOutputCache;
			EnableVersionHeader = parent.EnableVersionHeader;
			RequireRootSaveAsPath = parent.RequireRootSaveAsPath;
			IdleTimeout = parent.IdleTimeout;
			Enable = parent.Enable;
		}
	}
}

