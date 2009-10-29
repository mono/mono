//
// conversion.cs: various routines for implementing conversions.
//
// Authors:
//   Miguel de Icaza (miguel@ximian.com)
//   Ravi Pratap (ravi@ximian.com)
//   Marek Safar (marek.safar@gmail.com)
//
// Copyright 2001, 2002, 2003 Ximian, Inc.
// Copyright 2003-2008 Novell, Inc.
//

namespace Mono.CSharp {
	using System;
	using System.Collections;
	using System.Diagnostics;
	using System.Reflection;
	using System.Reflection.Emit;

	//
	// A container class for all the conversion operations
	//
	static class Convert {
		
		static EmptyExpression MyEmptyExpr;
		static DoubleHash explicit_conv;
		static DoubleHash implicit_conv;
		
		static Convert ()
		{
			Reset ();
		}
		
		public static void Reset ()
		{
			MyEmptyExpr = null;
			explicit_conv = new DoubleHash (100);
			implicit_conv = new DoubleHash (100);
		}
		
		static Type TypeParam_EffectiveBaseType (GenericConstraints gc)
		{
			ArrayList list = new ArrayList ();
			list.Add (gc.EffectiveBaseClass);
			foreach (Type t in gc.InterfaceConstraints) {
				if (!TypeManager.IsGenericParameter (t))
					continue;

				GenericConstraints new_gc = TypeManager.GetTypeParameterConstraints (t);
				if (new_gc != null)
					list.Add (TypeParam_EffectiveBaseType (new_gc));
			}
			return FindMostEncompassedType (list);
		}

		//
		// From a one-dimensional array-type S[] to System.Collections.IList<T> and base
		// interfaces of this interface, provided there is an implicit reference conversion
		// from S to T.
		//
		static bool Array_To_IList (Type array, Type list, bool isExplicit)
		{
			if ((array.GetArrayRank () != 1) || !TypeManager.IsGenericType (list))
				return false;

			Type gt = TypeManager.DropGenericTypeArguments (list);
			if ((gt != TypeManager.generic_ilist_type) &&
			    (gt != TypeManager.generic_icollection_type) &&
			    (gt != TypeManager.generic_ienumerable_type))
				return false;

			Type element_type = TypeManager.GetElementType (array);
			Type arg_type = TypeManager.TypeToCoreType (TypeManager.GetTypeArguments (list) [0]);

			if (element_type == arg_type)
				return true;

			if (isExplicit)
				return ExplicitReferenceConversionExists (element_type, arg_type);

			Type t = TypeManager.GetElementType (array);
			if (MyEmptyExpr == null)
				MyEmptyExpr = new EmptyExpression (t);
			else
				MyEmptyExpr.SetType (t);

			return ImplicitReferenceConversionExists (MyEmptyExpr, arg_type);
		}
		
		static bool IList_To_Array(Type list, Type array)
		{
			if (!TypeManager.IsGenericType (list) || !array.IsArray || array.GetArrayRank() != 1)
				return false;
			
			Type gt = TypeManager.DropGenericTypeArguments (list);
			if (gt != TypeManager.generic_ilist_type &&
				gt != TypeManager.generic_icollection_type &&
				gt != TypeManager.generic_ienumerable_type)
				return false;
			
			Type arg_type = TypeManager.TypeToCoreType (TypeManager.GetTypeArguments(list)[0]);
			Type element_type = TypeManager.GetElementType(array);
			
			if (element_type == arg_type)
				return true;
			
			if (MyEmptyExpr == null)
				MyEmptyExpr = new EmptyExpression(element_type);
			else
				MyEmptyExpr.SetType(element_type);
				
			return ImplicitReferenceConversionExists(MyEmptyExpr, arg_type) || ExplicitReferenceConversionExists(element_type, arg_type);
		}

		static Expression ImplicitTypeParameterConversion (Expression expr,
								   Type target_type)
		{
			Type expr_type = expr.Type;

			GenericConstraints gc = TypeManager.GetTypeParameterConstraints (expr_type);

			if (gc == null) {
				if (target_type == TypeManager.object_type)
					return new BoxedCast (expr, target_type);

				return null;
			}

			// We're converting from a type parameter which is known to be a reference type.
			Type base_type = TypeParam_EffectiveBaseType (gc);

			if (TypeManager.IsSubclassOf (base_type, target_type))
				return new ClassCast (expr, target_type);

			if (target_type.IsInterface) {
				if (TypeManager.ImplementsInterface (base_type, target_type))
					return new ClassCast (expr, target_type);

				foreach (Type t in gc.InterfaceConstraints) {
					if (TypeManager.IsSubclassOf (t, target_type))
						return new ClassCast (expr, target_type);
					if (TypeManager.ImplementsInterface (t, target_type))
						return new ClassCast (expr, target_type);
				}
			}

			foreach (Type t in gc.InterfaceConstraints) {
				if (!TypeManager.IsGenericParameter (t))
					continue;
				if (TypeManager.IsSubclassOf (t, target_type))
					return new ClassCast (expr, target_type);
				if (TypeManager.ImplementsInterface (t, target_type))
					return new ClassCast (expr, target_type);
			}

			return null;
		}

		static bool ImplicitTypeParameterBoxingConversion (Type expr_type, Type target_type,
								   out bool use_class_cast)
		{
			GenericConstraints gc = TypeManager.GetTypeParameterConstraints (expr_type);

			if (gc == null) {
				use_class_cast = false;
				return target_type == TypeManager.object_type;
			}

			use_class_cast = true;

			if (!gc.HasReferenceTypeConstraint)
				return false;

			// We're converting from a type parameter which is known to be a reference type.
			Type base_type = TypeParam_EffectiveBaseType (gc);

			if (TypeManager.IsSubclassOf (base_type, target_type))
				return true;

			if (target_type.IsInterface) {
				if (TypeManager.ImplementsInterface (base_type, target_type))
					return true;

				foreach (Type t in gc.InterfaceConstraints) {
					if (TypeManager.IsSubclassOf (t, target_type))
						return true;
					if (TypeManager.ImplementsInterface (t, target_type))
						return true;
				}
			}

			foreach (Type t in gc.InterfaceConstraints) {
				if (!TypeManager.IsGenericParameter (t))
					continue;
				if (TypeManager.IsSubclassOf (t, target_type))
					return true;
				if (TypeManager.ImplementsInterface (t, target_type))
					return true;
			}

			use_class_cast = false;
			return false;
		}

		static Expression ExplicitTypeParameterConversion (Expression source, Type source_type, Type target_type)
		{
			if (TypeManager.IsGenericParameter (target_type)) {
				GenericConstraints gc = TypeManager.GetTypeParameterConstraints (target_type);
				if (gc == null)
					return null;

				foreach (Type iface in gc.InterfaceConstraints) {
					if (!TypeManager.IsGenericParameter (iface))
						continue;

					if (TypeManager.IsSubclassOf (source_type, iface))
						return source == null ? EmptyExpression.Null : new ClassCast (source, target_type, true);
				}
			}

			if (target_type.IsInterface)
				return source == null ? EmptyExpression.Null : new ClassCast (source, target_type, true);

			return null;
		}

		static Expression ImplicitReferenceConversion (Expression expr, Type target_type, bool explicit_cast)
		{
			Type expr_type = expr.Type;

			if (expr_type == null && expr.eclass == ExprClass.MethodGroup){
				// if we are a method group, emit a warning

				expr.Emit (null);
			}

			if (expr_type == TypeManager.void_type)
				return null;

			if (TypeManager.IsGenericParameter (expr_type))
				return ImplicitTypeParameterConversion (expr, target_type);

			//
			// from the null type to any reference-type.
			//
			NullLiteral nl = expr as NullLiteral;
			if (nl != null) {
				return nl.ConvertImplicitly(target_type);
			}

			if (ImplicitReferenceConversionExists (expr, target_type)) {
				// 
				// Avoid wrapping implicitly convertible reference type
				//
				if (!explicit_cast)
					return expr;

				return EmptyCast.Create (expr, target_type);
			}

			bool use_class_cast;
			if (ImplicitBoxingConversionExists (expr, target_type, out use_class_cast)) {
				if (use_class_cast)
					return new ClassCast (expr, target_type);
				else
					return new BoxedCast (expr, target_type);
			}

			return null;
		}

