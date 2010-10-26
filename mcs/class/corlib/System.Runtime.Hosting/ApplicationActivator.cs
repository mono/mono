//
// System.Runtime.Hosting.ApplicationActivator class
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

#if NET_2_0 && !MICRO_LIB

using System.Runtime.InteropServices;
using System.Runtime.Remoting;
using System.Security;
using System.Security.Policy;

namespace System.Runtime.Hosting {

	[ComVisible (true)]
	[MonoTODO ("missing manifest support")]
	public class ApplicationActivator {

		public ApplicationActivator ()
		{
		}

		public virtual ObjectHandle CreateInstance (ActivationContext activationContext)
		{
			return CreateInstance (activationContext, null);
		}

		public virtual ObjectHandle CreateInstance (ActivationContext activationContext, string[] activationCustomData)
		{
			if (activationContext == null)
				throw new ArgumentNullException ("activationContext");

			// TODO : compare activationContext with the one from this domain
			// and use it if it match

			// TODO : we must pass the activationCustomData in the AppDomainSetup
			// even if the activationCustomData don't match ???

			AppDomainSetup setup = new AppDomainSetup (activationContext);
			return CreateInstanceHelper (setup);
		}

		protected static ObjectHandle CreateInstanceHelper (AppDomainSetup adSetup)
		{
			if (adSetup == null)
				throw new ArgumentNullException ("adSetup");

			if (adSetup.ActivationArguments == null) {
				string msg = Locale.GetText ("{0} is missing it's {1} property");
				throw new ArgumentException (String.Format (msg, "AppDomainSetup", "ActivationArguments"), "adSetup");
			}

			HostSecurityManager hsm = null;
			if (AppDomain.CurrentDomain.DomainManager != null)
				hsm = AppDomain.CurrentDomain.DomainManager.HostSecurityManager;
			else
				hsm = new HostSecurityManager (); // default

			Evidence applicationEvidence = new Evidence ();
			applicationEvidence.AddHost (adSetup.ActivationArguments);
			TrustManagerContext context = new TrustManagerContext ();
			ApplicationTrust trust = hsm.DetermineApplicationTrust (applicationEvidence, null, context);
			if (!trust.IsApplicationTrustedToRun) {
				string msg = Locale.GetText ("Current policy doesn't allow execution of addin.");
				throw new PolicyException (msg);
			}

			// FIXME: we're missing the information from the manifest
			AppDomain ad = AppDomain.CreateDomain ("friendlyName", null, adSetup);
			return ad.CreateInstance ("assemblyName", "typeName", null);
		}
	}
}

#endif
