//
// System.Security.Policy.DomainApplicationMembershipCondition
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
	public sealed class DomainApplicationMembershipCondition : IConstantMembershipCondition, IMembershipCondition {

		private readonly int version = 1;

		public DomainApplicationMembershipCondition ()
		{
		}

		[MonoTODO ("documentation relies on (obsoleted) IApplicationDescription")]
		public bool Check (Evidence evidence)
		{
			//return (AppDomain.CurrentDomain.ApplicationDescription != null);
			return false;
		}

		public IMembershipCondition Copy () 
		{ 
			return new DomainApplicationMembershipCondition ();
		}
		
		public override bool Equals (object o) 
		{ 
			return (o is DomainApplicationMembershipCondition); 
		}
		
		public void FromXml (SecurityElement e)
		{
			FromXml (e, null);
		}
		
		public void FromXml (SecurityElement e, PolicyLevel level)
		{
			MembershipConditionHelper.CheckSecurityElement (e, "e", version, version);
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
				return "Domain";
			else
				return "Domain - " + ac.Identity.FullName;
		}
		
		public SecurityElement ToXml () 
		{ 
			return ToXml (null);
		}
		
		public SecurityElement ToXml (PolicyLevel level) 
		{
			// PolicyLevel isn't used as there's no need to resolve NamedPermissionSet references
			SecurityElement se = MembershipConditionHelper.Element (typeof (DomainApplicationMembershipCondition), version);
			// nothing to add
			return se;
		}
	}
}

#endif
