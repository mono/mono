//
// System.Security.AccessControl.ObjectAce implementation
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

#if NET_2_0

using System.Security.Principal;

namespace System.Security.AccessControl
{
	public sealed class ObjectAce : QualifiedAce
	{
		public ObjectAce (AceFlags aceFlags, AceQualifier qualifier,
				  int accessMask, SecurityIdentifier sid,
				  ObjectAceFlags flags, Guid type,
				  Guid inheritedType, bool isCallback,
				  byte[] opaque)
			: base (InheritanceFlags.None, PropagationFlags.None, qualifier, isCallback, opaque)
		{
			AceFlags = aceFlags;
			SecurityIdentifier = sid;
			object_ace_flags = flags;
			object_ace_type = type;
			inherited_object_type = inheritedType;
		}

		Guid object_ace_type, inherited_object_type;
		ObjectAceFlags object_ace_flags;

		[MonoTODO]
		public override int BinaryLength
		{
			get {
				throw new NotImplementedException ();
			}
		}

		public Guid InheritedObjectAceType {
			get { return inherited_object_type; }
			set { inherited_object_type = value; }
		}
		
		public ObjectAceFlags ObjectAceFlags {
			get { return object_ace_flags; }
			set { object_ace_flags = value; }
		}
		
		public Guid ObjectAceType {
			get { return object_ace_type; }
			set { object_ace_type = value; }
		}

		[MonoTODO]
		public override void GetBinaryForm (byte[] binaryForm,
						    int offset)
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static int MaxOpaqueLength (bool isCallback)
		{
			throw new NotImplementedException ();
		}
	}
}

#endif
