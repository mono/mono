//
// System.Security.AccessControl.SystemAcl implementation
//
// Authors:
//	Dick Porter  <dick@ximian.com>
//	Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2006-2007 Novell, Inc (http://www.novell.com)
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
	public sealed class SystemAcl : CommonAcl
	{
		public SystemAcl (bool isContainer, bool isDS, int capacity)
			: this (isContainer, isDS, AclRevision, capacity)
		{
		}
		
		public SystemAcl (bool isContainer, bool isDS, RawAcl rawAcl)
			: base (isContainer, isDS,
			        (rawAcl == null) ? AclRevision : rawAcl.Revision,
			        0)
		{
			if (rawAcl != null)
			{
				foreach (var ace in rawAcl)
					RawAcl.InsertAce(RawAcl.Count, ace);
			}
		}
		
		public SystemAcl (bool isContainer, bool isDS, byte revision,
				  int capacity)
			: base (isContainer, isDS, revision, capacity)
		{
		}

		public void AddAudit (AuditFlags auditFlags,
				      SecurityIdentifier sid, int accessMask,
				      InheritanceFlags inheritanceFlags,
				      PropagationFlags propagationFlags)
		{
			// CommonAce?
			throw new NotImplementedException ();
		}
		
		public void AddAudit (AuditFlags auditFlags,
				      SecurityIdentifier sid, int accessMask,
				      InheritanceFlags inheritanceFlags,
				      PropagationFlags propagationFlags,
				      ObjectAceFlags objectFlags,
				      Guid objectType,
				      Guid inheritedObjectType)
		{
			// ObjectAce?
			throw new NotImplementedException ();
		}
		
		public bool RemoveAudit (AuditFlags auditFlags,
					 SecurityIdentifier sid,
					 int accessMask,
					 InheritanceFlags inheritanceFlags,
					 PropagationFlags propagationFlags)
		{
			throw new NotImplementedException ();
		}
		
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
		
		public void RemoveAuditSpecific (AuditFlags auditFlags,
						 SecurityIdentifier sid,
						 int accessMask,
						 InheritanceFlags inheritanceFlags,
						 PropagationFlags propagationFlags)
		{
			throw new NotImplementedException ();
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
			throw new NotImplementedException ();
		}
		
		public void SetAudit (AuditFlags auditFlags,
				      SecurityIdentifier sid,
				      int accessMask,
				      InheritanceFlags inheritanceFlags,
				      PropagationFlags propagationFlags)
		{
			throw new NotImplementedException ();
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
			throw new NotImplementedException ();
		}

		internal override string GetSddlForm(ControlFlags sdFlags, bool isDacl)
		{
			throw new NotImplementedException();
		}
	}
}

