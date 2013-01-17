//
// System.Security.AccessControl.CommonAcl implementation
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

using System.Collections.Generic;
using System.Security.Principal;

namespace System.Security.AccessControl
{
	/* NB: Note the Remarks section in the CommonAcl class docs
	 * concerning ACE management
	 */
	public abstract class CommonAcl : GenericAcl
	{
		const int default_capacity = 10; // FIXME: not verified

		internal delegate bool RemoveAcesCallback<T> (T ace);

		internal CommonAcl (bool isContainer, bool isDS, RawAcl rawAcl)
		{
			if (rawAcl == null) {
				rawAcl = new RawAcl (isDS ? AclRevisionDS : AclRevision, default_capacity);
			} else {
				// The RawAcl ACEs are cloned.
				byte[] binaryForm = new byte [rawAcl.BinaryLength];
				rawAcl.GetBinaryForm (binaryForm, 0);
				rawAcl = new RawAcl (binaryForm, 0);
			}

			Init (isContainer, isDS, rawAcl);
		}

		internal CommonAcl (bool isContainer, bool isDS, byte revision, int capacity)
		{
			Init (isContainer, isDS, new RawAcl (revision, capacity));
		}

		internal CommonAcl (bool isContainer, bool isDS, int capacity)
			: this (isContainer, isDS, isDS ? AclRevisionDS : AclRevision, capacity)
		{
		}

		void Init (bool isContainer, bool isDS, RawAcl rawAcl)
		{
			is_container = isContainer;
			is_ds = isDS;
			raw_acl = rawAcl;
			CanonicalizeAndClearAefa ();
		}

		bool is_aefa, is_canonical, is_container, is_ds;
		internal RawAcl raw_acl;

		public override sealed int BinaryLength {
			get { return raw_acl.BinaryLength; }
		}

		public override sealed int Count {
			get { return raw_acl.Count; }
		}

		public bool IsCanonical {
			get { return is_canonical; }
		}

		public bool IsContainer {
			get { return is_container; }
		}
		
		public bool IsDS {
			get { return is_ds; }
		}

		// See CommonSecurityDescriptorTest's AefaModifiedFlagIsStoredOnDiscretionaryAcl unit test.
		internal bool IsAefa {
			get { return is_aefa; }
			set { is_aefa = value; }
		}
		
		public override sealed byte Revision {
			get { return raw_acl.Revision; }
		}
		
		public override sealed GenericAce this[int index] {
			get { return CopyAce (raw_acl [index]); }
			set { throw new NotSupportedException (); }
		}
		
		public override sealed void GetBinaryForm (byte[] binaryForm, int offset)
		{
			raw_acl.GetBinaryForm (binaryForm, offset);
		}
		
		public void Purge (SecurityIdentifier sid)
		{
			RequireCanonicity ();
			RemoveAces<KnownAce> (ace => ace.SecurityIdentifier == sid);
		}

		public void RemoveInheritedAces ()
		{
			RequireCanonicity ();
			RemoveAces<GenericAce> (ace => ace.IsInherited);
		}

		internal void RequireCanonicity ()
		{
			if (!IsCanonical)
				throw new InvalidOperationException("ACL is not canonical.");
		}
		
		internal void CanonicalizeAndClearAefa ()
		{
			RemoveAces<GenericAce> (IsAceMeaningless);

			is_canonical = TestCanonicity ();
			
			if (IsCanonical) {
				ApplyCanonicalSortToExplicitAces ();
				MergeExplicitAces ();
			}
			
			IsAefa = false;
		}
		
		internal virtual bool IsAceMeaningless (GenericAce ace)
		{
			AceFlags flags = ace.AceFlags;

			KnownAce knownAce = ace as KnownAce;
			if (knownAce != null) {
				if (0 == knownAce.AccessMask) return true;
				if (0 != (flags & AceFlags.InheritOnly)) {
					if (knownAce is ObjectAce) return true;
					if (!IsContainer) return true;
					if (0 == (flags & (AceFlags.ContainerInherit|AceFlags.ObjectInherit))) return true;
				}
			}

			return false;
		}

		bool TestCanonicity ()
		{
			foreach (GenericAce ace in this) {
				if (!(ace is QualifiedAce)) return false;
			}

			bool gotInheritedAce = false;
			foreach (QualifiedAce ace in this) {
				if (ace.IsInherited) {
					gotInheritedAce = true;
				} else {
					if (gotInheritedAce) return false;
				}
			}

			bool gotExplicitAllow = false;
			foreach (QualifiedAce ace in this) {
				if (ace.IsInherited) break;
				if (AceQualifier.AccessAllowed == ace.AceQualifier) {
					gotExplicitAllow = true;
				} else if (AceQualifier.AccessDenied == ace.AceQualifier) {
					if (gotExplicitAllow) return false;
				}
			}

			return true;
		}

