//
// System.AppDomainManager class
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004-2005,2009 Novell, Inc (http://www.novell.com)
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

#if NET_2_0 && !MICRO_LIB

using System.Reflection;
using System.Runtime.Hosting;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using System.Threading;

namespace System {

	[ComVisible (true)]
	[SecurityPermission (SecurityAction.LinkDemand, Infrastructure = true)]
	[SecurityPermission (SecurityAction.InheritanceDemand, Infrastructure = true)]
	public class AppDomainManager : MarshalByRefObject {
		private ApplicationActivator _activator;
		private AppDomainManagerInitializationOptions _flags;

		public AppDomainManager ()
		{
			_flags = AppDomainManagerInitializationOptions.None;
		}

		public virtual ApplicationActivator ApplicationActivator {
			get {
				if (_activator == null)
					_activator = new ApplicationActivator ();
				 return _activator;
			}
		}

		public virtual Assembly EntryAssembly {
			get { return Assembly.GetEntryAssembly (); }
		}

		[MonoTODO]
		public virtual HostExecutionContextManager HostExecutionContextManager {
			get { throw new NotImplementedException (); }
		}

		public virtual HostSecurityManager HostSecurityManager {
			get { return null; }
		}

		public AppDomainManagerInitializationOptions InitializationFlags {
			get { return _flags; }
			set { _flags = value; }	
		}

		// methods

		public virtual AppDomain CreateDomain (string friendlyName, Evidence securityInfo, AppDomainSetup appDomainInfo)
		{
			InitializeNewDomain (appDomainInfo);
			AppDomain ad = CreateDomainHelper (friendlyName, securityInfo, appDomainInfo);

			// supply app domain policy ?
			if ((HostSecurityManager.Flags & HostSecurityManagerOptions.HostPolicyLevel) == HostSecurityManagerOptions.HostPolicyLevel) {
				PolicyLevel pl = HostSecurityManager.DomainPolicy;
				if (pl != null) {
					ad.SetAppDomainPolicy (pl);
				}
			}

			return ad;
		}

		public virtual void InitializeNewDomain (AppDomainSetup appDomainInfo)
		{
			// default does nothing (as documented)
		}

		// available in FX2.0 with service pack 1, including the 2.0 shipped as part of FX3.5
		public virtual bool CheckSecuritySettings (SecurityState state)
		{
			return false;
		}

		// static

		// FIXME: maybe AppDomain.CreateDomain should be calling this?
		protected static AppDomain CreateDomainHelper (string friendlyName, Evidence securityInfo, AppDomainSetup appDomainInfo)
		{
			return AppDomain.CreateDomain (friendlyName, securityInfo, appDomainInfo);
		}
	}
}

#endif
