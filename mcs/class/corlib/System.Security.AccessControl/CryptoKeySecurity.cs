//
// System.Security.AccessControl.CryptoKeySecurity implementation
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//	Dick Porter <dick@ximian.com>
//	Atsushi Enomoto  <atsushi@ximian.com>
//	James Bellinger  <jfb@zer7.com>
//
// Copyright (C) 2005-2007 Novell, Inc (http://www.novell.com)
// Copyright (C) 2012      James Bellinger
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

namespace System.Security.AccessControl
{
	public sealed class CryptoKeySecurity : NativeObjectSecurity
	{
		public CryptoKeySecurity ()
			: base (false, ResourceType.Unknown)
		{
		}

		public CryptoKeySecurity (CommonSecurityDescriptor securityDescriptor)
			: base (securityDescriptor, ResourceType.Unknown)
		{

		}
		
		public override Type AccessRightType {
			get { return typeof (CryptoKeyRights); }
		}
		
		public override Type AccessRuleType {
			get { return typeof (CryptoKeyAccessRule); }
		}

		public override Type AuditRuleType {
			get { return typeof (CryptoKeyAuditRule); }
		}
		
		public override sealed AccessRule AccessRuleFactory (IdentityReference identityReference, int accessMask,
								     bool isInherited, InheritanceFlags inheritanceFlags,
								     PropagationFlags propagationFlags, AccessControlType type)
		{
			return new CryptoKeyAccessRule (identityReference, (CryptoKeyRights) accessMask, type);
		}
		
		public void AddAccessRule (CryptoKeyAccessRule rule)
		{
			AddAccessRule ((AccessRule)rule);
		}
		
		public bool RemoveAccessRule (CryptoKeyAccessRule rule)
		{
			return RemoveAccessRule ((AccessRule)rule);
		}
		
		public void RemoveAccessRuleAll (CryptoKeyAccessRule rule)
		{
			RemoveAccessRuleAll ((AccessRule)rule);
		}
		
		public void RemoveAccessRuleSpecific (CryptoKeyAccessRule rule)
		{
			RemoveAccessRuleSpecific ((AccessRule)rule);
		}
		
		public void ResetAccessRule (CryptoKeyAccessRule rule)
		{
			ResetAccessRule ((AccessRule)rule);
		}
		
		public void SetAccessRule (CryptoKeyAccessRule rule)
		{
			SetAccessRule ((AccessRule)rule);
		}
		
		public override sealed AuditRule AuditRuleFactory (IdentityReference identityReference, int accessMask,
								   bool isInherited, InheritanceFlags inheritanceFlags,
								   PropagationFlags propagationFlags, AuditFlags flags)
		{
			return new CryptoKeyAuditRule (identityReference, (CryptoKeyRights) accessMask, flags);
		}
		
		public void AddAuditRule (CryptoKeyAuditRule rule)
		{
			AddAuditRule ((AuditRule)rule);
		}
		
		public bool RemoveAuditRule (CryptoKeyAuditRule rule)
		{
			return RemoveAuditRule((AuditRule)rule);
		}
		
		public void RemoveAuditRuleAll (CryptoKeyAuditRule rule)
		{
			RemoveAuditRuleAll((AuditRule)rule);
		}
		
		public void RemoveAuditRuleSpecific (CryptoKeyAuditRule rule)
		{
			RemoveAuditRuleSpecific((AuditRule)rule);
		}
		
		public void SetAuditRule (CryptoKeyAuditRule rule)
		{
			SetAuditRule((AuditRule)rule);
		}
	}
}

