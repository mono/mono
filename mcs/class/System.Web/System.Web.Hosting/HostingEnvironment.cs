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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Permissions;
using System.Threading;
using System.Threading.Tasks;
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
		static BackgroundWorkScheduler _backgroundWorkScheduler = null; // created on demand
		static readonly Task<object> _completedTask = Task.FromResult<object>(null);

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

		public static bool InClientBuildManager {
			get {
				// Mono doesn't have a ClientBuildManager, so we can't be in it. Simple as that.
				return false;
			}
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

		// Schedules a task which can run in the background, independent of any request.
		// This differs from a normal ThreadPool work item in that ASP.NET can keep track
		// of how many work items registered through this API are currently running, and
		// the ASP.NET runtime will try not to delay AppDomain shutdown until these work
		// items have finished executing.
		//
		// Usage notes:
		// - This API cannot be called outside of an ASP.NET-managed AppDomain.
		// - The caller's ExecutionContext is not flowed to the work item.
		// - Scheduled work items are not guaranteed to ever execute, e.g., when AppDomain
		//   shutdown has already started by the time this API was called.
		// - The provided CancellationToken will be signaled when the application is
		//   shutting down. The work item should make every effort to honor this token.
		//   If a work item does not honor this token and continues executing it will
		//   eventually be considered rogue, and the ASP.NET runtime will rudely unload
		//   the AppDomain without waiting for the work item to finish.
		//
		// This overload of QueueBackgroundWorkItem takes a void-returning callback; the
		// work item will be considered finished when the callback returns.
		[SecurityPermission(SecurityAction.LinkDemand, Unrestricted = true)]
		public static void QueueBackgroundWorkItem(Action<CancellationToken> workItem) {
			if (workItem == null) {
				throw new ArgumentNullException("workItem");
			}

			QueueBackgroundWorkItem(ct => { workItem(ct); return _completedTask; });
		}

		// See documentation on the other overload for a general API overview.
		//
		// This overload of QueueBackgroundWorkItem takes a Task-returning callback; the
		// work item will be considered finished when the returned Task transitions to a
		// terminal state.
		[SecurityPermission(SecurityAction.LinkDemand, Unrestricted = true)]
		public static void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem) {
			if (workItem == null) {
				throw new ArgumentNullException("workItem");
			}
			if (Host == null) {
				throw new InvalidOperationException(); // can only be called within an ASP.NET AppDomain
			}

			QueueBackgroundWorkItemInternal(workItem);
		}

		static void QueueBackgroundWorkItemInternal(Func<CancellationToken, Task> workItem) {
			Debug.Assert(workItem != null);

			BackgroundWorkScheduler scheduler = Volatile.Read(ref _backgroundWorkScheduler);

			// If the scheduler doesn't exist, lazily create it, but only allow one instance to ever be published to the backing field
			if (scheduler == null) {
				BackgroundWorkScheduler newlyCreatedScheduler = new BackgroundWorkScheduler(UnregisterObject, WriteUnhandledException);
				scheduler = Interlocked.CompareExchange(ref _backgroundWorkScheduler, newlyCreatedScheduler, null) ?? newlyCreatedScheduler;
				if (scheduler == newlyCreatedScheduler) {
					RegisterObject(scheduler); // Only call RegisterObject if we just created the "winning" one
				}
			}

			scheduler.ScheduleWorkItem(workItem);
		}

		static void WriteUnhandledException (AppDomain appDomain, Exception exception)
		{
			Console.Error.WriteLine ("Error in background work item: " + exception);
		}
	}
}

