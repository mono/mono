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
		
		static public void Error_CannotConvertType (Location loc, Type source, Type target)
		{
			Report.Error (30, loc, "Cannot convert type '" +
				      TypeManager.CSharpName (source) + "' to '" +
				      TypeManager.CSharpName (target) + "'");
		}

		static Expression TypeParameter_to_Null (Expression expr, Type target_type,
							 Location loc)
		{
			if (!TypeParameter_to_Null (target_type)) {
				Report.Error (403, loc, "Cannot convert null to the type " +
					      "parameter `{0}' becaues it could be a value " +
					      "type.  Consider using `default ({0})' instead.",
					      target_type);
				return null;
			}

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

		static Type TypeParam_EffectiveBaseType (EmitContext ec, Type t)
		{
			GenericConstraints gc = TypeManager.GetTypeParameterConstraints (t);
			if (gc == null)
				return TypeManager.object_type;

			return TypeParam_EffectiveBaseType (ec, gc);
		}

		static Type TypeParam_EffectiveBaseType (EmitContext ec, GenericConstraints gc)
		{
			ArrayList list = new ArrayList ();
			list.Add (gc.EffectiveBaseClass);
			foreach (Type t in gc.InterfaceConstraints) {
				if (!t.IsGenericParameter)
					continue;

				GenericConstraints new_gc = TypeManager.GetTypeParameterConstraints (t);
				if (new_gc != null)
					list.Add (TypeParam_EffectiveBaseType (ec, new_gc));
			}
			return FindMostEncompassedType (ec, list);
		}

		static Expression TypeParameterConversion (Expression expr, bool is_reference, Type target_type)
		{
			if (is_reference)
				return new EmptyCast (expr, target_type);
			else
				return new BoxedCast (expr, target_type);
		}

		static Expression ImplicitTypeParameterConversion (EmitContext ec, Expression expr,
								   Type target_type)
		{
			Type expr_type = expr.Type;

			GenericConstraints gc = TypeManager.GetTypeParameterConstraints (expr_type);

			if (gc == null) {
				if (target_type == TypeManager.object_type)
					return new BoxedCast (expr);

				return null;
			}

			// We're converting from a type parameter which is known to be a reference type.
			bool is_reference = gc.IsReferenceType;
			Type base_type = TypeParam_EffectiveBaseType (ec, gc);

			if (TypeManager.IsSubclassOf (base_type, target_type))
				return TypeParameterConversion (expr, is_reference, target_type);

			if (target_type.IsInterface) {
				if (TypeManager.ImplementsInterface (base_type, target_type))
					return TypeParameterConversion (expr, is_reference, target_type);

				foreach (Type t in gc.InterfaceConstraints) {
					if (TypeManager.IsSubclassOf (t, target_type))
						return TypeParameterConversion (expr, is_reference, target_type);
				}
			}

			foreach (Type t in gc.InterfaceConstraints) {
				if (!t.IsGenericParameter)
					continue;
				if (TypeManager.IsSubclassOf (t, target_type))
					return TypeParameterConversion (expr, is_reference, target_type);
			}

			return null;
		}

		static EmptyExpression MyEmptyExpr;
		static public Expression WideningReferenceConversion (EmitContext ec, Expression expr, Type target_type)
		{
			Type expr_type = expr.Type;

			if (expr_type == null && expr.eclass == ExprClass.MethodGroup){
				// if we are a method group, emit a warning

				expr.Emit (null);
			}

			if (expr_type == TypeManager.void_type)
				return null;

			if (expr_type.IsGenericParameter)
				return ImplicitTypeParameterConversion (ec, expr, target_type);
				
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
					return new BoxedCast (expr);
				if (expr_type.IsClass || expr_type.IsInterface || expr_type == TypeManager.enum_type){
					if (expr_type == TypeManager.anonymous_method_type)
						return null;
					return new EmptyCast (expr, target_type);
				}

				return null;
			} else if (target_type == TypeManager.value_type) {
				if (TypeManager.IsValueType (expr_type))
					return new BoxedCast (expr);
				if (expr_type == TypeManager.null_type)
					return new NullCast (expr, target_type);

				return null;
			} else if (TypeManager.IsSubclassOf (expr_type, target_type)) {
				//
				// Special case: enumeration to System.Enum.
				// System.Enum is not a value type, it is a class, so we need
				// a boxing conversion
				//
				if (expr_type.IsEnum || expr_type.IsGenericParameter)
					return new BoxedCast (expr);

				return new EmptyCast (expr, target_type);
			}

			// This code is kind of mirrored inside WideningStandardConversionExists
			// with the small distinction that we only probe there
			//
			// Always ensure that the code here and there is in sync

			// from the null type to any reference-type.
			if (expr_type == TypeManager.null_type){
				if (target_type.IsPointer)
					return new EmptyCast (expr, target_type);
					
				if (!target_type.IsValueType)
					return new NullCast (expr, target_type);

				// VB.NET specific: Convert Nothing to value types

				Expression e = NothingToPrimitiveConstants (expr, target_type);
				if (e != null)
					return e;

				return new NullCast (expr, target_type);
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
						if (WideningStandardConversionExists (ConstantEC, MyEmptyExpr,
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
		public static bool WideningReferenceConversionExists (EmitContext ec, Expression expr, Type target_type)
		{
			Type expr_type = expr.Type;

			if (expr_type.IsGenericParameter)
				return ImplicitTypeParameterConversion (ec, expr, target_type) != null;

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
			// from WideningReferenceConversion so make sure code remains in sync
				
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
						if (WideningStandardConversionExists (ConstantEC, MyEmptyExpr,
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
		static public Expression WideningNumericConversion (EmitContext ec, Expression expr,
								    Type target_type, Location loc)
		{
			Type expr_type = expr.Type;

			//
			// Attempt to do the implicit constant expression conversions

			if (expr is Constant){
				Expression e;

				e = WideningConstantConversions (target_type, (Constant) expr);
				if (e != null)
					return e;
				
				if (expr is IntConstant){
					e = TryWideningIntConversion (target_type, (IntConstant) expr);
					
					if (e != null)
						return e;
				} 
			}
			
 			Type real_target_type = target_type;

			// VB.NET specific: Convert an enum to it's
			// underlying numeric type or any type that
			// it's underlyinmg type has widening
			// conversion to.

			if (expr_type.IsSubclassOf (TypeManager.enum_type)){
				if (target_type == TypeManager.enum_type ||
				    target_type == TypeManager.object_type) {
					if (expr is EnumConstant)
						expr = ((EnumConstant) expr).Child;
					// We really need all these casts here .... :-(
					expr = new BoxedCast (new EmptyCast (expr, expr_type));
					return new EmptyCast (expr, target_type);
				} 

				//
				// Notice that we have kept the expr_type unmodified, which is only
				// used later on to 
				if (expr is EnumConstant)
					expr = ((EnumConstant) expr).Child;
				else
					expr = new EmptyCast (expr, TypeManager.EnumToUnderlying (expr_type));
				expr_type = expr.Type;

				if (expr_type == target_type)
					return expr;
			}

			if (expr_type == TypeManager.sbyte_type){
				//
				// From sbyte to short, int, long, float, double.
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
			} else if (expr_type == TypeManager.byte_type){
				//
				// From byte to short, ushort, int, uint, long, ulong, float, double
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
					return new ImplicitNew (ec, "System", "Decimal", loc, expr);
			} else if (expr_type == TypeManager.short_type){
				//
				// From short to int, long, float, double
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
					return new ImplicitNew (ec, "System", "Decimal", loc, expr);
			} else if (expr_type == TypeManager.ushort_type){
				//
				// From ushort to int, uint, long, ulong, float, double
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
			} else if (expr_type == TypeManager.int32_type){
				//
				// From int to long, float, double
				//
				if (real_target_type == TypeManager.int64_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_I8);
				if (real_target_type == TypeManager.double_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R8);
				if (real_target_type == TypeManager.float_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R4);
				if (real_target_type == TypeManager.decimal_type)
					return new ImplicitNew (ec, "System", "Decimal", loc, expr);
			} else if (expr_type == TypeManager.uint32_type){
				//
				// From uint to long, ulong, float, double
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
			} else if (expr_type == TypeManager.int64_type){
				//
				// From long/ulong to float, double
				//
				if (real_target_type == TypeManager.double_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R8);
				if (real_target_type == TypeManager.float_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R4);	
				if (real_target_type == TypeManager.decimal_type)
					return new ImplicitNew (ec, "System", "Decimal", loc, expr);	
			} else if (expr_type == TypeManager.float_type){
				//
				// float to double
				//
				if (real_target_type == TypeManager.double_type)
					return new OpcodeCast (expr, target_type, OpCodes.Conv_R8);
			} else if (expr_type == TypeManager.decimal_type){
				//
				// From decimal to float, double
				//
				if (real_target_type == TypeManager.double_type)
					return new ImplicitInvocation (ec, "System", "Convert", "ToDouble", loc, expr);	
				if (real_target_type == TypeManager.float_type)
					return new ImplicitInvocation (ec, "System", "Convert" ,"ToSingle", loc, expr);	
			}

			return null;
		}


		/// <summary>
		///  Same as WideningStandardConversionExists except that it also looks at
		///  implicit user defined conversions - needed for overload resolution
		/// </summary>
		public static bool WideningConversionExists (EmitContext ec, Expression expr, Type target_type)
		{
			if (expr is NullLiteral) {
				if (target_type.IsGenericParameter)
					return TypeParameter_to_Null (target_type);

				if (TypeManager.IsNullableType (target_type))
					return true;
			}

			if (WideningStandardConversionExists (ec, expr, target_type))
				return true;

			//
			// VB.NET has no notion of User defined conversions
			//

// 			Expression dummy = ImplicitUserConversion (ec, expr, target_type, Location.Null);

// 			if (dummy != null)
// 				return true;

			return false;
		}

		//
		// VB.NET has no notion of User defined conversions
		//

// 		public static bool ImplicitUserConversionExists (EmitContext ec, Type source, Type target)
// 		{
// 			Expression dummy = ImplicitUserConversion (
// 				ec, new EmptyExpression (source), target, Location.Null);
// 			return dummy != null;
// 		}

		/// <summary>
		///  Determines if a standard implicit conversion exists from
		///  expr_type to target_type
		///
		///  ec should point to a real EmitContext if expr.Type is TypeManager.anonymous_method_type.
		/// </summary>
		public static bool WideningStandardConversionExists (EmitContext ec, Expression expr, Type target_type)
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
				// From sbyte to short, int, long, float, double.
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
				// From byte to short, ushort, int, uint, long, ulong, float, double
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
				// From short to int, long, float, double
				// 
				if ((target_type == TypeManager.int32_type) ||
				    (target_type == TypeManager.int64_type) ||
				    (target_type == TypeManager.double_type) ||
				    (target_type == TypeManager.float_type) ||
				    (target_type == TypeManager.decimal_type))
					return true;
					
			} else if (expr_type == TypeManager.ushort_type){
				//
				// From ushort to int, uint, long, ulong, float, double
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
				// From int to long, float, double
				//
				if ((target_type == TypeManager.int64_type) ||
				    (target_type == TypeManager.double_type) ||
				    (target_type == TypeManager.float_type) ||
				    (target_type == TypeManager.decimal_type))
					return true;
					
			} else if (expr_type == TypeManager.uint32_type){
				//
				// From uint to long, ulong, float, double
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
				// From long/ulong to float, double
				//
				if ((target_type == TypeManager.double_type) ||
				    (target_type == TypeManager.float_type) ||
				    (target_type == TypeManager.decimal_type))
					return true;
				    
			} else if (expr_type == TypeManager.char_type){
				//
				// From char to ushort, int, uint, long, ulong, float, double
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
						//
						// This should not happen frequently, so we can create an object
						// to test compatibility
						//
						Expression c = ImplicitDelegateCreation.Create (ec, mg, target_type, Location.Null);
						return c != null;
					}
				}
			}
			
			if (WideningReferenceConversionExists (ec, expr, target_type))
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
					if (Byte.MinValue >= 0 && value <= Byte.MaxValue)
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
				if (v > 0)
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
			// see TryWideningIntConversion
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

				AnonymousMethod am = (AnonymousMethod) expr;

				Expression conv = am.Compatible (ec, target_type, true);
				if (conv != null)
					return true;
			}

			return false;
		}

		//
		// Used internally by FindMostEncompassedType, this is used
		// to avoid creating lots of objects in the tight loop inside
		// FindMostEncompassedType
		//
		static EmptyExpression priv_fmet_param;
		
		/// <summary>
		///  Finds "most encompassed type" according to the spec (13.4.2)
		///  amongst the methods in the MethodGroupExpr
		/// </summary>
		static Type FindMostEncompassedType (EmitContext ec, ArrayList types)
		{
			Type best = null;

			if (priv_fmet_param == null)
				priv_fmet_param = new EmptyExpression ();

			foreach (Type t in types){
				priv_fmet_param.SetType (t);
				
				if (best == null) {
					best = t;
					continue;
				}
				
				if (WideningStandardConversionExists (ec, priv_fmet_param, best))
					best = t;
			}

			return best;
		}

		//
		// Used internally by FindMostEncompassingType, this is used
		// to avoid creating lots of objects in the tight loop inside
		// FindMostEncompassingType
		//
		static EmptyExpression priv_fmee_ret;
		
		/// <summary>
		///  Finds "most encompassing type" according to the spec (13.4.2)
		///  amongst the types in the given set
		/// </summary>
		static Type FindMostEncompassingType (EmitContext ec, ArrayList types)
		{
			Type best = null;

			if (priv_fmee_ret == null)
				priv_fmee_ret = new EmptyExpression ();

			foreach (Type t in types){
				priv_fmee_ret.SetType (best);

				if (best == null) {
					best = t;
					continue;
				}

				if (WideningStandardConversionExists (ec, priv_fmee_ret, t))
					best = t;
			}
			
			return best;
		}

		//
		// Used to avoid creating too many objects
		//
		static EmptyExpression priv_fms_expr;
		
		/// <summary>
		///   Finds the most specific source Sx according to the rules of the spec (13.4.4)
		///   by making use of FindMostEncomp* methods. Applies the correct rules separately
		///   for explicit and implicit conversion operators.
		/// </summary>
		static public Type FindMostSpecificSource (EmitContext ec, MethodGroupExpr me,
							   Expression source, bool apply_explicit_conv_rules,
							   Location loc)
		{
			ArrayList src_types_set = new ArrayList ();
			
			if (priv_fms_expr == null)
				priv_fms_expr = new EmptyExpression ();

			//
			// If any operator converts from S then Sx = S
			//
			Type source_type = source.Type;
			foreach (MethodBase mb in me.Methods){
				ParameterData pd = Invocation.GetParameterData (mb);
				Type param_type = pd.ParameterType (0);

				if (param_type == source_type)
					return param_type;

				if (apply_explicit_conv_rules) {
					//
					// From the spec :
					// Find the set of applicable user-defined conversion operators, U.  This set
					// consists of the
					// user-defined implicit or explicit conversion operators declared by
					// the classes or structs in D that convert from a type encompassing
					// or encompassed by S to a type encompassing or encompassed by T
					//
					priv_fms_expr.SetType (param_type);
					if (WideningStandardConversionExists (ec, priv_fms_expr, source_type))
						src_types_set.Add (param_type);
					else {
						if (WideningStandardConversionExists (ec, source, param_type))
							src_types_set.Add (param_type);
					}
				} else {
					//
					// Only if S is encompassed by param_type
					//
					if (WideningStandardConversionExists (ec, source, param_type))
						src_types_set.Add (param_type);
				}
			}
			
			//
			// Explicit Conv rules
			//
			if (apply_explicit_conv_rules) {
				ArrayList candidate_set = new ArrayList ();

				foreach (Type param_type in src_types_set){
					if (WideningStandardConversionExists (ec, source, param_type))
						candidate_set.Add (param_type);
				}

				if (candidate_set.Count != 0)
					return FindMostEncompassedType (ec, candidate_set);
			}

			//
			// Final case
			//
			if (apply_explicit_conv_rules)
				return FindMostEncompassingType (ec, src_types_set);
			else
				return FindMostEncompassedType (ec, src_types_set);
		}

		//
		// Useful in avoiding proliferation of objects
		//
		static EmptyExpression priv_fmt_expr;
		
		/// <summary>
		///  Finds the most specific target Tx according to section 13.4.4
		/// </summary>
		static public Type FindMostSpecificTarget (EmitContext ec, MethodGroupExpr me,
							   Type target, bool apply_explicit_conv_rules,
							   Location loc)
		{
			ArrayList tgt_types_set = new ArrayList ();
			
			if (priv_fmt_expr == null)
				priv_fmt_expr = new EmptyExpression ();
			
			//
			// If any operator converts to T then Tx = T
			//
			foreach (MethodInfo mi in me.Methods){
				Type ret_type = mi.ReturnType;

				if (ret_type == target)
					return ret_type;

				if (apply_explicit_conv_rules) {
					//
					// From the spec :
					// Find the set of applicable user-defined conversion operators, U.
					//
					// This set consists of the
					// user-defined implicit or explicit conversion operators declared by
					// the classes or structs in D that convert from a type encompassing
					// or encompassed by S to a type encompassing or encompassed by T
					//
					priv_fms_expr.SetType (ret_type);
					if (WideningStandardConversionExists (ec, priv_fms_expr, target))
						tgt_types_set.Add (ret_type);
					else {
						priv_fms_expr.SetType (target);
						if (WideningStandardConversionExists (ec, priv_fms_expr, ret_type))
							tgt_types_set.Add (ret_type);
					}
				} else {
					//
					// Only if T is encompassed by param_type
					//
					priv_fms_expr.SetType (ret_type);
					if (WideningStandardConversionExists (ec, priv_fms_expr, target))
						tgt_types_set.Add (ret_type);
				}
			}

			//
			// Explicit conv rules
			//
			if (apply_explicit_conv_rules) {
				ArrayList candidate_set = new ArrayList ();

				foreach (Type ret_type in tgt_types_set){
					priv_fmt_expr.SetType (ret_type);
					
					if (WideningStandardConversionExists (ec, priv_fmt_expr, target))
						candidate_set.Add (ret_type);
				}

				if (candidate_set.Count != 0)
					return FindMostEncompassingType (ec, candidate_set);
			}
			
			//
			// Okay, final case !
			//
			if (apply_explicit_conv_rules)
				return FindMostEncompassedType (ec, tgt_types_set);
			else 
				return FindMostEncompassingType (ec, tgt_types_set);
		}
		
		/// <summary>
		///  User-defined Implicit conversions
		/// </summary>

		//
		// VB.NET has no notion of User defined conversions
		//

// 		static public Expression ImplicitUserConversion (EmitContext ec, Expression source,
// 								 Type target, Location loc)
// 		{
// 			return UserDefinedConversion (ec, source, target, loc, false);
// 		}

		/// <summary>
		///  User-defined Explicit conversions
		/// </summary>

		//
		// VB.NET has no notion of User defined conversions
		//

// 		static public Expression ExplicitUserConversion (EmitContext ec, Expression source,
// 								 Type target, Location loc)
// 		{
// 			return UserDefinedConversion (ec, source, target, loc, true);
// 		}

		static DoubleHash explicit_conv = new DoubleHash (100);
		static DoubleHash implicit_conv = new DoubleHash (100);
		/// <summary>
		///   Computes the MethodGroup for the user-defined conversion
		///   operators from source_type to target_type.  `look_for_explicit'
		///   controls whether we should also include the list of explicit
		///   operators
		/// </summary>
		static MethodGroupExpr GetConversionOperators (EmitContext ec,
							       Type source_type, Type target_type,
							       Location loc, bool look_for_explicit)
		{
			Expression mg1 = null, mg2 = null;
			Expression mg5 = null, mg6 = null, mg7 = null, mg8 = null;
			string op_name;

			op_name = "op_Implicit";

			MethodGroupExpr union3;
			object r;
			if ((look_for_explicit ? explicit_conv : implicit_conv).Lookup (source_type, target_type, out r))
				return (MethodGroupExpr) r;

			mg1 = Expression.MethodLookup (ec, source_type, op_name, loc);
			if (source_type.BaseType != null)
				mg2 = Expression.MethodLookup (ec, source_type.BaseType, op_name, loc);

			if (mg1 == null)
				union3 = (MethodGroupExpr) mg2;
			else if (mg2 == null)
				union3 = (MethodGroupExpr) mg1;
			else
				union3 = Invocation.MakeUnionSet (mg1, mg2, loc);

			mg1 = Expression.MethodLookup (ec, target_type, op_name, loc);
			if (mg1 != null){
				if (union3 != null)
					union3 = Invocation.MakeUnionSet (union3, mg1, loc);
				else
					union3 = (MethodGroupExpr) mg1;
			}

			if (target_type.BaseType != null)
				mg1 = Expression.MethodLookup (ec, target_type.BaseType, op_name, loc);
			
			if (mg1 != null){
				if (union3 != null)
					union3 = Invocation.MakeUnionSet (union3, mg1, loc);
				else
					union3 = (MethodGroupExpr) mg1;
			}

			MethodGroupExpr union4 = null;

			if (look_for_explicit) {
				op_name = "op_Explicit";

				mg5 = Expression.MemberLookup (ec, source_type, op_name, loc);
				if (source_type.BaseType != null)
					mg6 = Expression.MethodLookup (ec, source_type.BaseType, op_name, loc);
				
				mg7 = Expression.MemberLookup (ec, target_type, op_name, loc);
				if (target_type.BaseType != null)
					mg8 = Expression.MethodLookup (ec, target_type.BaseType, op_name, loc);
				
				MethodGroupExpr union5 = Invocation.MakeUnionSet (mg5, mg6, loc);
				MethodGroupExpr union6 = Invocation.MakeUnionSet (mg7, mg8, loc);

				union4 = Invocation.MakeUnionSet (union5, union6, loc);
			}
			
			MethodGroupExpr ret = Invocation.MakeUnionSet (union3, union4, loc);
			(look_for_explicit ? explicit_conv : implicit_conv).Insert (source_type, target_type, ret);
			return ret;
		}
		
		/// <summary>
		///   User-defined conversions
		/// </summary>

		//
		// VB.NET has no notion of User defined conversions. This method is not used.
		//
		static public Expression UserDefinedConversion (EmitContext ec, Expression source,
								Type target, Location loc,
								bool look_for_explicit)
		{
			MethodGroupExpr union;
			Type source_type = source.Type;
			MethodBase method = null;

			if (TypeManager.IsNullableType (source_type) && TypeManager.IsNullableType (target))
				return new Nullable.LiftedConversion (
					source, target, true, look_for_explicit, loc).Resolve (ec);

			union = GetConversionOperators (ec, source_type, target, loc, look_for_explicit);
			if (union == null)
				return null;
			
			Type most_specific_source, most_specific_target;

			most_specific_source = FindMostSpecificSource (ec, union, source, look_for_explicit, loc);
			if (most_specific_source == null)
				return null;

			most_specific_target = FindMostSpecificTarget (ec, union, target, look_for_explicit, loc);
			if (most_specific_target == null) 
				return null;

			int count = 0;

			
			foreach (MethodBase mb in union.Methods){
				ParameterData pd = Invocation.GetParameterData (mb);
				MethodInfo mi = (MethodInfo) mb;
				
				if (pd.ParameterType (0) == most_specific_source &&
				    mi.ReturnType == most_specific_target) {
					method = mb;
					count++;
				}
			}
			
			if (method == null || count > 1)
				return null;
			
			
			//
			// This will do the conversion to the best match that we
			// found.  Now we need to perform an implict standard conversion
			// if the best match was not the type that we were requested
			// by target.
			//
			if (look_for_explicit)
				source = WideningAndNarrowingConversionStandard (ec, source, most_specific_source, loc);
			else
				source = WideningConversionStandard (ec, source, most_specific_source, loc);

			if (source == null)
				return null;

			Expression e;
			e =  new UserCast ((MethodInfo) method, source, loc);
			if (e.Type != target){
				if (!look_for_explicit)
					e = WideningConversionStandard (ec, e, target, loc);
				else
					e = WideningAndNarrowingConversionStandard (ec, e, target, loc);
			}

			return e;
		}
		
		/// <summary>
		///   Converts implicitly the resolved expression `expr' into the
		///   `target_type'.  It returns a new expression that can be used
		///   in a context that expects a `target_type'. 
		/// </summary>
		static public Expression WideningConversion (EmitContext ec, Expression expr,
							     Type target_type, Location loc)
		{
			Expression e;

			if (target_type == null)
				throw new Exception ("Target type is null");

			e = WideningConversionStandard (ec, expr, target_type, loc);
			if (e != null)
				return e;

			//
			// VB.NET has no notion of User defined conversions
			//

// 			e = ImplicitUserConversion (ec, expr, target_type, loc);
// 			if (e != null)
// 				return e;

			return null;
		}

		
		/// <summary>
		///   Attempts to apply the `Standard Implicit
		///   Conversion' rules to the expression `expr' into
		///   the `target_type'.  It returns a new expression
		///   that can be used in a context that expects a
		///   `target_type'.
		///
		///   This is different from `WideningConversion' in that the
		///   user defined implicit conversions are excluded. 
		/// </summary>
		static public Expression WideningConversionStandard (EmitContext ec, Expression expr,
								     Type target_type, Location loc)
		{
			Type expr_type = expr.Type;
			Expression e;

			if (expr is NullLiteral) {
				if (target_type.IsGenericParameter)
					return TypeParameter_to_Null (expr, target_type, loc);

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
						return ImplicitDelegateCreation.Create (ec, mg, target_type, loc);
				}
			}

			if (expr_type.Equals (target_type) && !TypeManager.IsNullType (expr_type))
				return expr;

			e = WideningNumericConversion (ec, expr, target_type, loc);
			if (e != null)
				return e;

			e = WideningReferenceConversion (ec, expr, target_type);
			if (e != null)
				return e;
			
			if ((target_type == TypeManager.enum_type ||
			     target_type.IsSubclassOf (TypeManager.enum_type)) &&
			    expr is IntLiteral){
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
					}
				}
				
				if (target_type.IsPointer) {
					if (expr_type == TypeManager.null_type)
						return new EmptyCast (expr, target_type);

					if (expr_type == TypeManager.void_ptr_type)
						return new EmptyCast (expr, target_type);
				}
			}

			if (expr_type == TypeManager.anonymous_method_type){
				if (!TypeManager.IsDelegateType (target_type)){
					Report.Error (1660, loc,
							      "Cannot convert anonymous method to `{0}', since it is not a delegate",
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

			//
			// VB.NET specific conversions
			//

			e = WideningStringConversions (ec, expr, target_type, loc);
			if (e != null)
				return e;

			
			return null;
		}

		/// <summary>
		///   Attempts to perform an implicit constant conversion of the IntConstant
		///   into a different data type using casts (See Implicit Constant
		///   Expression Conversions)
		/// </summary>
		static public Expression TryWideningIntConversion (Type target_type, IntConstant ic)
		{
			int value = ic.Value;

			if (value == 0 && ic is IntLiteral && TypeManager.IsEnumType (target_type)){
				Type underlying = TypeManager.EnumToUnderlying (target_type);
				Constant e = (Constant) ic;
				
				//
				// Possibly, we need to create a different 0 literal before passing
				// to EnumConstant
				//
				if (underlying == TypeManager.int64_type)
					e = new LongLiteral (0);
				else if (underlying == TypeManager.uint64_type)
					e = new ULongLiteral (0);

				return new EnumConstant (e, target_type);
			}

			//
			// If `target_type' is an interface and the type of `ic' implements the interface
			// e.g. target_type is IComparable, IConvertible, IFormattable
			//
			if (target_type.IsInterface && target_type.IsAssignableFrom (ic.Type))
				return new BoxedCast (ic);

			return null;
		}

		/// <summary>
		///   Attempts to perform an implicit constant conversion of the IntConstant
		///   into a different data type using casts (See Implicit Constant
		///   Expression Conversions)
		/// </summary>
		static public Expression WideningConstantConversions (Type target_type, Constant const_expr)
		{
			Constant ret_expr;
			
			Type const_expr_type = const_expr.Type;
			Location loc = const_expr.Location;

			if (target_type == TypeManager.short_type){
				if (const_expr_type == TypeManager.byte_type)
					return const_expr.ToShort (loc);
			}

			if (target_type == TypeManager.int32_type){
				if (const_expr_type == TypeManager.byte_type ||
					const_expr_type == TypeManager.short_type)
					return const_expr.ToInt (loc);
			} 

			if (target_type == TypeManager.int64_type){
				if (const_expr_type == TypeManager.byte_type ||
					const_expr_type == TypeManager.short_type ||
					const_expr_type == TypeManager.int32_type)
					return const_expr.ToLong (loc);
			}

			if (target_type == TypeManager.double_type) {
				if (const_expr_type == TypeManager.float_type)
					return const_expr.ToDouble (loc);
			}
			
			return null;
		}

		static public Constant NothingToPrimitiveConstants (Expression expr, Type target_type)
		{
			NullLiteral null_literal = (NullLiteral) expr;
			Location loc = null_literal.Location;
			Type real_target_type = target_type ;
			Constant retval = null;
			
			if (null_literal == null) 
				throw new Exception ("FIXME: I was expecting that I would always get only NullLiterals");

			if (target_type.IsSubclassOf(TypeManager.enum_type))
				real_target_type = TypeManager.EnumToUnderlying (target_type);

			if (real_target_type == TypeManager.bool_type)
				retval = null_literal.ToBoolean (loc);
			else if (real_target_type == TypeManager.byte_type)
				retval = null_literal.ToByte (loc);
			else if (real_target_type == TypeManager.short_type)
				retval = null_literal.ToShort (loc);
			else if (real_target_type == TypeManager.int32_type)
				retval = null_literal.ToInt (loc);
			else if (real_target_type == TypeManager.int64_type)
				null_literal.ToLong (loc);
			else if (real_target_type == TypeManager.decimal_type)
				retval = null_literal.ToDecimal (loc);
			else if (real_target_type == TypeManager.float_type)
				retval = null_literal.ToLong (loc);
			else if (real_target_type == TypeManager.double_type)
				retval = null_literal.ToDouble (loc);
			else if (real_target_type == TypeManager.char_type)
				retval = null_literal.ToChar (loc);
			else if (real_target_type == TypeManager.string_type)
				retval = new StringConstant (null);
			
			if (real_target_type != target_type && retval != null)
				retval = new EnumConstant (retval, target_type);

			return retval;
		}

		static public void Error_CannotWideningConversion (Location loc, Type source, Type target)
		{
			if (source.Name == target.Name){
				Report.ExtraInformation (loc,
					 String.Format (
						"The type {0} has two conflicting definitons, one comes from {0} and the other from {1}",
						source.Assembly.FullName, target.Assembly.FullName));
							 
			}
			Report.Error (29, loc, "Cannot convert implicitly from {0} to `{1}'",
				      source == TypeManager.anonymous_method_type ?
				      "anonymous method" : "`" + TypeManager.CSharpName (source) + "'",
				      TypeManager.CSharpName (target));
		}

		/// <summary>
		///   Attempts to implicitly convert `source' into `target_type', using
		///   WideningConversion.  If there is no implicit conversion, then
		///   an error is signaled
		/// </summary>
		static public Expression WideningConversionRequired (EmitContext ec, Expression source,
								     Type target_type, Location loc)
		{
			Expression e;

			int errors = Report.Errors;
			e = WideningConversion (ec, source, target_type, loc);
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

			if (source is Constant){
				Constant c = (Constant) source;

				Expression.Error_ConstantValueCannotBeConverted (loc, c.AsString (), target_type);
				return null;
			}
			
			Error_CannotWideningConversion (loc, source.Type, target_type);

			return null;
		}

		static void Error_664 (Location loc, string type, string suffix) {
			Report.Error (664, loc,
				"Literal of type double cannot be implicitly converted to type '{0}'. Add suffix '{1}' to create a literal of this type",
				type, suffix);
		}

		/// <summary>
		///   Performs the explicit numeric conversions
		/// </summary>

		/// <summary>
		///   Performs the explicit numeric conversions
		/// </summary>
		static Expression NarrowingNumericConversion (EmitContext ec, Expression expr, Type target_type, Location loc)
		{
			Type expr_type = expr.Type;

			//
			// If we have an enumeration, extract the underlying type,
			// use this during the comparison, but wrap around the original
			// target_type
			//
			Type real_target_type = target_type;

			if (TypeManager.IsEnumType (real_target_type))
				real_target_type = TypeManager.EnumToUnderlying (real_target_type);

			if (WideningStandardConversionExists (ec, expr, real_target_type)){
 				Expression ce = WideningConversionStandard (ec, expr, real_target_type, loc);

				if (real_target_type != target_type)
					return new EmptyCast (ce, target_type);
				return ce;
			}
			
			if (expr_type == TypeManager.sbyte_type){
				//
				// From sbyte to byte, ushort, uint, ulong, char
				//
				if (real_target_type == TypeManager.byte_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I1_U1);
				if (real_target_type == TypeManager.ushort_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I1_U2);
				if (real_target_type == TypeManager.uint32_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I1_U4);
				if (real_target_type == TypeManager.uint64_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I1_U8);
				if (real_target_type == TypeManager.char_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I1_CH);
			} else if (expr_type == TypeManager.byte_type){
				//
				// From byte to sbyte and char
				//
				if (real_target_type == TypeManager.sbyte_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.U1_I1);
			} else if (expr_type == TypeManager.short_type){
				//
				// From short to byte
				//
				if (real_target_type == TypeManager.sbyte_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I2_I1);
				if (real_target_type == TypeManager.byte_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I2_U1);
				if (real_target_type == TypeManager.ushort_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I2_U2);
				if (real_target_type == TypeManager.uint32_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I2_U4);
				if (real_target_type == TypeManager.uint64_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I2_U8);
			} else if (expr_type == TypeManager.ushort_type){
				//
				// From ushort to sbyte, byte, short, char
				//
				if (real_target_type == TypeManager.sbyte_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.U2_I1);
				if (real_target_type == TypeManager.byte_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.U2_U1);
				if (real_target_type == TypeManager.short_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.U2_I2);
				if (real_target_type == TypeManager.char_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.U2_CH);
			} else if (expr_type == TypeManager.int32_type){
				//
				// From int to byte, short
				//
				if (real_target_type == TypeManager.sbyte_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I4_I1);
				if (real_target_type == TypeManager.byte_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I4_U1);
				if (real_target_type == TypeManager.short_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I4_I2);
				if (real_target_type == TypeManager.ushort_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I4_U2);
				if (real_target_type == TypeManager.uint32_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I4_U4);
				if (real_target_type == TypeManager.uint64_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I4_U8);
			} else if (expr_type == TypeManager.uint32_type){
				//
				// From uint to sbyte, byte, short, ushort, int, char
				//
				if (real_target_type == TypeManager.sbyte_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.U4_I1);
				if (real_target_type == TypeManager.byte_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.U4_U1);
				if (real_target_type == TypeManager.short_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.U4_I2);
				if (real_target_type == TypeManager.ushort_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.U4_U2);
				if (real_target_type == TypeManager.int32_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.U4_I4);
				if (real_target_type == TypeManager.char_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.U4_CH);
			} else if (expr_type == TypeManager.int64_type){
				//
				// From long to byte, short, int
				//
				if (real_target_type == TypeManager.sbyte_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I8_I1);
				if (real_target_type == TypeManager.byte_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I8_U1);
				if (real_target_type == TypeManager.short_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I8_I2);
				if (real_target_type == TypeManager.ushort_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I8_U2);
				if (real_target_type == TypeManager.int32_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I8_I4);
				if (real_target_type == TypeManager.uint32_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I8_U4);
				if (real_target_type == TypeManager.uint64_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.I8_U8);
			} else if (expr_type == TypeManager.uint64_type){
				//
				// From ulong to sbyte, byte, short, ushort, int, uint, long, char
				//
				if (real_target_type == TypeManager.sbyte_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.U8_I1);
				if (real_target_type == TypeManager.byte_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.U8_U1);
				if (real_target_type == TypeManager.short_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.U8_I2);
				if (real_target_type == TypeManager.ushort_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.U8_U2);
				if (real_target_type == TypeManager.int32_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.U8_I4);
				if (real_target_type == TypeManager.uint32_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.U8_U4);
				if (real_target_type == TypeManager.int64_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.U8_I8);
				if (real_target_type == TypeManager.char_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.U8_CH);
			} else if (expr_type == TypeManager.float_type){
				//
				// From float to byte, short, int, long, decimal
				//
				if (real_target_type == TypeManager.sbyte_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.R4_I1);
				if (real_target_type == TypeManager.byte_type)
					return new FloatingToFixedCast (ec, expr, target_type, ConvCast.Mode.R8_U1);
				if (real_target_type == TypeManager.short_type)
					return new FloatingToFixedCast (ec, expr, target_type, ConvCast.Mode.R8_I2);
				if (real_target_type == TypeManager.ushort_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.R4_U2);
				if (real_target_type == TypeManager.int32_type)
					return new FloatingToFixedCast (ec, expr, target_type, ConvCast.Mode.R8_I4);
				if (real_target_type == TypeManager.uint32_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.R4_U4);
				if (real_target_type == TypeManager.int64_type)
					return new FloatingToFixedCast (ec, expr, target_type, ConvCast.Mode.R8_I8);
				if (real_target_type == TypeManager.uint64_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.R4_U8);
				if (real_target_type == TypeManager.decimal_type)
					return new ImplicitNew (ec, "System", "Decimal", loc, expr);	
			} else if (expr_type == TypeManager.double_type){
				//
				// From double to byte, short, int, long, float, decimal
				//
				if (real_target_type == TypeManager.sbyte_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.R8_I1);
				if (real_target_type == TypeManager.byte_type)
					return new FloatingToFixedCast (ec, expr, target_type, ConvCast.Mode.R8_U1);
				if (real_target_type == TypeManager.short_type)
					return new FloatingToFixedCast (ec, expr, target_type, ConvCast.Mode.R8_I2);
				if (real_target_type == TypeManager.ushort_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.R8_U2);
				if (real_target_type == TypeManager.int32_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.R8_I4);
				if (real_target_type == TypeManager.uint32_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.R8_U4);
				if (real_target_type == TypeManager.int64_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.R8_I8);
				if (real_target_type == TypeManager.uint64_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.R8_U8);

				if (real_target_type == TypeManager.float_type)
					return new ConvCast (ec, expr, target_type, ConvCast.Mode.R8_R4);
				if (real_target_type == TypeManager.decimal_type)
					return new ImplicitNew (ec, "System", "Decimal", loc, expr);
			} else if (expr_type == TypeManager.decimal_type){
				//
				// From decimal to byte, short, int, long
				//
				if (real_target_type == TypeManager.byte_type)
					return new ImplicitInvocation (ec, "System", "Convert" , "ToByte", loc, expr);	
				if (real_target_type == TypeManager.short_type)
					return new ImplicitInvocation (ec, "System", "Convert", "ToInt16", loc, expr);	
				if (real_target_type == TypeManager.int32_type)
					return new ImplicitInvocation (ec, "System", "Convert", "ToInt32", loc, expr);	
				if (real_target_type == TypeManager.int64_type)
					return new ImplicitInvocation (ec, "System", "Convert", "ToInt64", loc, expr);	
			} 

			return null;
		}

		/// <summary> 
		/// VB.NET specific: Convert to and from boolean
		/// </summary>

		static public Expression BooleanConversions (EmitContext ec, Expression expr,
								    Type target_type, Location loc)
		{
			Type expr_type = expr.Type;
 			Type real_target_type = target_type;

			if (expr_type == TypeManager.bool_type) {

				//
				// From boolean to byte, short, int,
				// long, float, double, decimal
				//

				if (real_target_type == TypeManager.byte_type)
					return new BooleanToNumericCast (expr, target_type, OpCodes.Conv_U1);
				if (real_target_type == TypeManager.short_type)
					return new BooleanToNumericCast (expr, target_type, OpCodes.Conv_I2);
				if (real_target_type == TypeManager.int32_type)
					return new BooleanToNumericCast (expr, target_type, OpCodes.Conv_I4);
				if (real_target_type == TypeManager.int64_type)
					return new BooleanToNumericCast (expr, target_type, OpCodes.Conv_I8);
				if (real_target_type == TypeManager.float_type)
					return new BooleanToNumericCast (expr, target_type, OpCodes.Conv_R4);
				if (real_target_type == TypeManager.double_type)
					return new BooleanToNumericCast (expr, target_type, OpCodes.Conv_R8);
				if (real_target_type == TypeManager.decimal_type) {
					return new ImplicitInvocation (ec, "DecimalType", "FromBoolean", loc, expr);
				}
			} if (real_target_type == TypeManager.bool_type) {

				//
				// From byte, short, int, long, float,
				// double, decimal to boolean
				//

				if (expr_type == TypeManager.byte_type ||
					expr_type == TypeManager.short_type ||
					expr_type == TypeManager.int32_type ||
					expr_type == TypeManager.int64_type || 
					expr_type == TypeManager.float_type || 
					expr_type == TypeManager.double_type)
						return new NumericToBooleanCast (expr, expr_type);
				if (expr_type == TypeManager.decimal_type) {
					return new ImplicitInvocation (ec, "System", "Convert", "ToBoolean", loc, expr);
				}
			}

			return null;
		}

		/// <summary> 
		/// VB.NET specific: Widening conversions to string
		/// </summary>

		static public Expression WideningStringConversions (EmitContext ec, Expression expr,
								    Type target_type, Location loc)

		{
			Type expr_type = expr.Type;
 			Type real_target_type = target_type;

			if (real_target_type == TypeManager.string_type) {
				//
				// From char to string
				//
				if (expr_type == TypeManager.char_type)
					return new ImplicitInvocation (ec, "StringType", "FromChar", loc, expr);
			}

			if(expr_type.IsArray && (expr_type.GetElementType() == TypeManager.char_type)) {
				//
				// From char array to string
				//
				return new ImplicitNew (ec, "System", "String", loc, expr);
			}

			return null;
		}
		
		/// <summary> 
		/// VB.NET specific: Narrowing conversions involving strings
		/// </summary>

		static public Expression NarrowingStringConversions (EmitContext ec, Expression expr,
								    Type target_type, Location loc)
		{
			Type expr_type = expr.Type;
 			Type real_target_type = target_type;

			// FIXME: Need to take care of Constants

			if (expr_type == TypeManager.string_type) {

				//
				// From string to chararray, bool,
				// byte, short, char, int, long,
				// float, double, decimal and date 
				//

				if (real_target_type.IsArray && (real_target_type.GetElementType() == TypeManager.char_type))
					return new ImplicitInvocation (ec, "CharArrayType", "FromString", loc, expr);
				if (real_target_type == TypeManager.bool_type)
					return new ImplicitInvocation (ec, "BooleanType", "FromString", loc, expr);
				if (real_target_type == TypeManager.byte_type)
					return new ImplicitInvocation (ec, "ByteType", "FromString", loc, expr);
				if (real_target_type == TypeManager.short_type)
					return new ImplicitInvocation (ec, "ShortType", "FromString", loc, expr);
				if (real_target_type == TypeManager.char_type)
					return new ImplicitInvocation (ec, "CharType", "FromString", loc, expr);
				if (real_target_type == TypeManager.int32_type)
					return new ImplicitInvocation (ec, "IntegerType", "FromString", loc, expr);
				if (real_target_type == TypeManager.int64_type)
					return new ImplicitInvocation (ec, "LongType", "FromString", loc, expr);
				if (real_target_type == TypeManager.float_type)
					return new ImplicitInvocation (ec, "SingleType", "FromString", loc, expr);
				if (real_target_type == TypeManager.double_type)
					return new ImplicitInvocation (ec, "DoubleType", "FromString", loc, expr);
				if (real_target_type == TypeManager.decimal_type)
					return new ImplicitInvocation (ec, "DecimalType", "FromString", loc, expr);
				if (real_target_type == TypeManager.date_type)
					return new ImplicitInvocation (ec, "DateType", "FromString", loc, expr);
			} if (real_target_type == TypeManager.string_type) {

				//
				// From bool, byte, short, char, int,
				// long, float, double, decimal and
				// date to string
				//

				if (expr_type == TypeManager.bool_type)
					return new ImplicitInvocation (ec, "StringType", "FromBoolean", loc, expr);
				if (expr_type == TypeManager.byte_type)
					return new ImplicitInvocation (ec, "StringType", "FromByte", loc, expr);
				if (expr_type == TypeManager.short_type)
					return new ImplicitInvocation (ec, "StringType", "FromShort", loc, expr);
				if (expr_type == TypeManager.int32_type)
					return new ImplicitInvocation (ec, "StringType", "FromInteger", loc, expr);
				if (expr_type == TypeManager.int64_type)
					return new ImplicitInvocation (ec, "StringType", "FromLong", loc, expr);
				if (expr_type == TypeManager.float_type)
					return new ImplicitInvocation (ec, "StringType", "FromSingle", loc, expr);
				if (expr_type == TypeManager.double_type)
					return new ImplicitInvocation (ec, "StringType", "FromDouble", loc, expr);
				if (expr_type == TypeManager.decimal_type)
					return new ImplicitInvocation (ec, "StringType", "FromDecimal", loc, expr);
				if (expr_type == TypeManager.date_type)
					return new ImplicitInvocation (ec, "StringType", "FromDate", loc, expr);
			}

			return null;
		}

		/// <summary>
		///  Returns whether an explicit reference conversion can be performed
		///  from source_type to target_type
		/// </summary>
		public static bool NarrowingReferenceConversionExists (Type source_type, Type target_type)
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
						if (NarrowingReferenceConversionExists (source_element_type,
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
		static Expression NarrowingReferenceConversion (Expression source, Type target_type)
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
						if (NarrowingReferenceConversionExists (source_element_type,
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
		static public Expression WideningAndNarrowingConversion (EmitContext ec, Expression expr,
							     Type target_type, Location loc)
		{
			Type expr_type = expr.Type;
			Type original_expr_type = expr_type;

			if (expr_type.IsSubclassOf (TypeManager.enum_type)){
				if (target_type == TypeManager.enum_type ||
				    target_type == TypeManager.object_type) {
					if (expr is EnumConstant)
						expr = ((EnumConstant) expr).Child;
					// We really need all these casts here .... :-(
					expr = new BoxedCast (new EmptyCast (expr, expr_type));
					return new EmptyCast (expr, target_type);
				} else if ((expr_type == TypeManager.enum_type) && target_type.IsValueType &&
					   target_type.IsSubclassOf (TypeManager.enum_type))
					return new UnboxCast (expr, target_type);

				//
				// Notice that we have kept the expr_type unmodified, which is only
				// used later on to 
				if (expr is EnumConstant)
					expr = ((EnumConstant) expr).Child;
				else
					expr = new EmptyCast (expr, TypeManager.EnumToUnderlying (expr_type));
				expr_type = expr.Type;
			}

			int errors = Report.Errors;
			Expression ne = WideningConversionStandard (ec, expr, target_type, loc);
			if (Report.Errors > errors)
				return null;

			if (ne != null)
				return ne;

			if (TypeManager.IsNullableType (expr.Type) && TypeManager.IsNullableType (target_type))
				return new Nullable.LiftedConversion (
					expr, target_type, false, true, loc).Resolve (ec);

			ne = NarrowingNumericConversion (ec, expr, target_type, loc);
			if (ne != null)
				return ne;

			//
			// Unboxing conversion.
			//
			if (expr_type == TypeManager.object_type && target_type.IsValueType)
				return new UnboxCast (expr, target_type);

			//
			// Skip the NarrowingReferenceConversion because we can not convert
			// from Null to a ValueType, and ExplicitReference wont check against
			// null literal explicitly
			//
			if (expr_type != TypeManager.null_type){
				ne = NarrowingReferenceConversion (expr, target_type);
				if (ne != null)
					return ne;
			}

		skip_explicit:
			if (ec.InUnsafe){
				if (target_type.IsPointer){
					if (expr_type.IsPointer)
						return new EmptyCast (expr, target_type);
					
					if (expr_type == TypeManager.sbyte_type ||
					    expr_type == TypeManager.byte_type ||
					    expr_type == TypeManager.short_type ||
					    expr_type == TypeManager.ushort_type ||
					    expr_type == TypeManager.int32_type ||
					    expr_type == TypeManager.uint32_type ||
					    expr_type == TypeManager.uint64_type ||
					    expr_type == TypeManager.int64_type)
						return new OpcodeCast (expr, target_type, OpCodes.Conv_U);
				}
				if (expr_type.IsPointer){
					Expression e = null;
					
					if (target_type == TypeManager.sbyte_type)
						e = new OpcodeCast (expr, target_type, OpCodes.Conv_I1);
					else if (target_type == TypeManager.byte_type)
						e = new OpcodeCast (expr, target_type, OpCodes.Conv_U1);
					else if (target_type == TypeManager.short_type)
						e = new OpcodeCast (expr, target_type, OpCodes.Conv_I2);
					else if (target_type == TypeManager.ushort_type)
						e = new OpcodeCast (expr, target_type, OpCodes.Conv_U2);
					else if (target_type == TypeManager.int32_type)
						e = new OpcodeCast (expr, target_type, OpCodes.Conv_I4);
					else if (target_type == TypeManager.uint32_type)
						e = new OpcodeCast (expr, target_type, OpCodes.Conv_U4);
					else if (target_type == TypeManager.uint64_type)
						e = new OpcodeCast (expr, target_type, OpCodes.Conv_U8);
					else if (target_type == TypeManager.int64_type){
						e = new OpcodeCast (expr, target_type, OpCodes.Conv_I8);
					}

					if (e != null){
						Expression ci, ce;

						ci = WideningConversionStandard (ec, e, target_type, loc);

						if (ci != null)
							return ci;

						ce = NarrowingNumericConversion (ec, e, target_type, loc);
						if (ce != null)
							return ce;
						//
						// We should always be able to go from an uint32
						// implicitly or explicitly to the other integral
						// types
						//
						throw new Exception ("Internal compiler error");
					}
				}
			}

			//
			// VB.NET specific conversions
			//

			ne = BooleanConversions (ec, expr, target_type, loc);
			if (ne != null)
				return ne;

			ne = NarrowingStringConversions (ec, expr, target_type, loc);
			if (ne != null)
				return ne;
			
			
			//
			// VB.NET has no notion of User defined conversions
			//

// 			ne = ExplicitUserConversion (ec, expr, target_type, loc);
// 			if (ne != null)
// 				return ne;

			if (expr is NullLiteral){
				Report.Error (37, loc, "Cannot convert null to value type `" +
					      TypeManager.CSharpName (target_type) + "'");
				return null;
			}
				
			Error_CannotConvertType (loc, original_expr_type, target_type);
			return null;
		}

		/// <summary>
		///   Same as WideningAndNarrowingConversion, only it doesn't include user defined conversions
		/// </summary>
		static public Expression WideningAndNarrowingConversionStandard (EmitContext ec, Expression expr,
								     Type target_type, Location l)
		{
			int errors = Report.Errors;
			Expression ne = WideningConversionStandard (ec, expr, target_type, l);
			if (Report.Errors > errors)
				return null;

			if (ne != null)
				return ne;

			if (TypeManager.IsNullableType (expr.Type) && TypeManager.IsNullableType (target_type))
				return new Nullable.LiftedConversion (
					expr, target_type, false, true, l).Resolve (ec);

			ne = NarrowingNumericConversion (ec, expr, target_type, l);
			if (ne != null)
				return ne;

			ne = NarrowingReferenceConversion (expr, target_type);
			if (ne != null)
				return ne;

			Error_CannotConvertType (l, expr.Type, target_type);
			return null;
		}

		/// <summary>
		///   Entry point for VB.NET specific implicit conversions
		/// </summary>
		static public Expression ImplicitVBConversion (EmitContext ec, Expression expr,
							     Type target_type, Location loc)
		{
			if (RootContext.StricterTypeChecking)
				return WideningConversion (ec, expr, target_type, loc);
			else
				return WideningAndNarrowingConversion(ec, expr, target_type, loc);
		}

		/// <summary>
		///   Mandates VB.NET specific implicit conversions
		/// </summary>
		static public Expression ImplicitVBConversionRequired (EmitContext ec, Expression source,
								     Type target_type, Location loc)
		{
			Expression e;


			int errors = Report.Errors;
			e = ImplicitVBConversion (ec, source, target_type, loc);
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

			if (source is Constant){
				Constant c = (Constant) source;

				Expression.Error_ConstantValueCannotBeConverted (loc, c.AsString (), target_type);
				return null;
			}
			
			Error_CannotWideningConversion (loc, source.Type, target_type);

			return null;
		}

		/// <summary>
		///   Entry point for VB.NET specific explicit conversions
		/// </summary>
		static public Expression ExplicitVBConversion (EmitContext ec, Expression expr,
							     Type target_type, Location loc)
		{
				return WideningAndNarrowingConversion(ec, expr, target_type, loc);
		}

	}
}
