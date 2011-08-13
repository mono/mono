//
// System.Security.AccessControl.CommonSecurityDescriptor implementation
//
// Author:
//	Dick Porter  <dick@ximian.com>
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

using System.Security.Principal;

namespace System.Security.AccessControl {
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
			Init(isContainer, isDS, rawSecurityDescriptor);
		}
		
		public CommonSecurityDescriptor (bool isContainer, bool isDS, string sddlForm)
		{
			Init(isContainer, isDS, new RawSecurityDescriptor(sddlForm));
		}
		
		public CommonSecurityDescriptor (bool isContainer, bool isDS, byte[] binaryForm, int offset) 
		{
			Init(isContainer, isDS, new RawSecurityDescriptor(binaryForm, offset));
		}
		
		public CommonSecurityDescriptor (bool isContainer, bool isDS,
						 ControlFlags flags,
						 SecurityIdentifier owner,
						 SecurityIdentifier group,
						 SystemAcl systemAcl,
						 DiscretionaryAcl discretionaryAcl)
		{
			this.isContainer = isContainer;
			this.isDS = isDS;
			this.flags = flags;
			this.owner = owner;
			this.group = group;
			this.systemAcl = systemAcl;
			this.discretionaryAcl = discretionaryAcl;
		}
		
		public override ControlFlags ControlFlags
		{
			get {
				return(flags);
			}
		}
		
		public DiscretionaryAcl DiscretionaryAcl
		{
			get {
				return(discretionaryAcl);
			}
			set {
				if (value == null) {
					/* FIXME: add a "full access" ACE */
				}
				
				discretionaryAcl = value;
			}
		}
		
		public override SecurityIdentifier Group
		{
			get {
				return(group);
			}
			set {
				group = value;
			}
		}

		public bool IsContainer
		{
			get {
				return(isContainer);
			}
		}
		
		public bool IsDiscretionaryAclCanonical
		{
			get {
				throw new NotImplementedException ();
			}
		}
		
		public bool IsDS
		{
			get {
				return(isDS);
			}
		}
		
		public bool IsSystemAclCanonical
		{
			get {
				throw new NotImplementedException ();
			}
		}
		
		public override SecurityIdentifier Owner
		{
			get {
				return(owner);
			}
			set {
				owner = value;
			}
		}
		
		public SystemAcl SystemAcl
		{
			get {
				return(systemAcl);
			}
			set {
				systemAcl = value;
			}
		}
		
		public void PurgeAccessControl (SecurityIdentifier sid)
		{
			throw new NotImplementedException ();
		}
		
		public void PurgeAudit (SecurityIdentifier sid)
		{
			throw new NotImplementedException ();
		}
		
		public void SetDiscretionaryAclProtection (bool isProtected,
							   bool preserveInheritance)
		{
			throw new NotImplementedException ();
		}
		
		public void SetSystemAclProtection (bool isProtected,
						    bool preserveInheritance)
		{
			throw new NotImplementedException ();
		}

		internal override GenericAcl InternalDacl {
			get { return discretionaryAcl; }
		}

		internal override GenericAcl InternalSacl {
			get { return systemAcl; }
		}

		internal void SetControlFlags(ControlFlags value)
		{
			flags = value;
		}

		private void Init(bool isContainer, bool isDS, RawSecurityDescriptor rawSecurityDescriptor)
		{
			this.isContainer = isContainer;
			this.isDS = isDS;
			this.flags = rawSecurityDescriptor.ControlFlags;
			this.owner = rawSecurityDescriptor.Owner;
			this.group = rawSecurityDescriptor.Group;
			this.systemAcl = new SystemAcl(isContainer, isDS, rawSecurityDescriptor.SystemAcl);
			this.discretionaryAcl = new DiscretionaryAcl(isContainer, isDS, rawSecurityDescriptor.DiscretionaryAcl);
		}
	}
}

