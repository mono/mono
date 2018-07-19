//
// System.Security.AccessControl.SystemAcl implementation
//
// Authors:
//	Dick Porter  <dick@ximian.com>
//	Atsushi Enomoto  <atsushi@ximian.com>
//	James Bellinger  <jfb@zer7.com>
//
// Copyright (C) 2006-2007 Novell, Inc (http://www.novell.com)
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
	public sealed class SystemAcl : CommonAcl
	{
		public SystemAcl (bool isContainer, bool isDS, int capacity)
			: base (isContainer, isDS, capacity)
		{
		}
		
		public SystemAcl (bool isContainer, bool isDS, RawAcl rawAcl)
			: base (isContainer, isDS, rawAcl)
		{
		}
		
		public SystemAcl (bool isContainer, bool isDS, byte revision, int capacity)
			: base (isContainer, isDS, revision, capacity)
		{
		}

		public void AddAudit (AuditFlags auditFlags,
				      SecurityIdentifier sid, int accessMask,
				      InheritanceFlags inheritanceFlags,
				      PropagationFlags propagationFlags)
		{
			AddAce (AceQualifier.SystemAudit, sid, accessMask,
				inheritanceFlags, propagationFlags, auditFlags);
		}
		
		public void AddAudit (AuditFlags auditFlags,
				      SecurityIdentifier sid, int accessMask,
				      InheritanceFlags inheritanceFlags,
				      PropagationFlags propagationFlags,
				      ObjectAceFlags objectFlags,
				      Guid objectType,
				      Guid inheritedObjectType)
		{
			AddAce (AceQualifier.SystemAudit, sid, accessMask,
				inheritanceFlags, propagationFlags, auditFlags,
				objectFlags, objectType, inheritedObjectType);
		}

		public void AddAudit (SecurityIdentifier sid, ObjectAuditRule rule)
		{
			AddAudit (rule.AuditFlags, sid, rule.AccessMask, rule.InheritanceFlags, rule.PropagationFlags, rule.ObjectFlags, rule.ObjectType, rule.InheritedObjectType);
		}

		[MonoTODO]
		public bool RemoveAudit (AuditFlags auditFlags,
					 SecurityIdentifier sid,
					 int accessMask,
					 InheritanceFlags inheritanceFlags,
					 PropagationFlags propagationFlags)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public bool RemoveAudit (AuditFlags auditFlags,
					 SecurityIdentifier sid,
					 int accessMask,
					 InheritanceFlags inheritanceFlags,
					 PropagationFlags propagationFlags,
					 ObjectAceFlags objectFlags,
					 Guid objectType,
					 Guid inheritedObjectType)
		{
			throw new NotImplementedException ();
		}

		public bool RemoveAudit (SecurityIdentifier sid, ObjectAuditRule rule)
		{
			return RemoveAudit (rule.AuditFlags, sid, rule.AccessMask, rule.InheritanceFlags, rule.PropagationFlags, rule.ObjectFlags, rule.ObjectType, rule.InheritedObjectType);
		}

		public void RemoveAuditSpecific (AuditFlags auditFlags,
						 SecurityIdentifier sid,
						 int accessMask,
						 InheritanceFlags inheritanceFlags,
						 PropagationFlags propagationFlags)
		{
			RemoveAceSpecific (AceQualifier.SystemAudit, sid, accessMask,
					   inheritanceFlags, propagationFlags, auditFlags);

		}
		
		public void RemoveAuditSpecific (AuditFlags auditFlags,
						 SecurityIdentifier sid,
						 int accessMask,
						 InheritanceFlags inheritanceFlags,
						 PropagationFlags propagationFlags,
						 ObjectAceFlags objectFlags,
						 Guid objectType,
						 Guid inheritedObjectType)
		{
			RemoveAceSpecific (AceQualifier.SystemAudit, sid, accessMask,
					   inheritanceFlags, propagationFlags, auditFlags,
					   objectFlags, objectType, inheritedObjectType);

		}

		public void RemoveAuditSpecific (SecurityIdentifier sid, ObjectAuditRule rule)
		{
			RemoveAuditSpecific (rule.AuditFlags, sid, rule.AccessMask, rule.InheritanceFlags, rule.PropagationFlags, rule.ObjectFlags, rule.ObjectType, rule.InheritedObjectType);
		}

		public void SetAudit (AuditFlags auditFlags,
				      SecurityIdentifier sid,
				      int accessMask,
				      InheritanceFlags inheritanceFlags,
				      PropagationFlags propagationFlags)
		{
			SetAce (AceQualifier.SystemAudit, sid, accessMask,
				inheritanceFlags, propagationFlags, auditFlags);
		}
		
		public void SetAudit (AuditFlags auditFlags,
				      SecurityIdentifier sid,
				      int accessMask,
				      InheritanceFlags inheritanceFlags,
				      PropagationFlags propagationFlags,
				      ObjectAceFlags objectFlags,
				      Guid objectType,
				      Guid inheritedObjectType)
		{
			SetAce (AceQualifier.SystemAudit, sid, accessMask,
				inheritanceFlags, propagationFlags, auditFlags,
				objectFlags, objectType, inheritedObjectType);
		}

		public void SetAudit (SecurityIdentifier sid, ObjectAuditRule rule)
		{
			SetAudit (rule.AuditFlags, sid, rule.AccessMask, rule.InheritanceFlags, rule.PropagationFlags, rule.ObjectFlags, rule.ObjectType, rule.InheritedObjectType);
		}

		internal override void ApplyCanonicalSortToExplicitAces ()
		{
			int explicitCount = GetCanonicalExplicitAceCount ();
			ApplyCanonicalSortToExplicitAces (0, explicitCount);
		}
		
		internal override int GetAceInsertPosition (AceQualifier aceQualifier)
		{
			return 0;
		}
		
		internal override bool IsAceMeaningless (GenericAce ace)
		{
			if (base.IsAceMeaningless (ace)) return true;
			if (!IsValidAuditFlags (ace.AuditFlags)) return true;
			
			QualifiedAce qace = ace as QualifiedAce;
			if (null != qace) {
				if (!(AceQualifier.SystemAudit == qace.AceQualifier ||
				      AceQualifier.SystemAlarm == qace.AceQualifier)) return true;
			}
			
			return false;
		}
		
		static bool IsValidAuditFlags (AuditFlags auditFlags)
		{
			return auditFlags != AuditFlags.None &&
			       auditFlags == ((AuditFlags.Success|AuditFlags.Failure) & auditFlags);
		}
	}
}

