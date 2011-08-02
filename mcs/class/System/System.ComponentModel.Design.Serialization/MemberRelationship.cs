//
// System.ComponentModel.Design.Serialization.MemberRelationship
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

namespace System.ComponentModel.Design.Serialization
{
	public struct MemberRelationship
	{

		public static readonly MemberRelationship Empty = new MemberRelationship ();

		private object _owner;
		private MemberDescriptor _member;

		public MemberRelationship (object owner, MemberDescriptor member)
		{
			_owner = owner;
			_member = member;
		}

		public bool IsEmpty {
			get { return (_owner == null); }
		}

		public object Owner {
			get { return _owner; }
		}

		public MemberDescriptor Member {
			get { return _member; }
		}

		public static bool operator == (MemberRelationship left, MemberRelationship right)
		{
			if (left.Owner == right.Owner && left.Member == right.Member)
				return true;
			else
				return false;
		}

		public static bool operator != (MemberRelationship left, MemberRelationship right)
		{
			return !(left == right);
		}

		public override int GetHashCode ()
		{
			if (_owner != null && _member != null)
				return _member.GetHashCode () ^ _owner.GetHashCode ();
			return base.GetHashCode ();
		}

		public override bool Equals (object o)
		{
			if (o is MemberRelationship) {
				return ((MemberRelationship)o) == this;
			}
			return false;
		}
	}
}
