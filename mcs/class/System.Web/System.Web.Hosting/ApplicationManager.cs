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
using System.Collections.Generic;
using System.IO;
using System.Security.Permissions;
using System.Security.Policy;
using System.Threading;

namespace System.Web.Hosting {
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public sealed class ApplicationManager : MarshalByRefObject {
		static ApplicationManager instance = new ApplicationManager ();
		int users;
		Dictionary <string, BareApplicationHost> id_to_host;

		ApplicationManager ()
		{
			id_to_host = new Dictionary<string, BareApplicationHost> ();
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

		public IRegisteredObject CreateObject (string appId, Type type, string virtualPath,
							string physicalPath, bool failIfExists, bool throwOnError)
		{
			if (appId == null)
				throw new ArgumentNullException ("appId");

			if (!VirtualPathUtility.IsAbsolute (virtualPath))
				throw new ArgumentException ("Relative path no allowed.", "virtualPath");

			if (physicalPath == null || physicalPath == "")
				throw new ArgumentException ("Cannot be null or empty", "physicalPath");

			// 'type' is not checked. If it's null, we'll throw a NullReferenceException
			if (!typeof (IRegisteredObject).IsAssignableFrom (type))
				throw new ArgumentException (String.Concat ("Type '", type.Name, "' does not implement IRegisteredObject."), "type");

			//
			// ArgumentException is thrown for the physical path from the internal object created
			// in the new application domain.
			BareApplicationHost host = null;
			if (id_to_host.ContainsKey (appId))
				host = id_to_host [appId];

			IRegisteredObject ireg = null;
			if (host != null) {
				ireg = CheckIfExists (host, type, failIfExists);
				if (ireg != null)
					return ireg;
			}

			try {
				if (host == null)
					host = CreateHost (appId, virtualPath, physicalPath);
				ireg = host.CreateInstance (type);
			} catch (Exception) {
				if (throwOnError)
					throw;
			}

			if (ireg != null && host.GetObject (type) == null) // If not registered from ctor...
				host.RegisterObject (ireg, true);

			return ireg;
		}

		// Used from ClientBuildManager
		internal BareApplicationHost CreateHostWithCheck (string appId, string vpath, string ppath)
		{
			if (id_to_host.ContainsKey (appId))
				throw new InvalidOperationException ("Already have a host with the same appId");

			return CreateHost (appId, vpath, ppath);
		}

		BareApplicationHost CreateHost (string appId, string vpath, string ppath)
		{
			BareApplicationHost host;
			host = (BareApplicationHost) ApplicationHost.CreateApplicationHost (typeof (BareApplicationHost), vpath, ppath);
			host.Manager = this;
			host.AppID = appId;
			id_to_host [appId] = host;
			return host;
		}

		internal void RemoveHost (string appId)
		{
			id_to_host.Remove (appId);
		}

		IRegisteredObject CheckIfExists (BareApplicationHost host, Type type, bool failIfExists)
		{
			IRegisteredObject ireg = host.GetObject (type);
			if (ireg == null)
				return null;

			if (failIfExists)
				throw new InvalidOperationException (String.Concat ("Well known object of type '", type.Name, "' already exists in this domain."));

			return ireg;
		}

		public static ApplicationManager GetApplicationManager ()
		{
			return instance;
		}

		public IRegisteredObject GetObject (string appId, Type type)
		{
			if (appId == null)
				throw new ArgumentNullException ("appId");

			if (type == null)
				throw new ArgumentNullException ("type");

			BareApplicationHost host = null;
			if (!id_to_host.ContainsKey (appId))
				return null;

			host = id_to_host [appId];
			return host.GetObject (type);
		}

		public ApplicationInfo [] GetRunningApplications ()
		{
			ICollection<string> coll = id_to_host.Keys;
			string [] keys = new string [coll.Count];
			coll.CopyTo (keys, 0);
			ApplicationInfo [] result = new ApplicationInfo [coll.Count];
			int i = 0;
			foreach (string str in keys) {
				BareApplicationHost host = id_to_host [str];
				result [i++] = new ApplicationInfo (str, host.PhysicalPath, host.VirtualPath);
			}

			return result;
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

		public void ShutdownAll ()
		{
			ICollection<string> coll = id_to_host.Keys;
			string [] keys = new string [coll.Count];
			coll.CopyTo (keys, 0);
			foreach (string str in keys) {
				BareApplicationHost host = id_to_host [str];
				host.Shutdown ();
			}

			id_to_host.Clear ();
		}

		public void ShutdownApplication (string appId)
		{
			if (appId == null)
				throw new ArgumentNullException ("appId");

			BareApplicationHost host = id_to_host [appId];
			if (host == null)
				return;

			host.Shutdown ();
		}

		public void StopObject (string appId, Type type)
		{
			if (appId == null)
				throw new ArgumentNullException ("appId");

			if (type == null)
				throw new ArgumentNullException ("type");

			if (!id_to_host.ContainsKey (appId))
				return;

			BareApplicationHost host = id_to_host [appId];
			if (host == null)
				return;

			host.StopObject (type);
		}
	}
}

#endif

