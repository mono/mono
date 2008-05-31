//
// System.Security.Policy.ApplicationSecurityManager class
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Security.Policy {

	[ComVisible (true)]
	public static class ApplicationSecurityManager {

		private const string config = "ApplicationTrust.config";

		static private IApplicationTrustManager _appTrustManager;
		static private ApplicationTrustCollection _userAppTrusts;

		// properties

		// FIXME replace MonoTrustManager with one inside SWF"
		public static IApplicationTrustManager ApplicationTrustManager {
			[SecurityPermission (SecurityAction.Demand, ControlPolicy = true)]
			get {
				if (_appTrustManager == null) {
					_appTrustManager = new MonoTrustManager ();
				}
				return _appTrustManager;
			}
		}

		public static ApplicationTrustCollection UserApplicationTrusts {
			get {
				if (_userAppTrusts == null) {
					_userAppTrusts = new ApplicationTrustCollection ();
				}
				return _userAppTrusts;
			}
		}

		// methods

		[MonoTODO ("Missing application manifest support")]
		[SecurityPermission (SecurityAction.Demand, ControlPolicy = true, ControlEvidence = true)]
		public static bool DetermineApplicationTrust (ActivationContext activationContext, TrustManagerContext context)
		{
// FIXME: a null activationContext throw a NullReferenceException but calling directly the ApplicationTrustManager.DetermineApplicationTrust doesn't
			if (activationContext == null)
				throw new NullReferenceException ("activationContext");
//				throw new ArgumentNullException ("activationContext");
			ApplicationTrust at = ApplicationTrustManager.DetermineApplicationTrust (activationContext, context);
			return at.IsApplicationTrustedToRun;
		}
	}
}

#endif
