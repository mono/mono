//
// reflection.cs: System.Reflection and System.Reflection.Emit specific implementations
//
// Author: Marek Safar (marek.safar@gmail.com)
//
// Dual licensed under the terms of the MIT X11 or GNU GPL
//
// Copyright 2009-2010 Novell, Inc. 
//
//

using System;
using System.Collections.Generic;

namespace Mono.CSharp
{
	public class ReflectionImporter : MetadataImporter
	{
		public ReflectionImporter ()
			: base ()
		{
			Initialize ();
		}

		protected override MemberKind DetermineKindFromBaseType (Type baseType)
		{
			if (baseType == typeof (ValueType))
				return MemberKind.Struct;

			if (baseType == typeof (System.Enum))
				return MemberKind.Enum;

			if (baseType == typeof (MulticastDelegate))
				return MemberKind.Delegate;

			return MemberKind.Class;
		}

		void Initialize ()
		{
			//
			// Setup mapping for build-in types to avoid duplication of their definition
			//
			buildin_types.Add (typeof (object), TypeManager.object_type);
			buildin_types.Add (typeof (System.ValueType), TypeManager.value_type);
			buildin_types.Add (typeof (System.Attribute), TypeManager.attribute_type);

			buildin_types.Add (typeof (int), TypeManager.int32_type);
			buildin_types.Add (typeof (long), TypeManager.int64_type);
			buildin_types.Add (typeof (uint), TypeManager.uint32_type);
			buildin_types.Add (typeof (ulong), TypeManager.uint64_type);
			buildin_types.Add (typeof (byte), TypeManager.byte_type);
			buildin_types.Add (typeof (sbyte), TypeManager.sbyte_type);
			buildin_types.Add (typeof (short), TypeManager.short_type);
			buildin_types.Add (typeof (ushort), TypeManager.ushort_type);

			buildin_types.Add (typeof (System.Collections.IEnumerator), TypeManager.ienumerator_type);
			buildin_types.Add (typeof (System.Collections.IEnumerable), TypeManager.ienumerable_type);
			buildin_types.Add (typeof (System.IDisposable), TypeManager.idisposable_type);

			buildin_types.Add (typeof (char), TypeManager.char_type);
			buildin_types.Add (typeof (string), TypeManager.string_type);
			buildin_types.Add (typeof (float), TypeManager.float_type);
			buildin_types.Add (typeof (double), TypeManager.double_type);
			buildin_types.Add (typeof (decimal), TypeManager.decimal_type);
			buildin_types.Add (typeof (bool), TypeManager.bool_type);
			buildin_types.Add (typeof (System.IntPtr), TypeManager.intptr_type);
			buildin_types.Add (typeof (System.UIntPtr), TypeManager.uintptr_type);

			buildin_types.Add (typeof (System.MulticastDelegate), TypeManager.multicast_delegate_type);
			buildin_types.Add (typeof (System.Delegate), TypeManager.delegate_type);
			buildin_types.Add (typeof (System.Enum), TypeManager.enum_type);
			buildin_types.Add (typeof (System.Array), TypeManager.array_type);
			buildin_types.Add (typeof (void), TypeManager.void_type);
			buildin_types.Add (typeof (System.Type), TypeManager.type_type);
			buildin_types.Add (typeof (System.Exception), TypeManager.exception_type);
			buildin_types.Add (typeof (System.RuntimeFieldHandle), TypeManager.runtime_field_handle_type);
			buildin_types.Add (typeof (System.RuntimeTypeHandle), TypeManager.runtime_handle_type);
		}
	}
}