//
// System.Security.AccessControl.DiscretionaryAcl implementation
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
	public sealed class DiscretionaryAcl : CommonAcl
	{
		public DiscretionaryAcl (bool isContainer, bool isDS, int capacity)
			: base (isContainer, isDS, capacity)
		{
		}
		
		public DiscretionaryAcl (bool isContainer, bool isDS, RawAcl rawAcl)
			: base (isContainer, isDS, rawAcl)
		{
		}
		
		public DiscretionaryAcl (bool isContainer, bool isDS, byte revision, int capacity)
			: base (isContainer, isDS, revision, capacity)
		{
		}

		public void AddAccess (AccessControlType accessType,
				       SecurityIdentifier sid, int accessMask,
				       InheritanceFlags inheritanceFlags,
				       PropagationFlags propagationFlags)
		{
			QualifiedAce ace = AddAccessGetQualifiedAce (accessType, sid, accessMask,
								     inheritanceFlags, propagationFlags);
			AddAccess (ace);
		}
		
		public void AddAccess (AccessControlType accessType,
				       SecurityIdentifier sid, int accessMask,
				       InheritanceFlags inheritanceFlags,
				       PropagationFlags propagationFlags,
				       ObjectAceFlags objectFlags,
				       Guid objectType,
				       Guid inheritedObjectType)
		{
			QualifiedAce ace = AddAccessGetQualifiedAce (accessType, sid, accessMask,
								     inheritanceFlags, propagationFlags,
								     objectFlags, objectType, inheritedObjectType);
			AddAccess (ace);
		}

		QualifiedAce AddAccessGetQualifiedAce (AccessControlType accessType,
						       SecurityIdentifier sid, int accessMask,
						       InheritanceFlags inheritanceFlags,
						       PropagationFlags propagationFlags,
						       ObjectAceFlags objectFlags,
						       Guid objectType,
						       Guid inheritedObjectType)
		{
			if (!IsDS)
				throw new InvalidOperationException ("For this overload, IsDS must be true.");
				
			if (ObjectAceFlags.None == objectFlags)
				return AddAccessGetQualifiedAce (accessType, sid, accessMask, inheritanceFlags, propagationFlags);
			
			AceQualifier qualifier = GetAceQualifier (accessType);
			AceFlags flags = GetAceFlags (inheritanceFlags, propagationFlags);
			return new ObjectAce (flags, qualifier, accessMask, sid,
					      objectFlags, objectType, inheritedObjectType, false, null);
		}
		
		QualifiedAce AddAccessGetQualifiedAce (AccessControlType accessType,
						       SecurityIdentifier sid, int accessMask,
						       InheritanceFlags inheritanceFlags,
						       PropagationFlags propagationFlags)
		{
			AceQualifier qualifier = GetAceQualifier (accessType);
			AceFlags flags = GetAceFlags (inheritanceFlags, propagationFlags);
			return new CommonAce (flags, qualifier, accessMask, sid, false, null);
		}
		
		void AddAccess (QualifiedAce newAce)
		{
			RequireCanonicity ();
				
			int pos; // Canonical order is explicit deny, explicit allow, inherited.
			if (AceQualifier.AccessAllowed == newAce.AceQualifier)
				pos = GetCanonicalExplicitDenyAceCount ();
			else
				pos = 0;
			
			raw_acl.InsertAce (pos, newAce);
			CleanAndRetestCanonicity ();
		}
		
		public bool RemoveAccess (AccessControlType accessType,
					  SecurityIdentifier sid,
					  int accessMask,
					  InheritanceFlags inheritanceFlags,
					  PropagationFlags propagationFlags)
		{
			throw new NotImplementedException ();
		}
		
		public bool RemoveAccess (AccessControlType accessType,
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
		
		public void RemoveAccessSpecific (AccessControlType accessType,
						  SecurityIdentifier sid,
						  int accessMask,
						  InheritanceFlags inheritanceFlags,
						  PropagationFlags propagationFlags)
		{
			RequireCanonicity ();
			AceQualifier qualifier = GetAceQualifier (accessType);
			RemoveAces<CommonAce> (ace =>
			{
				if (ace.AccessMask != accessMask) return false;
				if (ace.AceQualifier != qualifier) return false;
				if (ace.SecurityIdentifier != sid) return false;
				if (ace.InheritanceFlags != inheritanceFlags) return false;
				if (InheritanceFlags.None != inheritanceFlags)
					if (ace.PropagationFlags != propagationFlags) return false;
				return true;
			});
			CleanAndRetestCanonicity ();
		}
		
		public void RemoveAccessSpecific (AccessControlType accessType,
						  SecurityIdentifier sid,
						  int accessMask,
						  InheritanceFlags inheritanceFlags,
						  PropagationFlags propagationFlags,
						  ObjectAceFlags objectFlags,
						  Guid objectType,
						  Guid inheritedObjectType)
		{
			if (!IsDS)
				throw new InvalidOperationException ("For this overload, IsDS must be true.");
				
			if (ObjectAceFlags.None == objectFlags) {
				RemoveAccessSpecific (accessType, sid, accessMask, inheritanceFlags, propagationFlags);
				return;
			}

			RequireCanonicity ();
			AceQualifier qualifier = GetAceQualifier (accessType);
			RemoveAces<ObjectAce> (ace =>
			{
				if (ace.AccessMask != accessMask) return false;
				if (ace.AceQualifier != qualifier) return false;
				if (ace.SecurityIdentifier != sid) return false;
				if (ace.InheritanceFlags != inheritanceFlags) return false;
				if (InheritanceFlags.None != inheritanceFlags)
					if (ace.PropagationFlags != propagationFlags) return false;
				if (ace.ObjectAceFlags != objectFlags) return false;
				if (0 != (objectFlags & ObjectAceFlags.ObjectAceTypePresent))
					if (ace.ObjectAceType != objectType) return false;
				if (0 != (objectFlags & ObjectAceFlags.InheritedObjectAceTypePresent))
					if (ace.InheritedObjectAceType != objectType) return false;
				return true;
			});
			CleanAndRetestCanonicity ();
		}
		
		public void SetAccess (AccessControlType accessType,
				       SecurityIdentifier sid,
				       int accessMask,
				       InheritanceFlags inheritanceFlags,
				       PropagationFlags propagationFlags)
		{
			QualifiedAce ace = AddAccessGetQualifiedAce (accessType, sid, accessMask,
								     inheritanceFlags, propagationFlags);
			SetAccess (ace);
		}
		
		public void SetAccess (AccessControlType accessType,
				       SecurityIdentifier sid,
				       int accessMask,
				       InheritanceFlags inheritanceFlags,
				       PropagationFlags propagationFlags,
				       ObjectAceFlags objectFlags,
				       Guid objectType,
				       Guid inheritedObjectType)
		{
			QualifiedAce ace = AddAccessGetQualifiedAce (accessType, sid, accessMask,
								     inheritanceFlags, propagationFlags,
								     objectFlags, objectType, inheritedObjectType);
			SetAccess (ace);
		}

		void SetAccess (QualifiedAce newAce)
		{
			RequireCanonicity ();
			
			RemoveAces<QualifiedAce> (oldAce =>
			{
				return oldAce.AceQualifier == newAce.AceQualifier &&
				       oldAce.SecurityIdentifier == newAce.SecurityIdentifier;
			});
			CleanAndRetestCanonicity ();
						
			AddAccess (newAce);
		}

		internal override void ApplyCanonicalSortToExplicitAces ()
		{
			int explicitCount = GetCanonicalExplicitAceCount ();
			int explicitDenys = GetCanonicalExplicitDenyAceCount ();

			ApplyCanonicalSortToExplicitAces (0, explicitDenys);
			ApplyCanonicalSortToExplicitAces (explicitDenys, explicitCount - explicitDenys);
		}
		
		internal override bool IsAceMeaningless (GenericAce ace)
		{
			if (base.IsAceMeaningless (ace)) return true;
			
			QualifiedAce qace = ace as QualifiedAce;
			if (null != qace) {
				return !(AceQualifier.AccessAllowed == qace.AceQualifier ||
				         AceQualifier.AccessDenied  == qace.AceQualifier);
			}

			return false;
		}
		
		AceFlags GetAceFlags (InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags)
		{
			if (InheritanceFlags.None != inheritanceFlags && !IsContainer)
				throw new ArgumentException ("Flags only work with containers.", "inheritanceFlags");
			
			if (InheritanceFlags.None == inheritanceFlags && PropagationFlags.None != propagationFlags)
				throw new ArgumentException ("Propagation flags need inheritance flags.", "propagationFlags");
			
			AceFlags flags = AceFlags.None;
			if (0 != ((InheritanceFlags.ContainerInherit) & inheritanceFlags))
				flags |= AceFlags.ContainerInherit;
			if (0 != ((InheritanceFlags.ObjectInherit) & inheritanceFlags))
				flags |= AceFlags.ObjectInherit;
			if (0 != ((PropagationFlags.InheritOnly) & propagationFlags))
				flags |= AceFlags.InheritOnly;
			if (0 != ((PropagationFlags.NoPropagateInherit) & propagationFlags))
				flags |= AceFlags.NoPropagateInherit;
			return flags;
		}
		
		static AceQualifier GetAceQualifier (AccessControlType accessType)
		{
			if (AccessControlType.Allow == accessType)
				return AceQualifier.AccessAllowed;
			else if (AccessControlType.Deny == accessType)
				return AceQualifier.AccessDenied;
			else
				throw new ArgumentOutOfRangeException ("accessType");
		}
	}
}