		//
		// 6.1.6 Implicit reference conversions
		//
		public static bool ImplicitReferenceConversionExists (Expression expr, Type target_type)
		{
			if (TypeManager.IsStruct (target_type))
				return false;

			Type expr_type = expr.Type;

			// from the null type to any reference-type.
			if (expr_type == TypeManager.null_type)
				return target_type != InternalType.AnonymousMethod;

			if (TypeManager.IsGenericParameter (expr_type))
				return ImplicitTypeParameterConversion (expr, target_type) != null;

			//
			// notice that it is possible to write "ValueType v = 1", the ValueType here
			// is an abstract class, and not really a value type, so we apply the same rules.
			//
			if (target_type == TypeManager.object_type || TypeManager.IsDynamicType (target_type)) {
				//
				// A pointer type cannot be converted to object
				//
				if (expr_type.IsPointer)
					return false;

				if (TypeManager.IsValueType (expr_type))
					return false;

				if (expr_type.IsClass || expr_type.IsInterface || expr_type == TypeManager.enum_type){
					// No mcs internal types are convertible
					return expr_type.Module != typeof (Convert).Module;
				}

				return false;
			} else if (target_type == TypeManager.value_type) {
				return expr_type == TypeManager.enum_type;
			} else if (TypeManager.IsSubclassOf (expr_type, target_type)) {
				//
				// Special case: enumeration to System.Enum.
				// System.Enum is not a value type, it is a class, so we need
				// a boxing conversion
				//
				if (target_type == TypeManager.enum_type || TypeManager.IsGenericParameter (expr_type))
					return false;
				
				return true;
			}

			// This code is kind of mirrored inside ImplicitStandardConversionExists
			// with the small distinction that we only probe there
			//
			// Always ensure that the code here and there is in sync

			// from any class-type S to any interface-type T.
			if (target_type.IsInterface) {
				if (TypeManager.ImplementsInterface (expr_type, target_type)){
					return !TypeManager.IsGenericParameter (expr_type) &&
						!TypeManager.IsValueType (expr_type);
				}
			}

			if (expr_type.IsArray) {
				// from an array-type S to an array-type of type T
				if (target_type.IsArray && expr_type.GetArrayRank () == target_type.GetArrayRank ()) {

					//
					// Both SE and TE are reference-types
					//
					Type expr_element_type = TypeManager.GetElementType (expr_type);
					if (!TypeManager.IsReferenceType (expr_element_type))
						return false;

					Type target_element_type = TypeManager.GetElementType (target_type);
					if (!TypeManager.IsReferenceType (target_element_type))
						return false;

					if (MyEmptyExpr == null)
						MyEmptyExpr = new EmptyExpression (expr_element_type);
					else
						MyEmptyExpr.SetType (expr_element_type);

					return ImplicitStandardConversionExists (MyEmptyExpr, target_element_type);
				}

				// from an array-type to System.Array
				if (target_type == TypeManager.array_type)
					return true;

				// from an array-type of type T to IList<T>
				if (Array_To_IList (expr_type, target_type, false))
					return true;

				return false;
			}

			if (TypeManager.IsVariantOf (expr_type, target_type))
				return true;

			// from any interface type S to interface-type T.
			if (expr_type.IsInterface && target_type.IsInterface) {
				return TypeManager.ImplementsInterface (expr_type, target_type);
			}

			// from any delegate type to System.Delegate
			if (target_type == TypeManager.delegate_type &&
				(expr_type == TypeManager.delegate_type || TypeManager.IsDelegateType (expr_type)))
				return true;

			if (TypeManager.IsEqual (expr_type, target_type))
				return true;

			return false;
		}

		public static bool ImplicitBoxingConversionExists (Expression expr, Type target_type,
								   out bool use_class_cast)
		{
			Type expr_type = expr.Type;
			use_class_cast = false;
			
			//
			// From any value-type to the type object.
			//
			if (target_type == TypeManager.object_type || TypeManager.IsDynamicType (target_type)) {
				//
				// A pointer type cannot be converted to object
				//
				if (expr_type.IsPointer)
					return false;

				return TypeManager.IsValueType (expr_type);
			}
			
			//
			// From any value-type to the type System.ValueType.
			//
			if (target_type == TypeManager.value_type)
				return TypeManager.IsValueType (expr_type);

			if (target_type == TypeManager.enum_type) {
				//
				// From any enum-type to the type System.Enum.
				//
				if (TypeManager.IsEnumType (expr_type))
					return true;
				//
				// From any nullable-type with an underlying enum-type to the type System.Enum
				//
				if (TypeManager.IsNullableType (expr_type))
					return TypeManager.IsEnumType (TypeManager.GetTypeArguments (expr_type) [0]);
			}

			if (TypeManager.IsSubclassOf (expr_type, target_type)) {
				//
				// Don't box same type arguments
				//
				if (TypeManager.IsGenericParameter (expr_type) && expr_type != target_type)
					return true;

				return false;
			}

			// This code is kind of mirrored inside ImplicitStandardConversionExists
			// with the small distinction that we only probe there
			//
			// Always ensure that the code here and there is in sync

			// from any class-type S to any interface-type T.
			if (target_type.IsInterface) {
				if (TypeManager.ImplementsInterface (expr_type, target_type))
					return TypeManager.IsGenericParameter (expr_type) ||
						TypeManager.IsValueType (expr_type);
			}

			if (TypeManager.IsGenericParameter (expr_type))
				return ImplicitTypeParameterBoxingConversion (
					expr_type, target_type, out use_class_cast);

			return false;
		}

		public static Expression ImplicitNulableConversion (ResolveContext ec, Expression expr, Type target_type)
		{
			Type expr_type = expr.Type;

			//
			// From null to any nullable type
			//
			if (expr_type == TypeManager.null_type)
				return ec == null ? EmptyExpression.Null : Nullable.LiftedNull.Create (target_type, expr.Location);

			// S -> T?
			Type t_el = TypeManager.TypeToCoreType (TypeManager.GetTypeArguments (target_type)[0]);

			// S? -> T?
			if (TypeManager.IsNullableType (expr_type))
				expr_type = TypeManager.TypeToCoreType (TypeManager.GetTypeArguments (expr_type)[0]);

			//
			// Predefined implicit identity or implicit numeric conversion
			// has to exist between underlying type S and underlying type T
			//

			// Handles probing
			if (ec == null) {
				if (expr_type == t_el)
					return EmptyExpression.Null;

				return ImplicitNumericConversion (null, expr_type, t_el);
			}

			Expression unwrap;
			if (expr_type != expr.Type)
				unwrap = Nullable.Unwrap.Create (expr);
			else
				unwrap = expr;

			Expression conv = expr_type == t_el ? unwrap : ImplicitNumericConversion (unwrap, expr_type, t_el);
			if (conv == null)
				return null;

			if (expr_type != expr.Type)
				return new Nullable.Lifted (conv, unwrap, target_type).Resolve (ec);

			// Do constant optimization for S -> T?
			if (unwrap is Constant)
				conv = ((Constant) unwrap).ConvertImplicitly (t_el);

			return Nullable.Wrap.Create (conv, target_type);
		}

		/// <summary>
		///   Implicit Numeric Conversions.
		///
		///   expr is the expression to convert, returns a new expression of type
		///   target_type or null if an implicit conversion is not possible.
		/// </summary>
		public static Expression ImplicitNumericConversion (Expression expr, Type target_type)
		{
			return ImplicitNumericConversion (expr, expr.Type, target_type);
		}

