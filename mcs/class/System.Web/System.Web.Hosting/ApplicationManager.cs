//
// System.Web.Hosting.ApplicationManager
// 
// Author:
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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
using System.Threading;

namespace System.Web.Hosting {
	public sealed class ApplicationManager : MarshalByRefObject {
		static ApplicationManager instance = new ApplicationManager ();
		int users;

		private ApplicationManager ()
		{
		}

		public void Close ()
		{
			if (Interlocked.Decrement (ref users) == 0)
				ShutdownAll ();
		}

		public IRegisteredObject CreateObject (string appId, Type type, string virtualPath,
							string physicalPath, bool failIfExists)
		{
			return CreateObject (appId, type, virtualPath, physicalPath, failIfExists, true);
		}

		[MonoTODO]
		public IRegisteredObject CreateObject (string appId, Type type, string virtualPath,
							string physicalPath, bool failIfExists, bool throwOnError)
		{
			throw new NotImplementedException ();
		}

		public static ApplicationManager GetApplicationManager ()
		{
			return instance;
		}

		[MonoTODO]
		public IRegisteredObject GetObject (string appId, Type type)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public ApplicationInfo [] GetRunningApplications ()
		{
			throw new NotImplementedException ();
		}

		public override object InitializeLifetimeService ()
		{
			return null;
		}

		public bool IsIdle ()
		{
			throw new NotImplementedException ();
		}

		public void Open ()
		{
			Interlocked.Increment (ref users);
		}

		[MonoTODO]
		public void ShutdownAll ()
		{
			// == HostingEnvironment.InitiateShutdown in all appdomains managed by this instance
		}

		[MonoTODO]
		public void ShutdownApplication (string appId)
		{
			if (appId == null)
				throw new ArgumentNullException ("appId");

			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void StopObject (string appId, Type type)
		{
			if (appId == null)
				throw new ArgumentNullException ("appId");

			if (type == null)
				throw new ArgumentNullException ("type");

			throw new NotImplementedException ();
		}
	}
}

#endif

