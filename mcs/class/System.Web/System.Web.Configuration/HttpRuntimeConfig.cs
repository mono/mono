//
// System.Web.Configuration.HttpRuntimeConfig
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Novell, Inc. (http://www.novell.com)
//

using System;
using System.CodeDom.Compiler;
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
				context = HttpContext.Context;

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