		static Expression ImplicitNumericConversion (Expression expr, Type expr_type, Type target_type)
		{
			if (expr_type == TypeManager.sbyte_type){
				//
				// From sbyte to short, int, long, float, double, decimal
				//
				if (target_type == TypeManager.int32_type)
					return expr == null ? EmptyExpression.Null : new OpcodeCast (expr, target_type, OpCodes.Conv_I4);
				if (target_type == TypeManager.int64_type)
					return expr == null ? EmptyExpression.Null : new OpcodeCast (expr, target_type, OpCodes.Conv_I8);
				if (target_type == TypeManager.double_type)
					return expr == null ? EmptyExpression.Null : new OpcodeCast (expr, target_type, OpCodes.Conv_R8);
				if (target_type == TypeManager.float_type)
					return expr == null ? EmptyExpression.Null : new OpcodeCast (expr, target_type, OpCodes.Conv_R4);
				if (target_type == TypeManager.short_type)
					return expr == null ? EmptyExpression.Null : new OpcodeCast (expr, target_type, OpCodes.Conv_I2);
				if (target_type == TypeManager.decimal_type)
					return expr == null ? EmptyExpression.Null : new CastToDecimal (expr);
			} else if (expr_type == TypeManager.byte_type){
				//
				// From byte to short, ushort, int, uint, long, ulong, float, double, decimal
				//
				if (target_type == TypeManager.int32_type || target_type == TypeManager.uint32_type ||
				    target_type == TypeManager.short_type || target_type == TypeManager.ushort_type)
					return expr == null ? EmptyExpression.Null : EmptyCast.Create (expr, target_type);

				if (target_type == TypeManager.uint64_type)
					return expr == null ? EmptyExpression.Null : new OpcodeCast (expr, target_type, OpCodes.Conv_U8);
				if (target_type == TypeManager.int64_type)
					return expr == null ? EmptyExpression.Null : new OpcodeCast (expr, target_type, OpCodes.Conv_I8);
				if (target_type == TypeManager.float_type)
					return expr == null ? EmptyExpression.Null : new OpcodeCast (expr, target_type, OpCodes.Conv_R4);
				if (target_type == TypeManager.double_type)
					return expr == null ? EmptyExpression.Null : new OpcodeCast (expr, target_type, OpCodes.Conv_R8);
				if (target_type == TypeManager.decimal_type)
					return expr == null ? EmptyExpression.Null : new CastToDecimal (expr);

			} else if (expr_type == TypeManager.short_type){
				//
				// From short to int, long, float, double, decimal
				//
				if (target_type == TypeManager.int32_type)
					return expr == null ? EmptyExpression.Null : EmptyCast.Create (expr, target_type);
				if (target_type == TypeManager.int64_type)
					return expr == null ? EmptyExpression.Null : new OpcodeCast (expr, target_type, OpCodes.Conv_I8);
				if (target_type == TypeManager.double_type)
					return expr == null ? EmptyExpression.Null : new OpcodeCast (expr, target_type, OpCodes.Conv_R8);
				if (target_type == TypeManager.float_type)
					return expr == null ? EmptyExpression.Null : new OpcodeCast (expr, target_type, OpCodes.Conv_R4);
				if (target_type == TypeManager.decimal_type)
					return expr == null ? EmptyExpression.Null : new CastToDecimal (expr);

			} else if (expr_type == TypeManager.ushort_type){
				//
				// From ushort to int, uint, long, ulong, float, double, decimal
				//
				if (target_type == TypeManager.int32_type || target_type == TypeManager.uint32_type)
					return expr == null ? EmptyExpression.Null : EmptyCast.Create (expr, target_type);
				
				if (target_type == TypeManager.uint64_type)
					return expr == null ? EmptyExpression.Null : new OpcodeCast (expr, target_type, OpCodes.Conv_U8);
				if (target_type == TypeManager.int64_type)
					return expr == null ? EmptyExpression.Null : new OpcodeCast (expr, target_type, OpCodes.Conv_I8);
				if (target_type == TypeManager.double_type)
					return expr == null ? EmptyExpression.Null : new OpcodeCast (expr, target_type, OpCodes.Conv_R8);
				if (target_type == TypeManager.float_type)
					return expr == null ? EmptyExpression.Null : new OpcodeCast (expr, target_type, OpCodes.Conv_R4);
				if (target_type == TypeManager.decimal_type)
					return expr == null ? EmptyExpression.Null : new CastToDecimal (expr);
			} else if (expr_type == TypeManager.int32_type){
				//
				// From int to long, float, double, decimal
				//
				if (target_type == TypeManager.int64_type)
					return expr == null ? EmptyExpression.Null : new OpcodeCast (expr, target_type, OpCodes.Conv_I8);
				if (target_type == TypeManager.double_type)
					return expr == null ? EmptyExpression.Null : new OpcodeCast (expr, target_type, OpCodes.Conv_R8);
				if (target_type == TypeManager.float_type)
					return expr == null ? EmptyExpression.Null : new OpcodeCast (expr, target_type, OpCodes.Conv_R4);
				if (target_type == TypeManager.decimal_type)
					return expr == null ? EmptyExpression.Null : new CastToDecimal (expr);
			} else if (expr_type == TypeManager.uint32_type){
				//
				// From uint to long, ulong, float, double, decimal
				//
				if (target_type == TypeManager.int64_type)
					return expr == null ? EmptyExpression.Null : new OpcodeCast (expr, target_type, OpCodes.Conv_U8);
				if (target_type == TypeManager.uint64_type)
					return expr == null ? EmptyExpression.Null : new OpcodeCast (expr, target_type, OpCodes.Conv_U8);
				if (target_type == TypeManager.double_type)
					return expr == null ? EmptyExpression.Null : new OpcodeCast (new OpcodeCast (expr, target_type, OpCodes.Conv_R_Un), target_type, OpCodes.Conv_R8);
				if (target_type == TypeManager.float_type)
					return expr == null ? EmptyExpression.Null : new OpcodeCast (new OpcodeCast (expr, target_type, OpCodes.Conv_R_Un), target_type, OpCodes.Conv_R4);
				if (target_type == TypeManager.decimal_type)
					return expr == null ? EmptyExpression.Null : new CastToDecimal (expr);
			} else if (expr_type == TypeManager.int64_type){
				//
				// From long/ulong to float, double
				//
				if (target_type == TypeManager.double_type)
					return expr == null ? EmptyExpression.Null : new OpcodeCast (expr, target_type, OpCodes.Conv_R8);
				if (target_type == TypeManager.float_type)
					return expr == null ? EmptyExpression.Null : new OpcodeCast (expr, target_type, OpCodes.Conv_R4);
				if (target_type == TypeManager.decimal_type)
					return expr == null ? EmptyExpression.Null : new CastToDecimal (expr);
			} else if (expr_type == TypeManager.uint64_type){
				//
				// From ulong to float, double
				//
				if (target_type == TypeManager.double_type)
					return expr == null ? EmptyExpression.Null : new OpcodeCast (new OpcodeCast (expr, target_type, OpCodes.Conv_R_Un), target_type, OpCodes.Conv_R8);
				if (target_type == TypeManager.float_type)
					return expr == null ? EmptyExpression.Null : new OpcodeCast (new OpcodeCast (expr, target_type, OpCodes.Conv_R_Un), target_type, OpCodes.Conv_R4);
				if (target_type == TypeManager.decimal_type)
					return expr == null ? EmptyExpression.Null : new CastToDecimal (expr);
			} else if (expr_type == TypeManager.char_type){
				//
				// From char to ushort, int, uint, long, ulong, float, double, decimal
				//
				if ((target_type == TypeManager.ushort_type) ||
				    (target_type == TypeManager.int32_type) ||
				    (target_type == TypeManager.uint32_type))
					return expr == null ? EmptyExpression.Null : EmptyCast.Create (expr, target_type);
				if (target_type == TypeManager.uint64_type)
					return expr == null ? EmptyExpression.Null : new OpcodeCast (expr, target_type, OpCodes.Conv_U8);
				if (target_type == TypeManager.int64_type)
					return expr == null ? EmptyExpression.Null : new OpcodeCast (expr, target_type, OpCodes.Conv_I8);
				if (target_type == TypeManager.float_type)
					return expr == null ? EmptyExpression.Null : new OpcodeCast (expr, target_type, OpCodes.Conv_R4);
				if (target_type == TypeManager.double_type)
					return expr == null ? EmptyExpression.Null : new OpcodeCast (expr, target_type, OpCodes.Conv_R8);
				if (target_type == TypeManager.decimal_type)
					return expr == null ? EmptyExpression.Null : new CastToDecimal (expr);
			} else if (expr_type == TypeManager.float_type){
				//
				// float to double
				//
				if (target_type == TypeManager.double_type)
					return expr == null ? EmptyExpression.Null : new OpcodeCast (expr, target_type, OpCodes.Conv_R8);
			}

			return null;
		}

