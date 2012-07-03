//
// System.Security.AccessControl.CommonSecurityDescriptor implementation
//
// Author:
//	Dick Porter  <dick@ximian.com>
//	James Bellinger  <jfb@zer7.com>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
// Copyright (C) 2012 James Bellinger
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

using System;
using System.Security.Principal;

namespace System.Security.AccessControl
{
	public sealed class CommonSecurityDescriptor : GenericSecurityDescriptor
	{
		bool isContainer;
		bool isDS;
		ControlFlags flags;
		SecurityIdentifier owner;
		SecurityIdentifier group;
		SystemAcl systemAcl;
		DiscretionaryAcl discretionaryAcl;
		
		public CommonSecurityDescriptor (bool isContainer, bool isDS, RawSecurityDescriptor rawSecurityDescriptor)
		{
			Init (isContainer, isDS, rawSecurityDescriptor);
		}
		
		public CommonSecurityDescriptor (bool isContainer, bool isDS, string sddlForm)
		{
			Init (isContainer, isDS, new RawSecurityDescriptor (sddlForm));
		}
		
		public CommonSecurityDescriptor (bool isContainer, bool isDS, byte[] binaryForm, int offset)
		{
			Init (isContainer, isDS, new RawSecurityDescriptor (binaryForm, offset));
		}
		
		public CommonSecurityDescriptor (bool isContainer, bool isDS,
						 ControlFlags flags,
						 SecurityIdentifier owner,
						 SecurityIdentifier group,
						 SystemAcl systemAcl,
						 DiscretionaryAcl discretionaryAcl)
		{
			Init (isContainer, isDS, flags, owner, group, systemAcl, discretionaryAcl);
		}
		
		void Init (bool isContainer, bool isDS, RawSecurityDescriptor rawSecurityDescriptor)
		{
			if (null == rawSecurityDescriptor)
				throw new ArgumentNullException ("rawSecurityDescriptor");
				
			SystemAcl sacl = null;
			if (null != rawSecurityDescriptor.SystemAcl)
				sacl = new SystemAcl (isContainer, isDS, rawSecurityDescriptor.SystemAcl);
				
			DiscretionaryAcl dacl = null;
			if (null != rawSecurityDescriptor.DiscretionaryAcl)
				dacl = new DiscretionaryAcl (isContainer, isDS, rawSecurityDescriptor.DiscretionaryAcl);
				
			Init (isContainer, isDS,
			      rawSecurityDescriptor.ControlFlags,
			      rawSecurityDescriptor.Owner,
			      rawSecurityDescriptor.Group,
			      sacl, dacl);
		}
		
		void Init (bool isContainer, bool isDS,
			   ControlFlags flags,
			   SecurityIdentifier owner,
			   SecurityIdentifier group,
			   SystemAcl systemAcl,
			   DiscretionaryAcl discretionaryAcl)
		{
			IsContainer = isContainer;
			IsDS = isDS;
			Owner = owner;
			Group = group;
			SystemAcl = systemAcl;
			DiscretionaryAcl = discretionaryAcl;
			
			this.flags = flags & ~ControlFlags.SystemAclPresent;
		}
		
		public override ControlFlags ControlFlags {
			get {
				ControlFlags realFlags = flags;
				
				realFlags |= ControlFlags.DiscretionaryAclPresent;
				realFlags |= ControlFlags.SelfRelative;
				if (SystemAcl != null)
					realFlags |= ControlFlags.SystemAclPresent;
					
				return realFlags;
			}
		}
		
		public DiscretionaryAcl DiscretionaryAcl {
			get { return discretionaryAcl; }
			set {
				if (value == null) {
					value = new DiscretionaryAcl (IsContainer, IsDS, 1);
					value.AddAccess (AccessControlType.Allow, new SecurityIdentifier ("WD"), -1,
							InheritanceFlags.None, PropagationFlags.None);
				}
				
				CheckAclConsistency (value);
				discretionaryAcl = value;
			}
		}
		
		public override SecurityIdentifier Group {
			get { return group;  }
			set { group = value; }
		}

		public bool IsContainer {
			get { return isContainer; }
		}
		
		public bool IsDiscretionaryAclCanonical {
			get { return DiscretionaryAcl.IsCanonical; }
		}
		
		public bool IsDS {
			get { return isDS; }
		}
		
		public bool IsSystemAclCanonical {
			get { return SystemAcl == null || SystemAcl.IsCanonical; }
		}
		
		public override SecurityIdentifier Owner {
			get { return owner;  }
			set { owner = value; }
		}
		
		public SystemAcl SystemAcl {
			get { return systemAcl;  }
			set {
				if (value != null)
					CheckAclConsistency (value);
					
				systemAcl = value;
			}
		}
		
		public void PurgeAccessControl (SecurityIdentifier sid)
		{
			DiscretionaryAcl.Purge (sid);
		}
		
		public void PurgeAudit (SecurityIdentifier sid)
		{
			if (SystemAcl != null)
				SystemAcl.Purge (sid);
		}
		
		public void SetDiscretionaryAclProtection (bool isProtected,
							   bool preserveInheritance)
		{
			if (!isProtected) {
				flags &= ~ControlFlags.DiscretionaryAclProtected;
				return;
			}
			
			flags |= ControlFlags.DiscretionaryAclProtected;
			if (!preserveInheritance)
				DiscretionaryAcl.RemoveInheritedAces ();
		}
		
		public void SetSystemAclProtection (bool isProtected,
						    bool preserveInheritance)
		{
			if (!isProtected) {
				flags &= ~ControlFlags.SystemAclProtected;
				return;
			}
			
			flags |= ControlFlags.SystemAclProtected;
			if (!preserveInheritance)
				SystemAcl.RemoveInheritedAces ();
		}
		
		void CheckAclConsistency (CommonAcl acl)
		{
			if (IsContainer != acl.IsContainer)
				throw new ArgumentExcetion ("IsContainer must match between descriptor and ACL.");
			
			if (IsDS != acl.IsDS)
				throw new ArgumentException ("IsDS must match between descriptor and ACL.");
		}
	}
}

