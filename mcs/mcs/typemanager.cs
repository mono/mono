//
// typemanager.cs: C# type manager
//
// Author: Miguel de Icaza (miguel@gnu.org)
//         Ravi Pratap     (ravi@ximian.com)
//         Marek Safar     (marek.safar@seznam.cz)
//
// Dual licensed under the terms of the MIT X11 or GNU GPL
//
// Copyright 2001-2003 Ximian, Inc (http://www.ximian.com)
// Copyright 2003-2008 Novell, Inc.
//

using System;
using System.Globalization;
using System.Collections.Generic;
using System.Text;

namespace Mono.CSharp
{
	//
	// All compiler build-in types (they have to exist otherwise the compiler will not work)
	//
	public class BuildinTypes
	{
		public readonly BuildinTypeSpec Object;
		public readonly BuildinTypeSpec ValueType;
		public readonly BuildinTypeSpec Attribute;

		public readonly BuildinTypeSpec Int;
		public readonly BuildinTypeSpec UInt;
		public readonly BuildinTypeSpec Long;
		public readonly BuildinTypeSpec ULong;
		public readonly BuildinTypeSpec Float;
		public readonly BuildinTypeSpec Double;
		public readonly BuildinTypeSpec Char;
		public readonly BuildinTypeSpec Short;
		public readonly BuildinTypeSpec Decimal;
		public readonly BuildinTypeSpec Bool;
		public readonly BuildinTypeSpec SByte;
		public readonly BuildinTypeSpec Byte;
		public readonly BuildinTypeSpec UShort;
		public readonly BuildinTypeSpec String;

		public readonly BuildinTypeSpec Enum;
		public readonly BuildinTypeSpec Delegate;
		public readonly BuildinTypeSpec MulticastDelegate;
		public readonly BuildinTypeSpec Void;
		public readonly BuildinTypeSpec Array;
		public readonly BuildinTypeSpec Type;
		public readonly BuildinTypeSpec IEnumerator;
		public readonly BuildinTypeSpec IEnumerable;
		public readonly BuildinTypeSpec IDisposable;
		public readonly BuildinTypeSpec IntPtr;
		public readonly BuildinTypeSpec UIntPtr;
		public readonly BuildinTypeSpec RuntimeFieldHandle;
		public readonly BuildinTypeSpec RuntimeTypeHandle;
		public readonly BuildinTypeSpec Exception;

		//
		// These are internal buil-in types which depend on other
		// build-in type (mostly object)
		//
		public readonly BuildinTypeSpec Dynamic;

		// Predefined operators tables
		public readonly Binary.PredefinedOperator[] OperatorsBinaryStandard;
		public readonly Binary.PredefinedOperator[] OperatorsBinaryEquality;
		public readonly Binary.PredefinedOperator[] OperatorsBinaryUnsafe;
		public readonly TypeSpec[][] OperatorsUnary;
		public readonly TypeSpec[] OperatorsUnaryMutator;

		public readonly TypeSpec[] BinaryPromotionsTypes;
		public readonly TypeSpec[] SwitchUserTypes;

		readonly BuildinTypeSpec[] types;

