//
// System.Security.Policy.ApplicationMembershipCondition
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

namespace System.Security.Policy {

	[Serializable]
	public sealed class ApplicationMembershipCondition : IConstantMembershipCondition, IMembershipCondition {

		private readonly int version = 1;

		private bool _lookAtDir;

		public ApplicationMembershipCondition ()
		{
			_lookAtDir = true;
		}

		[MonoTODO ("fx 2.0 beta 1 has some related parts obsoleted - waiting...")]
		public bool Check (Evidence evidence)
		{
			if (evidence == null)
				return false;
			
			IEnumerator e = evidence.GetHostEnumerator ();
			while (e.MoveNext ()) {
				// TODO: from samples it seems related to IApplicationDescription and HostContext
				// but some are obsoleted - so this should be moving to ApplicationIdentity ?
			}
			return false;
		}

		public IMembershipCondition Copy () 
		{ 
			// _lookAtDir value isn't copied (see unit tests)
			return new ApplicationMembershipCondition ();
		}
		
		public override bool Equals (object o) 
		{
			// _lookAtDir isn't part of Equals computation (see unit tests)
			return (o is ApplicationMembershipCondition); 
		}
		
		public void FromXml (SecurityElement e)
		{
			FromXml (e, null);
		}
		
		public void FromXml (SecurityElement e, PolicyLevel level)
		{
			MembershipConditionHelper.CheckSecurityElement (e, "e", version, version);
			if (!Boolean.TryParse (e.Attribute ("LookAtDir"), out _lookAtDir))
				_lookAtDir = false;
			// PolicyLevel isn't used as there's no need to resolve NamedPermissionSet references
		}
		
		public override int GetHashCode () 
		{
			return -1;
		}
		
		public override string ToString () 
		{
			ActivationContext ac = AppDomain.CurrentDomain.ActivationContext;
			if (ac == null)
				return "Application";
			else
				return "Application - " + ac.Identity.FullName;
		}
		
		public SecurityElement ToXml () 
		{
			return ToXml (null);
		}
		
		public SecurityElement ToXml (PolicyLevel level) 
		{
			// PolicyLevel isn't used as there's no need to resolve NamedPermissionSet references
			SecurityElement se = MembershipConditionHelper.Element (typeof (ApplicationMembershipCondition), version);
			if (_lookAtDir)
				se.AddAttribute ("LookAtDir", "true");
			return se;
		}
	}
}

#endif