		internal int GetCanonicalExplicitDenyAceCount ()
		{
			int i;
			for (i = 0; i < Count; i ++) {
				if (raw_acl [i].IsInherited) break;

				QualifiedAce ace = raw_acl [i] as QualifiedAce;
				if (ace == null || ace.AceQualifier != AceQualifier.AccessDenied) break;
			}
			return i;
		}
		
		internal int GetCanonicalExplicitAceCount ()
		{
			int i;
			for (i = 0; i < Count; i ++)
				if (raw_acl [i].IsInherited) break;
			return i;
		}
		
		void MergeExplicitAces ()
		{
			int explicitCount = GetCanonicalExplicitAceCount ();
			
			for (int i = 0; i < explicitCount - 1; ) {
				GenericAce mergedAce = MergeExplicitAcePair (raw_acl [i], raw_acl [i + 1]);
				if (null != mergedAce) {
					raw_acl [i] = mergedAce;
					raw_acl.RemoveAce (i + 1);
					explicitCount --;
				} else {
					i ++;
				}
			}
		}

		GenericAce MergeExplicitAcePair (GenericAce ace1, GenericAce ace2)
		{
			QualifiedAce qace1 = ace1 as QualifiedAce;
			QualifiedAce qace2 = ace2 as QualifiedAce;
			if (!(null != qace1 && null != qace2)) return null;
			if (!(qace1.AceQualifier == qace2.AceQualifier)) return null;
			if (!(qace1.SecurityIdentifier == qace2.SecurityIdentifier)) return null;
			
			AceFlags aceFlags1 = qace1.AceFlags, aceFlags2 = qace2.AceFlags, aceFlagsNew;
			int accessMask1 = qace1.AccessMask, accessMask2 = qace2.AccessMask, accessMaskNew;
			
			if (!IsContainer) {
				aceFlags1 &= ~AceFlags.InheritanceFlags;
				aceFlags2 &= ~AceFlags.InheritanceFlags;
			}
			
			if (aceFlags1 != aceFlags2) {
				if (accessMask1 != accessMask2) return null;
				if ((aceFlags1 & ~(AceFlags.ContainerInherit|AceFlags.ObjectInherit)) ==
				    (aceFlags2 & ~(AceFlags.ContainerInherit|AceFlags.ObjectInherit))) {
					aceFlagsNew = aceFlags1|aceFlags2; // merge InheritanceFlags
					accessMaskNew = accessMask1;
				} else if ((aceFlags1 & ~(AceFlags.SuccessfulAccess|AceFlags.FailedAccess)) ==
					   (aceFlags2 & ~(AceFlags.SuccessfulAccess|AceFlags.FailedAccess))) {
					aceFlagsNew = aceFlags1|aceFlags2; // merge AuditFlags
					accessMaskNew = accessMask1;
				} else {
					return null;
				}
			} else {
				aceFlagsNew = aceFlags1;
				accessMaskNew = accessMask1|accessMask2;
			}
			
			CommonAce cace1 = ace1 as CommonAce;
			CommonAce cace2 = ace2 as CommonAce;
			if (null != cace1 && null != cace2) {
				return new CommonAce (aceFlagsNew, cace1.AceQualifier, accessMaskNew,
					cace1.SecurityIdentifier, cace1.IsCallback, cace1.GetOpaque());
			}
			
			ObjectAce oace1 = ace1 as ObjectAce;
			ObjectAce oace2 = ace2 as ObjectAce;
			if (null != oace1 && null != oace2) {
				// See DiscretionaryAclTest.GuidEmptyMergesRegardlessOfFlagsAndOpaqueDataIsNotConsidered
				Guid type1, inheritedType1; GetObjectAceTypeGuids(oace1, out type1, out inheritedType1);
				Guid type2, inheritedType2; GetObjectAceTypeGuids(oace2, out type2, out inheritedType2);
				
				if (type1 == type2 && inheritedType1 == inheritedType2) {
					return new ObjectAce (aceFlagsNew, oace1.AceQualifier, accessMaskNew,
						oace1.SecurityIdentifier,
						oace1.ObjectAceFlags, oace1.ObjectAceType, oace1.InheritedObjectAceType,
						oace1.IsCallback, oace1.GetOpaque());
				}
			}
			
			return null;
		}
		
		static void GetObjectAceTypeGuids(ObjectAce ace, out Guid type, out Guid inheritedType)
		{
			type = Guid.Empty; inheritedType = Guid.Empty;
			if (0 != (ace.ObjectAceFlags & ObjectAceFlags.ObjectAceTypePresent))
				type = ace.ObjectAceType;
			if (0 != (ace.ObjectAceFlags & ObjectAceFlags.InheritedObjectAceTypePresent))
				inheritedType = ace.InheritedObjectAceType;
		}

