//
// System.Web.Hosting.HostingEnvironment.cs
//
// Author:
//	Chris Toshok (toshok@ximian.com)
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//

//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.Web.Caching;

namespace System.Web.Hosting {

	public sealed class HostingEnvironment : MarshalByRefObject
	{
		public HostingEnvironment ()
		{
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

		[MonoTODO]
		public static Exception InitializationException {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO] // 'true' if this is inside an ApplicationManager
		public static bool IsHosted {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public static ApplicationShutdownReason ShutdownReason {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public static string SiteName {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public static VirtualPathProvider VirtualPathProvider {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public static void DecrementBusyCount ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static IDisposable Impersonate ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static IDisposable Impersonate (IntPtr token)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static IDisposable Impersonate (IntPtr userToken, string virtualPath)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static void IncrementBusyCount ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override object InitializeLifetimeService ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static void InitiateShutdown ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static string MapPath (string virtualPath)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static void RegisterObject (IRegisteredObject obj)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static void RegisterVirtualPathProvider (VirtualPathProvider virtualPathProvider)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static IDisposable SetCultures (string virtualPath)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static IDisposable SetCultures ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static void UnregisterObject (IRegisteredObject obj)
		{
			throw new NotImplementedException ();
		}
	}

}

#endif