		public BuildinTypes ()
		{
			Object = new BuildinTypeSpec (MemberKind.Class, "System", "Object", BuildinTypeSpec.Type.Object);
			ValueType = new BuildinTypeSpec (MemberKind.Class, "System", "ValueType", BuildinTypeSpec.Type.ValueType);
			Attribute = new BuildinTypeSpec (MemberKind.Class, "System", "Attribute", BuildinTypeSpec.Type.Attribute);

			Int = new BuildinTypeSpec (MemberKind.Struct, "System", "Int32", BuildinTypeSpec.Type.Int);
			Long = new BuildinTypeSpec (MemberKind.Struct, "System", "Int64", BuildinTypeSpec.Type.Long);
			UInt = new BuildinTypeSpec (MemberKind.Struct, "System", "UInt32", BuildinTypeSpec.Type.UInt);
			ULong = new BuildinTypeSpec (MemberKind.Struct, "System", "UInt64", BuildinTypeSpec.Type.ULong);
			Byte = new BuildinTypeSpec (MemberKind.Struct, "System", "Byte", BuildinTypeSpec.Type.Byte);
			SByte = new BuildinTypeSpec (MemberKind.Struct, "System", "SByte", BuildinTypeSpec.Type.SByte);
			Short = new BuildinTypeSpec (MemberKind.Struct, "System", "Int16", BuildinTypeSpec.Type.Short);
			UShort = new BuildinTypeSpec (MemberKind.Struct, "System", "UInt16", BuildinTypeSpec.Type.UShort);

			IEnumerator = new BuildinTypeSpec (MemberKind.Interface, "System.Collections", "IEnumerator", BuildinTypeSpec.Type.IEnumerator);
			IEnumerable = new BuildinTypeSpec (MemberKind.Interface, "System.Collections", "IEnumerable", BuildinTypeSpec.Type.IEnumerable);
			IDisposable = new BuildinTypeSpec (MemberKind.Interface, "System", "IDisposable", BuildinTypeSpec.Type.IDisposable);

			Char = new BuildinTypeSpec (MemberKind.Struct, "System", "Char", BuildinTypeSpec.Type.Char);
			String = new BuildinTypeSpec (MemberKind.Class, "System", "String", BuildinTypeSpec.Type.String);
			Float = new BuildinTypeSpec (MemberKind.Struct, "System", "Single", BuildinTypeSpec.Type.Float);
			Double = new BuildinTypeSpec (MemberKind.Struct, "System", "Double", BuildinTypeSpec.Type.Double);
			Decimal = new BuildinTypeSpec (MemberKind.Struct, "System", "Decimal", BuildinTypeSpec.Type.Decimal);
			Bool = new BuildinTypeSpec (MemberKind.Struct, "System", "Boolean", BuildinTypeSpec.Type.Bool);
			IntPtr = new BuildinTypeSpec (MemberKind.Struct, "System", "IntPtr", BuildinTypeSpec.Type.IntPtr);
			UIntPtr = new BuildinTypeSpec (MemberKind.Struct, "System", "UIntPtr", BuildinTypeSpec.Type.UIntPtr);

			MulticastDelegate = new BuildinTypeSpec (MemberKind.Class, "System", "MulticastDelegate", BuildinTypeSpec.Type.MulticastDelegate);
			Delegate = new BuildinTypeSpec (MemberKind.Class, "System", "Delegate", BuildinTypeSpec.Type.Delegate);
			Enum = new BuildinTypeSpec (MemberKind.Class, "System", "Enum", BuildinTypeSpec.Type.Enum);
			Array = new BuildinTypeSpec (MemberKind.Class, "System", "Array", BuildinTypeSpec.Type.Array);
			Void = new BuildinTypeSpec (MemberKind.Void, "System", "Void", BuildinTypeSpec.Type.Other);
			Type = new BuildinTypeSpec (MemberKind.Class, "System", "Type", BuildinTypeSpec.Type.Type);
			Exception = new BuildinTypeSpec (MemberKind.Class, "System", "Exception", BuildinTypeSpec.Type.Exception);
			RuntimeFieldHandle = new BuildinTypeSpec (MemberKind.Struct, "System", "RuntimeFieldHandle", BuildinTypeSpec.Type.Other);
			RuntimeTypeHandle = new BuildinTypeSpec (MemberKind.Struct, "System", "RuntimeTypeHandle", BuildinTypeSpec.Type.Other);

			// TODO: Maybe I should promote it to different kind for faster compares
			Dynamic = new BuildinTypeSpec ("dynamic", BuildinTypeSpec.Type.Dynamic);

			OperatorsBinaryStandard = Binary.CreateStandardOperatorsTable (this);
			OperatorsBinaryEquality = Binary.CreateEqualityOperatorsTable (this);
			OperatorsBinaryUnsafe = Binary.CreatePointerOperatorsTable (this);
			OperatorsUnary = Unary.CreatePredefinedOperatorsTable (this);
			OperatorsUnaryMutator = UnaryMutator.CreatePredefinedOperatorsTable (this);

			BinaryPromotionsTypes = ConstantFold.CreateBinaryPromotionsTypes (this);
			SwitchUserTypes = Switch.CreateSwitchUserTypes (this);

			types = new BuildinTypeSpec[] {
				Object, ValueType, Attribute,
				Int, UInt, Long, ULong, Float, Double, Char, Short, Decimal, Bool, SByte, Byte, UShort, String,
				Enum, Delegate, MulticastDelegate, Void, Array, Type, IEnumerator, IEnumerable, IDisposable,
				IntPtr, UIntPtr, RuntimeFieldHandle, RuntimeTypeHandle, Exception };
		}

		public BuildinTypeSpec[] AllTypes {
			get {
				return types;
			}
		}

		public bool CheckDefinitions (ModuleContainer module)
		{
			var ctx = module.Compiler;
			foreach (var p in types) {
				var found = PredefinedType.Resolve (module, p.Kind, p.Namespace, p.Name, p.Arity, Location.Null);
				if (found == null || found == p)
					continue;

				var tc = found.MemberDefinition as TypeContainer;
				if (tc != null) {
					var ns = module.GlobalRootNamespace.GetNamespace (p.Namespace, false);
					ns.ReplaceTypeWithPredefined (found, p);

					tc.SetPredefinedSpec (p);
					p.SetDefinition (found);
				}
			}

			if (ctx.Report.Errors != 0)
				return false;

			// Set internal build-in types
			Dynamic.SetDefinition (Object);

			return true;
		}
	}