		internal abstract void ApplyCanonicalSortToExplicitAces ();
		
		internal void ApplyCanonicalSortToExplicitAces (int start, int count)
		{
			int i, j;
			for (i = start + 1; i < start + count; i ++)
			{
				KnownAce ace = (KnownAce)raw_acl [i];
				SecurityIdentifier sid = ace.SecurityIdentifier;
				for (j = i; j > start && ((KnownAce)raw_acl [j - 1]).SecurityIdentifier.CompareTo (sid) > 0; j --)
					raw_acl [j] = raw_acl [j - 1];
				raw_acl [j] = ace;
			}
		}
		
		internal override string GetSddlForm (ControlFlags sdFlags, bool isDacl)
		{
			return raw_acl.GetSddlForm (sdFlags, isDacl);
		}

		internal void RemoveAces<T> (RemoveAcesCallback<T> callback)
			where T : GenericAce
		{
			for (int i = 0; i < raw_acl.Count; ) {
				if (raw_acl [i] is T && callback ((T)raw_acl [i])) {
					raw_acl.RemoveAce (i);
				} else {
					i ++;
				}
			}
		}
		
		// DiscretionaryAcl/SystemAcl shared implementation below...
		internal void AddAce (AceQualifier aceQualifier,
				      SecurityIdentifier sid, int accessMask,
				      InheritanceFlags inheritanceFlags,
				      PropagationFlags propagationFlags,
				      AuditFlags auditFlags)
		{
			QualifiedAce ace = AddAceGetQualifiedAce (aceQualifier, sid, accessMask,
								  inheritanceFlags, propagationFlags, auditFlags);
			AddAce (ace);
		}
		
		internal void AddAce (AceQualifier aceQualifier,
				      SecurityIdentifier sid, int accessMask,
				      InheritanceFlags inheritanceFlags,
				      PropagationFlags propagationFlags,
				      AuditFlags auditFlags,
				      ObjectAceFlags objectFlags,
				      Guid objectType,
				      Guid inheritedObjectType)
		{
			QualifiedAce ace = AddAceGetQualifiedAce (aceQualifier, sid, accessMask,
								  inheritanceFlags, propagationFlags, auditFlags,
								  objectFlags, objectType, inheritedObjectType);
			AddAce (ace);
		}
		
		QualifiedAce AddAceGetQualifiedAce (AceQualifier aceQualifier,
						    SecurityIdentifier sid, int accessMask,
						    InheritanceFlags inheritanceFlags,
						    PropagationFlags propagationFlags,
						    AuditFlags auditFlags,
						    ObjectAceFlags objectFlags,
						    Guid objectType,
						    Guid inheritedObjectType)
		{
			if (!IsDS)
				throw new InvalidOperationException ("For this overload, IsDS must be true.");
				
			if (ObjectAceFlags.None == objectFlags)
				return AddAceGetQualifiedAce (aceQualifier, sid, accessMask,
							      inheritanceFlags, propagationFlags, auditFlags);
			
			AceFlags flags = GetAceFlags (inheritanceFlags, propagationFlags, auditFlags);
			return new ObjectAce (flags, aceQualifier, accessMask, sid,
					      objectFlags, objectType, inheritedObjectType, false, null);
		}
		
		QualifiedAce AddAceGetQualifiedAce (AceQualifier aceQualifier,
						    SecurityIdentifier sid, int accessMask,
						    InheritanceFlags inheritanceFlags,
						    PropagationFlags propagationFlags,
						    AuditFlags auditFlags)
		{
			AceFlags flags = GetAceFlags (inheritanceFlags, propagationFlags, auditFlags);
			return new CommonAce (flags, aceQualifier, accessMask, sid, false, null);
		}
		
		void AddAce (QualifiedAce newAce)
		{
			RequireCanonicity ();
				
			int pos = GetAceInsertPosition (newAce.AceQualifier);
			raw_acl.InsertAce (pos, CopyAce (newAce));
			CanonicalizeAndClearAefa ();
		}
		
		static GenericAce CopyAce (GenericAce ace)
		{
			byte[] binaryForm = new byte[ace.BinaryLength];
			ace.GetBinaryForm (binaryForm, 0);
			return GenericAce.CreateFromBinaryForm (binaryForm, 0);
		}
		
		internal abstract int GetAceInsertPosition (AceQualifier aceQualifier);
		
