//
// System.Web.Hosting.HostingEnvironment.cs
//
// Author:
//	Chris Toshok (toshok@ximian.com)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//

//
// Copyright (C) 2005,2006 Novell, Inc (http://www.novell.com)
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
using System.Globalization;
using System.Security.Permissions;
using System.Threading;
using System.Web.Configuration;
using System.Web.Caching;
using System.Web.Util;

namespace System.Web.Hosting {

	[AspNetHostingPermission (SecurityAction.Demand, Level = AspNetHostingPermissionLevel.Medium)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.High)]
	public sealed class HostingEnvironment : MarshalByRefObject
	{
		static bool is_hosted;
#pragma warning disable 0649
		static string site_name;
		static ApplicationShutdownReason shutdown_reason;
#pragma warning restore 0649
		internal static BareApplicationHost Host;
		static VirtualPathProvider vpath_provider = (HttpRuntime.AppDomainAppVirtualPath == null) ? null :
								new DefaultVirtualPathProvider ();
		static int busy_count;

		internal static bool HaveCustomVPP {
			get;
			private set;
		}
		
		public HostingEnvironment ()
		{
			// The documentation says that this is called once per domain by the ApplicationManager and
			// then it throws InvalidOperationException whenever called.
			throw new InvalidOperationException ();
		}

		public static string ApplicationID {
			get { return HttpRuntime.AppDomainAppId; }
		}

		public static string ApplicationPhysicalPath {
			get { return HttpRuntime.AppDomainAppPath; }
		}

		public static string ApplicationVirtualPath {
			get { return HttpRuntime.AppDomainAppVirtualPath; }
		}

		public static Cache Cache {
			get { return HttpRuntime.Cache; }
		}

		public static Exception InitializationException {
			get { return HttpApplication.InitializationException; }
		}

		public static bool IsHosted {
			get { return is_hosted; }
			internal set { is_hosted = value; }
		}

		public static ApplicationShutdownReason ShutdownReason {
			get { return shutdown_reason; }
		}

		public static string SiteName {
			get { return site_name; }
			internal set { site_name = value; }
		}

		public static VirtualPathProvider VirtualPathProvider {
			get { return vpath_provider; }
		}

		public static void DecrementBusyCount ()
		{
			Interlocked.Decrement (ref busy_count);
		}

		[MonoTODO ("Not implemented")]
		public static IDisposable Impersonate ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Not implemented")]
		public static IDisposable Impersonate (IntPtr token)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO ("Not implemented")]
		public static IDisposable Impersonate (IntPtr userToken, string virtualPath)
		{
			throw new NotImplementedException ();
		}

		public static void IncrementBusyCount ()
		{
			Interlocked.Increment (ref busy_count);
		}

		public override object InitializeLifetimeService ()
		{
			return null;
		}

		public static void InitiateShutdown ()
		{
			HttpRuntime.UnloadAppDomain ();
		}

		public static string MapPath (string virtualPath)
		{
			if (virtualPath == null || virtualPath == "")
				throw new ArgumentNullException ("virtualPath");
			
			HttpContext context = HttpContext.Current;
			HttpRequest req = context == null ? null : context.Request;
			if (req == null)
				return null;

			return req.MapPath (virtualPath);
		}

		public static void RegisterObject (IRegisteredObject obj)
		{
			if (obj == null)
				throw new ArgumentNullException ("obj");

			if (Host != null)
				Host.RegisterObject (obj, false);
		}

		public static void RegisterVirtualPathProvider (VirtualPathProvider virtualPathProvider)
		{
			if (HttpRuntime.AppDomainAppVirtualPath == null)
				throw new InvalidOperationException ();

			if (virtualPathProvider == null)
				throw new ArgumentNullException ("virtualPathProvider");

			VirtualPathProvider previous = vpath_provider;
			vpath_provider = virtualPathProvider;
			vpath_provider.InitializeAndSetPrevious (previous);
			if (!(virtualPathProvider is DefaultVirtualPathProvider))
				HaveCustomVPP = true;
			else
				HaveCustomVPP = false;
		}
		
		public static IDisposable SetCultures (string virtualPath)
		{
			GlobalizationSection gs = WebConfigurationManager.GetSection ("system.web/globalization", virtualPath) as GlobalizationSection;
			IDisposable ret = Thread.CurrentThread.CurrentCulture as IDisposable;
			string culture = gs.Culture;
			if (String.IsNullOrEmpty (culture))
				return ret;
			Thread.CurrentThread.CurrentCulture = new CultureInfo (culture);
			return ret;
		}

		public static IDisposable SetCultures ()
		{
			return SetCultures ("~/");
		}

		public static void UnregisterObject (IRegisteredObject obj)
		{
			if (obj == null)
				throw new ArgumentNullException ("obj");

			if (Host != null)
				Host.UnregisterObject (obj);
		}
	}
}