	//
	// Compiler predefined types. Usually used for compiler generated
	// code or for comparison against well known framework type. They
	// may not exist as they are optional
	//
	class PredefinedTypes
	{
		public readonly PredefinedType ArgIterator;
		public readonly PredefinedType MarshalByRefObject;
		public readonly PredefinedType RuntimeHelpers;
		public readonly PredefinedType IAsyncResult;
		public readonly PredefinedType AsyncCallback;
		public readonly PredefinedType RuntimeArgumentHandle;
		public readonly PredefinedType CharSet;
		public readonly PredefinedType IsVolatile;
		public readonly PredefinedType IEnumeratorGeneric;
		public readonly PredefinedType IListGeneric;
		public readonly PredefinedType ICollectionGeneric;
		public readonly PredefinedType IEnumerableGeneric;
		public readonly PredefinedType Nullable;
		public readonly PredefinedType Activator;
		public readonly PredefinedType Interlocked;
		public readonly PredefinedType Monitor;
		public readonly PredefinedType NotSupportedException;
		public readonly PredefinedType RuntimeFieldHandle;
		public readonly PredefinedType RuntimeMethodHandle;
		public readonly PredefinedType SecurityAction;
		public readonly PredefinedType Dictionary;
		public readonly PredefinedType Hashtable;

		//
		// C# 3.0
		//
		public readonly PredefinedType Expression;
		public readonly PredefinedType ExpressionGeneric;
		public readonly PredefinedType ParameterExpression;
		public readonly PredefinedType FieldInfo;
		public readonly PredefinedType MethodBase;
		public readonly PredefinedType MethodInfo;
		public readonly PredefinedType ConstructorInfo;

		//
		// C# 4.0
		//
		public readonly PredefinedType Binder;
		public readonly PredefinedType CallSite;
		public readonly PredefinedType CallSiteGeneric;
		public readonly PredefinedType BinderFlags;

		public PredefinedTypes (ModuleContainer module)
		{
			var TypedReference = new PredefinedType (module, MemberKind.Struct, "System", "TypedReference");
			ArgIterator = new PredefinedType (module, MemberKind.Struct, "System", "ArgIterator");

			MarshalByRefObject = new PredefinedType (module, MemberKind.Class, "System", "MarshalByRefObject");
			RuntimeHelpers = new PredefinedType (module, MemberKind.Class, "System.Runtime.CompilerServices", "RuntimeHelpers");
			IAsyncResult = new PredefinedType (module, MemberKind.Interface, "System", "IAsyncResult");
			AsyncCallback = new PredefinedType (module, MemberKind.Delegate, "System", "AsyncCallback");
			RuntimeArgumentHandle = new PredefinedType (module, MemberKind.Struct, "System", "RuntimeArgumentHandle");
			CharSet = new PredefinedType (module, MemberKind.Enum, "System.Runtime.InteropServices", "CharSet");
			IsVolatile = new PredefinedType (module, MemberKind.Class, "System.Runtime.CompilerServices", "IsVolatile");
			IEnumeratorGeneric = new PredefinedType (module, MemberKind.Interface, "System.Collections.Generic", "IEnumerator", 1);
			IListGeneric = new PredefinedType (module, MemberKind.Interface, "System.Collections.Generic", "IList", 1);
			ICollectionGeneric = new PredefinedType (module, MemberKind.Interface, "System.Collections.Generic", "ICollection", 1);
			IEnumerableGeneric = new PredefinedType (module, MemberKind.Interface, "System.Collections.Generic", "IEnumerable", 1);
			Nullable = new PredefinedType (module, MemberKind.Struct, "System", "Nullable", 1);
			Activator = new PredefinedType (module, MemberKind.Class, "System", "Activator");
			Interlocked = new PredefinedType (module, MemberKind.Class, "System.Threading", "Interlocked");
			Monitor = new PredefinedType (module, MemberKind.Class, "System.Threading", "Monitor");
			NotSupportedException = new PredefinedType (module, MemberKind.Class, "System", "NotSupportedException");
			RuntimeFieldHandle = new PredefinedType (module, MemberKind.Struct, "System", "RuntimeFieldHandle");
			RuntimeMethodHandle = new PredefinedType (module, MemberKind.Struct, "System", "RuntimeMethodHandle");
			SecurityAction = new PredefinedType (module, MemberKind.Enum, "System.Security.Permissions", "SecurityAction");
			Dictionary = new PredefinedType (module, MemberKind.Class, "System.Collections.Generic", "Dictionary", 2);
			Hashtable = new PredefinedType (module, MemberKind.Class, "System.Collections", "Hashtable");

			Expression = new PredefinedType (module, MemberKind.Class, "System.Linq.Expressions", "Expression");
			ExpressionGeneric = new PredefinedType (module, MemberKind.Class, "System.Linq.Expressions", "Expression", 1);
			ParameterExpression = new PredefinedType (module, MemberKind.Class, "System.Linq.Expressions", "ParameterExpression");
			FieldInfo = new PredefinedType (module, MemberKind.Class, "System.Reflection", "FieldInfo");
			MethodBase = new PredefinedType (module, MemberKind.Class, "System.Reflection", "MethodBase");
			MethodInfo = new PredefinedType (module, MemberKind.Class, "System.Reflection", "MethodInfo");
			ConstructorInfo = new PredefinedType (module, MemberKind.Class, "System.Reflection", "ConstructorInfo");

			CallSite = new PredefinedType (module, MemberKind.Class, "System.Runtime.CompilerServices", "CallSite");
			CallSiteGeneric = new PredefinedType (module, MemberKind.Class, "System.Runtime.CompilerServices", "CallSite", 1);
			Binder = new PredefinedType (module, MemberKind.Class, "Microsoft.CSharp.RuntimeBinder", "Binder");
			BinderFlags = new PredefinedType (module, MemberKind.Enum, "Microsoft.CSharp.RuntimeBinder", "CSharpBinderFlags");

			//
			// Define types which are used for comparison. It does not matter
			// if they don't exist as no error report is needed
			//
			if (TypedReference.Define ())
				TypedReference.TypeSpec.IsSpecialRuntimeType = true;

			if (ArgIterator.Define ())
				ArgIterator.TypeSpec.IsSpecialRuntimeType = true;

			if (IEnumerableGeneric.Define ())
				IEnumerableGeneric.TypeSpec.IsGenericIterateInterface = true;

			if (IListGeneric.Define ())
				IListGeneric.TypeSpec.IsGenericIterateInterface = true;

			if (ICollectionGeneric.Define ())
				ICollectionGeneric.TypeSpec.IsGenericIterateInterface = true;

			if (Nullable.Define ())
				Nullable.TypeSpec.IsNullableType = true;

			if (ExpressionGeneric.Define ())
				ExpressionGeneric.TypeSpec.IsExpressionTreeType = true;
		}
	}

