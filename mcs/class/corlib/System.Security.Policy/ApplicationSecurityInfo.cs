//
// System.Security.Policy.ApplicationSecurityInfo class
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System.Collections;
using System.Globalization;
using System.Security.Permissions;

namespace System.Security.Policy {

	public sealed class ApplicationSecurityInfo {

		private ActivationContext _context;
		private Evidence _evidence;
		private ApplicationId _appid;
		private Hashtable _requests;
		private PermissionSet _defaultSet;
		private ApplicationId _deployid;

		public ApplicationSecurityInfo (ActivationContext activationContext)
		{
			if (activationContext == null)
				throw new ArgumentNullException ("activationContext");
			_context = activationContext;
		}

		public Evidence ApplicationEvidence {
			get { return _evidence; }
			set {
				if (value == null)
					throw new ArgumentNullException ("ApplicationEvidence");
				_evidence = value;
			}
		}

		public ApplicationId ApplicationId {
			get { return _appid; }
			set {
				if (value == null)
					throw new ArgumentNullException ("ApplicationId");
				_appid = value;
			}
		}

		public IDictionary AssemblyRequests {
			get { return (IDictionary) _requests; }
			set {
				if (value == null)
					throw new ArgumentNullException ("AssemblyRequests");
				if (!(value is Hashtable))
					throw new ArgumentException (Locale.GetText ("wrong type"));

				_requests = (Hashtable) value;
			}
		}

		public PermissionSet DefaultRequestSet {
			get {
				if (_defaultSet == null)
					return new PermissionSet (PermissionState.None);
				return _defaultSet; // FIXME: copy or reference ?
			}
			set {
				if (value == null)
					throw new ArgumentNullException ("DefaultRequestSet");
				_defaultSet = value;
			}
		}

		public ApplicationId DeploymentId {
			get { return _deployid; }
			set {
				if (value == null)
					throw new ArgumentNullException ("DeploymentId");
				_deployid = value;
			}
		}

		// methods

		[MonoTODO]
		public override bool Equals (object obj)
		{
			if (obj == null)
				return false;
			ApplicationSecurityInfo asi = (obj as ApplicationSecurityInfo);
			if (asi == null)
				return false;
			// TODO
			return false;
		}

		public void FromXml (SecurityElement element)
		{
			FromXml (element, null);
		}

		[MonoTODO]
		public void FromXml (SecurityElement element, PolicyLevel level)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}

		[MonoTODO]
		public bool IsInApplication (Evidence evidence)
		{
			if (evidence == null)
				return false; // ???

			IEnumerator e = evidence.GetHostEnumerator ();
			while (e.MoveNext ()) {
			}

			e = evidence.GetAssemblyEnumerator ();
			while (e.MoveNext ()) {
			}

			// we found them all!
			return true;
		}

		public SecurityElement ToXml ()
		{
			return ToXml (null);
		}

		[MonoTODO]
		public SecurityElement ToXml (PolicyLevel level)
		{
			throw new NotImplementedException ();
		}
	}
}

#endif
