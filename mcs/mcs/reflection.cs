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
		public ReflectionImporter (BuildinTypes buildin)
			: base ()
		{
			Initialize (buildin);
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

		void Initialize (BuildinTypes buildin)
		{
			//
			// Setup mapping for build-in types to avoid duplication of their definition
			//
			buildin_types.Add (typeof (object), buildin.Object);
			buildin_types.Add (typeof (System.ValueType), buildin.ValueType);
			buildin_types.Add (typeof (System.Attribute), buildin.Attribute);

			buildin_types.Add (typeof (int), buildin.Int);
			buildin_types.Add (typeof (long), buildin.Long);
			buildin_types.Add (typeof (uint), buildin.UInt);
			buildin_types.Add (typeof (ulong), buildin.ULong);
			buildin_types.Add (typeof (byte), buildin.Byte);
			buildin_types.Add (typeof (sbyte), buildin.SByte);
			buildin_types.Add (typeof (short), buildin.Short);
			buildin_types.Add (typeof (ushort), buildin.UShort);

			buildin_types.Add (typeof (System.Collections.IEnumerator), buildin.IEnumerator);
			buildin_types.Add (typeof (System.Collections.IEnumerable), buildin.IEnumerable);
			buildin_types.Add (typeof (System.IDisposable), buildin.IDisposable);

			buildin_types.Add (typeof (char), buildin.Char);
			buildin_types.Add (typeof (string), buildin.String);
			buildin_types.Add (typeof (float), buildin.Float);
			buildin_types.Add (typeof (double), buildin.Double);
			buildin_types.Add (typeof (decimal), buildin.Decimal);
			buildin_types.Add (typeof (bool), buildin.Bool);
			buildin_types.Add (typeof (System.IntPtr), buildin.IntPtr);
			buildin_types.Add (typeof (System.UIntPtr), buildin.UIntPtr);

			buildin_types.Add (typeof (System.MulticastDelegate), buildin.MulticastDelegate);
			buildin_types.Add (typeof (System.Delegate), buildin.Delegate);
			buildin_types.Add (typeof (System.Enum), buildin.Enum);
			buildin_types.Add (typeof (System.Array), buildin.Array);
			buildin_types.Add (typeof (void), buildin.Void);
			buildin_types.Add (typeof (System.Type), buildin.Type);
			buildin_types.Add (typeof (System.Exception), buildin.Exception);
			buildin_types.Add (typeof (System.RuntimeFieldHandle), buildin.RuntimeFieldHandle);
			buildin_types.Add (typeof (System.RuntimeTypeHandle), buildin.RuntimeTypeHandle);
		}
	}
}