	class PredefinedMembers
	{
		public readonly PredefinedMember<MethodSpec> ActivatorCreateInstance;
		public readonly PredefinedMember<MethodSpec> DecimalCtor;
		public readonly PredefinedMember<MethodSpec> DecimalCtorInt;
		public readonly PredefinedMember<MethodSpec> DecimalCtorLong;
		public readonly PredefinedMember<MethodSpec> DecimalConstantAttributeCtor;
		public readonly PredefinedMember<MethodSpec> DefaultMemberAttributeCtor;
		public readonly PredefinedMember<MethodSpec> DelegateCombine;
		public readonly PredefinedMember<MethodSpec> DelegateEqual;
		public readonly PredefinedMember<MethodSpec> DelegateInequal;
		public readonly PredefinedMember<MethodSpec> DelegateRemove;
		public readonly PredefinedMember<MethodSpec> DynamicAttributeCtor;
		public readonly PredefinedMember<MethodSpec> FieldInfoGetFieldFromHandle;
		public readonly PredefinedMember<MethodSpec> FieldInfoGetFieldFromHandle2;
		public readonly PredefinedMember<MethodSpec> IDisposableDispose;
		public readonly PredefinedMember<MethodSpec> IEnumerableGetEnumerator;
		public readonly PredefinedMember<MethodSpec> InterlockedCompareExchange;
		public readonly PredefinedMember<MethodSpec> InterlockedCompareExchange_T;
		public readonly PredefinedMember<MethodSpec> FixedBufferAttributeCtor;
		public readonly PredefinedMember<MethodSpec> MethodInfoGetMethodFromHandle;
		public readonly PredefinedMember<MethodSpec> MethodInfoGetMethodFromHandle2;
		public readonly PredefinedMember<MethodSpec> MonitorEnter;
		public readonly PredefinedMember<MethodSpec> MonitorEnter_v4;
		public readonly PredefinedMember<MethodSpec> MonitorExit;
		public readonly PredefinedMember<PropertySpec> RuntimeCompatibilityWrapNonExceptionThrows;
		public readonly PredefinedMember<MethodSpec> RuntimeHelpersInitializeArray;
		public readonly PredefinedMember<PropertySpec> RuntimeHelpersOffsetToStringData;
		public readonly PredefinedMember<ConstSpec> SecurityActionRequestMinimum;
		public readonly PredefinedMember<FieldSpec> StringEmpty;
		public readonly PredefinedMember<MethodSpec> StringEqual;
		public readonly PredefinedMember<MethodSpec> StringInequal;
		public readonly PredefinedMember<MethodSpec> StructLayoutAttributeCtor;
		public readonly PredefinedMember<FieldSpec> StructLayoutCharSet;
		public readonly PredefinedMember<FieldSpec> StructLayoutPack;
		public readonly PredefinedMember<FieldSpec> StructLayoutSize;
		public readonly PredefinedMember<MethodSpec> TypeGetTypeFromHandle;