		/// <summary>
		///  Same as ImplicitStandardConversionExists except that it also looks at
		///  implicit user defined conversions - needed for overload resolution
		/// </summary>
		public static bool ImplicitConversionExists (ResolveContext ec, Expression expr, Type target_type)
		{
			if (ImplicitStandardConversionExists (expr, target_type))
				return true;

			if (expr.Type == InternalType.AnonymousMethod) {
				if (!TypeManager.IsDelegateType (target_type) &&
					TypeManager.DropGenericTypeArguments (target_type) != TypeManager.expression_type)
					return false;

				AnonymousMethodExpression ame = (AnonymousMethodExpression) expr;
				return ame.ImplicitStandardConversionExists (ec, target_type);
			}
			
			if (expr.eclass == ExprClass.MethodGroup) {
				if (TypeManager.IsDelegateType (target_type) && RootContext.Version != LanguageVersion.ISO_1) {
					MethodGroupExpr mg = expr as MethodGroupExpr;
					if (mg != null)
						return DelegateCreation.ImplicitStandardConversionExists (ec, mg, target_type);
				}

				return false;
			}

			return ImplicitUserConversion (ec, expr, target_type, Location.Null) != null;
		}

		public static bool ImplicitUserConversionExists (ResolveContext ec, Type source, Type target)
		{
			return ImplicitUserConversion (ec, new EmptyExpression (source), target, Location.Null) != null;
		}

		/// <summary>
		///  Determines if a standard implicit conversion exists from
		///  expr_type to target_type
		///
		///  ec should point to a real EmitContext if expr.Type is TypeManager.anonymous_method_type.
		/// </summary>
		public static bool ImplicitStandardConversionExists (Expression expr, Type target_type)
		{
			Type expr_type = expr.Type;

			if (expr_type == TypeManager.null_type) {
				NullLiteral nl = expr as NullLiteral;
				if (nl != null)
					return nl.ConvertImplicitly (target_type) != null;
			}

			if (expr_type == TypeManager.void_type)
				return false;

			if (TypeManager.IsEqual (expr_type, target_type))
				return true;

			if (TypeManager.IsNullableType (target_type)) {
				return ImplicitNulableConversion (null, expr, target_type) != null;
			}

			// First numeric conversions
			if (ImplicitNumericConversion (null, expr_type, target_type) != null)
				return true;

			if (ImplicitReferenceConversionExists (expr, target_type))
				return true;

			bool use_class_cast;
			if (ImplicitBoxingConversionExists (expr, target_type, out use_class_cast))
				return true;

			//
			// Implicit Constant Expression Conversions
			//
			if (expr is IntConstant){
				int value = ((IntConstant) expr).Value;

				if (target_type == TypeManager.sbyte_type){
					if (value >= SByte.MinValue && value <= SByte.MaxValue)
						return true;
				} else if (target_type == TypeManager.byte_type){
					if (value >= 0 && value <= Byte.MaxValue)
						return true;
				} else if (target_type == TypeManager.short_type){
					if (value >= Int16.MinValue && value <= Int16.MaxValue)
						return true;
				} else if (target_type == TypeManager.ushort_type){
					if (value >= UInt16.MinValue && value <= UInt16.MaxValue)
						return true;
				} else if (target_type == TypeManager.uint32_type){
					if (value >= 0)
						return true;
				} else if (target_type == TypeManager.uint64_type){
					 //
					 // we can optimize this case: a positive int32
					 // always fits on a uint64.  But we need an opcode
					 // to do it.
					 //
					if (value >= 0)
						return true;
				}

				if (value == 0 && expr is IntLiteral && TypeManager.IsEnumType (target_type))
					return true;
			}

			if (expr is LongConstant && target_type == TypeManager.uint64_type){
				//
				// Try the implicit constant expression conversion
				// from long to ulong, instead of a nice routine,
				// we just inline it
				//
				long v = ((LongConstant) expr).Value;
				if (v >= 0)
					return true;
			}

			//
			// If `expr_type' implements `target_type' (which is an iface)
			// see TryImplicitIntConversion
			//
			if (target_type.IsInterface && TypeManager.ImplementsInterface (expr_type, target_type))
				return true;

			if (target_type == TypeManager.void_ptr_type && expr_type.IsPointer)
				return true;

			// Conversion from __arglist to System.ArgIterator
			if (expr_type == InternalType.Arglist)
				return target_type == TypeManager.arg_iterator_type;

			return false;
		}

		/// <summary>
		///  Finds "most encompassed type" according to the spec (13.4.2)
		///  amongst the methods in the MethodGroupExpr
		/// </summary>
		static Type FindMostEncompassedType (ArrayList types)
		{
			Type best = null;

			if (types.Count == 0)
				return null;

			if (types.Count == 1)
				return (Type) types [0];

			EmptyExpression expr = EmptyExpression.Grab ();

			foreach (Type t in types) {
				if (best == null) {
					best = t;
					continue;
				}

				expr.SetType (t);
				if (ImplicitStandardConversionExists (expr, best))
					best = t;
			}

			expr.SetType (best);
			foreach (Type t in types) {
				if (best == t)
					continue;
				if (!ImplicitStandardConversionExists (expr, t)) {
					best = null;
					break;
				}
			}

			EmptyExpression.Release (expr);

			return best;
		}

		/// <summary>
		///  Finds "most encompassing type" according to the spec (13.4.2)
		///  amongst the types in the given set
		/// </summary>
		static Type FindMostEncompassingType (ArrayList types)
		{
			Type best = null;

			if (types.Count == 0)
				return null;

			if (types.Count == 1)
				return (Type) types [0];

			EmptyExpression expr = EmptyExpression.Grab ();

			foreach (Type t in types) {
				if (best == null) {
					best = t;
					continue;
				}

				expr.SetType (best);
				if (ImplicitStandardConversionExists (expr, t))
					best = t;
			}

			foreach (Type t in types) {
				if (best == t)
					continue;
				expr.SetType (t);
				if (!ImplicitStandardConversionExists (expr, best)) {
					best = null;
					break;
				}
			}

			EmptyExpression.Release (expr);

			return best;
		}

		/// <summary>
		///   Finds the most specific source Sx according to the rules of the spec (13.4.4)
		///   by making use of FindMostEncomp* methods. Applies the correct rules separately
		///   for explicit and implicit conversion operators.
		/// </summary>
		static public Type FindMostSpecificSource (IList list,
							   Expression source, bool apply_explicit_conv_rules)
		{
			ArrayList src_types_set = new ArrayList ();

			//
			// If any operator converts from S then Sx = S
			//
			Type source_type = source.Type;
			foreach (MethodBase mb in list){
				AParametersCollection pd = TypeManager.GetParameterData (mb);
				Type param_type = pd.Types [0];

				if (param_type == source_type)
					return param_type;

				src_types_set.Add (param_type);
			}

			//
			// Explicit Conv rules
			//
			if (apply_explicit_conv_rules) {
				ArrayList candidate_set = new ArrayList ();

				foreach (Type param_type in src_types_set){
					if (ImplicitStandardConversionExists (source, param_type))
						candidate_set.Add (param_type);
				}

				if (candidate_set.Count != 0)
					return FindMostEncompassedType (candidate_set);
			}

			//
			// Final case
			//
			if (apply_explicit_conv_rules)
				return FindMostEncompassingType (src_types_set);
			else
				return FindMostEncompassedType (src_types_set);
		}

		/// <summary>
		///  Finds the most specific target Tx according to section 13.4.4
		/// </summary>
		static public Type FindMostSpecificTarget (IList list,
							   Type target, bool apply_explicit_conv_rules)
		{
			ArrayList tgt_types_set = new ArrayList ();

			//
			// If any operator converts to T then Tx = T
			//
			foreach (MethodInfo mi in list){
				Type ret_type = TypeManager.TypeToCoreType (mi.ReturnType);
				if (ret_type == target)
					return ret_type;

				tgt_types_set.Add (ret_type);
			}

			//
			// Explicit conv rules
			//
			if (apply_explicit_conv_rules) {
				ArrayList candidate_set = new ArrayList ();

				EmptyExpression expr = EmptyExpression.Grab ();

				foreach (Type ret_type in tgt_types_set){
					expr.SetType (ret_type);

					if (ImplicitStandardConversionExists (expr, target))
						candidate_set.Add (ret_type);
				}

				EmptyExpression.Release (expr);

				if (candidate_set.Count != 0)
					return FindMostEncompassingType (candidate_set);
			}

			//
			// Okay, final case !
			//
			if (apply_explicit_conv_rules)
				return FindMostEncompassedType (tgt_types_set);
			else
				return FindMostEncompassingType (tgt_types_set);
		}

