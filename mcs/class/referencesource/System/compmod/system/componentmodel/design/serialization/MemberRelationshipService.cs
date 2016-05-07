//------------------------------------------------------------------------------
// <copyright file="MemberRelationshipService.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------
namespace System.ComponentModel.Design.Serialization {
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Security.Permissions;

    /// <devdoc>
    ///    A member relationship service is used by a serializer to announce that one
    ///    property is related to a property on another object. Consider a code
    ///    based serialization scheme where code is of the following form:
    /// 
    ///    object1.Property1 = object2.Property2
    /// 
    ///    Upon interpretation of this code, Property1 on object1 will be
    ///    set to the return value of object2.Property2.  But the relationship
    ///    between these two objects is lost.  Serialization schemes that
    ///    wish to maintain this relationship may install a MemberRelationshipService
    ///    into the serialization manager.  When an object is deserialized
    ///    this serivce will be notified of these relationships.  It is up to the service
    ///    to act on these notifications if it wishes.  During serialization, the
    ///    service is also consulted.  If a relationship exists the same
    ///    relationship is maintained by the serializer.
    /// </devdoc>
    [HostProtection(SharedState = true)]
    public abstract class MemberRelationshipService
    {
        private Dictionary<RelationshipEntry,RelationshipEntry> _relationships = new Dictionary<RelationshipEntry,RelationshipEntry>();

        /// <devdoc>
        ///    Returns the the current relationship associated with the source, or MemberRelationship.Empty if
        ///    there is no relationship.  Also sets a relationship between two objects.  Empty
        ///    can also be passed as the property value, in which case the relationship will
        ///    be cleared.
        /// </devdoc>
        [SuppressMessage("Microsoft.Design", "CA1043:UseIntegralOrStringArgumentForIndexers")]
        public MemberRelationship this[MemberRelationship source] {
            get {
                if (source.Owner == null) throw new ArgumentNullException("Owner");
                if (source.Member== null) throw new ArgumentNullException("Member");

                return GetRelationship(source);
            }
            set {
                if (source.Owner == null) throw new ArgumentNullException("Owner");
                if (source.Member == null) throw new ArgumentNullException("Member");

                SetRelationship(source, value);
            }
        }

        /// <devdoc>
        ///    Returns the the current relationship associated with the source, or null if
        ///    there is no relationship.  Also sets a relationship between two objects.  Null
        ///    can be passed as the property value, in which case the relationship will
        ///    be cleared.
        /// </devdoc>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1023:IndexersShouldNotBeMultidimensional")]
        public MemberRelationship this[object sourceOwner, MemberDescriptor sourceMember] {
            get {
                if (sourceOwner == null) throw new ArgumentNullException("sourceOwner");
                if (sourceMember == null) throw new ArgumentNullException("sourceMember");

                return GetRelationship(new MemberRelationship(sourceOwner, sourceMember));
            }
            set {
                if (sourceOwner == null) throw new ArgumentNullException("sourceOwner");
                if (sourceMember == null) throw new ArgumentNullException("sourceMember");

                SetRelationship(new MemberRelationship(sourceOwner, sourceMember), value);
            }
        }

        /// <devdoc>
        ///    This is the implementation API for returning relationships.  The default implementation stores the 
        ///    relationship in a table.  Relationships are stored weakly, so they do not keep an object alive.
        /// </devdoc>
        protected virtual MemberRelationship GetRelationship(MemberRelationship source) {
            RelationshipEntry retVal;

            if (_relationships != null && _relationships.TryGetValue(new RelationshipEntry(source), out retVal) && retVal.Owner.IsAlive) {
                return new MemberRelationship(retVal.Owner.Target, retVal.Member);
            }

            return MemberRelationship.Empty;
        }

