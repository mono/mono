//
// typemanager.cs: C# type manager
//
// Author: Miguel de Icaza (miguel@gnu.org)
//         Ravi Pratap     (ravi@ximian.com)
//         Marek Safar     (marek.safar@gmail.com)
//
// Dual licensed under the terms of the MIT X11 or GNU GPL
//
// Copyright 2001-2003 Ximian, Inc (http://www.ximian.com)
// Copyright 2003-2011 Novell, Inc.
// Copyright 2011 Xamarin Inc
//

using System;
using System.Globalization;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Mono.CSharp
{
	//
	// All compiler built-in types (they have to exist otherwise the compiler will not work)
	//
	public class BuiltinTypes
	{
		public readonly BuiltinTypeSpec Object;
		public readonly BuiltinTypeSpec ValueType;
		public readonly BuiltinTypeSpec Attribute;

		public readonly BuiltinTypeSpec Int;
		public readonly BuiltinTypeSpec UInt;
		public readonly BuiltinTypeSpec Long;
		public readonly BuiltinTypeSpec ULong;
		public readonly BuiltinTypeSpec Float;
		public readonly BuiltinTypeSpec Double;
		public readonly BuiltinTypeSpec Char;
		public readonly BuiltinTypeSpec Short;
		public readonly BuiltinTypeSpec Decimal;
		public readonly BuiltinTypeSpec Bool;
		public readonly BuiltinTypeSpec SByte;
		public readonly BuiltinTypeSpec Byte;
		public readonly BuiltinTypeSpec UShort;
		public readonly BuiltinTypeSpec String;

		public readonly BuiltinTypeSpec Enum;
		public readonly BuiltinTypeSpec Delegate;
		public readonly BuiltinTypeSpec MulticastDelegate;
		public readonly BuiltinTypeSpec Void;
		public readonly BuiltinTypeSpec Array;
		public readonly BuiltinTypeSpec Type;
		public readonly BuiltinTypeSpec IEnumerator;
		public readonly BuiltinTypeSpec IEnumerable;
		public readonly BuiltinTypeSpec IDisposable;
		public readonly BuiltinTypeSpec IntPtr;
		public readonly BuiltinTypeSpec UIntPtr;
		public readonly BuiltinTypeSpec RuntimeFieldHandle;
		public readonly BuiltinTypeSpec RuntimeTypeHandle;
		public readonly BuiltinTypeSpec Exception;

		//
		// These are internal buil-in types which depend on other
		// build-in type (mostly object)
		//
		public readonly BuiltinTypeSpec Dynamic;

		// Predefined operators tables
		public readonly Binary.PredefinedOperator[] OperatorsBinaryStandard;
		public readonly Binary.PredefinedOperator[] OperatorsBinaryEquality;
		public readonly Binary.PredefinedOperator[] OperatorsBinaryUnsafe;
		public readonly TypeSpec[][] OperatorsUnary;
		public readonly TypeSpec[] OperatorsUnaryMutator;

		public readonly TypeSpec[] BinaryPromotionsTypes;

		readonly BuiltinTypeSpec[] types;

		public BuiltinTypes ()
		{
			Object = new BuiltinTypeSpec (MemberKind.Class, "System", "Object", BuiltinTypeSpec.Type.Object);
			ValueType = new BuiltinTypeSpec (MemberKind.Class, "System", "ValueType", BuiltinTypeSpec.Type.ValueType);
			Attribute = new BuiltinTypeSpec (MemberKind.Class, "System", "Attribute", BuiltinTypeSpec.Type.Attribute);

			Int = new BuiltinTypeSpec (MemberKind.Struct, "System", "Int32", BuiltinTypeSpec.Type.Int);
			Long = new BuiltinTypeSpec (MemberKind.Struct, "System", "Int64", BuiltinTypeSpec.Type.Long);
			UInt = new BuiltinTypeSpec (MemberKind.Struct, "System", "UInt32", BuiltinTypeSpec.Type.UInt);
			ULong = new BuiltinTypeSpec (MemberKind.Struct, "System", "UInt64", BuiltinTypeSpec.Type.ULong);
			Byte = new BuiltinTypeSpec (MemberKind.Struct, "System", "Byte", BuiltinTypeSpec.Type.Byte);
			SByte = new BuiltinTypeSpec (MemberKind.Struct, "System", "SByte", BuiltinTypeSpec.Type.SByte);
			Short = new BuiltinTypeSpec (MemberKind.Struct, "System", "Int16", BuiltinTypeSpec.Type.Short);
			UShort = new BuiltinTypeSpec (MemberKind.Struct, "System", "UInt16", BuiltinTypeSpec.Type.UShort);

			IEnumerator = new BuiltinTypeSpec (MemberKind.Interface, "System.Collections", "IEnumerator", BuiltinTypeSpec.Type.IEnumerator);
			IEnumerable = new BuiltinTypeSpec (MemberKind.Interface, "System.Collections", "IEnumerable", BuiltinTypeSpec.Type.IEnumerable);
			IDisposable = new BuiltinTypeSpec (MemberKind.Interface, "System", "IDisposable", BuiltinTypeSpec.Type.IDisposable);

			Char = new BuiltinTypeSpec (MemberKind.Struct, "System", "Char", BuiltinTypeSpec.Type.Char);
			String = new BuiltinTypeSpec (MemberKind.Class, "System", "String", BuiltinTypeSpec.Type.String);
			Float = new BuiltinTypeSpec (MemberKind.Struct, "System", "Single", BuiltinTypeSpec.Type.Float);
			Double = new BuiltinTypeSpec (MemberKind.Struct, "System", "Double", BuiltinTypeSpec.Type.Double);
			Decimal = new BuiltinTypeSpec (MemberKind.Struct, "System", "Decimal", BuiltinTypeSpec.Type.Decimal);
			Bool = new BuiltinTypeSpec (MemberKind.Struct, "System", "Boolean", BuiltinTypeSpec.Type.Bool);
			IntPtr = new BuiltinTypeSpec (MemberKind.Struct, "System", "IntPtr", BuiltinTypeSpec.Type.IntPtr);
			UIntPtr = new BuiltinTypeSpec (MemberKind.Struct, "System", "UIntPtr", BuiltinTypeSpec.Type.UIntPtr);

			MulticastDelegate = new BuiltinTypeSpec (MemberKind.Class, "System", "MulticastDelegate", BuiltinTypeSpec.Type.MulticastDelegate);
			Delegate = new BuiltinTypeSpec (MemberKind.Class, "System", "Delegate", BuiltinTypeSpec.Type.Delegate);
			Enum = new BuiltinTypeSpec (MemberKind.Class, "System", "Enum", BuiltinTypeSpec.Type.Enum);
			Array = new BuiltinTypeSpec (MemberKind.Class, "System", "Array", BuiltinTypeSpec.Type.Array);
			Void = new BuiltinTypeSpec (MemberKind.Void, "System", "Void", BuiltinTypeSpec.Type.Other);
			Type = new BuiltinTypeSpec (MemberKind.Class, "System", "Type", BuiltinTypeSpec.Type.Type);
			Exception = new BuiltinTypeSpec (MemberKind.Class, "System", "Exception", BuiltinTypeSpec.Type.Exception);
			RuntimeFieldHandle = new BuiltinTypeSpec (MemberKind.Struct, "System", "RuntimeFieldHandle", BuiltinTypeSpec.Type.Other);
			RuntimeTypeHandle = new BuiltinTypeSpec (MemberKind.Struct, "System", "RuntimeTypeHandle", BuiltinTypeSpec.Type.Other);

			// TODO: Maybe I should promote it to different kind for faster compares
			Dynamic = new BuiltinTypeSpec ("dynamic", BuiltinTypeSpec.Type.Dynamic);

			OperatorsBinaryStandard = Binary.CreateStandardOperatorsTable (this);
			OperatorsBinaryEquality = Binary.CreateEqualityOperatorsTable (this);
			OperatorsBinaryUnsafe = Binary.CreatePointerOperatorsTable (this);
			OperatorsUnary = Unary.CreatePredefinedOperatorsTable (this);
			OperatorsUnaryMutator = UnaryMutator.CreatePredefinedOperatorsTable (this);

			BinaryPromotionsTypes = ConstantFold.CreateBinaryPromotionsTypes (this);

			types = new BuiltinTypeSpec[] {
				Object, ValueType, Attribute,
				Int, UInt, Long, ULong, Float, Double, Char, Short, Decimal, Bool, SByte, Byte, UShort, String,
				Enum, Delegate, MulticastDelegate, Void, Array, Type, IEnumerator, IEnumerable, IDisposable,
				IntPtr, UIntPtr, RuntimeFieldHandle, RuntimeTypeHandle, Exception };
		}

		public BuiltinTypeSpec[] AllTypes {
			get {
				return types;
			}
		}

		public bool CheckDefinitions (ModuleContainer module)
		{
			var ctx = module.Compiler;
			foreach (var p in types) {
				var found = PredefinedType.Resolve (module, p.Kind, p.Namespace, p.Name, p.Arity, true, true);
				if (found == null || found == p)
					continue;

				var tc = found.MemberDefinition as TypeDefinition;
				if (tc != null) {
					var ns = module.GlobalRootNamespace.GetNamespace (p.Namespace, false);
					ns.SetBuiltinType (p);

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
		public readonly PredefinedType TypedReference;
		public readonly PredefinedType MarshalByRefObject;
		public readonly PredefinedType RuntimeHelpers;
		public readonly PredefinedType IAsyncResult;
		public readonly PredefinedType AsyncCallback;
		public readonly PredefinedType RuntimeArgumentHandle;
		public readonly PredefinedType CharSet;
		public readonly PredefinedType IsVolatile;
		public readonly PredefinedType IEnumeratorGeneric;
		public readonly PredefinedType IListGeneric;
		public readonly PredefinedType IReadOnlyListGeneric;
		public readonly PredefinedType ICollectionGeneric;
		public readonly PredefinedType IReadOnlyCollectionGeneric;
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
		public readonly PredefinedType Array;

		public readonly TypeSpec[] SwitchUserTypes;

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
		public readonly PredefinedType MemberBinding;

		//
		// C# 4.0
		//
		public readonly PredefinedType Binder;
		public readonly PredefinedType CallSite;
		public readonly PredefinedType CallSiteGeneric;
		public readonly PredefinedType BinderFlags;

		//
		// C# 5.0
		//
		public readonly PredefinedType AsyncVoidMethodBuilder;
		public readonly PredefinedType AsyncTaskMethodBuilder;
		public readonly PredefinedType AsyncTaskMethodBuilderGeneric;
		public readonly PredefinedType Action;
		public readonly PredefinedType Task;
		public readonly PredefinedType TaskGeneric;
		public readonly PredefinedType IAsyncStateMachine;
		public readonly PredefinedType INotifyCompletion;
		public readonly PredefinedType ICriticalNotifyCompletion;

		// C# 6.0
		public readonly PredefinedType IFormattable;
		public readonly PredefinedType FormattableString;
		public readonly PredefinedType FormattableStringFactory;

		// C# 7.0
		public readonly PredefinedType[] Tuples;

		public PredefinedTypes (ModuleContainer module)
		{
			TypedReference = new PredefinedType (module, MemberKind.Struct, "System", "TypedReference");
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
			IReadOnlyListGeneric = new PredefinedType (module, MemberKind.Interface, "System.Collections.Generic", "IReadOnlyList", 1);
			ICollectionGeneric = new PredefinedType (module, MemberKind.Interface, "System.Collections.Generic", "ICollection", 1);
			IReadOnlyCollectionGeneric = new PredefinedType (module, MemberKind.Interface, "System.Collections.Generic", "IReadOnlyCollection", 1);
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
			Array = new PredefinedType (module, MemberKind.Class, "System", "Array");

			Expression = new PredefinedType (module, MemberKind.Class, "System.Linq.Expressions", "Expression");
			ExpressionGeneric = new PredefinedType (module, MemberKind.Class, "System.Linq.Expressions", "Expression", 1);
			MemberBinding = new PredefinedType (module, MemberKind.Class, "System.Linq.Expressions", "MemberBinding");
			ParameterExpression = new PredefinedType (module, MemberKind.Class, "System.Linq.Expressions", "ParameterExpression");
			FieldInfo = new PredefinedType (module, MemberKind.Class, "System.Reflection", "FieldInfo");
			MethodBase = new PredefinedType (module, MemberKind.Class, "System.Reflection", "MethodBase");
			MethodInfo = new PredefinedType (module, MemberKind.Class, "System.Reflection", "MethodInfo");
			ConstructorInfo = new PredefinedType (module, MemberKind.Class, "System.Reflection", "ConstructorInfo");

			CallSite = new PredefinedType (module, MemberKind.Class, "System.Runtime.CompilerServices", "CallSite");
			CallSiteGeneric = new PredefinedType (module, MemberKind.Class, "System.Runtime.CompilerServices", "CallSite", 1);
			Binder = new PredefinedType (module, MemberKind.Class, "Microsoft.CSharp.RuntimeBinder", "Binder");
			BinderFlags = new PredefinedType (module, MemberKind.Enum, "Microsoft.CSharp.RuntimeBinder", "CSharpBinderFlags");

			Action = new PredefinedType (module, MemberKind.Delegate, "System", "Action");
			AsyncVoidMethodBuilder = new PredefinedType (module, MemberKind.Struct, "System.Runtime.CompilerServices", "AsyncVoidMethodBuilder");
			AsyncTaskMethodBuilder = new PredefinedType (module, MemberKind.Struct, "System.Runtime.CompilerServices", "AsyncTaskMethodBuilder");
			AsyncTaskMethodBuilderGeneric = new PredefinedType (module, MemberKind.Struct, "System.Runtime.CompilerServices", "AsyncTaskMethodBuilder", 1);
			Task = new PredefinedType (module, MemberKind.Class, "System.Threading.Tasks", "Task");
			TaskGeneric = new PredefinedType (module, MemberKind.Class, "System.Threading.Tasks", "Task", 1);
			IAsyncStateMachine = new PredefinedType (module, MemberKind.Interface, "System.Runtime.CompilerServices", "IAsyncStateMachine");
			INotifyCompletion = new PredefinedType (module, MemberKind.Interface, "System.Runtime.CompilerServices", "INotifyCompletion");
			ICriticalNotifyCompletion = new PredefinedType (module, MemberKind.Interface, "System.Runtime.CompilerServices", "ICriticalNotifyCompletion");

			IFormattable = new PredefinedType (module, MemberKind.Interface, "System", "IFormattable");
			FormattableString = new PredefinedType (module, MemberKind.Class, "System", "FormattableString");
			FormattableStringFactory = new PredefinedType (module, MemberKind.Class, "System.Runtime.CompilerServices", "FormattableStringFactory");

			//
			// Define types which are used for comparison. It does not matter
			// if they don't exist as no error report is needed
			//
			if (TypedReference.Define ())
				TypedReference.TypeSpec.IsSpecialRuntimeType = true;

			if (ArgIterator.Define ())
				ArgIterator.TypeSpec.IsSpecialRuntimeType = true;

			if (IEnumerableGeneric.Define ())
				IEnumerableGeneric.TypeSpec.IsArrayGenericInterface = true;

			if (IListGeneric.Define ())
				IListGeneric.TypeSpec.IsArrayGenericInterface = true;

			if (IReadOnlyListGeneric.Define ())
				IReadOnlyListGeneric.TypeSpec.IsArrayGenericInterface = true;

			if (ICollectionGeneric.Define ())
				ICollectionGeneric.TypeSpec.IsArrayGenericInterface = true;

			if (IReadOnlyCollectionGeneric.Define ())
				IReadOnlyCollectionGeneric.TypeSpec.IsArrayGenericInterface = true;

			if (Nullable.Define ())
				Nullable.TypeSpec.IsNullableType = true;

			if (ExpressionGeneric.Define ())
				ExpressionGeneric.TypeSpec.IsExpressionTreeType = true;

			Task.Define ();
			if (TaskGeneric.Define ())
				TaskGeneric.TypeSpec.IsGenericTask = true;

			SwitchUserTypes = Switch.CreateSwitchUserTypes (module, Nullable.TypeSpec);

			IFormattable.Define ();
			FormattableString.Define ();

			Tuples = new PredefinedType [8];
			for (int i = 0; i < Tuples.Length; i++) {
				var pt = new PredefinedType (module, MemberKind.Struct, "System", "ValueTuple", i + 1);
				Tuples [i] = pt;
				if (pt.Define ())
					pt.TypeSpec.IsTupleType = true;
			}
		}
	}

	class PredefinedMembers
	{
		public readonly PredefinedMember<MethodSpec> ActivatorCreateInstance;
		public readonly PredefinedMember<MethodSpec> ArrayEmpty;
		public readonly PredefinedMember<MethodSpec> AsyncTaskMethodBuilderCreate;
		public readonly PredefinedMember<MethodSpec> AsyncTaskMethodBuilderStart;
		public readonly PredefinedMember<MethodSpec> AsyncTaskMethodBuilderSetResult;
		public readonly PredefinedMember<MethodSpec> AsyncTaskMethodBuilderSetException;
		public readonly PredefinedMember<MethodSpec> AsyncTaskMethodBuilderSetStateMachine;
		public readonly PredefinedMember<MethodSpec> AsyncTaskMethodBuilderOnCompleted;
		public readonly PredefinedMember<MethodSpec> AsyncTaskMethodBuilderOnCompletedUnsafe;
		public readonly PredefinedMember<PropertySpec> AsyncTaskMethodBuilderTask;
		public readonly PredefinedMember<MethodSpec> AsyncTaskMethodBuilderGenericCreate;
		public readonly PredefinedMember<MethodSpec> AsyncTaskMethodBuilderGenericStart;
		public readonly PredefinedMember<MethodSpec> AsyncTaskMethodBuilderGenericSetResult;
		public readonly PredefinedMember<MethodSpec> AsyncTaskMethodBuilderGenericSetException;
		public readonly PredefinedMember<MethodSpec> AsyncTaskMethodBuilderGenericSetStateMachine;
		public readonly PredefinedMember<MethodSpec> AsyncTaskMethodBuilderGenericOnCompleted;
		public readonly PredefinedMember<MethodSpec> AsyncTaskMethodBuilderGenericOnCompletedUnsafe;
		public readonly PredefinedMember<PropertySpec> AsyncTaskMethodBuilderGenericTask;
		public readonly PredefinedMember<MethodSpec> AsyncVoidMethodBuilderCreate;
		public readonly PredefinedMember<MethodSpec> AsyncVoidMethodBuilderStart;
		public readonly PredefinedMember<MethodSpec> AsyncVoidMethodBuilderSetException;
		public readonly PredefinedMember<MethodSpec> AsyncVoidMethodBuilderSetResult;
		public readonly PredefinedMember<MethodSpec> AsyncVoidMethodBuilderSetStateMachine;
		public readonly PredefinedMember<MethodSpec> AsyncVoidMethodBuilderOnCompleted;
		public readonly PredefinedMember<MethodSpec> AsyncVoidMethodBuilderOnCompletedUnsafe;
		public readonly PredefinedMember<MethodSpec> AsyncStateMachineAttributeCtor;
		public readonly PredefinedMember<MethodSpec> DebuggerBrowsableAttributeCtor;
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
		public readonly PredefinedMember<MethodSpec> MethodInfoCreateDelegate;
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
		public readonly PredefinedMember<FieldSpec> StructLayoutSize;
		public readonly PredefinedMember<MethodSpec> TypeGetTypeFromHandle;
		public readonly PredefinedMember<MethodSpec> TupleElementNamesAttributeCtor;

		public PredefinedMembers (ModuleContainer module)
		{
			var types = module.PredefinedTypes;
			var atypes = module.PredefinedAttributes;
			var btypes = module.Compiler.BuiltinTypes;

			var tp = new TypeParameter (0, new MemberName ("T"), null, null, Variance.None);

			ActivatorCreateInstance = new PredefinedMember<MethodSpec> (module, types.Activator,
				MemberFilter.Method ("CreateInstance", 1, ParametersCompiled.EmptyReadOnlyParameters, null));

			ArrayEmpty = new PredefinedMember<MethodSpec> (module, types.Array,
				MemberFilter.Method ("Empty", 1, ParametersCompiled.EmptyReadOnlyParameters, null));

			// TODO: Must me static
			AsyncTaskMethodBuilderCreate = new PredefinedMember<MethodSpec> (module, types.AsyncTaskMethodBuilder,
				MemberFilter.Method ("Create", 0, ParametersCompiled.EmptyReadOnlyParameters, types.AsyncTaskMethodBuilder.TypeSpec));

			AsyncTaskMethodBuilderSetResult = new PredefinedMember<MethodSpec> (module, types.AsyncTaskMethodBuilder,
				MemberFilter.Method ("SetResult", 0, ParametersCompiled.EmptyReadOnlyParameters, btypes.Void));

			AsyncTaskMethodBuilderSetStateMachine = new PredefinedMember<MethodSpec> (module, types.AsyncTaskMethodBuilder,
				"SetStateMachine", MemberKind.Method, () => new[] {
						types.IAsyncStateMachine.TypeSpec
				}, btypes.Void);

			AsyncTaskMethodBuilderSetException = new PredefinedMember<MethodSpec> (module, types.AsyncTaskMethodBuilder,
				MemberFilter.Method ("SetException", 0,
				ParametersCompiled.CreateFullyResolved (btypes.Exception), btypes.Void));

			AsyncTaskMethodBuilderOnCompleted = new PredefinedMember<MethodSpec> (module, types.AsyncTaskMethodBuilder,
				MemberFilter.Method ("AwaitOnCompleted", 2,
					new ParametersImported (
						new[] {
								new ParameterData (null, Parameter.Modifier.REF),
								new ParameterData (null, Parameter.Modifier.REF)
							},
						new[] {
								new TypeParameterSpec (0, tp, SpecialConstraint.None, Variance.None, null),
								new TypeParameterSpec (1, tp, SpecialConstraint.None, Variance.None, null)
							}, false),
					btypes.Void));

			AsyncTaskMethodBuilderOnCompletedUnsafe = new PredefinedMember<MethodSpec> (module, types.AsyncTaskMethodBuilder,
				MemberFilter.Method ("AwaitUnsafeOnCompleted", 2,
					new ParametersImported (
						new[] {
								new ParameterData (null, Parameter.Modifier.REF),
								new ParameterData (null, Parameter.Modifier.REF)
							},
						new[] {
								new TypeParameterSpec (0, tp, SpecialConstraint.None, Variance.None, null),
								new TypeParameterSpec (1, tp, SpecialConstraint.None, Variance.None, null)
							}, false),
					btypes.Void));

			AsyncTaskMethodBuilderStart = new PredefinedMember<MethodSpec> (module, types.AsyncTaskMethodBuilder,
				MemberFilter.Method ("Start", 1,
					new ParametersImported (
						new[] {
								new ParameterData (null, Parameter.Modifier.REF),
							},
						new[] {
								new TypeParameterSpec (0, tp, SpecialConstraint.None, Variance.None, null),
							}, false),
					btypes.Void));

			AsyncTaskMethodBuilderTask = new PredefinedMember<PropertySpec> (module, types.AsyncTaskMethodBuilder,
				MemberFilter.Property ("Task", null));

			// TODO: Must me static
			AsyncTaskMethodBuilderGenericCreate = new PredefinedMember<MethodSpec> (module, types.AsyncTaskMethodBuilderGeneric,
				MemberFilter.Method ("Create", 0, ParametersCompiled.EmptyReadOnlyParameters, types.AsyncVoidMethodBuilder.TypeSpec));

			AsyncTaskMethodBuilderGenericSetResult = new PredefinedMember<MethodSpec> (module, types.AsyncTaskMethodBuilderGeneric,
				"SetResult", MemberKind.Method, () => new TypeSpec[] {
						types.AsyncTaskMethodBuilderGeneric.TypeSpec.MemberDefinition.TypeParameters[0]
				}, btypes.Void);

			AsyncTaskMethodBuilderGenericSetStateMachine = new PredefinedMember<MethodSpec> (module, types.AsyncTaskMethodBuilderGeneric,
				"SetStateMachine", MemberKind.Method, () => new[] {
						types.IAsyncStateMachine.TypeSpec
				}, btypes.Void);

			AsyncTaskMethodBuilderGenericSetException = new PredefinedMember<MethodSpec> (module, types.AsyncTaskMethodBuilderGeneric,
				MemberFilter.Method ("SetException", 0,
				ParametersCompiled.CreateFullyResolved (btypes.Exception), btypes.Void));

			AsyncTaskMethodBuilderGenericOnCompleted = new PredefinedMember<MethodSpec> (module, types.AsyncTaskMethodBuilderGeneric,
				MemberFilter.Method ("AwaitOnCompleted", 2,
					new ParametersImported (
						new[] {
								new ParameterData (null, Parameter.Modifier.REF),
								new ParameterData (null, Parameter.Modifier.REF)
							},
						new[] {
								new TypeParameterSpec (0, tp, SpecialConstraint.None, Variance.None, null),
								new TypeParameterSpec (1, tp, SpecialConstraint.None, Variance.None, null)
							}, false),
					btypes.Void));

			AsyncTaskMethodBuilderGenericOnCompletedUnsafe = new PredefinedMember<MethodSpec> (module, types.AsyncTaskMethodBuilderGeneric,
				MemberFilter.Method ("AwaitUnsafeOnCompleted", 2,
					new ParametersImported (
						new[] {
								new ParameterData (null, Parameter.Modifier.REF),
								new ParameterData (null, Parameter.Modifier.REF)
							},
						new[] {
								new TypeParameterSpec (0, tp, SpecialConstraint.None, Variance.None, null),
								new TypeParameterSpec (1, tp, SpecialConstraint.None, Variance.None, null)
							}, false),
					btypes.Void));

			AsyncTaskMethodBuilderGenericStart = new PredefinedMember<MethodSpec> (module, types.AsyncTaskMethodBuilderGeneric,
				MemberFilter.Method ("Start", 1,
					new ParametersImported (
						new[] {
								new ParameterData (null, Parameter.Modifier.REF),
							},
						new[] {
								new TypeParameterSpec (0, tp, SpecialConstraint.None, Variance.None, null),
							}, false),
					btypes.Void));

			AsyncTaskMethodBuilderGenericTask = new PredefinedMember<PropertySpec> (module, types.AsyncTaskMethodBuilderGeneric,
				MemberFilter.Property ("Task", null));

			// TODO: Must me static
			AsyncVoidMethodBuilderCreate = new PredefinedMember<MethodSpec> (module, types.AsyncVoidMethodBuilder,
				MemberFilter.Method ("Create", 0, ParametersCompiled.EmptyReadOnlyParameters, types.AsyncVoidMethodBuilder.TypeSpec));

			AsyncVoidMethodBuilderSetException = new PredefinedMember<MethodSpec> (module, types.AsyncVoidMethodBuilder,
				MemberFilter.Method ("SetException", 0, null, btypes.Void));

			AsyncVoidMethodBuilderSetResult = new PredefinedMember<MethodSpec> (module, types.AsyncVoidMethodBuilder,
				MemberFilter.Method ("SetResult", 0, ParametersCompiled.EmptyReadOnlyParameters, btypes.Void));

			AsyncVoidMethodBuilderSetStateMachine = new PredefinedMember<MethodSpec> (module, types.AsyncVoidMethodBuilder,
				"SetStateMachine", MemberKind.Method, () => new[] {
						types.IAsyncStateMachine.TypeSpec
				}, btypes.Void);

			AsyncVoidMethodBuilderOnCompleted = new PredefinedMember<MethodSpec> (module, types.AsyncVoidMethodBuilder,
				MemberFilter.Method ("AwaitOnCompleted", 2,
					new ParametersImported (
						new[] {
								new ParameterData (null, Parameter.Modifier.REF),
								new ParameterData (null, Parameter.Modifier.REF)
							},
						new[] {
								new TypeParameterSpec (0, tp, SpecialConstraint.None, Variance.None, null),
								new TypeParameterSpec (1, tp, SpecialConstraint.None, Variance.None, null)
							}, false),
					btypes.Void));

			AsyncVoidMethodBuilderOnCompletedUnsafe = new PredefinedMember<MethodSpec> (module, types.AsyncVoidMethodBuilder,
				MemberFilter.Method ("AwaitUnsafeOnCompleted", 2,
					new ParametersImported (
						new[] {
								new ParameterData (null, Parameter.Modifier.REF),
								new ParameterData (null, Parameter.Modifier.REF)
							},
						new[] {
								new TypeParameterSpec (0, tp, SpecialConstraint.None, Variance.None, null),
								new TypeParameterSpec (1, tp, SpecialConstraint.None, Variance.None, null)
							}, false),
					btypes.Void));

			AsyncVoidMethodBuilderStart = new PredefinedMember<MethodSpec> (module, types.AsyncVoidMethodBuilder,
				MemberFilter.Method ("Start", 1,
					new ParametersImported (
						new[] {
								new ParameterData (null, Parameter.Modifier.REF),
							},
						new[] {
								new TypeParameterSpec (0, tp, SpecialConstraint.None, Variance.None, null),
							}, false),
					btypes.Void));

			AsyncStateMachineAttributeCtor = new PredefinedMember<MethodSpec> (module, atypes.AsyncStateMachine,
				MemberFilter.Constructor (ParametersCompiled.CreateFullyResolved (
					btypes.Type)));

			DebuggerBrowsableAttributeCtor = new PredefinedMember<MethodSpec> (module, atypes.DebuggerBrowsable,
				MemberFilter.Constructor (null));

			DecimalCtor = new PredefinedMember<MethodSpec> (module, btypes.Decimal,
				MemberFilter.Constructor (ParametersCompiled.CreateFullyResolved (
					btypes.Int, btypes.Int, btypes.Int, btypes.Bool, btypes.Byte)));

			DecimalCtorInt = new PredefinedMember<MethodSpec> (module, btypes.Decimal,
				MemberFilter.Constructor (ParametersCompiled.CreateFullyResolved (btypes.Int)));

			DecimalCtorLong = new PredefinedMember<MethodSpec> (module, btypes.Decimal,
				MemberFilter.Constructor (ParametersCompiled.CreateFullyResolved (btypes.Long)));

			DecimalConstantAttributeCtor = new PredefinedMember<MethodSpec> (module, atypes.DecimalConstant,
				MemberFilter.Constructor (ParametersCompiled.CreateFullyResolved (
					btypes.Byte, btypes.Byte, btypes.UInt, btypes.UInt, btypes.UInt)));

			DefaultMemberAttributeCtor = new PredefinedMember<MethodSpec> (module, atypes.DefaultMember,
				MemberFilter.Constructor (ParametersCompiled.CreateFullyResolved (btypes.String)));

			DelegateCombine = new PredefinedMember<MethodSpec> (module, btypes.Delegate, "Combine", btypes.Delegate, btypes.Delegate);
			DelegateRemove = new PredefinedMember<MethodSpec> (module, btypes.Delegate, "Remove", btypes.Delegate, btypes.Delegate);

			DelegateEqual = new PredefinedMember<MethodSpec> (module, btypes.Delegate,
				new MemberFilter (Operator.GetMetadataName (Operator.OpType.Equality), 0, MemberKind.Operator, null, btypes.Bool));

			DelegateInequal = new PredefinedMember<MethodSpec> (module, btypes.Delegate,
				new MemberFilter (Operator.GetMetadataName (Operator.OpType.Inequality), 0, MemberKind.Operator, null, btypes.Bool));

			DynamicAttributeCtor = new PredefinedMember<MethodSpec> (module, atypes.Dynamic,
				MemberFilter.Constructor (ParametersCompiled.CreateFullyResolved (
					ArrayContainer.MakeType (module, btypes.Bool))));

			FieldInfoGetFieldFromHandle = new PredefinedMember<MethodSpec> (module, types.FieldInfo,
				"GetFieldFromHandle", MemberKind.Method, types.RuntimeFieldHandle);

			FieldInfoGetFieldFromHandle2 = new PredefinedMember<MethodSpec> (module, types.FieldInfo,
				"GetFieldFromHandle", MemberKind.Method, types.RuntimeFieldHandle, new PredefinedType (btypes.RuntimeTypeHandle));

			FixedBufferAttributeCtor = new PredefinedMember<MethodSpec> (module, atypes.FixedBuffer,
				MemberFilter.Constructor (ParametersCompiled.CreateFullyResolved (btypes.Type, btypes.Int)));

			IDisposableDispose = new PredefinedMember<MethodSpec> (module, btypes.IDisposable, "Dispose", TypeSpec.EmptyTypes);

			IEnumerableGetEnumerator = new PredefinedMember<MethodSpec> (module, btypes.IEnumerable,
				"GetEnumerator", TypeSpec.EmptyTypes);

			InterlockedCompareExchange = new PredefinedMember<MethodSpec> (module, types.Interlocked,
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

			InterlockedCompareExchange_T = new PredefinedMember<MethodSpec> (module, types.Interlocked,
				MemberFilter.Method ("CompareExchange", 1,
					new ParametersImported (
						new[] {
								new ParameterData (null, Parameter.Modifier.REF),
								new ParameterData (null, Parameter.Modifier.NONE),
								new ParameterData (null, Parameter.Modifier.NONE)
							},
						new[] {
								new TypeParameterSpec (0, tp, SpecialConstraint.None, Variance.None, null),
								new TypeParameterSpec (0, tp, SpecialConstraint.None, Variance.None, null),
								new TypeParameterSpec (0, tp, SpecialConstraint.None, Variance.None, null),
							}, false),
					null));

			MethodInfoGetMethodFromHandle = new PredefinedMember<MethodSpec> (module, types.MethodBase,
				"GetMethodFromHandle", MemberKind.Method, types.RuntimeMethodHandle);

			MethodInfoGetMethodFromHandle2 = new PredefinedMember<MethodSpec> (module, types.MethodBase,
				"GetMethodFromHandle", MemberKind.Method, types.RuntimeMethodHandle, new PredefinedType (btypes.RuntimeTypeHandle));

			MethodInfoCreateDelegate = new PredefinedMember<MethodSpec> (module, types.MethodInfo,
			                                                             "CreateDelegate", MemberKind.Method,
			                                                             new PredefinedType (btypes.Type), new PredefinedType (btypes.Object));

			MonitorEnter = new PredefinedMember<MethodSpec> (module, types.Monitor, "Enter", btypes.Object);

			MonitorEnter_v4 = new PredefinedMember<MethodSpec> (module, types.Monitor,
				MemberFilter.Method ("Enter", 0,
					new ParametersImported (new[] {
							new ParameterData (null, Parameter.Modifier.NONE),
							new ParameterData (null, Parameter.Modifier.REF)
						},
					new[] {
							btypes.Object, btypes.Bool
						}, false), null));

			MonitorExit = new PredefinedMember<MethodSpec> (module, types.Monitor, "Exit", btypes.Object);

			RuntimeCompatibilityWrapNonExceptionThrows = new PredefinedMember<PropertySpec> (module, atypes.RuntimeCompatibility,
				MemberFilter.Property ("WrapNonExceptionThrows", btypes.Bool));

			RuntimeHelpersInitializeArray = new PredefinedMember<MethodSpec> (module, types.RuntimeHelpers,
				"InitializeArray", btypes.Array, btypes.RuntimeFieldHandle);

			RuntimeHelpersOffsetToStringData = new PredefinedMember<PropertySpec> (module, types.RuntimeHelpers,
				MemberFilter.Property ("OffsetToStringData", btypes.Int));

			SecurityActionRequestMinimum = new PredefinedMember<ConstSpec> (module, types.SecurityAction, "RequestMinimum",
				MemberKind.Field, types.SecurityAction);

			StringEmpty = new PredefinedMember<FieldSpec> (module, btypes.String, MemberFilter.Field ("Empty", btypes.String));

			StringEqual = new PredefinedMember<MethodSpec> (module, btypes.String,
				new MemberFilter (Operator.GetMetadataName (Operator.OpType.Equality), 0, MemberKind.Operator, null, btypes.Bool));

			StringInequal = new PredefinedMember<MethodSpec> (module, btypes.String,
				new MemberFilter (Operator.GetMetadataName (Operator.OpType.Inequality), 0, MemberKind.Operator, null, btypes.Bool));

			StructLayoutAttributeCtor = new PredefinedMember<MethodSpec> (module, atypes.StructLayout,
				MemberFilter.Constructor (ParametersCompiled.CreateFullyResolved (btypes.Short)));

			StructLayoutCharSet = new PredefinedMember<FieldSpec> (module, atypes.StructLayout, "CharSet",
				MemberKind.Field, types.CharSet);

			StructLayoutSize = new PredefinedMember<FieldSpec> (module, atypes.StructLayout,
				MemberFilter.Field ("Size", btypes.Int));

			TypeGetTypeFromHandle = new PredefinedMember<MethodSpec> (module, btypes.Type, "GetTypeFromHandle", btypes.RuntimeTypeHandle);

			TupleElementNamesAttributeCtor = new PredefinedMember<MethodSpec> (module, atypes.TupleElementNames,
				MemberFilter.Constructor (ParametersCompiled.CreateFullyResolved (
					ArrayContainer.MakeType (module, btypes.String))));
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
		bool defined;

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

		public PredefinedType (BuiltinTypeSpec type)
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

			if (!defined) {
				defined = true;
				type = Resolve (module, kind, ns, name, arity, false, false);
			}

			return type != null;
		}

		public string GetSignatureForError ()
		{
			return ns + "." + name;
		}

		public static TypeSpec Resolve (ModuleContainer module, MemberKind kind, string ns, string name, int arity, bool required, bool reportErrors)
		{
			//
			// Cannot call it with true because it could create non-existent namespaces for
			// predefined types. It's set to true only for build-in types which all must
			// exist therefore it does not matter, for predefined types we don't want to create
			// fake namespaces when type is optional and does not exist (e.g. System.Linq).
			//
			Namespace type_ns = module.GlobalRootNamespace.GetNamespace (ns, required);

			IList<TypeSpec> found = null;
			if (type_ns != null)
				found = type_ns.GetAllTypes (name);

			if (found == null) {
				if (reportErrors)
					module.Compiler.Report.Error (518, "The predefined type `{0}.{1}' is not defined or imported", ns, name);

				return null;
			}

			TypeSpec best_match = null;
			foreach (var candidate in found) {
				if (candidate.Kind != kind) {
					if (candidate.Kind == MemberKind.Struct && kind == MemberKind.Void && candidate.MemberDefinition is TypeContainer) {
						// Void is declared as struct but we keep it internally as
						// special kind, the swap will be done by caller
					} else {
						continue;
					}
				}

				if (candidate.Arity != arity)
					continue;

				if ((candidate.Modifiers & Modifiers.INTERNAL) != 0 && !candidate.MemberDefinition.IsInternalAsPublic (module.DeclaringAssembly))
					continue;

				if (best_match == null) {
					best_match = candidate;
					continue;
				}

				var other_match = best_match;
				if (!best_match.MemberDefinition.IsImported &&
					module.Compiler.BuiltinTypes.Object.MemberDefinition.DeclaringAssembly == candidate.MemberDefinition.DeclaringAssembly) {
					best_match = candidate;
				}

				string location;
				if (best_match.MemberDefinition is MemberCore) {
					location = ((MemberCore) best_match.MemberDefinition).Location.Name;
				} else {
					var assembly = (ImportedAssemblyDefinition) best_match.MemberDefinition.DeclaringAssembly;
					location = Path.GetFileName (assembly.Location);
				}

				module.Compiler.Report.SymbolRelatedToPreviousError (other_match);
				module.Compiler.Report.SymbolRelatedToPreviousError (candidate);

				module.Compiler.Report.Warning (1685, 1,
					"The predefined type `{0}.{1}' is defined multiple times. Using definition from `{2}'",
					ns, name, location);

				break;
			}

			if (best_match == null && reportErrors) {
				var found_member = found[0];

				if (found_member.Kind == MemberKind.MissingType) {
					// CSC: should be different error number
					module.Compiler.Report.Error (518, "The predefined type `{0}.{1}' is defined in an assembly that is not referenced.", ns, name);
				} else {
					Location loc;
					if (found_member.MemberDefinition is MemberCore) {
						loc = ((MemberCore) found_member.MemberDefinition).Location;
					} else {
						loc = Location.Null;
						module.Compiler.Report.SymbolRelatedToPreviousError (found_member);
					}

					module.Compiler.Report.Error (520, loc, "The predefined type `{0}.{1}' is not declared correctly", ns, name);
				}
			}

			return best_match;
		}

		public TypeSpec Resolve ()
		{
			if (type == null)
				type = Resolve (module, kind, ns, name, arity, false, true);

			return type;
		}
	}

	public class PredefinedMember<T> where T : MemberSpec
	{
		readonly ModuleContainer module;
		T member;
		TypeSpec declaring_type;
		readonly PredefinedType declaring_type_predefined;
		MemberFilter filter;
		readonly Func<TypeSpec[]> filter_builder;

		public PredefinedMember (ModuleContainer module, PredefinedType type, MemberFilter filter)
		{
			this.module = module;
			this.declaring_type_predefined = type;
			this.filter = filter;
		}

		public PredefinedMember (ModuleContainer module, TypeSpec type, MemberFilter filter)
		{
			this.module = module;
			this.declaring_type = type;
			this.filter = filter;
		}

		public PredefinedMember (ModuleContainer module, PredefinedType type, string name, params TypeSpec[] types)
			: this (module, type, MemberFilter.Method (name, 0, ParametersCompiled.CreateFullyResolved (types), null))
		{
		}

		public PredefinedMember (ModuleContainer module, PredefinedType type, string name, MemberKind kind, params PredefinedType[] types)
			: this (module, type, new MemberFilter (name, 0, kind, null, null))
		{
			filter_builder = () => {
				var ptypes = new TypeSpec[types.Length];
				for (int i = 0; i < ptypes.Length; ++i) {
					var p = types[i];
					if (!p.Define ())
						return null;

					ptypes[i] = p.TypeSpec;
				}

				return ptypes;
			};
		}

		public PredefinedMember (ModuleContainer module, PredefinedType type, string name, MemberKind kind, Func<TypeSpec[]> typesBuilder, TypeSpec returnType)
			: this (module, type, new MemberFilter (name, 0, kind, null, returnType))
		{
			filter_builder = typesBuilder;
		}

		public PredefinedMember (ModuleContainer module, BuiltinTypeSpec type, string name, params TypeSpec[] types)
			: this (module, type, MemberFilter.Method (name, 0, ParametersCompiled.CreateFullyResolved (types), null))
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

			if (filter_builder != null) {
				var types = filter_builder ();

				if (filter.Kind == MemberKind.Field)
					filter = new MemberFilter (filter.Name, filter.Arity, filter.Kind, null, types [0]);
				else
					filter = new MemberFilter (filter.Name, filter.Arity, filter.Kind,
						ParametersCompiled.CreateFullyResolved (types), filter.MemberType);
			}

			member = MemberCache.FindMember (declaring_type, filter, BindingRestriction.DeclaredOnly) as T;
			if (member == null)
				return null;

			if (!member.IsAccessible (module))
				return null;

			return member;
		}

		public T Resolve (Location loc)
		{
			if (Get () != null)
				return member;

			if (declaring_type == null) {
				if (declaring_type_predefined.Resolve () == null)
					return null;
			}

			if (filter_builder != null) {
				filter = new MemberFilter (filter.Name, filter.Arity, filter.Kind,
					ParametersCompiled.CreateFullyResolved (filter_builder ()), filter.MemberType);
			}

			string method_args = null;
			if (filter.Parameters != null)
				method_args = filter.Parameters.GetSignatureForError ();

			module.Compiler.Report.Error (656, loc, "The compiler required member `{0}.{1}{2}' could not be found or is inaccessible",
				declaring_type.GetSignatureForError (), filter.Name, method_args);

			return null;
		}
	}

	public class AwaiterDefinition
	{
		public PropertySpec IsCompleted { get; set; }
		public MethodSpec GetResult { get; set; }
		public bool INotifyCompletion { get; set; }

		public bool IsValidPattern {
			get {
				return IsCompleted != null && GetResult != null && IsCompleted.HasGet;
			}
		}
	}

	partial class TypeManager {

	static public string CSharpName(IList<TypeSpec> types)
	{
		if (types.Count == 0)
			return string.Empty;

		StringBuilder sb = new StringBuilder ();
		for (int i = 0; i < types.Count; ++i) {
			if (i > 0)
				sb.Append (",");

			sb.Append (types [i].GetSignatureForError ());
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
		if (t.IsUnmanaged)
			return true;

		while (t.IsPointer)
			t = ((ElementTypeSpec) t).Element;

		rc.Compiler.Report.SymbolRelatedToPreviousError (t);
		rc.Compiler.Report.Error (208, loc,
			"Cannot take the address of, get the size of, or declare a pointer to a managed type `{0}'",
			t.GetSignatureForError ());

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