		/// <summary>
		///  User-defined Implicit conversions
		/// </summary>
		static public Expression ImplicitUserConversion (ResolveContext ec, Expression source,
								 Type target, Location loc)
		{
			return UserDefinedConversion (ec, source, target, loc, false, true);
		}

		/// <summary>
		///  User-defined Explicit conversions
		/// </summary>
		static public Expression ExplicitUserConversion (ResolveContext ec, Expression source,
								 Type target, Location loc)
		{
			return UserDefinedConversion (ec, source, target, loc, true, true);
		}

		static void AddConversionOperators (ArrayList list,
						    Expression source, Type target_type,
						    bool look_for_explicit,
						    MethodGroupExpr mg)
		{
			if (mg == null)
				return;

			Type source_type = source.Type;
			EmptyExpression expr = EmptyExpression.Grab ();

			//
			// LAMESPEC: Undocumented IntPtr/UIntPtr conversions
			// IntPtr -> uint uses int
			// UIntPtr -> long uses ulong
			//
			if (source_type == TypeManager.intptr_type) {
				if (target_type == TypeManager.uint32_type)
					target_type = TypeManager.int32_type;
			} else if (source_type == TypeManager.uintptr_type) {
				if (target_type == TypeManager.int64_type)
					target_type = TypeManager.uint64_type;
			}

			foreach (MethodInfo m in mg.Methods) {
				AParametersCollection pd = TypeManager.GetParameterData (m);
				Type return_type = TypeManager.TypeToCoreType (m.ReturnType);
				Type arg_type = pd.Types [0];

				if (source_type != arg_type) {
					if (!ImplicitStandardConversionExists (source, arg_type)) {
						if (!look_for_explicit)
							continue;
						expr.SetType (arg_type);
						if (!ImplicitStandardConversionExists (expr, source_type))
							continue;
					}
				}

				if (target_type != return_type) {
					expr.SetType (return_type);
					if (!ImplicitStandardConversionExists (expr, target_type)) {
						if (!look_for_explicit)
							continue;
						expr.SetType (target_type);
						if (!ImplicitStandardConversionExists (expr, return_type))
							continue;
					}
				}

				// See LAMESPEC: Exclude IntPtr -> int conversion
				if (source_type == TypeManager.uintptr_type && return_type == TypeManager.uint32_type)
					continue;

				list.Add (m);
			}

			EmptyExpression.Release (expr);
		}

		/// <summary>
		///   Compute the user-defined conversion operator from source_type to target_type.
		///   `look_for_explicit' controls whether we should also include the list of explicit operators
		/// </summary>
		static MethodInfo GetConversionOperator (CompilerContext ctx, Type container_type, Expression source, Type target_type, bool look_for_explicit)
		{
			ArrayList ops = new ArrayList (4);

			Type source_type = source.Type;

			if (source_type != TypeManager.decimal_type) {
				AddConversionOperators (ops, source, target_type, look_for_explicit,
					Expression.MethodLookup (ctx, container_type, source_type, "op_Implicit", Location.Null) as MethodGroupExpr);
				if (look_for_explicit) {
					AddConversionOperators (ops, source, target_type, look_for_explicit,
						Expression.MethodLookup (ctx,
							container_type, source_type, "op_Explicit", Location.Null) as MethodGroupExpr);
				}
			}

			if (target_type != TypeManager.decimal_type) {
				AddConversionOperators (ops, source, target_type, look_for_explicit,
					Expression.MethodLookup (ctx, container_type, target_type, "op_Implicit", Location.Null) as MethodGroupExpr);
				if (look_for_explicit) {
					AddConversionOperators (ops, source, target_type, look_for_explicit,
						Expression.MethodLookup (ctx,
							container_type, target_type, "op_Explicit", Location.Null) as MethodGroupExpr);
				}
			}

			if (ops.Count == 0)
				return null;

			Type most_specific_source = FindMostSpecificSource (ops, source, look_for_explicit);
			if (most_specific_source == null)
				return null;

			Type most_specific_target = FindMostSpecificTarget (ops, target_type, look_for_explicit);
			if (most_specific_target == null)
				return null;

			MethodInfo method = null;

			foreach (MethodInfo m in ops) {
				if (TypeManager.TypeToCoreType (m.ReturnType) != most_specific_target)
					continue;
				if (TypeManager.GetParameterData (m).Types [0] != most_specific_source)
					continue;
				// Ambiguous: more than one conversion operator satisfies the signature.
				if (method != null)
					return null;
				method = m;
			}

			return method;
		}

		/// <summary>
		///   User-defined conversions
		/// </summary>
		public static Expression UserDefinedConversion (ResolveContext ec, Expression source,
								Type target, Location loc,
								bool look_for_explicit, bool return_convert)
		{
			Type source_type = source.Type;
			MethodInfo method = null;
			Expression expr = null;

			object o;
			DoubleHash hash;
			if (look_for_explicit) {
				hash = explicit_conv;
			} else {
				// Implicit user operators cannot convert to interfaces
				if (target.IsInterface)
					return null;

				hash = implicit_conv;
			}			

			if (!(source is Constant) && hash.Lookup (source_type, target, out o)) {
				method = (MethodInfo) o;
			} else {
				if (TypeManager.IsDynamicType (source_type))
					return null;

				method = GetConversionOperator (RootContext.ToplevelTypes.Compiler, null, source, target, look_for_explicit);
			}

			if (method != null) {
				Type most_specific_source = TypeManager.GetParameterData (method).Types[0];

				//
				// This will do the conversion to the best match that we
				// found.  Now we need to perform an implict standard conversion
				// if the best match was not the type that we were requested
				// by target.
				//
				if (look_for_explicit) {
					ReportPrinter temp = new SessionReportPrinter ();
					ReportPrinter prev = ec.Report.SetPrinter (temp);

					expr = ExplicitConversionStandard (ec, source, most_specific_source, loc);

					ec.Report.SetPrinter (prev);
					if (temp.ErrorsCount != 0)
						expr = null;
				} else {
					if (ImplicitStandardConversionExists (source, most_specific_source))
						expr = ImplicitConversionStandard (ec, source, most_specific_source, loc);
					else
						expr = null;
				}
			}

			if (expr == null) {
				bool nullable = false;

				if (TypeManager.IsNullableType (source_type)) {
					source = Nullable.Unwrap.Create (source);
					nullable = true;
				}

				Type target_underlying;
				if (TypeManager.IsNullableType (target)) {
					target_underlying = TypeManager.GetTypeArguments (target)[0];
					nullable = true;
				} else {
					// No implicit conversion S? -> T for non-reference type T
					if (!look_for_explicit && !TypeManager.IsReferenceType (target))
						nullable = false;

					target_underlying = target;
				}

				if (nullable) {
					expr = UserDefinedConversion (ec, source, target_underlying, loc, look_for_explicit, return_convert);

					// Do result expression lifting only when it's needed
					if (expr != null && (!look_for_explicit || TypeManager.IsReferenceType (target)))
						expr = new Nullable.Lifted (expr, source, target).Resolve (ec);

					return expr;
				}
			} else {
				expr = new UserCast (method, expr, loc).DoResolve (ec);

				if (return_convert && !TypeManager.IsEqual (expr.Type, target)) {
					if (look_for_explicit) {
						expr = ExplicitConversionStandard (ec, expr, target, loc);
					} else {
						expr = ImplicitConversionStandard (ec, expr, target, loc);
					}
				}
			}

			if (!(source is Constant))
				hash.Insert (source_type, target, method);

			return expr;
		}

		/// <summary>
		///   Converts implicitly the resolved expression `expr' into the
		///   `target_type'.  It returns a new expression that can be used
		///   in a context that expects a `target_type'.
		/// </summary>
		static public Expression ImplicitConversion (ResolveContext ec, Expression expr,
							     Type target_type, Location loc)
		{
			Expression e;

			if (target_type == null)
				throw new Exception ("Target type is null");

			e = ImplicitConversionStandard (ec, expr, target_type, loc);
			if (e != null)
				return e;

			e = ImplicitUserConversion (ec, expr, target_type, loc);
			if (e != null)
				return e;

			return null;
		}


