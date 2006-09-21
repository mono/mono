//
// generic.cs: Support classes for generics to reduce differences from GMCS
//
// Author:
//   Raja R Harinath <rharinath@novell.com>
//
// (C) 2006 Novell, Inc.
//
using System;
using System.Collections;

namespace Mono.CSharp {
	public abstract class TypeArguments {
		public int Count {
			get { throw new InternalErrorException ("cannot be called"); }
		}
		public bool IsUnbound {
			get { throw new InternalErrorException ("cannot be called"); }
		}
	}

	public abstract class TypeParameter : MemberCore {
		private TypeParameter ()
			: base (null, null, null)
		{
		}

		public bool IsSubclassOf (Type base_type)
		{
			throw new InternalErrorException ("cannot be called");
		}
	}
}
