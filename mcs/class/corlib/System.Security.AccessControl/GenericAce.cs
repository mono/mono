//
// System.Security.AccessControl.GenericAce implementation
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

using System.Collections;

namespace System.Security.AccessControl {
	public abstract class GenericAce
	{
		internal GenericAce (InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags)
		{
			inheritance = inheritanceFlags;
			propagation = propagationFlags;
		}

		internal GenericAce (AceType type)
		{
			if (type <= AceType.MaxDefinedAceType) {
				throw new ArgumentOutOfRangeException ("type");
			}
			this.ace_type = type;
		}

		InheritanceFlags inheritance;
		PropagationFlags propagation;
		AceFlags aceflags;
		AceType ace_type;

		public AceFlags AceFlags {
			get { return aceflags; }
			set { aceflags = value; }
		}
		
		public AceType AceType {
			get { return ace_type; }
		}
		
		public AuditFlags AuditFlags {
			get {
				AuditFlags ret = AuditFlags.None;
				if ((aceflags & AceFlags.SuccessfulAccess) != 0)
					ret |= AuditFlags.Success;
				if ((aceflags & AceFlags.FailedAccess) != 0)
					ret |= AuditFlags.Failure;
				return ret;
			}
		}
		
		public abstract int BinaryLength { get; }

		public InheritanceFlags InheritanceFlags {
			get { return inheritance; }
		}

		[MonoTODO]
		public bool IsInherited {
			get { return(false); }
		}

		public PropagationFlags PropagationFlags {
			get { return propagation; }
		}
		
		[MonoTODO]
		public GenericAce Copy ()
		{
			throw new NotImplementedException ();
		}
		
		[MonoTODO]
		public static GenericAce CreateFromBinaryForm (byte[] binaryForm, int offset)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override sealed bool Equals (object o)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public abstract void GetBinaryForm (byte[] binaryForm, int offset);

		[MonoTODO]
		public override sealed int GetHashCode ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static bool operator== (GenericAce left,
					       GenericAce right)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static bool operator!= (GenericAce left,
					       GenericAce right)
		{
			throw new NotImplementedException ();
		}
	}
}

#endif