		/// <summary>
		///   Attempts to apply the `Standard Implicit
		///   Conversion' rules to the expression `expr' into
		///   the `target_type'.  It returns a new expression
		///   that can be used in a context that expects a
		///   `target_type'.
		///
		///   This is different from `ImplicitConversion' in that the
		///   user defined implicit conversions are excluded.
		/// </summary>
		static public Expression ImplicitConversionStandard (ResolveContext ec, Expression expr,
								     Type target_type, Location loc)
		{
			return ImplicitConversionStandard (ec, expr, target_type, loc, false);
		}

		static Expression ImplicitConversionStandard (ResolveContext ec, Expression expr, Type target_type, Location loc, bool explicit_cast)
		{
			if (expr.eclass == ExprClass.MethodGroup){
				if (!TypeManager.IsDelegateType (target_type)){
					return null;
				}

				//
				// Only allow anonymous method conversions on post ISO_1
				//
				if (RootContext.Version != LanguageVersion.ISO_1){
					MethodGroupExpr mg = expr as MethodGroupExpr;
					if (mg != null)
						return ImplicitDelegateCreation.Create (
							ec, mg, target_type, loc);
				}
			}

			Type expr_type = expr.Type;
			Expression e;

			if (expr_type.Equals (target_type)) {
				if (expr_type != TypeManager.null_type && expr_type != InternalType.AnonymousMethod)
					return expr;
				return null;
			}

			if (TypeManager.IsVariantOf (expr_type, target_type)) {
				return expr;
			}

			if (TypeManager.IsNullableType (target_type))
				return ImplicitNulableConversion (ec, expr, target_type);

			//
			// Attempt to do the implicit constant expression conversions
			//
			Constant c = expr as Constant;
			if (c != null) {
				try {
					c = c.ConvertImplicitly (target_type);
				} catch {
					Console.WriteLine ("Conversion error happened in line {0}", loc);
					throw;
				}
				if (c != null)
					return c;
			}

			e = ImplicitNumericConversion (expr, expr_type, target_type);
			if (e != null)
				return e;

			e = ImplicitReferenceConversion (expr, target_type, explicit_cast);
			if (e != null)
				return e;

			if (expr is IntConstant && TypeManager.IsEnumType (target_type)){
				Constant i = (Constant) expr;
				//
				// LAMESPEC: Conversion from any 0 constant is allowed
				//
				// An implicit enumeration conversion permits the decimal-integer-literal 0
				// to be converted to any enum-type and to any nullable-type whose underlying
				// type is an enum-type
				//
				if (i.IsDefaultValue)
					return new EnumConstant (i, target_type);
			}

			if (ec.IsUnsafe) {
				if (expr_type.IsPointer){
					if (target_type == TypeManager.void_ptr_type)
						return EmptyCast.Create (expr, target_type);

					//
					// yep, comparing pointer types cant be done with
					// t1 == t2, we have to compare their element types.
					//
					if (target_type.IsPointer){
						if (TypeManager.GetElementType(target_type) == TypeManager.GetElementType(expr_type))
							return expr;

						//return null;
					}
				}

				if (expr_type == TypeManager.null_type && target_type.IsPointer)
					return EmptyCast.Create (new NullPointer (loc), target_type);
			}

			if (expr_type == InternalType.AnonymousMethod){
				AnonymousMethodExpression ame = (AnonymousMethodExpression) expr;
				Expression am = ame.Compatible (ec, target_type);
				if (am != null)
					return am.DoResolve (ec);
			}

			if (expr_type == InternalType.Arglist && target_type == TypeManager.arg_iterator_type)
				return expr;

			return null;
		}

		/// <summary>
		///   Attempts to implicitly convert `source' into `target_type', using
		///   ImplicitConversion.  If there is no implicit conversion, then
		///   an error is signaled
		/// </summary>
		static public Expression ImplicitConversionRequired (ResolveContext ec, Expression source,
								     Type target_type, Location loc)
		{
			Expression e = ImplicitConversion (ec, source, target_type, loc);
			if (e != null)
				return e;

			source.Error_ValueCannotBeConverted (ec, loc, target_type, false);
			return null;
		}