		public PredefinedMembers (ModuleContainer module)
		{
			var ctx = module.Compiler;
			var types = module.PredefinedTypes;
			var atypes = module.PredefinedAttributes;
			var btypes = module.Compiler.BuildinTypes;

			ActivatorCreateInstance = new PredefinedMember<MethodSpec> (ctx, types.Activator,
				MemberFilter.Method ("CreateInstance", 1, ParametersCompiled.EmptyReadOnlyParameters, null));

			DecimalCtor = new PredefinedMember<MethodSpec> (ctx, btypes.Decimal,
				MemberFilter.Constructor (ParametersCompiled.CreateFullyResolved (
					btypes.Int, btypes.Int, btypes.Int, btypes.Bool, btypes.Byte)));

			DecimalCtorInt = new PredefinedMember<MethodSpec> (ctx, btypes.Decimal,
				MemberFilter.Constructor (ParametersCompiled.CreateFullyResolved (btypes.Int)));

			DecimalCtorLong = new PredefinedMember<MethodSpec> (ctx, btypes.Decimal,
				MemberFilter.Constructor (ParametersCompiled.CreateFullyResolved (btypes.Long)));

			DecimalConstantAttributeCtor = new PredefinedMember<MethodSpec> (ctx, atypes.DecimalConstant,
				MemberFilter.Constructor (ParametersCompiled.CreateFullyResolved (
					btypes.Byte, btypes.Byte, btypes.UInt, btypes.UInt, btypes.UInt)));

			DefaultMemberAttributeCtor = new PredefinedMember<MethodSpec> (ctx, atypes.DefaultMember,
				MemberFilter.Constructor (ParametersCompiled.CreateFullyResolved (btypes.String)));

			DelegateCombine = new PredefinedMember<MethodSpec> (ctx, btypes.Delegate, "Combine", btypes.Delegate, btypes.Delegate);
			DelegateRemove = new PredefinedMember<MethodSpec> (ctx, btypes.Delegate, "Remove", btypes.Delegate, btypes.Delegate);

			DelegateEqual = new PredefinedMember<MethodSpec> (ctx, btypes.Delegate,
				new MemberFilter (Operator.GetMetadataName (Operator.OpType.Equality), 0, MemberKind.Operator, null, btypes.Bool));

			DelegateInequal = new PredefinedMember<MethodSpec> (ctx, btypes.Delegate,
				new MemberFilter (Operator.GetMetadataName (Operator.OpType.Inequality), 0, MemberKind.Operator, null, btypes.Bool));

			DynamicAttributeCtor = new PredefinedMember<MethodSpec> (ctx, atypes.Dynamic,
				MemberFilter.Constructor (ParametersCompiled.CreateFullyResolved (
					ArrayContainer.MakeType (module, btypes.Bool))));

			FieldInfoGetFieldFromHandle = new PredefinedMember<MethodSpec> (ctx, types.FieldInfo,
				"GetFieldFromHandle", MemberKind.Method, types.RuntimeFieldHandle);

			FieldInfoGetFieldFromHandle2 = new PredefinedMember<MethodSpec> (ctx, types.FieldInfo,
				"GetFieldFromHandle", MemberKind.Method, types.RuntimeFieldHandle, new PredefinedType (btypes.RuntimeTypeHandle));

			FixedBufferAttributeCtor = new PredefinedMember<MethodSpec> (ctx, atypes.FixedBuffer,
				MemberFilter.Constructor (ParametersCompiled.CreateFullyResolved (btypes.Type, btypes.Int)));

			IDisposableDispose = new PredefinedMember<MethodSpec> (ctx, btypes.IDisposable, "Dispose", TypeSpec.EmptyTypes);

			IEnumerableGetEnumerator = new PredefinedMember<MethodSpec> (ctx, btypes.IEnumerable,
				"GetEnumerator", TypeSpec.EmptyTypes);

			InterlockedCompareExchange = new PredefinedMember<MethodSpec> (ctx, types.Interlocked,
				MemberFilter.Method ("CompareExchange", 0,
					new ParametersImported (
						new[] {
								new ParameterData (null, Parameter.Modifier.REF),
								new ParameterData (null, Parameter.Modifier.NONE),
								new ParameterData (null, Parameter.Modifier.NONE)
							},
						new[] {
								btypes.Int, btypes.Int, btypes.Int
							},
						false),
				btypes.Int));

			InterlockedCompareExchange_T = new PredefinedMember<MethodSpec> (ctx, types.Interlocked,
				MemberFilter.Method ("CompareExchange", 1,
					new ParametersImported (
						new[] {
								new ParameterData (null, Parameter.Modifier.REF),
								new ParameterData (null, Parameter.Modifier.NONE),
								new ParameterData (null, Parameter.Modifier.NONE)
							},
						new[] {
								new TypeParameterSpec (0, null, SpecialConstraint.None, Variance.None, null),
								new TypeParameterSpec (0, null, SpecialConstraint.None, Variance.None, null),
								new TypeParameterSpec (0, null, SpecialConstraint.None, Variance.None, null),
							}, false),
					null));

			MethodInfoGetMethodFromHandle = new PredefinedMember<MethodSpec> (ctx, types.MethodBase,
				"GetMethodFromHandle", MemberKind.Method, types.RuntimeMethodHandle);

			MethodInfoGetMethodFromHandle2 = new PredefinedMember<MethodSpec> (ctx, types.MethodBase,
				"GetMethodFromHandle", MemberKind.Method, types.RuntimeMethodHandle, new PredefinedType (btypes.RuntimeTypeHandle));

			MonitorEnter = new PredefinedMember<MethodSpec> (ctx, types.Monitor, "Enter", btypes.Object);

			MonitorEnter_v4 = new PredefinedMember<MethodSpec> (ctx, types.Monitor,
				MemberFilter.Method ("Enter", 0,
					new ParametersImported (new[] {
							new ParameterData (null, Parameter.Modifier.NONE),
							new ParameterData (null, Parameter.Modifier.REF)
						},
					new[] {
							btypes.Object, btypes.Bool
						}, false), null));

			MonitorExit = new PredefinedMember<MethodSpec> (ctx, types.Monitor, "Exit", btypes.Object);

			RuntimeCompatibilityWrapNonExceptionThrows = new PredefinedMember<PropertySpec> (ctx, atypes.RuntimeCompatibility,
				MemberFilter.Property ("WrapNonExceptionThrows", btypes.Bool));

			RuntimeHelpersInitializeArray = new PredefinedMember<MethodSpec> (ctx, types.RuntimeHelpers,
				"InitializeArray", btypes.Array, btypes.RuntimeFieldHandle);

			RuntimeHelpersOffsetToStringData = new PredefinedMember<PropertySpec> (ctx, types.RuntimeHelpers,
				MemberFilter.Property ("OffsetToStringData", btypes.Int));

			SecurityActionRequestMinimum = new PredefinedMember<ConstSpec> (ctx, types.SecurityAction, "RequestMinimum",
				MemberKind.Field, types.SecurityAction);

			StringEmpty = new PredefinedMember<FieldSpec> (ctx, btypes.String, MemberFilter.Field ("Empty", btypes.String));

			StringEqual = new PredefinedMember<MethodSpec> (ctx, btypes.String,
				new MemberFilter (Operator.GetMetadataName (Operator.OpType.Equality), 0, MemberKind.Operator, null, btypes.Bool));

			StringInequal = new PredefinedMember<MethodSpec> (ctx, btypes.String,
				new MemberFilter (Operator.GetMetadataName (Operator.OpType.Inequality), 0, MemberKind.Operator, null, btypes.Bool));

			StructLayoutAttributeCtor = new PredefinedMember<MethodSpec> (ctx, atypes.StructLayout,
				MemberFilter.Constructor (ParametersCompiled.CreateFullyResolved (btypes.Short)));

			StructLayoutCharSet = new PredefinedMember<FieldSpec> (ctx, atypes.StructLayout, "CharSet",
				MemberKind.Field, types.CharSet);

			StructLayoutPack = new PredefinedMember<FieldSpec> (ctx, atypes.StructLayout,
				MemberFilter.Field ("Pack", btypes.Int));

			StructLayoutSize = new PredefinedMember<FieldSpec> (ctx, atypes.StructLayout,
				MemberFilter.Field ("Size", btypes.Int));

			TypeGetTypeFromHandle = new PredefinedMember<MethodSpec> (ctx, btypes.Type, "GetTypeFromHandle", btypes.RuntimeTypeHandle);
		}
	}

