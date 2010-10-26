//
// System.Security.HostSecurityManager class
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

#if NET_2_0 && !DISABLE_SECURITY

using System.Reflection;
using System.Runtime.Hosting;
using System.Runtime.InteropServices;
using System.Security.Policy;

namespace System.Security {

	[Serializable]
	[ComVisible (true)]
	public class HostSecurityManager {

		public HostSecurityManager ()
		{
		}

		public virtual PolicyLevel DomainPolicy {
			// always return null - may be overriden
			get { return null; }
		}

		public virtual HostSecurityManagerOptions Flags {
			get { return HostSecurityManagerOptions.AllFlags; }
		}

		public virtual ApplicationTrust DetermineApplicationTrust (Evidence applicationEvidence, Evidence activatorEvidence, TrustManagerContext context)
		{
			if (applicationEvidence == null)
				throw new ArgumentNullException ("applicationEvidence");

			ActivationArguments aa = null;
			foreach (object o in applicationEvidence) {
				aa = (o as ActivationArguments);
				if (aa != null)
					break;
			}

			if (aa == null) {
				string msg = Locale.GetText ("No {0} found in {1}.");
				throw new ArgumentException (string.Format (msg, "ActivationArguments", "Evidence"), "applicationEvidence");
			}
			if (aa.ActivationContext == null) {
				string msg = Locale.GetText ("No {0} found in {1}.");
				throw new ArgumentException (string.Format (msg, "ActivationContext", "ActivationArguments"), "applicationEvidence");
			}

			// FIXME: this part is still untested (requires manifest support)
			if (ApplicationSecurityManager.DetermineApplicationTrust (aa.ActivationContext, context)) {
				if (aa.ApplicationIdentity == null)
					return new ApplicationTrust ();
				else
					return new ApplicationTrust (aa.ApplicationIdentity);
			}
			return null;
		}

		public virtual Evidence ProvideAppDomainEvidence (Evidence inputEvidence)
		{
			// no changes - may be overriden
			return inputEvidence;
		}

		public virtual Evidence ProvideAssemblyEvidence (Assembly loadedAssembly, Evidence inputEvidence)
		{
			// no changes - may be overriden
			return inputEvidence;
		}

		public virtual PermissionSet ResolvePolicy (Evidence evidence)
		{
			if (evidence == null)
				throw new NullReferenceException ("evidence");
			return SecurityManager.ResolvePolicy (evidence);
		}
	}
}

#endif