		/// <summary>
		///   Performs the explicit numeric conversions
		///
		/// There are a few conversions that are not part of the C# standard,
		/// they were interim hacks in the C# compiler that were supposed to
		/// become explicit operators in the UIntPtr class and IntPtr class,
		/// but for historical reasons it did not happen, so the C# compiler
		/// ended up with these special hacks.
		///
		/// See bug 59800 for details.
		///
		/// The conversion are:
		///   UIntPtr->SByte
		///   UIntPtr->Int16
		///   UIntPtr->Int32
		///   IntPtr->UInt64
		///   UInt64->IntPtr
		///   SByte->UIntPtr
		///   Int16->UIntPtr
		///
		/// </summary>
		public static Expression ExplicitNumericConversion (Expression expr, Type target_type)
		{
			Type expr_type = expr.Type;
			Type real_target_type = target_type;

			if (expr_type == TypeManager.sbyte_type){
				//
				// From sbyte to byte, ushort, uint, ulong, char, uintptr
				//
				if (real_target_type == TypeManager.byte_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.I1_U1);
				if (real_target_type == TypeManager.ushort_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.I1_U2);
				if (real_target_type == TypeManager.uint32_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.I1_U4);
				if (real_target_type == TypeManager.uint64_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.I1_U8);
				if (real_target_type == TypeManager.char_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.I1_CH);

				// One of the built-in conversions that belonged in the class library
				if (real_target_type == TypeManager.uintptr_type){
					Expression u8e = new ConvCast (expr, TypeManager.uint64_type, ConvCast.Mode.I1_U8);

					return new OperatorCast (u8e, TypeManager.uintptr_type, true);
				}
			} else if (expr_type == TypeManager.byte_type){
				//
				// From byte to sbyte and char
				//
				if (real_target_type == TypeManager.sbyte_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.U1_I1);
				if (real_target_type == TypeManager.char_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.U1_CH);
			} else if (expr_type == TypeManager.short_type){
				//
				// From short to sbyte, byte, ushort, uint, ulong, char, uintptr
				//
				if (real_target_type == TypeManager.sbyte_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.I2_I1);
				if (real_target_type == TypeManager.byte_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.I2_U1);
				if (real_target_type == TypeManager.ushort_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.I2_U2);
				if (real_target_type == TypeManager.uint32_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.I2_U4);
				if (real_target_type == TypeManager.uint64_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.I2_U8);
				if (real_target_type == TypeManager.char_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.I2_CH);

				// One of the built-in conversions that belonged in the class library
				if (real_target_type == TypeManager.uintptr_type){
					Expression u8e = new ConvCast (expr, TypeManager.uint64_type, ConvCast.Mode.I2_U8);

					return new OperatorCast (u8e, TypeManager.uintptr_type, true);
				}
			} else if (expr_type == TypeManager.ushort_type){
				//
				// From ushort to sbyte, byte, short, char
				//
				if (real_target_type == TypeManager.sbyte_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.U2_I1);
				if (real_target_type == TypeManager.byte_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.U2_U1);
				if (real_target_type == TypeManager.short_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.U2_I2);
				if (real_target_type == TypeManager.char_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.U2_CH);
			} else if (expr_type == TypeManager.int32_type){
				//
				// From int to sbyte, byte, short, ushort, uint, ulong, char, uintptr
				//
				if (real_target_type == TypeManager.sbyte_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.I4_I1);
				if (real_target_type == TypeManager.byte_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.I4_U1);
				if (real_target_type == TypeManager.short_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.I4_I2);
				if (real_target_type == TypeManager.ushort_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.I4_U2);
				if (real_target_type == TypeManager.uint32_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.I4_U4);
				if (real_target_type == TypeManager.uint64_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.I4_U8);
				if (real_target_type == TypeManager.char_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.I4_CH);

				// One of the built-in conversions that belonged in the class library
				if (real_target_type == TypeManager.uintptr_type){
					Expression u8e = new ConvCast (expr, TypeManager.uint64_type, ConvCast.Mode.I2_U8);

					return new OperatorCast (u8e, TypeManager.uintptr_type, true);
				}
			} else if (expr_type == TypeManager.uint32_type){
				//
				// From uint to sbyte, byte, short, ushort, int, char
				//
				if (real_target_type == TypeManager.sbyte_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.U4_I1);
				if (real_target_type == TypeManager.byte_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.U4_U1);
				if (real_target_type == TypeManager.short_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.U4_I2);
				if (real_target_type == TypeManager.ushort_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.U4_U2);
				if (real_target_type == TypeManager.int32_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.U4_I4);
				if (real_target_type == TypeManager.char_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.U4_CH);
			} else if (expr_type == TypeManager.int64_type){
				//
				// From long to sbyte, byte, short, ushort, int, uint, ulong, char
				//
				if (real_target_type == TypeManager.sbyte_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.I8_I1);
				if (real_target_type == TypeManager.byte_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.I8_U1);
				if (real_target_type == TypeManager.short_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.I8_I2);
				if (real_target_type == TypeManager.ushort_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.I8_U2);
				if (real_target_type == TypeManager.int32_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.I8_I4);
				if (real_target_type == TypeManager.uint32_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.I8_U4);
				if (real_target_type == TypeManager.uint64_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.I8_U8);
				if (real_target_type == TypeManager.char_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.I8_CH);
			} else if (expr_type == TypeManager.uint64_type){
				//
				// From ulong to sbyte, byte, short, ushort, int, uint, long, char
				//
				if (real_target_type == TypeManager.sbyte_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.U8_I1);
				if (real_target_type == TypeManager.byte_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.U8_U1);
				if (real_target_type == TypeManager.short_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.U8_I2);
				if (real_target_type == TypeManager.ushort_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.U8_U2);
				if (real_target_type == TypeManager.int32_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.U8_I4);
				if (real_target_type == TypeManager.uint32_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.U8_U4);
				if (real_target_type == TypeManager.int64_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.U8_I8);
				if (real_target_type == TypeManager.char_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.U8_CH);

				// One of the built-in conversions that belonged in the class library
				if (real_target_type == TypeManager.intptr_type){
					return new OperatorCast (EmptyCast.Create (expr, TypeManager.int64_type),
								 TypeManager.intptr_type, true);
				}
			} else if (expr_type == TypeManager.char_type){
				//
				// From char to sbyte, byte, short
				//
				if (real_target_type == TypeManager.sbyte_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.CH_I1);
				if (real_target_type == TypeManager.byte_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.CH_U1);
				if (real_target_type == TypeManager.short_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.CH_I2);
			} else if (expr_type == TypeManager.float_type){
				//
				// From float to sbyte, byte, short,
				// ushort, int, uint, long, ulong, char
				// or decimal
				//
				if (real_target_type == TypeManager.sbyte_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.R4_I1);
				if (real_target_type == TypeManager.byte_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.R4_U1);
				if (real_target_type == TypeManager.short_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.R4_I2);
				if (real_target_type == TypeManager.ushort_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.R4_U2);
				if (real_target_type == TypeManager.int32_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.R4_I4);
				if (real_target_type == TypeManager.uint32_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.R4_U4);
				if (real_target_type == TypeManager.int64_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.R4_I8);
				if (real_target_type == TypeManager.uint64_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.R4_U8);
				if (real_target_type == TypeManager.char_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.R4_CH);
				if (real_target_type == TypeManager.decimal_type)
					return new CastToDecimal (expr, true);
			} else if (expr_type == TypeManager.double_type){
				//
				// From double to sbyte, byte, short,
				// ushort, int, uint, long, ulong,
				// char, float or decimal
				//
				if (real_target_type == TypeManager.sbyte_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.R8_I1);
				if (real_target_type == TypeManager.byte_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.R8_U1);
				if (real_target_type == TypeManager.short_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.R8_I2);
				if (real_target_type == TypeManager.ushort_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.R8_U2);
				if (real_target_type == TypeManager.int32_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.R8_I4);
				if (real_target_type == TypeManager.uint32_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.R8_U4);
				if (real_target_type == TypeManager.int64_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.R8_I8);
				if (real_target_type == TypeManager.uint64_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.R8_U8);
				if (real_target_type == TypeManager.char_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.R8_CH);
				if (real_target_type == TypeManager.float_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.R8_R4);
				if (real_target_type == TypeManager.decimal_type)
					return new CastToDecimal (expr, true);
			} else if (expr_type == TypeManager.uintptr_type){
				//
				// Various built-in conversions that belonged in the class library
				//
				// from uintptr to sbyte, short, int32
				//
				if (real_target_type == TypeManager.sbyte_type){
					Expression uint32e = new OperatorCast (expr, TypeManager.uint32_type, true);
					return new ConvCast (uint32e, TypeManager.sbyte_type, ConvCast.Mode.U4_I1);
				}
				if (real_target_type == TypeManager.short_type){
					Expression uint32e = new OperatorCast (expr, TypeManager.uint32_type, true);
					return new ConvCast (uint32e, TypeManager.sbyte_type, ConvCast.Mode.U4_I2);
				}
				if (real_target_type == TypeManager.int32_type){
					return EmptyCast.Create (new OperatorCast (expr, TypeManager.uint32_type, true),
							      TypeManager.int32_type);
				}
			} else if (expr_type == TypeManager.intptr_type){
				if (real_target_type == TypeManager.uint64_type){
					return EmptyCast.Create (new OperatorCast (expr, TypeManager.int64_type, true),
							      TypeManager.uint64_type);
				}
			} else if (expr_type == TypeManager.decimal_type) {
				return new CastFromDecimal (expr, target_type).Resolve ();
			}
			return null;
		}

		/// <summary>
		///  Returns whether an explicit reference conversion can be performed
		///  from source_type to target_type
		/// </summary>
		public static bool ExplicitReferenceConversionExists (Type source_type, Type target_type)
		{
			Expression e = ExplicitReferenceConversion (null, source_type, target_type);
			if (e == null)
				return false;

			if (e == EmptyExpression.Null)
				return true;

			throw new InternalErrorException ("Invalid probing conversion result");
		}

		/// <summary>
		///   Implements Explicit Reference conversions
		/// </summary>
		static Expression ExplicitReferenceConversion (Expression source, Type source_type, Type target_type)
		{
			bool target_is_value_type = TypeManager.IsStruct (target_type);

			//
			// From object to a generic parameter
			//
			if (source_type == TypeManager.object_type && TypeManager.IsGenericParameter (target_type))
				return source == null ? EmptyExpression.Null : new UnboxCast (source, target_type);

			//
			// Explicit type parameter conversion.
			//
			if (TypeManager.IsGenericParameter (source_type))
				return ExplicitTypeParameterConversion (source, source_type, target_type);

			//
			// From object to any reference type or value type (unboxing)
			//
			if (source_type == TypeManager.object_type)
				return source == null ? EmptyExpression.Null :
					target_is_value_type ? (Expression) new UnboxCast (source, target_type) : new ClassCast (source, target_type);

			//
			// Unboxing conversion from the types object and System.ValueType to any non-nullable-value-type
			//
			if (source_type == TypeManager.value_type && target_is_value_type)
				return source == null ? EmptyExpression.Null : new UnboxCast (source, target_type);

			//
			// From any class S to any class-type T, provided S is a base class of T
			//
			if (TypeManager.IsSubclassOf (target_type, source_type))
				return source == null ? EmptyExpression.Null : new ClassCast (source, target_type);

			//
			// From any class type S to any interface T, provides S is not sealed
			// and provided S does not implement T.
			//
			if (target_type.IsInterface && !source_type.IsSealed &&
				!TypeManager.ImplementsInterface (source_type, target_type)) {
				return source == null ? EmptyExpression.Null : new ClassCast (source, target_type);
			}

			//
			// From any interface-type S to to any class type T, provided T is not
			// sealed, or provided T implements S.
			//
			if (source_type.IsInterface) {
				if (!target_type.IsSealed || TypeManager.ImplementsInterface (target_type, source_type)) {
					if (target_type.IsClass)
						return source == null ? EmptyExpression.Null : new ClassCast (source, target_type);

					//
					// Unboxing conversion from any interface-type to any non-nullable-value-type that
					// implements the interface-type
					//
					return source == null ? EmptyExpression.Null : new UnboxCast (source, target_type);
				}

				//
				// From System.Collecitons.Generic.IList<T> and its base interfaces to a one-dimensional
				// array type S[], provided there is an implicit or explicit reference conversion from S to T.
				//
				if (IList_To_Array (source_type, target_type))
					return source == null ? EmptyExpression.Null : new ClassCast (source, target_type);

				return null;
			}

			if (source_type.IsArray) {
				if (target_type.IsArray) {
					//
					// From System.Array to any array-type
					//
					if (source_type == TypeManager.array_type)
						return source == null ? EmptyExpression.Null : new ClassCast (source, target_type);

					//
					// From an array type S with an element type Se to an array type T with an
					// element type Te provided all the following are true:
					//     * S and T differe only in element type, in other words, S and T
					//       have the same number of dimensions.
					//     * Both Se and Te are reference types
					//     * An explicit reference conversions exist from Se to Te
					//
					if (source_type.GetArrayRank () == target_type.GetArrayRank ()) {

						source_type = TypeManager.GetElementType (source_type);
						if (!TypeManager.IsReferenceType (source_type))
							return null;

						Type target_type_element = TypeManager.GetElementType (target_type);
						if (!TypeManager.IsReferenceType (target_type_element))
							return null;

						if (ExplicitReferenceConversionExists (source_type, target_type_element))
							return source == null ? EmptyExpression.Null : new ClassCast (source, target_type);
							
						return null;
					}
				}

				//
				// From a single-dimensional array type S[] to System.Collections.Generic.IList<T> and its base interfaces, 
				// provided that there is an explicit reference conversion from S to T
				//
				if (Array_To_IList (source_type, target_type, true))
					return source == null ? EmptyExpression.Null : new ClassCast (source, target_type);

				return null;
			}

			//
			// From System delegate to any delegate-type
			//
			if (source_type == TypeManager.delegate_type && TypeManager.IsDelegateType (target_type))
				return source == null ? EmptyExpression.Null : new ClassCast (source, target_type);

			return null;
		}

