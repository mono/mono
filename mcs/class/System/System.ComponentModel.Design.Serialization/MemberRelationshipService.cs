//
// System.ComponentModel.Design.Serialization.MemberRelationshipService
//
// Authors:	 
//	  Ivan N. Zlatev (contact@i-nZ.net)
//
// (C) 2007 Ivan N. Zlatev

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
using System.CodeDom;
using System.ComponentModel;
using System.Collections;

namespace System.ComponentModel.Design.Serialization
{
	public abstract class MemberRelationshipService
	{

		// MSDN: The default implementation stores relationships in a dictionary using weak references
		//       so the relationship table does not keep objects alive.
		// 
		private class MemberRelationshipWeakEntry
		{

			private WeakReference _ownerWeakRef;
			private MemberDescriptor _member;

			public MemberRelationshipWeakEntry (MemberRelationship relation)
			{
				_ownerWeakRef = new WeakReference (relation.Owner);
				_member = relation.Member;
			}

			public object Owner {
				get {
					if (_ownerWeakRef.IsAlive)
						return _ownerWeakRef.Target; 
					return null;
				}
			}

			public MemberDescriptor Member {
				get { return _member; }
			}

			public static bool operator == (MemberRelationshipWeakEntry left, MemberRelationshipWeakEntry right)
			{
				if (left.Owner == right.Owner && left.Member == right.Member)
					return true;
				else
					return false;
			}

			public static bool operator != (MemberRelationshipWeakEntry left, MemberRelationshipWeakEntry right)
			{
				return !(left == right);
			}

			public override int GetHashCode ()
			{
				if (this.Owner != null && _member != null)
					return _member.GetHashCode () ^ _ownerWeakRef.Target.GetHashCode ();
				return base.GetHashCode ();
			}

			public override bool Equals (object o)
			{
				if (o is MemberRelationshipWeakEntry) {
					return ((MemberRelationshipWeakEntry) o) == this;
				}
				return false;
			}
		}

		private Hashtable _relations;

		protected MemberRelationshipService ()
		{
			_relations = new Hashtable ();
		}

		public abstract bool SupportsRelationship (MemberRelationship source, MemberRelationship relationship);

		protected virtual MemberRelationship GetRelationship (MemberRelationship source)
		{
			if (source.IsEmpty)
				throw new ArgumentNullException ("source");

			MemberRelationshipWeakEntry entry = _relations[new MemberRelationshipWeakEntry (source)] as MemberRelationshipWeakEntry;
			if (entry != null)
				return new MemberRelationship (entry.Owner, entry.Member);
			return MemberRelationship.Empty;
		}

		protected virtual void SetRelationship (MemberRelationship source, MemberRelationship relationship)
		{
			if (source.IsEmpty)
				throw new ArgumentNullException ("source");

			if (!relationship.IsEmpty && !this.SupportsRelationship (source, relationship))
				throw new ArgumentException ("Relationship not supported.");

			_relations[new MemberRelationshipWeakEntry (source)] = new MemberRelationshipWeakEntry (relationship);
		}

		public MemberRelationship this [object owner, MemberDescriptor member] {
			get { return GetRelationship (new MemberRelationship (owner,  member)); }
			set { SetRelationship (new MemberRelationship (owner,  member), value); }
		}

		public MemberRelationship this [MemberRelationship source] {
			get { return GetRelationship (source); }
			set { SetRelationship (source, value); }
		}
	}
}
