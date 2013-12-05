/******************************************************************************
* The MIT License
*
* Permission is hereby granted, free of charge, to any person obtaining  a copy
* of this software and associated documentation files (the Software), to deal
* in the Software without restriction, including  without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to  permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*******************************************************************************/
using System;
using System.Security.AccessControl;
using System.Security.Principal;

namespace System.DirectoryServices
{
	public class ActiveDirectorySecurity : DirectoryObjectSecurity
	{
		public override Type AccessRightType {
			get {
				throw new NotImplementedException ();
			}
		}

		public override Type AccessRuleType {
			get {
				throw new NotImplementedException ();
			}
		}

		public override Type AuditRuleType {
			get {
				throw new NotImplementedException ();
			}
		}

		public ActiveDirectorySecurity ()
		{
		}

		public void AddAccessRule (ActiveDirectoryAccessRule rule)
		{
			throw new NotImplementedException ();
		}

		public void SetAccessRule (ActiveDirectoryAccessRule rule)
		{
			throw new NotImplementedException ();
		}

		public void ResetAccessRule (ActiveDirectoryAccessRule rule)
		{
			throw new NotImplementedException ();
		}

		public void RemoveAccess (IdentityReference identity, AccessControlType type)
		{
			throw new NotImplementedException ();
		}

		public bool RemoveAccessRule (ActiveDirectoryAccessRule rule)
		{
			throw new NotImplementedException ();
		}

		public void RemoveAccessRuleSpecific (ActiveDirectoryAccessRule rule)
		{
			throw new NotImplementedException ();
		}

		public override bool ModifyAccessRule (AccessControlModification modification, AccessRule rule, out bool modified)
		{
			throw new NotImplementedException ();
		}

		public override void PurgeAccessRules (IdentityReference identity)
		{
			throw new NotImplementedException ();
		}

		public void AddAuditRule (ActiveDirectoryAuditRule rule)
		{
			throw new NotImplementedException ();
		}

		public void SetAuditRule (ActiveDirectoryAuditRule rule)
		{
			throw new NotImplementedException ();
		}

		public void RemoveAudit (IdentityReference identity)
		{
			throw new NotImplementedException ();
		}

		public bool RemoveAuditRule (ActiveDirectoryAuditRule rule)
		{
			throw new NotImplementedException ();
		}

		public void RemoveAuditRuleSpecific (ActiveDirectoryAuditRule rule)
		{
			throw new NotImplementedException ();
		}

		public override bool ModifyAuditRule (AccessControlModification modification, AuditRule rule, out bool modified)
		{
			throw new NotImplementedException ();
		}

		public override void PurgeAuditRules (IdentityReference identity)
		{
			throw new NotImplementedException ();
		}

		public sealed override AccessRule AccessRuleFactory (IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type)
		{
			throw new NotImplementedException ();
		}

		public sealed override AccessRule AccessRuleFactory (IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AccessControlType type, Guid objectGuid, Guid inheritedObjectGuid)
		{
			throw new NotImplementedException ();
		}

		public sealed override AuditRule AuditRuleFactory (IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags)
		{
			throw new NotImplementedException ();
		}

		public sealed override AuditRule AuditRuleFactory (IdentityReference identityReference, int accessMask, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags, Guid objectGuid, Guid inheritedObjectGuid)
		{
			throw new NotImplementedException ();
		}
	}
}