	public class PredefinedType
	{
		readonly string name;
		readonly string ns;
		readonly int arity;
		readonly MemberKind kind;
		protected readonly ModuleContainer module;
		protected TypeSpec type;

		public PredefinedType (ModuleContainer module, MemberKind kind, string ns, string name, int arity)
			: this (module, kind, ns, name)
		{
			this.arity = arity;
		}

		public PredefinedType (ModuleContainer module, MemberKind kind, string ns, string name)
		{
			this.module = module;
			this.kind = kind;
			this.name = name;
			this.ns = ns;
		}

		public PredefinedType (BuildinTypeSpec type)
		{
			this.kind = type.Kind;
			this.name = type.Name;
			this.ns = type.Namespace;
			this.type = type;
		}

		#region Properties

		public int Arity {
			get {
				return arity;
			}
		}

		public bool IsDefined {
			get {
				return type != null;
			}
		}

		public string Name {
			get {
				return name;
			}
		}

		public string Namespace {
			get {
				return ns;
			}
		}

		public TypeSpec TypeSpec {
			get {
				return type;
			}
		}

		#endregion

		public bool Define ()
		{
			if (type != null)
				return true;

			Namespace type_ns = module.GlobalRootNamespace.GetNamespace (ns, true);
			var te = type_ns.LookupType (module, name, arity, true, Location.Null);
			if (te == null || te.Type.Kind != kind) {
				return false;
			}

			type = te.Type;
			return true;
		}

		public string GetSignatureForError ()
		{
			return ns + "." + name;
		}