		AceFlags GetAceFlags (InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags auditFlags)
		{
			if (InheritanceFlags.None != inheritanceFlags && !IsContainer)
				throw new ArgumentException ("Flags only work with containers.", "inheritanceFlags");
			
			if (InheritanceFlags.None == inheritanceFlags && PropagationFlags.None != propagationFlags)
				throw new ArgumentException ("Propagation flags need inheritance flags.", "propagationFlags");
			
			AceFlags flags = AceFlags.None;
			if (0 != (InheritanceFlags.ContainerInherit & inheritanceFlags))
				flags |= AceFlags.ContainerInherit;
			if (0 != (InheritanceFlags.ObjectInherit & inheritanceFlags))
				flags |= AceFlags.ObjectInherit;
			if (0 != (PropagationFlags.InheritOnly & propagationFlags))
				flags |= AceFlags.InheritOnly;
			if (0 != (PropagationFlags.NoPropagateInherit & propagationFlags))
				flags |= AceFlags.NoPropagateInherit;
			if (0 != (AuditFlags.Success & auditFlags))
				flags |= AceFlags.SuccessfulAccess;
			if (0 != (AuditFlags.Failure & auditFlags))
				flags |= AceFlags.FailedAccess;
			return flags;
		}
		
		internal void RemoveAceSpecific (AceQualifier aceQualifier,
						 SecurityIdentifier sid,
						 int accessMask,
						 InheritanceFlags inheritanceFlags,
						 PropagationFlags propagationFlags,
						 AuditFlags auditFlags)
		{
			RequireCanonicity ();
			RemoveAces<CommonAce> (ace =>
			{
				if (ace.AccessMask != accessMask) return false;
				if (ace.AceQualifier != aceQualifier) return false;
				if (ace.SecurityIdentifier != sid) return false;
				if (ace.InheritanceFlags != inheritanceFlags) return false;
				if (InheritanceFlags.None != inheritanceFlags)
					if (ace.PropagationFlags != propagationFlags) return false;
				if (ace.AuditFlags != auditFlags) return false;
				return true;
			});
			CanonicalizeAndClearAefa ();
		}
		
		internal void RemoveAceSpecific (AceQualifier aceQualifier,
						 SecurityIdentifier sid,
						 int accessMask,
						 InheritanceFlags inheritanceFlags,
						 PropagationFlags propagationFlags,
						 AuditFlags auditFlags,
						 ObjectAceFlags objectFlags,
						 Guid objectType,
						 Guid inheritedObjectType)
		{
			if (!IsDS)
				throw new InvalidOperationException ("For this overload, IsDS must be true.");
				
			if (ObjectAceFlags.None == objectFlags) {
				RemoveAceSpecific (aceQualifier, sid, accessMask, inheritanceFlags, propagationFlags, auditFlags);
				return;
			}

			RequireCanonicity ();
			RemoveAces<ObjectAce> (ace =>
			{
				if (ace.AccessMask != accessMask) return false;
				if (ace.AceQualifier != aceQualifier) return false;
				if (ace.SecurityIdentifier != sid) return false;
				if (ace.InheritanceFlags != inheritanceFlags) return false;
				if (InheritanceFlags.None != inheritanceFlags)
					if (ace.PropagationFlags != propagationFlags) return false;
				if (ace.AuditFlags != auditFlags) return false;
				if (ace.ObjectAceFlags != objectFlags) return false;
				if (0 != (objectFlags & ObjectAceFlags.ObjectAceTypePresent))
					if (ace.ObjectAceType != objectType) return false;
				if (0 != (objectFlags & ObjectAceFlags.InheritedObjectAceTypePresent))
					if (ace.InheritedObjectAceType != objectType) return false;
				return true;
			});
			CanonicalizeAndClearAefa ();
		}
		
		internal void SetAce (AceQualifier aceQualifier,
				      SecurityIdentifier sid,
				      int accessMask,
				      InheritanceFlags inheritanceFlags,
				      PropagationFlags propagationFlags,
				      AuditFlags auditFlags)
		{
			QualifiedAce ace = AddAceGetQualifiedAce (aceQualifier, sid, accessMask,
								  inheritanceFlags, propagationFlags, auditFlags);
			SetAce (ace);
		}
		
		internal void SetAce (AceQualifier aceQualifier,
				      SecurityIdentifier sid,
				      int accessMask,
				      InheritanceFlags inheritanceFlags,
				      PropagationFlags propagationFlags,
				      AuditFlags auditFlags,
				      ObjectAceFlags objectFlags,
				      Guid objectType,
				      Guid inheritedObjectType)
		{
			QualifiedAce ace = AddAceGetQualifiedAce (aceQualifier, sid, accessMask,
								  inheritanceFlags, propagationFlags, auditFlags,
								  objectFlags, objectType, inheritedObjectType);
			SetAce (ace);
		}
		
		void SetAce (QualifiedAce newAce)
		{
			RequireCanonicity ();
			
			RemoveAces<QualifiedAce> (oldAce =>
			{
				return oldAce.AceQualifier == newAce.AceQualifier &&
				       oldAce.SecurityIdentifier == newAce.SecurityIdentifier;
			});
			CanonicalizeAndClearAefa ();
						
			AddAce (newAce);
		}
	}
}

