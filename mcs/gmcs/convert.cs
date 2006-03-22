//
// conversion.cs: various routines for implementing conversions.
//
// Authors:
//   Miguel de Icaza (miguel@ximian.com)
//   Ravi Pratap (ravi@ximian.com)
//
// (C) 2001, 2002, 2003 Ximian, Inc.
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
	public class Convert {
		//
		// This is used to prettify the code: a null argument is allowed
		// for ImplicitStandardConversion as long as it is known that
		// no anonymous method will play a role.
		//
		// FIXME: renamed from `const' to `static' to allow bootstraping from older
		// versions of the compiler that could not cope with this construct.
		//
		public static EmitContext ConstantEC = null;
		
		static Expression TypeParameter_to_Null (Type target_type, Location loc)
		{
			if (!TypeParameter_to_Null (target_type)) {
				Report.Error (403, loc, "Cannot convert null to the type " +
					      "parameter `{0}' becaues it could be a value " +
					      "type. Consider using `default ({0})' instead.",
					      target_type.ToString ());
				return null;
			}

			Constant expr = new Nullable.NullableLiteral (target_type, loc);
			return new NullCast (expr, target_type);
		}

		static bool TypeParameter_to_Null (Type target_type)
		{
			GenericConstraints gc = TypeManager.GetTypeParameterConstraints (target_type);
			if (gc == null)
				return false;

			if (gc.HasReferenceTypeConstraint)
				return true;
			if (gc.HasClassConstraint && !TypeManager.IsValueType (gc.ClassConstraint))
				return true;

			return false;
		}

		static Type TypeParam_EffectiveBaseType (GenericConstraints gc)
		{
			ArrayList list = new ArrayList ();
			list.Add (gc.EffectiveBaseClass);
			foreach (Type t in gc.InterfaceConstraints) {
				if (!t.IsGenericParameter)
					continue;

				GenericConstraints new_gc = TypeManager.GetTypeParameterConstraints (t);
				if (new_gc != null)
					list.Add (TypeParam_EffectiveBaseType (new_gc));
			}
			return FindMostEncompassedType (list);
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

			if (!gc.HasClassConstraint && !gc.HasConstructorConstraint && !gc.HasReferenceTypeConstraint &&
				!gc.HasValueTypeConstraint)
				return new ClassCast (expr, target_type);

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
				if (!t.IsGenericParameter)
					continue;
				if (TypeManager.IsSubclassOf (t, target_type))
					return new ClassCast (expr, target_type);
				if (TypeManager.ImplementsInterface (t, target_type))
					return new ClassCast (expr, target_type);
			}

			return null;
		}

		static EmptyExpression MyEmptyExpr;
		static public Expression ImplicitReferenceConversion (Expression expr, Type target_type)
		{
			Type expr_type = expr.Type;

			if (expr_type == null && expr.eclass == ExprClass.MethodGroup){
				// if we are a method group, emit a warning

				expr.Emit (null);
			}

			if (expr_type == TypeManager.void_type)
				return null;

			if (expr_type.IsGenericParameter)
				return ImplicitTypeParameterConversion (expr, target_type);
				
			//
			// notice that it is possible to write "ValueType v = 1", the ValueType here
			// is an abstract class, and not really a value type, so we apply the same rules.
			//
			if (target_type == TypeManager.object_type) {
				//
				// A pointer type cannot be converted to object
				// 
				if (expr_type.IsPointer)
					return null;

				if (TypeManager.IsValueType (expr_type))
					return new BoxedCast (expr, target_type);
				if (expr_type.IsClass || expr_type.IsInterface || expr_type == TypeManager.enum_type){
					if (expr_type == TypeManager.anonymous_method_type)
						return null;
					return new EmptyCast (expr, target_type);
				}

				return null;
			} else if (target_type == TypeManager.value_type) {
				if (TypeManager.IsValueType (expr_type))
					return new BoxedCast (expr, target_type);
				if (expr_type == TypeManager.null_type)
					return new NullCast ((Constant)expr, target_type);

				return null;
			} else if (TypeManager.IsSubclassOf (expr_type, target_type)) {
				//
				// Special case: enumeration to System.Enum.
				// System.Enum is not a value type, it is a class, so we need
				// a boxing conversion
				//
				if (expr_type.IsEnum || expr_type.IsGenericParameter)
					return new BoxedCast (expr, target_type);

				return new EmptyCast (expr, target_type);
			}

			// This code is kind of mirrored inside ImplicitStandardConversionExists
			// with the small distinction that we only probe there
			//
			// Always ensure that the code here and there is in sync

			// from the null type to any reference-type.
			if (expr_type == TypeManager.null_type){
				if (target_type.IsPointer)
					return new EmptyCast (NullPointer.Null, target_type);
					
				if (!target_type.IsValueType) {
					if (expr is Constant)
						return new NullCast ((Constant)expr, target_type);

					// I found only one case when it happens -- Foo () ? null : null;
					Report.Warning (-100, 1, expr.Location, "The result of the expression is always `null'");
					return new NullCast (new NullLiteral (expr.Location), target_type);
				}
			}

			// from any class-type S to any interface-type T.
			if (target_type.IsInterface) {
				if (target_type != TypeManager.iconvertible_type &&
				    expr_type.IsValueType && (expr is Constant) &&
				    !(expr is IntLiteral || expr is BoolLiteral ||
				      expr is FloatLiteral || expr is DoubleLiteral ||
				      expr is LongLiteral || expr is CharLiteral ||
				      expr is StringLiteral || expr is DecimalLiteral ||
				      expr is UIntLiteral || expr is ULongLiteral)) {
					return null;
				}

				if (TypeManager.ImplementsInterface (expr_type, target_type)){
					if (expr_type.IsGenericParameter || TypeManager.IsValueType (expr_type))
						return new BoxedCast (expr, target_type);
					else
						return new EmptyCast (expr, target_type);
				}
			}

			// from any interface type S to interface-type T.
			if (expr_type.IsInterface && target_type.IsInterface) {
				if (TypeManager.ImplementsInterface (expr_type, target_type))
					return new EmptyCast (expr, target_type);
				else
					return null;
			}

			// from an array-type S to an array-type of type T
			if (expr_type.IsArray && target_type.IsArray) {
				if (expr_type.GetArrayRank () == target_type.GetArrayRank ()) {

					Type expr_element_type = TypeManager.GetElementType (expr_type);

					if (MyEmptyExpr == null)
						MyEmptyExpr = new EmptyExpression ();
						
					MyEmptyExpr.SetType (expr_element_type);
					Type target_element_type = TypeManager.GetElementType (target_type);

					if (!expr_element_type.IsValueType && !target_element_type.IsValueType)
						if (ImplicitStandardConversionExists (MyEmptyExpr,
										      target_element_type))
							return new EmptyCast (expr, target_type);
				}
			}
				
			// from an array-type to System.Array
			if (expr_type.IsArray && target_type == TypeManager.array_type)
				return new EmptyCast (expr, target_type);

			// from an array-type of type T to IEnumerable<T>
			if (expr_type.IsArray && TypeManager.IsIEnumerable (expr_type, target_type))
				return new EmptyCast (expr, target_type);

			// from any delegate type to System.Delegate
			if ((expr_type == TypeManager.delegate_type || TypeManager.IsDelegateType (expr_type)) &&
			    target_type == TypeManager.delegate_type)
				return new EmptyCast (expr, target_type);
					
			// from any array-type or delegate type into System.ICloneable.
			if (expr_type.IsArray ||
			    expr_type == TypeManager.delegate_type || TypeManager.IsDelegateType (expr_type))
				if (target_type == TypeManager.icloneable_type)
					return new EmptyCast (expr, target_type);

			// from a generic type definition to a generic instance.
			if (TypeManager.IsEqual (expr_type, target_type))
				return new EmptyCast (expr, target_type);

			return null;
		}

		//
		// Tests whether an implicit reference conversion exists between expr_type
		// and target_type
		//
		public static bool ImplicitReferenceConversionExists (Expression expr, Type target_type)
		{
			if (target_type.IsValueType)
				return false;

			Type expr_type = expr.Type;

			if (expr_type.IsGenericParameter)
				return ImplicitTypeParameterConversion (expr, target_type) != null;

			//
			// This is the boxed case.
			//
			if (target_type == TypeManager.object_type) {
				if (expr_type.IsClass || TypeManager.IsValueType (expr_type) ||
				    expr_type.IsInterface || expr_type == TypeManager.enum_type)
					if (target_type != TypeManager.anonymous_method_type)
					return true;

				return false;
			} else if (TypeManager.IsSubclassOf (expr_type, target_type))
				return true;

			// Please remember that all code below actually comes
			// from ImplicitReferenceConversion so make sure code remains in sync
				
			// from any class-type S to any interface-type T.
			if (target_type.IsInterface) {
				if (target_type != TypeManager.iconvertible_type &&
				    expr_type.IsValueType && (expr is Constant) &&
				    !(expr is IntLiteral || expr is BoolLiteral ||
				      expr is FloatLiteral || expr is DoubleLiteral ||
				      expr is LongLiteral || expr is CharLiteral ||
				      expr is StringLiteral || expr is DecimalLiteral ||
				      expr is UIntLiteral || expr is ULongLiteral)) {
					return false;
				}
				
				if (TypeManager.ImplementsInterface (expr_type, target_type))
					return true;
			}
				
			// from any interface type S to interface-type T.
			if (expr_type.IsInterface && target_type.IsInterface)
				if (TypeManager.ImplementsInterface (expr_type, target_type))
					return true;
				
			// from an array-type S to an array-type of type T
			if (expr_type.IsArray && target_type.IsArray) {
				if (expr_type.GetArrayRank () == target_type.GetArrayRank ()) {
						
					Type expr_element_type = expr_type.GetElementType ();

					if (MyEmptyExpr == null)
						MyEmptyExpr = new EmptyExpression ();
						
					MyEmptyExpr.SetType (expr_element_type);
					Type target_element_type = TypeManager.GetElementType (target_type);
						
					if (!expr_element_type.IsValueType && !target_element_type.IsValueType)
						if (ImplicitStandardConversionExists (MyEmptyExpr,
										      target_element_type))
							return true;
				}
			}
				
			// from an array-type to System.Array
			if (expr_type.IsArray && (target_type == TypeManager.array_type))
				return true;

			// from an array-type of type T to IEnumerable<T>
			if (expr_type.IsArray && TypeManager.IsIEnumerable (expr_type, target_type))
				return true;

			// from any delegate type to System.Delegate
			if ((expr_type == TypeManager.delegate_type || TypeManager.IsDelegateType (expr_type)) &&
			    target_type == TypeManager.delegate_type)
				if (target_type.IsAssignableFrom (expr_type))
					return true;
					
			// from any array-type or delegate type into System.ICloneable.
			if (expr_type.IsArray ||
			    expr_type == TypeManager.delegate_type || TypeManager.IsDelegateType (expr_type))
				if (target_type == TypeManager.icloneable_type)
					return true;
				
			// from the null type to any reference-type.
			if (expr_type == TypeManager.null_type){
				if (target_type.IsPointer)
					return true;
			
				if (!target_type.IsValueType)
					return true;
			}

			// from a generic type definition to a generic instance.
			if (TypeManager.IsEqual (expr_type, target_type))
				return true;

			return false;
		}

		/// <summary>
		///   Implicit Numeric Conversions.
		///
		///   expr is the expression to convert, returns a new expression of type
		///   target_type or null if an implicit conversion is not possible.
		/// </summary>
		static public Expression ImplicitNumericConversion (Expression expr,
								    Type target_type)
		{
			Type expr_type = expr.Type;

			//
			// Attempt to do the implicit constant expression conversions

			if (expr is Constant){
				if (expr is IntConstant){
					Expression e;
					
					e = TryImplicitIntConversion (target_type, (IntConstant) expr);
					
					if (e != null)
						return e;
				} else if (expr is LongConstant && target_type == TypeManager.uint64_type){
					//
					// Try the implicit constant expression conversion
					// from long to ulong, instead of a nice routine,
					// we just inline it
					//
					long v = ((LongConstant) expr).Value;
					if (v >= 0)
						return new ULongConstant ((ulong) v, expr.Location);
				} 
			}
			
 			Type real_target_type = target_type;

			if (expr_type == TypeManager.sbyte_type){
				//
				// From sbyte to short, int, long, float, double, decimal
				//
				if (real_target_type == TypeManager.int32_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I4);
				if (real_target_type == TypeManager.int64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I8);
				if (real_target_type == TypeManager.double_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R8);
				if (real_target_type == TypeManager.float_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R4);
				if (real_target_type == TypeManager.short_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I2);
				if (real_target_type == TypeManager.decimal_type)
					return new CastToDecimal (expr);
			} else if (expr_type == TypeManager.byte_type){
				//
				// From byte to short, ushort, int, uint, long, ulong, float, double, decimal
				// 
				if ((real_target_type == TypeManager.short_type) ||
				    (real_target_type == TypeManager.ushort_type) ||
				    (real_target_type == TypeManager.int32_type) ||
				    (real_target_type == TypeManager.uint32_type))
					return new EmptyCast (expr, target_type);

				if (real_target_type == TypeManager.uint64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U8);
				if (real_target_type == TypeManager.int64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I8);
				if (real_target_type == TypeManager.float_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R4);
				if (real_target_type == TypeManager.double_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R8);
				if (real_target_type == TypeManager.decimal_type)
					return new CastToDecimal (expr);
				
			} else if (expr_type == TypeManager.short_type){
				//
				// From short to int, long, float, double, decimal
				// 
				if (real_target_type == TypeManager.int32_type)
					return new EmptyCast (expr, target_type);
				if (real_target_type == TypeManager.int64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I8);
				if (real_target_type == TypeManager.double_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R8);
				if (real_target_type == TypeManager.float_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R4);
				if (real_target_type == TypeManager.decimal_type)
					return new CastToDecimal (expr);
				
			} else if (expr_type == TypeManager.ushort_type){
				//
				// From ushort to int, uint, long, ulong, float, double, decimal
				//
				if (real_target_type == TypeManager.uint32_type)
					return new EmptyCast (expr, target_type);

				if (real_target_type == TypeManager.uint64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U8);
				if (real_target_type == TypeManager.int32_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I4);
				if (real_target_type == TypeManager.int64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I8);
				if (real_target_type == TypeManager.double_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R8);
				if (real_target_type == TypeManager.float_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R4);
				if (real_target_type == TypeManager.decimal_type)
					return new CastToDecimal (expr);
			} else if (expr_type == TypeManager.int32_type){
				//
				// From int to long, float, double, decimal
				//
				if (real_target_type == TypeManager.int64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I8);
				if (real_target_type == TypeManager.double_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R8);
				if (real_target_type == TypeManager.float_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R4);
				if (real_target_type == TypeManager.decimal_type)
					return new CastToDecimal (expr);
			} else if (expr_type == TypeManager.uint32_type){
				//
				// From uint to long, ulong, float, double, decimal
				//
				if (real_target_type == TypeManager.int64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U8);
				if (real_target_type == TypeManager.uint64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U8);
				if (real_target_type == TypeManager.double_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R_Un,
							       OpCodes.Conv_R8);
				if (real_target_type == TypeManager.float_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R_Un,
							       OpCodes.Conv_R4);
				if (real_target_type == TypeManager.decimal_type)
					return new CastToDecimal (expr);
			} else if (expr_type == TypeManager.int64_type){
				//
				// From long/ulong to float, double
				//
				if (real_target_type == TypeManager.double_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R8);
				if (real_target_type == TypeManager.float_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R4);
				if (real_target_type == TypeManager.decimal_type)
					return new CastToDecimal (expr);
			} else if (expr_type == TypeManager.uint64_type){
				//
				// From ulong to float, double
				//
				if (real_target_type == TypeManager.double_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R_Un,
							       OpCodes.Conv_R8);
				if (real_target_type == TypeManager.float_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R_Un,
							       OpCodes.Conv_R4);
				if (real_target_type == TypeManager.decimal_type)
					return new CastToDecimal (expr);
			} else if (expr_type == TypeManager.char_type){
				//
				// From char to ushort, int, uint, long, ulong, float, double, decimal
				// 
				if ((real_target_type == TypeManager.ushort_type) ||
				    (real_target_type == TypeManager.int32_type) ||
				    (real_target_type == TypeManager.uint32_type))
					return new EmptyCast (expr, target_type);
				if (real_target_type == TypeManager.uint64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U8);
				if (real_target_type == TypeManager.int64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I8);
				if (real_target_type == TypeManager.float_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R4);
				if (real_target_type == TypeManager.double_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R8);
				if (real_target_type == TypeManager.decimal_type)
					return new CastToDecimal (expr);
			} else if (expr_type == TypeManager.float_type){
				//
				// float to double
				//
				if (real_target_type == TypeManager.double_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R8);
			}

			return null;
		}

		/// <summary>
		///  Same as ImplicitStandardConversionExists except that it also looks at
		///  implicit user defined conversions - needed for overload resolution
		/// </summary>
		public static bool ImplicitConversionExists (EmitContext ec, Expression expr, Type target_type)
		{
			if (expr is NullLiteral) {
				if (target_type.IsGenericParameter)
					return TypeParameter_to_Null (target_type);

				if (TypeManager.IsNullableType (target_type))
					return true;
			}

			if (ImplicitStandardConversionExists (expr, target_type))
				return true;

			Expression dummy = ImplicitUserConversion (ec, expr, target_type, Location.Null);

			if (dummy != null)
				return true;

			return false;
		}

		public static bool ImplicitUserConversionExists (EmitContext ec, Type source, Type target)
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

			if (expr_type == TypeManager.void_type)
				return false;

                        //Console.WriteLine ("Expr is {0}", expr);
                        //Console.WriteLine ("{0} -> {1} ?", expr_type, target_type);
			if (expr_type.Equals (target_type))
				return true;


			// First numeric conversions 

			if (expr_type == TypeManager.sbyte_type){
				//
				// From sbyte to short, int, long, float, double, decimal
				//
				if ((target_type == TypeManager.int32_type) || 
				    (target_type == TypeManager.int64_type) ||
				    (target_type == TypeManager.double_type) ||
				    (target_type == TypeManager.float_type)  ||
				    (target_type == TypeManager.short_type) ||
				    (target_type == TypeManager.decimal_type))
					return true;
				
			} else if (expr_type == TypeManager.byte_type){
				//
				// From byte to short, ushort, int, uint, long, ulong, float, double, decimal
				// 
				if ((target_type == TypeManager.short_type) ||
				    (target_type == TypeManager.ushort_type) ||
				    (target_type == TypeManager.int32_type) ||
				    (target_type == TypeManager.uint32_type) ||
				    (target_type == TypeManager.uint64_type) ||
				    (target_type == TypeManager.int64_type) ||
				    (target_type == TypeManager.float_type) ||
				    (target_type == TypeManager.double_type) ||
				    (target_type == TypeManager.decimal_type))
					return true;
	
			} else if (expr_type == TypeManager.short_type){
				//
				// From short to int, long, double, float, decimal
				// 
				if ((target_type == TypeManager.int32_type) ||
				    (target_type == TypeManager.int64_type) ||
				    (target_type == TypeManager.double_type) ||
				    (target_type == TypeManager.float_type) ||
				    (target_type == TypeManager.decimal_type))
					return true;
					
			} else if (expr_type == TypeManager.ushort_type){
				//
				// From ushort to int, uint, long, ulong, double, float, decimal
				//
				if ((target_type == TypeManager.uint32_type) ||
				    (target_type == TypeManager.uint64_type) ||
				    (target_type == TypeManager.int32_type) ||
				    (target_type == TypeManager.int64_type) ||
				    (target_type == TypeManager.double_type) ||
				    (target_type == TypeManager.float_type) ||
				    (target_type == TypeManager.decimal_type))
					return true;
				    
			} else if (expr_type == TypeManager.int32_type){
				//
				// From int to long, double, float, decimal
				//
				if ((target_type == TypeManager.int64_type) ||
				    (target_type == TypeManager.double_type) ||
				    (target_type == TypeManager.float_type) ||
				    (target_type == TypeManager.decimal_type))
					return true;
					
			} else if (expr_type == TypeManager.uint32_type){
				//
				// From uint to long, ulong, double, float, decimal
				//
				if ((target_type == TypeManager.int64_type) ||
				    (target_type == TypeManager.uint64_type) ||
				    (target_type == TypeManager.double_type) ||
				    (target_type == TypeManager.float_type) ||
				    (target_type == TypeManager.decimal_type))
					return true;
					
			} else if ((expr_type == TypeManager.uint64_type) ||
				   (expr_type == TypeManager.int64_type)) {
				//
				// From long/ulong to double, float, decimal
				//
				if ((target_type == TypeManager.double_type) ||
				    (target_type == TypeManager.float_type) ||
				    (target_type == TypeManager.decimal_type))
					return true;
				    
			} else if (expr_type == TypeManager.char_type){
				//
				// From char to ushort, int, uint, ulong, long, float, double, decimal
				// 
				if ((target_type == TypeManager.ushort_type) ||
				    (target_type == TypeManager.int32_type) ||
				    (target_type == TypeManager.uint32_type) ||
				    (target_type == TypeManager.uint64_type) ||
				    (target_type == TypeManager.int64_type) ||
				    (target_type == TypeManager.float_type) ||
				    (target_type == TypeManager.double_type) ||
				    (target_type == TypeManager.decimal_type))
					return true;

			} else if (expr_type == TypeManager.float_type){
				//
				// float to double
				//
				if (target_type == TypeManager.double_type)
					return true;
			}	
			
			if (expr.eclass == ExprClass.MethodGroup){
				if (TypeManager.IsDelegateType (target_type) && RootContext.Version != LanguageVersion.ISO_1){
					MethodGroupExpr mg = expr as MethodGroupExpr;
					if (mg != null){
						return DelegateCreation.ImplicitStandardConversionExists (mg, target_type) != null;
					}
				}
			}
			
			if (ImplicitReferenceConversionExists (expr, target_type))
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
			
			if ((target_type == TypeManager.enum_type ||
			     target_type.IsSubclassOf (TypeManager.enum_type)) &&
			     expr is IntLiteral){
				IntLiteral i = (IntLiteral) expr;

				if (i.Value == 0)
					return true;
			}

			//
			// If `expr_type' implements `target_type' (which is an iface)
			// see TryImplicitIntConversion
			// 
			if (target_type.IsInterface && target_type.IsAssignableFrom (expr_type))
				return true;

			if (target_type == TypeManager.void_ptr_type && expr_type.IsPointer)
				return true;

			if (TypeManager.IsNullableType (expr_type) && TypeManager.IsNullableType (target_type))
				return true;

			if (expr_type == TypeManager.anonymous_method_type){
				if (!TypeManager.IsDelegateType (target_type))
					return false;

				return true;
			}

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
				ParameterData pd = TypeManager.GetParameterData (mb);
				Type param_type = pd.ParameterType (0);

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
				Type ret_type = mi.ReturnType;
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
		static public Expression ImplicitUserConversion (EmitContext ec, Expression source,
								 Type target, Location loc)
		{
			return UserDefinedConversion (ec, source, target, loc, false);
		}

		/// <summary>
		///  User-defined Explicit conversions
		/// </summary>
		static public Expression ExplicitUserConversion (EmitContext ec, Expression source,
								 Type target, Location loc)
		{
			return UserDefinedConversion (ec, source, target, loc, true);
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
			foreach (MethodInfo m in mg.Methods) {
				ParameterData pd = TypeManager.GetParameterData (m);
				Type return_type = m.ReturnType;
				Type arg_type = pd.ParameterType (0);

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

				list.Add (m);
			}

			EmptyExpression.Release (expr);
		}

		/// <summary>
		///   Compute the user-defined conversion operator from source_type to target_type. 
		///   `look_for_explicit' controls whether we should also include the list of explicit operators
		/// </summary>
		static MethodInfo GetConversionOperator (EmitContext ec, Expression source, Type target_type, bool look_for_explicit)
		{
			ArrayList ops = new ArrayList (4);

			Type source_type = source.Type;

			if (source_type != TypeManager.decimal_type) {
				AddConversionOperators (ops, source, target_type, look_for_explicit,
					Expression.MethodLookup (ec, source_type, "op_Implicit", Location.Null) as MethodGroupExpr);
				if (look_for_explicit) {
					AddConversionOperators (ops, source, target_type, look_for_explicit,
						Expression.MethodLookup (
							ec, source_type, "op_Explicit", Location.Null) as MethodGroupExpr);
				}
			}

			if (target_type != TypeManager.decimal_type) {
				AddConversionOperators (ops, source, target_type, look_for_explicit,
					Expression.MethodLookup (ec, target_type, "op_Implicit", Location.Null) as MethodGroupExpr);
				if (look_for_explicit) {
					AddConversionOperators (ops, source, target_type, look_for_explicit,
						Expression.MethodLookup (
							ec, target_type, "op_Explicit", Location.Null) as MethodGroupExpr);
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
				if (m.ReturnType != most_specific_target)
					continue;
				if (TypeManager.GetParameterData (m).ParameterType (0) != most_specific_source)
					continue;
				// Ambiguous: more than one conversion operator satisfies the signature.
				if (method != null)
					return null;
				method = m;
			}

			return method;
		}

		static DoubleHash explicit_conv = new DoubleHash (100);
		static DoubleHash implicit_conv = new DoubleHash (100);

		/// <summary>
		///   User-defined conversions
		/// </summary>
		static public Expression UserDefinedConversion (EmitContext ec, Expression source,
								Type target, Location loc,
								bool look_for_explicit)
		{
			Type source_type = source.Type;
			MethodInfo method = null;

			if (TypeManager.IsNullableType (source_type) && TypeManager.IsNullableType (target))
				return new Nullable.LiftedConversion (
					source, target, true, look_for_explicit, loc).Resolve (ec);

			object o;
			DoubleHash hash = look_for_explicit ? explicit_conv : implicit_conv;

			if (!(source is Constant) && hash.Lookup (source_type, target, out o)) {
				method = (MethodInfo) o;
			} else {
				method = GetConversionOperator (ec, source, target, look_for_explicit);
				if (!(source is Constant))
					hash.Insert (source_type, target, method);
			}

			if (method == null)
				return null;
			
			Type most_specific_source = TypeManager.GetParameterData (method).ParameterType (0);

			//
			// This will do the conversion to the best match that we
			// found.  Now we need to perform an implict standard conversion
			// if the best match was not the type that we were requested
			// by target.
			//
			if (look_for_explicit)
				source = ExplicitConversionStandard (ec, source, most_specific_source, loc);
			else
				source = ImplicitConversionStandard (ec, source, most_specific_source, loc);

			if (source == null)
				return null;

			Expression e;
			e =  new UserCast (method, source, loc);
			if (e.Type != target){
				if (!look_for_explicit)
					e = ImplicitConversionStandard (ec, e, target, loc);
				else
					e = ExplicitConversionStandard (ec, e, target, loc);
			}

			return e;
		}
		
		/// <summary>
		///   Converts implicitly the resolved expression `expr' into the
		///   `target_type'.  It returns a new expression that can be used
		///   in a context that expects a `target_type'. 
		/// </summary>
		static public Expression ImplicitConversion (EmitContext ec, Expression expr,
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
		static public Expression ImplicitConversionStandard (EmitContext ec, Expression expr,
								     Type target_type, Location loc)
		{
			Type expr_type = expr.Type;
			Expression e;

			if (expr is NullLiteral) {
				if (target_type.IsGenericParameter)
					return TypeParameter_to_Null (target_type, loc);

				if (TypeManager.IsNullableType (target_type))
					return new Nullable.NullableLiteral (target_type, loc);
			}

			if (TypeManager.IsNullableType (expr_type) && TypeManager.IsNullableType (target_type))
				return new Nullable.LiftedConversion (
					expr, target_type, false, false, loc).Resolve (ec);

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
							ec, mg, target_type, false, loc);
				}
			}

			if (expr_type.Equals (target_type) && !TypeManager.IsNullType (expr_type))
				return expr;

			e = ImplicitNumericConversion (expr, target_type);
			if (e != null)
				return e;

			e = ImplicitReferenceConversion (expr, target_type);
			if (e != null)
				return e;
			
			if (TypeManager.IsEnumType (target_type) && expr is IntLiteral){
				IntLiteral i = (IntLiteral) expr;

				if (i.Value == 0)
					return new EnumConstant ((Constant) expr, target_type);
			}

			if (ec.InUnsafe) {
				if (expr_type.IsPointer){
					if (target_type == TypeManager.void_ptr_type)
						return new EmptyCast (expr, target_type);

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
					return new EmptyCast (NullPointer.Null, target_type);
			}

			if (expr_type == TypeManager.anonymous_method_type){
				if (!TypeManager.IsDelegateType (target_type)){
					Report.Error (1660, loc,
						"Cannot convert anonymous method block to type `{0}' because it is not a delegate type",
						TypeManager.CSharpName (target_type));
					return null;
				}

				AnonymousMethod am = (AnonymousMethod) expr;
				int errors = Report.Errors;

				Expression conv = am.Compatible (ec, target_type, false);
				if (conv != null)
					return conv;
				
				//
				// We return something instead of null, to avoid
				// the duplicate error, since am.Compatible would have
				// reported that already
				//
				if (errors != Report.Errors)
					return new EmptyCast (expr, target_type);
			}
			
			return null;
		}

		/// <summary>
		///   Attempts to perform an implicit constant conversion of the IntConstant
		///   into a different data type using casts (See Implicit Constant
		///   Expression Conversions)
		/// </summary>
		static public Expression TryImplicitIntConversion (Type target_type, IntConstant ic)
		{
			int value = ic.Value;

			if (target_type == TypeManager.sbyte_type){
				if (value >= SByte.MinValue && value <= SByte.MaxValue)
					return new SByteConstant ((sbyte) value, ic.Location);
			} else if (target_type == TypeManager.byte_type){
				if (value >= Byte.MinValue && value <= Byte.MaxValue)
					return new ByteConstant ((byte) value, ic.Location);
			} else if (target_type == TypeManager.short_type){
				if (value >= Int16.MinValue && value <= Int16.MaxValue)
					return new ShortConstant ((short) value, ic.Location);
			} else if (target_type == TypeManager.ushort_type){
				if (value >= UInt16.MinValue && value <= UInt16.MaxValue)
					return new UShortConstant ((ushort) value, ic.Location);
			} else if (target_type == TypeManager.uint32_type){
				if (value >= 0)
					return new UIntConstant ((uint) value, ic.Location);
			} else if (target_type == TypeManager.uint64_type){
				//
				// we can optimize this case: a positive int32
				// always fits on a uint64.  But we need an opcode
				// to do it.
				//
				if (value >= 0)
					return new ULongConstant ((ulong) value, ic.Location);
			} else if (target_type == TypeManager.double_type)
				return new DoubleConstant ((double) value, ic.Location);
			else if (target_type == TypeManager.float_type)
				return new FloatConstant ((float) value, ic.Location);
			
			if (value == 0 && ic is IntLiteral && TypeManager.IsEnumType (target_type)){
				Type underlying = TypeManager.EnumToUnderlying (target_type);
				Constant e = (Constant) ic;
				
				//
				// Possibly, we need to create a different 0 literal before passing
				// to EnumConstant
				//
				if (underlying == TypeManager.int64_type)
					e = new LongLiteral (0, ic.Location);
				else if (underlying == TypeManager.uint64_type)
					e = new ULongLiteral (0, ic.Location);

				return new EnumConstant (e, target_type);
			}

			//
			// If `target_type' is an interface and the type of `ic' implements the interface
			// e.g. target_type is IComparable, IConvertible, IFormattable
			//
			if (target_type.IsInterface && target_type.IsAssignableFrom (ic.Type))
				return new BoxedCast (ic, target_type);

			return null;
		}

		/// <summary>
		///   Attempts to implicitly convert `source' into `target_type', using
		///   ImplicitConversion.  If there is no implicit conversion, then
		///   an error is signaled
		/// </summary>
		static public Expression ImplicitConversionRequired (EmitContext ec, Expression source,
								     Type target_type, Location loc)
		{
			Expression e;

			int errors = Report.Errors;
			e = ImplicitConversion (ec, source, target_type, loc);
			if (Report.Errors > errors)
				return null;
			if (e != null)
				return e;

			if (source is DoubleLiteral) {
				if (target_type == TypeManager.float_type) {
					Error_664 (loc, "float", "f");
					return null;
				}
				if (target_type == TypeManager.decimal_type) {
					Error_664 (loc, "decimal", "m");
					return null;
				}
			}

			source.Error_ValueCannotBeConverted (loc, target_type, false);
			return null;
		}

		static void Error_664 (Location loc, string type, string suffix) {
			Report.Error (664, loc,
				"Literal of type double cannot be implicitly converted to type `{0}'. Add suffix `{1}' to create a literal of this type",
				type, suffix);
		}

		/// <summary>
		///   Performs the explicit numeric conversions
		/// </summary>
		public static Expression ExplicitNumericConversion (Expression expr, Type target_type)
		{
			Type expr_type = expr.Type;
			Type real_target_type = target_type;

			if (expr_type == TypeManager.sbyte_type){
				//
				// From sbyte to byte, ushort, uint, ulong, char
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
				// From short to sbyte, byte, ushort, uint, ulong, char
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
				// From int to sbyte, byte, short, ushort, uint, ulong, char
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
			bool target_is_type_param = target_type.IsGenericParameter;
			bool target_is_value_type = target_type.IsValueType;
			
			if (source_type == target_type)
				return true;
			
			//
			// From generic parameter to any type
			//
			if (source_type.IsGenericParameter)
				return true;

			//
			// From object to a generic parameter
			//
			if (source_type == TypeManager.object_type && target_is_type_param)
				return true;

			//
			// From object to any reference type
			//
			if (source_type == TypeManager.object_type && !target_is_value_type)
				return true;
					
			//
			// From any class S to any class-type T, provided S is a base class of T
			//
			if (TypeManager.IsSubclassOf (target_type, source_type))
				return true;

			//
			// From any interface type S to any interface T provided S is not derived from T
			//
			if (source_type.IsInterface && target_type.IsInterface){
				if (!TypeManager.IsSubclassOf (target_type, source_type))
					return true;
			}
			    
			//
			// From any class type S to any interface T, provided S is not sealed
			// and provided S does not implement T.
			//
			if (target_type.IsInterface && !source_type.IsSealed &&
			    !TypeManager.ImplementsInterface (source_type, target_type))
				return true;

			//
			// From any interface-type S to to any class type T, provided T is not
			// sealed, or provided T implements S.
			//
			if (source_type.IsInterface &&
			    (!target_type.IsSealed || TypeManager.ImplementsInterface (target_type, source_type)))
				return true;
			
			
			// From an array type S with an element type Se to an array type T with an 
			// element type Te provided all the following are true:
			//     * S and T differe only in element type, in other words, S and T
			//       have the same number of dimensions.
			//     * Both Se and Te are reference types
			//     * An explicit referenc conversions exist from Se to Te
			//
			if (source_type.IsArray && target_type.IsArray) {
				if (source_type.GetArrayRank () == target_type.GetArrayRank ()) {
					
					Type source_element_type = TypeManager.GetElementType (source_type);
					Type target_element_type = TypeManager.GetElementType (target_type);

					if (source_element_type.IsGenericParameter ||
					    (!source_element_type.IsValueType && !target_element_type.IsValueType))
						if (ExplicitReferenceConversionExists (source_element_type,
										       target_element_type))
							return true;
				}
			}
			

			// From System.Array to any array-type
			if (source_type == TypeManager.array_type &&
			    target_type.IsArray){
				return true;
			}

			//
			// From System delegate to any delegate-type
			//
			if (source_type == TypeManager.delegate_type &&
			    TypeManager.IsDelegateType (target_type))
				return true;

			//
			// From ICloneable to Array or Delegate types
			//
			if (source_type == TypeManager.icloneable_type &&
			    (target_type == TypeManager.array_type ||
			     target_type == TypeManager.delegate_type))
				return true;
			
			return false;
		}

		/// <summary>
		///   Implements Explicit Reference conversions
		/// </summary>
		static Expression ExplicitReferenceConversion (Expression source, Type target_type)
		{
			Type source_type = source.Type;
			bool target_is_type_param = target_type.IsGenericParameter;
			bool target_is_value_type = target_type.IsValueType;

			//
			// From object to a generic parameter
			//
			if (source_type == TypeManager.object_type && target_is_type_param)
				return new UnboxCast (source, target_type);

			//
			// From object to any reference type
			//
			if (source_type == TypeManager.object_type && !target_is_value_type)
				return new ClassCast (source, target_type);

			//
			// Unboxing conversion.
			//
			if (((source_type == TypeManager.enum_type &&
				!(source is EmptyCast)) ||
				source_type == TypeManager.value_type) && target_is_value_type)
				return new UnboxCast (source, target_type);

			//
			// From any class S to any class-type T, provided S is a base class of T
			//
			if (TypeManager.IsSubclassOf (target_type, source_type))
				return new ClassCast (source, target_type);

			//
			// From any interface type S to any interface T provided S is not derived from T
			//
			if (source_type.IsInterface && target_type.IsInterface){
				if (TypeManager.ImplementsInterface (source_type, target_type))
					return null;
				else
					return new ClassCast (source, target_type);
			}

			//
			// From any class type S to any interface T, provides S is not sealed
			// and provided S does not implement T.
			//
			if (target_type.IsInterface && !source_type.IsSealed) {
				if (TypeManager.ImplementsInterface (source_type, target_type))
					return null;
				else
					return new ClassCast (source, target_type);
				
			}

			//
			// From any interface-type S to to any class type T, provided T is not
			// sealed, or provided T implements S.
			//
			if (source_type.IsInterface) {
				if (!target_type.IsSealed || TypeManager.ImplementsInterface (target_type, source_type)) {
					if (target_type.IsClass)
						return new ClassCast (source, target_type);
					else
						return new UnboxCast (source, target_type);
				}

				return null;
			}
			
			// From an array type S with an element type Se to an array type T with an 
			// element type Te provided all the following are true:
			//     * S and T differe only in element type, in other words, S and T
			//       have the same number of dimensions.
			//     * Both Se and Te are reference types
			//     * An explicit referenc conversions exist from Se to Te
			//
			if (source_type.IsArray && target_type.IsArray) {
				if (source_type.GetArrayRank () == target_type.GetArrayRank ()) {
					
					Type source_element_type = TypeManager.GetElementType (source_type);
					Type target_element_type = TypeManager.GetElementType (target_type);
					
					if (!source_element_type.IsValueType && !target_element_type.IsValueType)
						if (ExplicitReferenceConversionExists (source_element_type,
										       target_element_type))
							return new ClassCast (source, target_type);
				}
			}
			

			// From System.Array to any array-type
			if (source_type == TypeManager.array_type &&
			    target_type.IsArray) {
				return new ClassCast (source, target_type);
			}

			//
			// From System delegate to any delegate-type
			//
			if (source_type == TypeManager.delegate_type &&
			    TypeManager.IsDelegateType (target_type))
				return new ClassCast (source, target_type);

			//
			// From ICloneable to Array or Delegate types
			//
			if (source_type == TypeManager.icloneable_type &&
			    (target_type == TypeManager.array_type ||
			     target_type == TypeManager.delegate_type))
				return new ClassCast (source, target_type);
			
			return null;
		}
		
		/// <summary>
		///   Performs an explicit conversion of the expression `expr' whose
		///   type is expr.Type to `target_type'.
		/// </summary>
		static public Expression ExplicitConversionCore (EmitContext ec, Expression expr,
								 Type target_type, Location loc)
		{
			Type expr_type = expr.Type;

			// Explicit conversion includes implicit conversion and it used for enum underlying types too
			Expression ne = ImplicitConversionStandard (ec, expr, target_type, loc);
			if (ne != null)
				return ne;

			//
			// Unboxing conversions; only object types can be convertible to enum
			//
			if (expr_type == TypeManager.object_type && target_type.IsValueType || expr_type == TypeManager.enum_type)
				return new UnboxCast (expr, target_type);

			if (TypeManager.IsEnumType (expr_type)) {
				if (expr is EnumConstant)
					return ExplicitConversionCore (ec, ((EnumConstant) expr).Child, target_type, loc);

				return ExplicitConversionCore (ec, new EmptyCast (expr, TypeManager.EnumToUnderlying (expr_type)), target_type, loc);
			}

			if (TypeManager.IsNullableType (expr_type) && TypeManager.IsNullableType (target_type))
				return new Nullable.LiftedConversion (
					expr, target_type, false, true, loc).Resolve (ec);

			if (TypeManager.IsEnumType (target_type)){
				Expression ce = ExplicitConversionCore (ec, expr, TypeManager.EnumToUnderlying (target_type), loc);
				if (ce != null)
					return new EmptyCast (ce, target_type);
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
				ne = ExplicitReferenceConversion (expr, target_type);
				if (ne != null)
					return ne;
			}

			if (ec.InUnsafe){
				ne = ExplicitUnsafe (expr, target_type);
				if (ne != null)
					return ne;
			}

			ne = ExplicitUserConversion (ec, expr, target_type, loc);
			if (ne != null)
				return ne;

			return null;
		}

		public static Expression ExplicitUnsafe (Expression expr, Type target_type)
		{
			Type expr_type = expr.Type;

			if (target_type.IsPointer){
				if (expr_type.IsPointer)
					return new EmptyCast (expr, target_type);
					
				if (expr_type == TypeManager.sbyte_type ||
					expr_type == TypeManager.short_type ||
					expr_type == TypeManager.int32_type ||
					expr_type == TypeManager.int64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I);

				if (expr_type == TypeManager.ushort_type ||
					expr_type == TypeManager.uint32_type ||
					expr_type == TypeManager.uint64_type ||
					expr_type == TypeManager.byte_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U);
			}
			
			if (expr_type.IsPointer){
				if (target_type == TypeManager.sbyte_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I1);
				else if (target_type == TypeManager.byte_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U1);
				else if (target_type == TypeManager.short_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I2);
				else if (target_type == TypeManager.ushort_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U2);
				else if (target_type == TypeManager.int32_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I4);
				else if (target_type == TypeManager.uint32_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U4);
				else if (target_type == TypeManager.uint64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_U8);
				else if (target_type == TypeManager.int64_type){
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I8);
				}
			}
			return null;
		}

		/// <summary>
		///   Same as ExplicitConversion, only it doesn't include user defined conversions
		/// </summary>
		static public Expression ExplicitConversionStandard (EmitContext ec, Expression expr,
								     Type target_type, Location l)
		{
			int errors = Report.Errors;
			Expression ne = ImplicitConversionStandard (ec, expr, target_type, l);
			if (Report.Errors > errors)
				return null;

			if (ne != null)
				return ne;

			if (TypeManager.IsNullableType (expr.Type) && TypeManager.IsNullableType (target_type))
				return new Nullable.LiftedConversion (
					expr, target_type, false, true, l).Resolve (ec);

			ne = ExplicitNumericConversion (expr, target_type);
			if (ne != null)
				return ne;

			ne = ExplicitReferenceConversion (expr, target_type);
			if (ne != null)
				return ne;

			if (ec.InUnsafe && expr.Type == TypeManager.void_ptr_type && target_type.IsPointer)
				return new EmptyCast (expr, target_type);

			expr.Error_ValueCannotBeConverted (l, target_type, true);
			return null;
		}

		/// <summary>
		///   Performs an explicit conversion of the expression `expr' whose
		///   type is expr.Type to `target_type'.
		/// </summary>
		static public Expression ExplicitConversion (EmitContext ec, Expression expr,
			Type target_type, Location loc)
		{
			Expression e = ExplicitConversionCore (ec, expr, target_type, loc);
			if (e != null)
				return e;

			expr.Error_ValueCannotBeConverted (loc, target_type, true);
			return null;
		}
	}
}