		public static TypeSpec Resolve (ModuleContainer module, MemberKind kind, string ns, string name, int arity, Location loc)
		{
			Namespace type_ns = module.GlobalRootNamespace.GetNamespace (ns, true);
			var te = type_ns.LookupType (module, name, arity, false, Location.Null);
			if (te == null) {
				module.Compiler.Report.Error (518, loc, "The predefined type `{0}.{1}' is not defined or imported", ns, name);
				return null;
			}

			var type = te.Type;
			if (type.Kind != kind) {
				if (type.Kind == MemberKind.Struct && kind == MemberKind.Void && type.MemberDefinition is TypeContainer) {
					// Void is declared as struct but we keep it internally as
					// special kind, the swap will be done by caller
				} else {
					module.Compiler.Report.Error (520, loc, "The predefined type `{0}.{1}' is not declared correctly", ns, name);
					return null;
				}
			}

			return type;
		}

		public TypeSpec Resolve (Location loc)
		{
			if (type == null)
				type = Resolve (module, kind, ns, name, arity, loc);

			return type;
		}
	}

	class PredefinedMember<T> where T : MemberSpec
	{
		readonly CompilerContext ctx;
		T member;
		TypeSpec declaring_type;
		readonly PredefinedType declaring_type_predefined;
		readonly PredefinedType[] parameters_predefined;
		MemberFilter filter;

		public PredefinedMember (CompilerContext ctx, PredefinedType type, MemberFilter filter)
		{
			this.ctx = ctx;
			this.declaring_type_predefined = type;
			this.filter = filter;
		}

		public PredefinedMember (CompilerContext ctx, TypeSpec type, MemberFilter filter)
		{
			this.ctx = ctx;
			this.declaring_type = type;
			this.filter = filter;
		}

		public PredefinedMember (CompilerContext ctx, PredefinedType type, string name, params TypeSpec[] types)
			: this (ctx, type, MemberFilter.Method (name, 0, ParametersCompiled.CreateFullyResolved (types), null))
		{
		}

		public PredefinedMember (CompilerContext ctx, PredefinedType type, string name, MemberKind kind, params PredefinedType[] types)
			: this (ctx, type, new MemberFilter (name, 0, kind, null, null))
		{
			parameters_predefined = types;
		}

		public PredefinedMember (CompilerContext ctx, BuildinTypeSpec type, string name, params TypeSpec[] types)
			: this (ctx, type, MemberFilter.Method (name, 0, ParametersCompiled.CreateFullyResolved (types), null))
		{
		}

		public T Get ()
		{
			if (member != null)
				return member;

			if (declaring_type == null) {
				if (!declaring_type_predefined.Define ())
					return null;

				declaring_type = declaring_type_predefined.TypeSpec;
			}

			if (parameters_predefined != null) {
				TypeSpec[] types = new TypeSpec [parameters_predefined.Length];
				for (int i = 0; i < types.Length; ++i) {
					var p = parameters_predefined [i];
					if (!p.Define ())
						return null;

					types[i] = p.TypeSpec;
				}

				if (filter.Kind == MemberKind.Field)
					filter = new MemberFilter (filter.Name, filter.Arity, filter.Kind, null, types [0]);
				else
					filter = new MemberFilter (filter.Name, filter.Arity, filter.Kind, ParametersCompiled.CreateFullyResolved (types), filter.MemberType);
			}

			member = MemberCache.FindMember (declaring_type, filter, BindingRestriction.DeclaredOnly) as T;
			if (member == null)
				return null;

			if (!member.IsAccessible (InternalType.FakeInternalType))
				return null;

			return member;
		}

		public T Resolve (Location loc)
		{
			if (member != null)
				return member;

			if (Get () != null)
				return member;

			if (declaring_type == null) {
				if (declaring_type_predefined.Resolve (loc) == null)
					return null;
			}

			if (parameters_predefined != null) {
				TypeSpec[] types = new TypeSpec[parameters_predefined.Length];
				for (int i = 0; i < types.Length; ++i) {
					var p = parameters_predefined[i];
					types[i] = p.Resolve (loc);
					if (types[i] == null)
						return null;
				}

				filter = new MemberFilter (filter.Name, filter.Arity, filter.Kind, ParametersCompiled.CreateFullyResolved (types), filter.MemberType);
			}

			string method_args = null;
			if (filter.Parameters != null)
				method_args = filter.Parameters.GetSignatureForError ();

			ctx.Report.Error (656, loc, "The compiler required member `{0}.{1}{2}' could not be found or is inaccessible",
				declaring_type.GetSignatureForError (), filter.Name, method_args);

			return null;
		}
	}

	partial class TypeManager {

	/// <summary>
	///   Returns the C# name of a type if possible, or the full type name otherwise
	/// </summary>
	static public string CSharpName (TypeSpec t)
	{
		return t.GetSignatureForError ();
	}

	static public string CSharpName (IList<TypeSpec> types)
	{
		if (types.Count == 0)
			return string.Empty;

		StringBuilder sb = new StringBuilder ();
		for (int i = 0; i < types.Count; ++i) {
			if (i > 0)
				sb.Append (",");

			sb.Append (CSharpName (types [i]));
		}
		return sb.ToString ();
	}

