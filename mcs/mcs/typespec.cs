//
// typespec.cs: Type specification
//
// Authors: Marek Safar (marek.safar@gmail.com)
//
// Dual licensed under the terms of the MIT X11 or GNU GPL
//
// Copyright 2010 Novell, Inc
//

using System;
using System.Collections.Generic;

namespace Mono.CSharp
{
	public class TypeSpec : MemberSpec
	{
		Type info;
		protected MemberCache cache;

		public TypeSpec (MemberKind kind, ITypeDefinition definition, Type info, string name, Modifiers modifiers)
			: base (kind, definition, name, modifiers)
		{
			this.info = info;
		}

		public TypeSpec BaseType { get; set; }

		public override Type DeclaringType {
			get { return info.DeclaringType; }
		}

		public Type MetaInfo {
			get { return info; }
		}

		public MemberCache MemberCache {
			get {
				if (cache == null) {
//					cache = new MemberCache (BaseType);

//					((ITypeDefinition) definition).LoadMembers (cache);
				}

				return cache;
			}
		}
	}

	public interface ITypeDefinition : IMemberDefinition
	{
		void LoadMembers (MemberCache cache);
	}
/*
	class InternalType : TypeSpec
	{
		public static readonly TypeSpec AnonymousMethod = new InternalType ("anonymous method");
		public static readonly TypeSpec Arglist = new InternalType ("__arglist");
//		public static readonly TypeSpec Dynamic = new DynamicType ();
		public static readonly TypeSpec MethodGroup = new InternalType ("method group");

		protected InternalType (string name)
			: base (null, null, name, Modifiers.PUBLIC)
		{
//			cache = MemberCache.Empty;
		}
	}
*/ 
}