		/// <summary>
		///   Performs an explicit conversion of the expression `expr' whose
		///   type is expr.Type to `target_type'.
		/// </summary>
		static public Expression ExplicitConversionCore (ResolveContext ec, Expression expr,
								 Type target_type, Location loc)
		{
			Type expr_type = expr.Type;

			// Explicit conversion includes implicit conversion and it used for enum underlying types too
			Expression ne = ImplicitConversionStandard (ec, expr, target_type, loc, true);
			if (ne != null)
				return ne;

			if (TypeManager.IsEnumType (expr_type)) {
				Expression underlying = EmptyCast.Create (expr, TypeManager.GetEnumUnderlyingType (expr_type));
				expr = ExplicitConversionCore (ec, underlying, target_type, loc);
				if (expr != null)
					return expr;

				return ExplicitUserConversion (ec, underlying, target_type, loc);				
			}

			if (TypeManager.IsEnumType (target_type)){
				//
				// Type System.Enum can be unboxed to any enum-type
				//
				if (expr_type == TypeManager.enum_type)
					return new UnboxCast (expr, target_type);

				Expression ce = ExplicitConversionCore (ec, expr, TypeManager.GetEnumUnderlyingType (target_type), loc);
				if (ce != null)
					return EmptyCast.Create (ce, target_type);
				
				//
				// LAMESPEC: IntPtr and UIntPtr conversion to any Enum is allowed
				//
				if (expr_type == TypeManager.intptr_type || expr_type == TypeManager.uintptr_type) {
					ne = ExplicitUserConversion (ec, expr, TypeManager.GetEnumUnderlyingType (target_type), loc);
					if (ne != null)
						return ExplicitConversionCore (ec, ne, target_type, loc);
				}
				
				return null;
			}

			ne = ExplicitNumericConversion (expr, target_type);
			if (ne != null)
				return ne;

			//
			// Skip the ExplicitReferenceConversion because we can not convert
			// from Null to a ValueType, and ExplicitReference wont check against
			// null literal explicitly
			//
			if (expr_type != TypeManager.null_type){
				ne = ExplicitReferenceConversion (expr, expr_type, target_type);
				if (ne != null)
					return ne;
			}

			if (ec.IsUnsafe){
				ne = ExplicitUnsafe (expr, target_type);
				if (ne != null)
					return ne;
			}
			
			return null;
		}

		public static Expression ExplicitUnsafe (Expression expr, Type target_type)
		{
			Type expr_type = expr.Type;

			if (target_type.IsPointer){
				if (expr_type.IsPointer)
					return EmptyCast.Create (expr, target_type);

				if (expr_type == TypeManager.sbyte_type ||
					expr_type == TypeManager.short_type ||
					expr_type == TypeManager.int32_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I);

				if (expr_type == TypeManager.ushort_type ||
					expr_type == TypeManager.uint32_type ||
					expr_type == TypeManager.byte_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U);

				if (expr_type == TypeManager.int64_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.I8_I);

				if (expr_type == TypeManager.uint64_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.U8_I);
			}

			if (expr_type.IsPointer){
				if (target_type == TypeManager.sbyte_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I1);
				if (target_type == TypeManager.byte_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U1);
				if (target_type == TypeManager.short_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I2);
				if (target_type == TypeManager.ushort_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U2);
				if (target_type == TypeManager.int32_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I4);
				if (target_type == TypeManager.uint32_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U4);
				if (target_type == TypeManager.int64_type)
					return new ConvCast (expr, target_type, ConvCast.Mode.I_I8);
				if (target_type == TypeManager.uint64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U8);
			}
			return null;
		}

		/// <summary>
		///   Same as ExplicitConversion, only it doesn't include user defined conversions
		/// </summary>
		static public Expression ExplicitConversionStandard (ResolveContext ec, Expression expr,
								     Type target_type, Location l)
		{
			int errors = ec.Report.Errors;
			Expression ne = ImplicitConversionStandard (ec, expr, target_type, l);
			if (ec.Report.Errors > errors)
				return null;

			if (ne != null)
				return ne;

			ne = ExplicitNumericConversion (expr, target_type);
			if (ne != null)
				return ne;

			ne = ExplicitReferenceConversion (expr, expr.Type, target_type);
			if (ne != null)
				return ne;

			if (ec.IsUnsafe && expr.Type == TypeManager.void_ptr_type && target_type.IsPointer)
				return EmptyCast.Create (expr, target_type);

			expr.Error_ValueCannotBeConverted (ec, l, target_type, true);
			return null;
		}

		/// <summary>
		///   Performs an explicit conversion of the expression `expr' whose
		///   type is expr.Type to `target_type'.
		/// </summary>
		static public Expression ExplicitConversion (ResolveContext ec, Expression expr,
			Type target_type, Location loc)
		{
			Expression e = ExplicitConversionCore (ec, expr, target_type, loc);
			if (e != null) {
				//
				// Don't eliminate explicit precission casts
				//
				if (e == expr) {
					if (target_type == TypeManager.float_type)
						return new OpcodeCast (expr, target_type, OpCodes.Conv_R4);
					
					if (target_type == TypeManager.double_type)
						return new OpcodeCast (expr, target_type, OpCodes.Conv_R8);
				}
					
				return e;
			}

			Type expr_type = expr.Type;
			if (TypeManager.IsNullableType (target_type)) {
				if (TypeManager.IsNullableType (expr_type)) {
					Type target = TypeManager.TypeToCoreType (TypeManager.GetTypeArguments (target_type)[0]);
					Expression unwrap = Nullable.Unwrap.Create (expr);
					e = ExplicitConversion (ec, unwrap, target, expr.Location);
					if (e == null)
						return null;

					return new Nullable.Lifted (e, unwrap, target_type).Resolve (ec);
				} else if (expr_type == TypeManager.object_type) {
					return new UnboxCast (expr, target_type);
				} else {
					Type target = TypeManager.TypeToCoreType (TypeManager.GetTypeArguments (target_type) [0]);

					e = ExplicitConversionCore (ec, expr, target, loc);
					if (e != null)
						return Nullable.Wrap.Create (e, target_type);
				}
			} else if (TypeManager.IsNullableType (expr_type)) {
				e = Nullable.Unwrap.Create (expr, false);

				bool use_class_cast;
				if (ImplicitBoxingConversionExists (e, target_type, out use_class_cast))
					return new BoxedCast (expr, target_type);
				
				e = ExplicitConversionCore (ec, e, target_type, loc);
				if (e != null)
					return EmptyCast.Create (e, target_type);
			}
			
			e = ExplicitUserConversion (ec, expr, target_type, loc);
			if (e != null)
				return e;			

			expr.Error_ValueCannotBeConverted (ec, loc, target_type, true);
			return null;
		}
	}
}
