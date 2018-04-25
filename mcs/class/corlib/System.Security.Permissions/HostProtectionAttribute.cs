//
// System.Security.Permissions.HostProtectionAttribute class
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

using System.Runtime.InteropServices;

namespace System.Security.Permissions {

	[AttributeUsage (AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct |
		AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Delegate, 
		AllowMultiple = true, Inherited = false)]
	[ComVisible (true)]
	[Serializable]
	public sealed class HostProtectionAttribute : CodeAccessSecurityAttribute {

		private HostProtectionResource _resources;

#if BOOTSTRAP_BASIC
		public HostProtectionAttribute (SecurityAction action = SecurityAction.LinkDemand)
#else
		public HostProtectionAttribute ()
			: base (SecurityAction.LinkDemand) 
		{
		}

		public HostProtectionAttribute (SecurityAction action)
#endif
			: base (action) 
		{
			if (action != SecurityAction.LinkDemand) {
				string msg = String.Format (Locale.GetText ("Only {0} is accepted."), SecurityAction.LinkDemand);
				throw new ArgumentException (msg, "action");
			}
		}


		public bool ExternalProcessMgmt {
			get { return ((_resources & HostProtectionResource.ExternalProcessMgmt) != 0); }
			set {
				if (value) {
					_resources |= HostProtectionResource.ExternalProcessMgmt;
				}
				else {
					_resources &= ~HostProtectionResource.ExternalProcessMgmt;
				}
			}
		}

		public bool ExternalThreading {
			get { return ((_resources & HostProtectionResource.ExternalThreading) != 0); }
			set {
				if (value) {
					_resources |= HostProtectionResource.ExternalThreading;
				}
				else {
					_resources &= ~HostProtectionResource.ExternalThreading;
				}
			}
		}

		public bool MayLeakOnAbort {
			get { return ((_resources & HostProtectionResource.MayLeakOnAbort) != 0); }
			set {
				if (value) {
					_resources |= HostProtectionResource.MayLeakOnAbort;
				}
				else {
					_resources &= ~HostProtectionResource.MayLeakOnAbort;
				}
			}
		}

		[ComVisible (true)]
		public bool SecurityInfrastructure {
			get { return ((_resources & HostProtectionResource.SecurityInfrastructure) != 0); }
			set {
				if (value) {
					_resources |= HostProtectionResource.SecurityInfrastructure;
				}
				else {
					_resources &= ~HostProtectionResource.SecurityInfrastructure;
				}
			}
		}

		public bool SelfAffectingProcessMgmt {
			get { return ((_resources & HostProtectionResource.SelfAffectingProcessMgmt) != 0); }
			set {
				if (value) {
					_resources |= HostProtectionResource.SelfAffectingProcessMgmt;
				}
				else {
					_resources &= ~HostProtectionResource.SelfAffectingProcessMgmt;
				}
			}
		}

		public bool SelfAffectingThreading {
			get { return ((_resources & HostProtectionResource.SelfAffectingThreading) != 0); }
			set {
				if (value) {
					_resources |= HostProtectionResource.SelfAffectingThreading;
				}
				else {
					_resources &= ~HostProtectionResource.SelfAffectingThreading;
				}
			}
		}

		public bool SharedState {
			get { return ((_resources & HostProtectionResource.SharedState) != 0); }
			set {
				if (value) {
					_resources |= HostProtectionResource.SharedState;
				}
				else {
					_resources &= ~HostProtectionResource.SharedState;
				}
			}
		}

		public bool Synchronization {
			get { return ((_resources & HostProtectionResource.Synchronization) != 0); }
			set {
				if (value) {
					_resources |= HostProtectionResource.Synchronization;
				}
				else {
					_resources &= ~HostProtectionResource.Synchronization;
				}
			}
		}

		public bool UI {
			get { return ((_resources & HostProtectionResource.UI) != 0); }
			set {
				if (value) {
					_resources |= HostProtectionResource.UI;
				}
				else {
					_resources &= ~HostProtectionResource.UI;
				}
			}
		}

		public HostProtectionResource Resources {
			get { return _resources; }
			set { _resources = value; }
		}


		public override IPermission CreatePermission ()
		{
#if MOBILE
			return null;
#else
			// looks like permission is internal
			return new HostProtectionPermission (_resources);
#endif
		}
	}
}

