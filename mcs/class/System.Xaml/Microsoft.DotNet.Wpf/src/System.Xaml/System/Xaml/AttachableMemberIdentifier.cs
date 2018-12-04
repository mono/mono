// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Xaml
{
    using System;

    public class AttachableMemberIdentifier : IEquatable<AttachableMemberIdentifier>
    {
        Type declaringType;
        string memberName;

        public AttachableMemberIdentifier(Type declaringType, string memberName)
        {
            this.declaringType = declaringType;
            this.memberName = memberName;
        }

        public string MemberName
        {
            get
            {
                return memberName;
            }
        }

        public Type DeclaringType
        {
            get
            {
                return this.declaringType;
            }
        }

        public static bool operator !=(AttachableMemberIdentifier left, AttachableMemberIdentifier right)
        {
            return !(left == right);
        }

        public static bool operator ==(AttachableMemberIdentifier left, AttachableMemberIdentifier right)
        {
            if (object.ReferenceEquals(left, null))
            {
                return object.ReferenceEquals(right, null);
            }
            return left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as AttachableMemberIdentifier);
        }

        public bool Equals(AttachableMemberIdentifier other)
        {
            if (other == null)
            {
                return false;
            }

            return this.declaringType == other.declaringType && this.memberName == other.memberName;
        }

        public override int GetHashCode()
        {
            int a = this.declaringType == null ? 0 : this.declaringType.GetHashCode();
            int b = this.memberName == null ? 0 : this.memberName.GetHashCode();
            return ((a << 5) + a) ^ b;
        }

        public override string ToString()
        {
            if (this.declaringType == null)
            {
                return this.memberName;
            }

            return this.declaringType.ToString() + "." + memberName;
        }
    }
}