        /// <devdoc>
        ///    This is the implementation API for returning relationships.  The default implementation stores the 
        ///    relationship in a table.  Relationships are stored weakly, so they do not keep an object alive.  Empty can be
        ///    passed in for relationship to remove the relationship.
        /// </devdoc>
        protected virtual void SetRelationship(MemberRelationship source, MemberRelationship relationship) {

            if (!relationship.IsEmpty && !SupportsRelationship(source, relationship)) {
                string sourceName = TypeDescriptor.GetComponentName(source.Owner);
                string relName = TypeDescriptor.GetComponentName(relationship.Owner);
                if (sourceName == null) {
                    sourceName = source.Owner.ToString();
                }
                if (relName == null) {
                    relName = relationship.Owner.ToString();
                }
                throw new ArgumentException(SR.GetString(SR.MemberRelationshipService_RelationshipNotSupported, sourceName, source.Member.Name, relName, relationship.Member.Name));
            }

            if (_relationships == null) {
                _relationships = new Dictionary<RelationshipEntry,RelationshipEntry>();
            }

            _relationships[new RelationshipEntry(source)] = new RelationshipEntry(relationship);
        }

        /// <devdoc>
        ///    Returns true if the provided relatinoship is supported.
        /// </devdoc>
        public abstract bool SupportsRelationship(MemberRelationship source, MemberRelationship relationship);

        /// <devdoc>
        ///    Used as storage in our relationship table
        /// </devdoc>
        private struct RelationshipEntry {
            internal WeakReference Owner;
            internal MemberDescriptor Member;
            private int hashCode;

            internal RelationshipEntry(MemberRelationship rel) {
                Owner = new WeakReference(rel.Owner);
                Member = rel.Member;
                hashCode = rel.Owner == null ? 0 : rel.Owner.GetHashCode();
            }


            public override bool Equals(object o) {
                if (o is RelationshipEntry) {
                    RelationshipEntry e = (RelationshipEntry)o;
                    return this == e;
                }

                return false;
            }

            public static bool operator==(RelationshipEntry re1, RelationshipEntry re2){
                object owner1 = (re1.Owner.IsAlive ? re1.Owner.Target : null);
                object owner2 = (re2.Owner.IsAlive ? re2.Owner.Target : null);
                return owner1 == owner2 && re1.Member.Equals(re2.Member);
            }

            public static bool operator!=(RelationshipEntry re1, RelationshipEntry re2){
                return !(re1 == re2);
            }

            public override int GetHashCode() {
                return hashCode;
            }
        }
    }

    /// <devdoc>
    ///    This class represents a single relationship between an object and a member.
    /// </devdoc>
    public struct MemberRelationship {
        private object _owner;
        private MemberDescriptor _member;

        public static readonly MemberRelationship Empty = new MemberRelationship();

        /// <devdoc>
        ///    Creates a new member relationship.
        /// </devdoc>
        public MemberRelationship(object owner, MemberDescriptor member) {
            if (owner == null) throw new ArgumentNullException("owner");
            if (member == null) throw new ArgumentNullException("member");

            _owner = owner;
            _member = member;
        }

        /// <devdoc>
        ///    Returns true if this relationship is empty.
        /// </devdoc>
        public bool IsEmpty {
            get {
                return _owner == null;
            }
        }

        /// <devdoc>
        ///    The member in this relationship.
        /// </devdoc>
        public MemberDescriptor Member {
            get {
                return _member;
            }
        }

        /// <devdoc>
        ///    The object owning the member.
        /// </devdoc>
        public object Owner {
            get {
                return _owner;
            }
        }

        /// <devdoc>
        ///    Infrastructure support to make this a first class struct
        /// </devdoc>
        public override bool Equals(object obj) {
            if (!(obj is MemberRelationship))
                return false;

            MemberRelationship rel = (MemberRelationship)obj;
            return rel.Owner == Owner && rel.Member == Member;
        }

        /// <devdoc>
        ///    Infrastructure support to make this a first class struct
        /// </devdoc>
        public override int GetHashCode() {
            if (_owner == null) return base.GetHashCode();
            return _owner.GetHashCode() ^ _member.GetHashCode();
        }
        /// <devdoc>
        ///    Infrastructure support to make this a first class struct
        /// </devdoc>
        public static bool operator ==(MemberRelationship left, MemberRelationship right) {
            return left.Owner == right.Owner && left.Member == right.Member;
        }

        /// <devdoc>
        ///    Infrastructure support to make this a first class struct
        /// </devdoc>
        public static bool operator !=(MemberRelationship left, MemberRelationship right) {
            return !(left == right);
        }
    }
}
