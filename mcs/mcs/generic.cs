//
// generic.cs: Support classes for generics to reduce differences from GMCS
//
// Author:
//   Raja R Harinath <rharinath@novell.com>
//
// (C) 2006 Novell, Inc.
//
using System;
using System.Reflection;
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

	public abstract class TypeParameter : MemberCore, IMemberContainer {
		private TypeParameter ()
			: base (null, null, null)
		{
		}

		public bool IsSubclassOf (Type base_type)
		{
			throw new InternalErrorException ("cannot be called");
		}

#region IMemberContainer
		public Type Type {
			get { throw new InternalErrorException ("cannot be called"); }
		}

		public MemberCache BaseCache {
			get { throw new InternalErrorException ("cannot be called"); }
		}

		public bool IsInterface {
			get { throw new InternalErrorException ("cannot be called"); }
		}

		public MemberList GetMembers (MemberTypes mt, BindingFlags bf)
		{
			return FindMembers (mt, bf, null, null);
		}

		public MemberCache MemberCache {
			get { throw new InternalErrorException ("cannot be called"); }
		}
#endregion

		public MemberList FindMembers (MemberTypes mt, BindingFlags bf,
					       MemberFilter filter, object criteria)
		{
			throw new InternalErrorException ("cannot be called");
		}
	}
}

namespace System.Reflection.Emit {
	// GRIEVOUS HACK
	abstract class GenericTypeParameterBuilder : Type {
	}
}