	static public string GetFullNameSignature (MemberSpec mi)
	{
		return mi.GetSignatureForError ();
	}

	static public string CSharpSignature (MemberSpec mb)
	{
		return mb.GetSignatureForError ();
	}

	// Obsolete
	public static bool IsDelegateType (TypeSpec t)
	{
		return t.IsDelegate;
	}
	
	// Obsolete
	public static bool IsEnumType (TypeSpec t)
	{
		return t.IsEnum;
	}

	//
	// Whether a type is unmanaged.  This is used by the unsafe code (25.2)
	//
	public static bool IsUnmanagedType (TypeSpec t)
	{
		var ds = t.MemberDefinition as DeclSpace;
		if (ds != null)
			return ds.IsUnmanagedType ();

		if (t.Kind == MemberKind.Void)
			return true;

		// Someone did the work of checking if the ElementType of t is unmanaged.  Let's not repeat it.
		if (t.IsPointer)
			return IsUnmanagedType (GetElementType (t));

		if (!IsValueType (t))
			return false;

		if (t.IsNested && t.DeclaringType.IsGenericOrParentIsGeneric)
			return false;

		return true;
	}

	//
	// Null is considered to be a reference type
	//			
	public static bool IsReferenceType (TypeSpec t)
	{
		if (t.IsGenericParameter)
			return ((TypeParameterSpec) t).IsReferenceType;

		return !t.IsStruct && !IsEnumType (t);
	}			
		
	public static bool IsValueType (TypeSpec t)
	{
		if (t.IsGenericParameter)
			return ((TypeParameterSpec) t).IsValueType;

		return t.IsStruct || IsEnumType (t);
	}

	public static bool IsFamilyAccessible (TypeSpec type, TypeSpec parent)
	{
//		TypeParameter tparam = LookupTypeParameter (type);
//		TypeParameter pparam = LookupTypeParameter (parent);

		if (type.Kind == MemberKind.TypeParameter && parent.Kind == MemberKind.TypeParameter) { // (tparam != null) && (pparam != null)) {
			if (type == parent)
				return true;

			throw new NotImplementedException ("net");
//			return tparam.IsSubclassOf (parent);
		}

		do {
			if (IsInstantiationOfSameGenericType (type, parent))
				return true;

			type = type.BaseType;
		} while (type != null);

		return false;
	}

	//
	// Checks whether `type' is a subclass or nested child of `base_type'.
	//
	public static bool IsNestedFamilyAccessible (TypeSpec type, TypeSpec base_type)
	{
		do {
			if (IsFamilyAccessible (type, base_type))
				return true;

			// Handle nested types.
			type = type.DeclaringType;
		} while (type != null);

		return false;
	}

	//
	// Checks whether `type' is a nested child of `parent'.
	//
	public static bool IsNestedChildOf (TypeSpec type, ITypeDefinition parent)
	{
		if (type == null)
			return false;

		if (type.MemberDefinition == parent)
			return false;

		type = type.DeclaringType;
		while (type != null) {
			if (type.MemberDefinition == parent)
				return true;

			type = type.DeclaringType;
		}

		return false;
	}

	public static TypeSpec GetElementType (TypeSpec t)
	{
		return ((ElementTypeSpec)t).Element;
	}

	/// <summary>
	/// This method is not implemented by MS runtime for dynamic types
	/// </summary>
	public static bool HasElementType (TypeSpec t)
	{
		return t is ElementTypeSpec;
	}

	/// <summary>
	///   Utility function that can be used to probe whether a type
	///   is managed or not.  
	/// </summary>
	public static bool VerifyUnmanaged (ModuleContainer rc, TypeSpec t, Location loc)
	{
		while (t.IsPointer)
			t = GetElementType (t);

		if (IsUnmanagedType (t))
			return true;

		rc.Compiler.Report.SymbolRelatedToPreviousError (t);
		rc.Compiler.Report.Error (208, loc,
			"Cannot take the address of, get the size of, or declare a pointer to a managed type `{0}'",
			CSharpName (t));

		return false;	
	}
#region Generics
	// This method always return false for non-generic compiler,
	// while Type.IsGenericParameter is returned if it is supported.
	public static bool IsGenericParameter (TypeSpec type)
	{
		return type.IsGenericParameter;
	}

	public static bool IsGenericType (TypeSpec type)
	{
		return type.IsGeneric;
	}

	public static TypeSpec[] GetTypeArguments (TypeSpec t)
	{
		// TODO: return empty array !!
		return t.TypeArguments;
	}

	/// <summary>
	///   Check whether `type' and `parent' are both instantiations of the same
	///   generic type.  Note that we do not check the type parameters here.
	/// </summary>
	public static bool IsInstantiationOfSameGenericType (TypeSpec type, TypeSpec parent)
	{
		return type == parent || type.MemberDefinition == parent.MemberDefinition;
	}
#endregion
}

}
