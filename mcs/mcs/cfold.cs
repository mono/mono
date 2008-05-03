//
// cfold.cs: Constant Folding
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Marek Safar (marek.safar@seznam.cz)
//
// Copyright 2002, 2003 Ximian, Inc.
// Copyright 2003-2008, Novell, Inc.
// 
using System;

namespace Mono.CSharp {

	public class ConstantFold {

		public static readonly Type[] binary_promotions = new Type[] { 
			TypeManager.decimal_type, TypeManager.double_type, TypeManager.float_type,
			TypeManager.uint64_type, TypeManager.int64_type, TypeManager.uint32_type };

		//
		// Performs the numeric promotions on the left and right expresions
		// and desposits the results on `lc' and `rc'.
		//
		// On success, the types of `lc' and `rc' on output will always match,
		// and the pair will be one of:
		//
		static bool DoBinaryNumericPromotions (ref Constant left, ref Constant right)
		{
			Type ltype = left.Type;
			Type rtype = right.Type;

			foreach (Type t in binary_promotions) {
				if (t == ltype)
					return t == rtype || ConvertPromotion (ref right, ref left, t);

				if (t == rtype)
					return t == ltype || ConvertPromotion (ref left, ref right, t);
			}

			left = left.ConvertImplicitly (TypeManager.int32_type);
			right = right.ConvertImplicitly (TypeManager.int32_type);
			return left != null && right != null;
		}

		static bool ConvertPromotion (ref Constant prim, ref Constant second, Type type)
		{
			Constant c = prim.ConvertImplicitly (type);
			if (c != null) {
				prim = c;
				return true;
			}

			if (type == TypeManager.uint32_type) {
				type = TypeManager.int64_type;
				prim = prim.ConvertImplicitly (type);
				second = second.ConvertImplicitly (type);
				return prim != null && second != null;
			}

			return false;
		}

		internal static void Error_CompileTimeOverflow (Location loc)
		{
			Report.Error (220, loc, "The operation overflows at compile time in checked mode");
		}
		
		/// <summary>
		///   Constant expression folder for binary operations.
		///
		///   Returns null if the expression can not be folded.
		/// </summary>
		static public Constant BinaryFold (EmitContext ec, Binary.Operator oper,
						     Constant left, Constant right, Location loc)
		{
			Constant result = null;

			if (left is EmptyConstantCast)
				return BinaryFold (ec, oper, ((EmptyConstantCast)left).child, right, loc);

			if (left is SideEffectConstant) {
				result = BinaryFold (ec, oper, ((SideEffectConstant) left).value, right, loc);
				if (result == null)
					return null;
				return new SideEffectConstant (result, left, loc);
			}

			if (right is EmptyConstantCast)
				return BinaryFold (ec, oper, left, ((EmptyConstantCast)right).child, loc);

			if (right is SideEffectConstant) {
				result = BinaryFold (ec, oper, left, ((SideEffectConstant) right).value, loc);
				if (result == null)
					return null;
				return new SideEffectConstant (result, right, loc);
			}

			Type lt = left.Type;
			Type rt = right.Type;
			bool bool_res;

			if (lt == TypeManager.bool_type && lt == rt) {
				bool lv = ((BoolConstant) left ).Value;
				bool rv = ((BoolConstant) right).Value;
				switch (oper) {
				case Binary.Operator.BitwiseAnd:
				case Binary.Operator.LogicalAnd:
					return new BoolConstant (lv && rv, left.Location);
				case Binary.Operator.BitwiseOr:
				case Binary.Operator.LogicalOr:
					return new BoolConstant (lv || rv, left.Location);
				case Binary.Operator.ExclusiveOr:
					return new BoolConstant (lv ^ rv, left.Location);
				case Binary.Operator.Equality:
					return new BoolConstant (lv == rv, left.Location);
				case Binary.Operator.Inequality:
					return new BoolConstant (lv != rv, left.Location);
				}
				return null;
			}

			//
			// During an enum evaluation, none of the rules are valid
			// Not sure whether it is bug in csc or in documentation
			//
			if (ec.InEnumContext){
				if (left is EnumConstant)
					left = ((EnumConstant) left).Child;
				
				if (right is EnumConstant)
					right = ((EnumConstant) right).Child;
			} else if (left is EnumConstant && rt == lt) {
				switch (oper){
					///
					/// E operator |(E x, E y);
					/// E operator &(E x, E y);
					/// E operator ^(E x, E y);
					/// 
					case Binary.Operator.BitwiseOr:
					case Binary.Operator.BitwiseAnd:
					case Binary.Operator.ExclusiveOr:
						return BinaryFold (ec, oper, ((EnumConstant)left).Child,
								((EnumConstant)right).Child, loc).TryReduce (ec, lt, loc);

					///
					/// U operator -(E x, E y);
					/// 
					case Binary.Operator.Subtraction:
						result = BinaryFold (ec, oper, ((EnumConstant)left).Child, ((EnumConstant)right).Child, loc);
						return result.TryReduce (ec, ((EnumConstant)left).Child.Type, loc);

					///
					/// bool operator ==(E x, E y);
					/// bool operator !=(E x, E y);
					/// bool operator <(E x, E y);
					/// bool operator >(E x, E y);
					/// bool operator <=(E x, E y);
					/// bool operator >=(E x, E y);
					/// 
					case Binary.Operator.Equality:				
					case Binary.Operator.Inequality:
					case Binary.Operator.LessThan:				
					case Binary.Operator.GreaterThan:
					case Binary.Operator.LessThanOrEqual:				
					case Binary.Operator.GreaterThanOrEqual:
						return BinaryFold(ec, oper, ((EnumConstant)left).Child, ((EnumConstant)right).Child, loc);
				}
				return null;
			}

			switch (oper){
			case Binary.Operator.BitwiseOr:
				if (!DoBinaryNumericPromotions (ref left, ref right))
					return null;

				if (left is IntConstant){
					int res = ((IntConstant) left).Value | ((IntConstant) right).Value;
					
					return new IntConstant (res, left.Location);
				}
				if (left is UIntConstant){
					uint res = ((UIntConstant)left).Value | ((UIntConstant)right).Value;
					
					return new UIntConstant (res, left.Location);
				}
				if (left is LongConstant){
					long res = ((LongConstant)left).Value | ((LongConstant)right).Value;
					
					return new LongConstant (res, left.Location);
				}
				if (left is ULongConstant){
					ulong res = ((ULongConstant)left).Value |
						((ULongConstant)right).Value;
					
					return new ULongConstant (res, left.Location);
				}
				break;
				
			case Binary.Operator.BitwiseAnd:
				if (!DoBinaryNumericPromotions (ref left, ref right))
					return null;
				
				///
				/// int operator &(int x, int y);
				/// uint operator &(uint x, uint y);
				/// long operator &(long x, long y);
				/// ulong operator &(ulong x, ulong y);
				///
				if (left is IntConstant){
					int res = ((IntConstant) left).Value & ((IntConstant) right).Value;
					return new IntConstant (res, left.Location);
				}
				if (left is UIntConstant){
					uint res = ((UIntConstant)left).Value & ((UIntConstant)right).Value;
					return new UIntConstant (res, left.Location);
				}
				if (left is LongConstant){
					long res = ((LongConstant)left).Value & ((LongConstant)right).Value;
					return new LongConstant (res, left.Location);
				}
				if (left is ULongConstant){
					ulong res = ((ULongConstant)left).Value &
						((ULongConstant)right).Value;
					
					return new ULongConstant (res, left.Location);
				}
				break;

			case Binary.Operator.ExclusiveOr:
				if (!DoBinaryNumericPromotions (ref left, ref right))
					return null;
				
				if (left is IntConstant){
					int res = ((IntConstant) left).Value ^ ((IntConstant) right).Value;
					return new IntConstant (res, left.Location);
				}
				if (left is UIntConstant){
					uint res = ((UIntConstant)left).Value ^ ((UIntConstant)right).Value;
					
					return  new UIntConstant (res, left.Location);
				}
				if (left is LongConstant){
					long res = ((LongConstant)left).Value ^ ((LongConstant)right).Value;
					
					return new LongConstant (res, left.Location);
				}
				if (left is ULongConstant){
					ulong res = ((ULongConstant)left).Value ^
						((ULongConstant)right).Value;
					
					return new ULongConstant (res, left.Location);
				}
				break;

			case Binary.Operator.Addition:
				//
				// If both sides are strings, then concatenate, if
				// one is a string, and the other is not, then defer
				// to runtime concatenation
				//
				if (lt == TypeManager.string_type || rt == TypeManager.string_type){
					if (lt == TypeManager.string_type && rt == TypeManager.string_type)
						return new StringConstant (
							((StringConstant) left).Value +
							((StringConstant) right).Value, left.Location);
					
					return null;
				}

				if (lt == TypeManager.null_type && lt == rt)
					return left;

				//
				// handle "E operator + (E x, U y)"
				// handle "E operator + (Y y, E x)"
				//
				EnumConstant lc = left as EnumConstant;
				EnumConstant rc = right as EnumConstant;
				if (lc != null || rc != null){
					if (lc == null) {
						lc = rc;
						lt = lc.Type;
						right = left;
					}

					// U has to be implicitly convetible to E.base
					right = right.ConvertImplicitly (lc.Child.Type);
					if (right == null)
						return null;

					result = BinaryFold (ec, oper, lc.Child, right, loc);
					if (result == null)
						return null;

					result = result.TryReduce (ec, lt, loc);
					if (result == null)
						return null;

					return new EnumConstant (result, lt);
				}

				if (!DoBinaryNumericPromotions (ref left, ref right))
					return null;

				try {
					if (left is DoubleConstant){
						double res;
						
						if (ec.ConstantCheckState)
							res = checked (((DoubleConstant) left).Value +
								       ((DoubleConstant) right).Value);
						else
							res = unchecked (((DoubleConstant) left).Value +
									 ((DoubleConstant) right).Value);
						
						return new DoubleConstant (res, left.Location);
					}
					if (left is FloatConstant){
						float res;
						
						if (ec.ConstantCheckState)
							res = checked (((FloatConstant) left).Value +
								       ((FloatConstant) right).Value);
						else
							res = unchecked (((FloatConstant) left).Value +
									 ((FloatConstant) right).Value);
						
						result = new FloatConstant (res, left.Location);
					} else if (left is ULongConstant){
						ulong res;
						
						if (ec.ConstantCheckState)
							res = checked (((ULongConstant) left).Value +
								       ((ULongConstant) right).Value);
						else
							res = unchecked (((ULongConstant) left).Value +
									 ((ULongConstant) right).Value);

						result = new ULongConstant (res, left.Location);
					} else if (left is LongConstant){
						long res;
						
						if (ec.ConstantCheckState)
							res = checked (((LongConstant) left).Value +
								       ((LongConstant) right).Value);
						else
							res = unchecked (((LongConstant) left).Value +
									 ((LongConstant) right).Value);
						
						result = new LongConstant (res, left.Location);
					} else if (left is UIntConstant){
						uint res;
						
						if (ec.ConstantCheckState)
							res = checked (((UIntConstant) left).Value +
								       ((UIntConstant) right).Value);
						else
							res = unchecked (((UIntConstant) left).Value +
									 ((UIntConstant) right).Value);
						
						result = new UIntConstant (res, left.Location);
					} else if (left is IntConstant){
						int res;

						if (ec.ConstantCheckState)
							res = checked (((IntConstant) left).Value +
								       ((IntConstant) right).Value);
						else
							res = unchecked (((IntConstant) left).Value +
									 ((IntConstant) right).Value);

						result = new IntConstant (res, left.Location);
					} else if (left is DecimalConstant) {
						decimal res;

						if (ec.ConstantCheckState)
							res = checked (((DecimalConstant) left).Value +
								((DecimalConstant) right).Value);
						else
							res = unchecked (((DecimalConstant) left).Value +
								((DecimalConstant) right).Value);

						result = new DecimalConstant (res, left.Location);
					} else {
						throw new Exception ( "Unexepected addition input: " + left);
					}
				} catch (OverflowException){
					Error_CompileTimeOverflow (loc);
				}

				return result;

			case Binary.Operator.Subtraction:
				//
				// handle "E operator - (E x, U y)"
				// handle "E operator - (Y y, E x)"
				//
				lc = left as EnumConstant;
				rc = right as EnumConstant;
				if (lc != null || rc != null){
					if (lc == null) {
						lc = rc;
						lt = lc.Type;
						right = left;
					}

					// U has to be implicitly convetible to E.base
					right = right.ConvertImplicitly (lc.Child.Type);
					if (right == null)
						return null;

					result = BinaryFold (ec, oper, lc.Child, right, loc);
					if (result == null)
						return null;

					result = result.TryReduce (ec, lt, loc);
					if (result == null)
						return null;

					return new EnumConstant (result, lt);
				}

				if (!DoBinaryNumericPromotions (ref left, ref right))
					return null;

				try {
					if (left is DoubleConstant){
						double res;
						
						if (ec.ConstantCheckState)
							res = checked (((DoubleConstant) left).Value -
								       ((DoubleConstant) right).Value);
						else
							res = unchecked (((DoubleConstant) left).Value -
									 ((DoubleConstant) right).Value);
						
						result = new DoubleConstant (res, left.Location);
					} else if (left is FloatConstant){
						float res;
						
						if (ec.ConstantCheckState)
							res = checked (((FloatConstant) left).Value -
								       ((FloatConstant) right).Value);
						else
							res = unchecked (((FloatConstant) left).Value -
									 ((FloatConstant) right).Value);
						
						result = new FloatConstant (res, left.Location);
					} else if (left is ULongConstant){
						ulong res;
						
						if (ec.ConstantCheckState)
							res = checked (((ULongConstant) left).Value -
								       ((ULongConstant) right).Value);
						else
							res = unchecked (((ULongConstant) left).Value -
									 ((ULongConstant) right).Value);
						
						result = new ULongConstant (res, left.Location);
					} else if (left is LongConstant){
						long res;
						
						if (ec.ConstantCheckState)
							res = checked (((LongConstant) left).Value -
								       ((LongConstant) right).Value);
						else
							res = unchecked (((LongConstant) left).Value -
									 ((LongConstant) right).Value);
						
						result = new LongConstant (res, left.Location);
					} else if (left is UIntConstant){
						uint res;
						
						if (ec.ConstantCheckState)
							res = checked (((UIntConstant) left).Value -
								       ((UIntConstant) right).Value);
						else
							res = unchecked (((UIntConstant) left).Value -
									 ((UIntConstant) right).Value);
						
						result = new UIntConstant (res, left.Location);
					} else if (left is IntConstant){
						int res;

						if (ec.ConstantCheckState)
							res = checked (((IntConstant) left).Value -
								       ((IntConstant) right).Value);
						else
							res = unchecked (((IntConstant) left).Value -
									 ((IntConstant) right).Value);

						result = new IntConstant (res, left.Location);
					} else if (left is DecimalConstant) {
						decimal res;

						if (ec.ConstantCheckState)
							res = checked (((DecimalConstant) left).Value -
								((DecimalConstant) right).Value);
						else
							res = unchecked (((DecimalConstant) left).Value -
								((DecimalConstant) right).Value);

						return new DecimalConstant (res, left.Location);
					} else {
						throw new Exception ( "Unexepected subtraction input: " + left);
					}
				} catch (OverflowException){
					Error_CompileTimeOverflow (loc);
				}

				return result;
				
			case Binary.Operator.Multiply:
				if (!DoBinaryNumericPromotions (ref left, ref right))
					return null;

				try {
					if (left is DoubleConstant){
						double res;
						
						if (ec.ConstantCheckState)
							res = checked (((DoubleConstant) left).Value *
								((DoubleConstant) right).Value);
						else
							res = unchecked (((DoubleConstant) left).Value *
								((DoubleConstant) right).Value);
						
						return new DoubleConstant (res, left.Location);
					} else if (left is FloatConstant){
						float res;
						
						if (ec.ConstantCheckState)
							res = checked (((FloatConstant) left).Value *
								((FloatConstant) right).Value);
						else
							res = unchecked (((FloatConstant) left).Value *
								((FloatConstant) right).Value);
						
						return new FloatConstant (res, left.Location);
					} else if (left is ULongConstant){
						ulong res;
						
						if (ec.ConstantCheckState)
							res = checked (((ULongConstant) left).Value *
								((ULongConstant) right).Value);
						else
							res = unchecked (((ULongConstant) left).Value *
								((ULongConstant) right).Value);
						
						return new ULongConstant (res, left.Location);
					} else if (left is LongConstant){
						long res;
						
						if (ec.ConstantCheckState)
							res = checked (((LongConstant) left).Value *
								((LongConstant) right).Value);
						else
							res = unchecked (((LongConstant) left).Value *
								((LongConstant) right).Value);
						
						return new LongConstant (res, left.Location);
					} else if (left is UIntConstant){
						uint res;
						
						if (ec.ConstantCheckState)
							res = checked (((UIntConstant) left).Value *
								((UIntConstant) right).Value);
						else
							res = unchecked (((UIntConstant) left).Value *
								((UIntConstant) right).Value);
						
						return new UIntConstant (res, left.Location);
					} else if (left is IntConstant){
						int res;

						if (ec.ConstantCheckState)
							res = checked (((IntConstant) left).Value *
								((IntConstant) right).Value);
						else
							res = unchecked (((IntConstant) left).Value *
								((IntConstant) right).Value);

						return new IntConstant (res, left.Location);
					} else if (left is DecimalConstant) {
						decimal res;

						if (ec.ConstantCheckState)
							res = checked (((DecimalConstant) left).Value *
								((DecimalConstant) right).Value);
						else
							res = unchecked (((DecimalConstant) left).Value *
								((DecimalConstant) right).Value);

						return new DecimalConstant (res, left.Location);
					} else {
						throw new Exception ( "Unexepected multiply input: " + left);
					}
				} catch (OverflowException){
					Error_CompileTimeOverflow (loc);
				}
				break;

			case Binary.Operator.Division:
				if (!DoBinaryNumericPromotions (ref left, ref right))
					return null;

				try {
					if (left is DoubleConstant){
						double res;
						
						if (ec.ConstantCheckState)
							res = checked (((DoubleConstant) left).Value /
								((DoubleConstant) right).Value);
						else
							res = unchecked (((DoubleConstant) left).Value /
								((DoubleConstant) right).Value);
						
						return new DoubleConstant (res, left.Location);
					} else if (left is FloatConstant){
						float res;
						
						if (ec.ConstantCheckState)
							res = checked (((FloatConstant) left).Value /
								((FloatConstant) right).Value);
						else
							res = unchecked (((FloatConstant) left).Value /
								((FloatConstant) right).Value);
						
						return new FloatConstant (res, left.Location);
					} else if (left is ULongConstant){
						ulong res;
						
						if (ec.ConstantCheckState)
							res = checked (((ULongConstant) left).Value /
								((ULongConstant) right).Value);
						else
							res = unchecked (((ULongConstant) left).Value /
								((ULongConstant) right).Value);
						
						return new ULongConstant (res, left.Location);
					} else if (left is LongConstant){
						long res;
						
						if (ec.ConstantCheckState)
							res = checked (((LongConstant) left).Value /
								((LongConstant) right).Value);
						else
							res = unchecked (((LongConstant) left).Value /
								((LongConstant) right).Value);
						
						return new LongConstant (res, left.Location);
					} else if (left is UIntConstant){
						uint res;
						
						if (ec.ConstantCheckState)
							res = checked (((UIntConstant) left).Value /
								((UIntConstant) right).Value);
						else
							res = unchecked (((UIntConstant) left).Value /
								((UIntConstant) right).Value);
						
						return new UIntConstant (res, left.Location);
					} else if (left is IntConstant){
						int res;

						if (ec.ConstantCheckState)
							res = checked (((IntConstant) left).Value /
								((IntConstant) right).Value);
						else
							res = unchecked (((IntConstant) left).Value /
								((IntConstant) right).Value);

						return new IntConstant (res, left.Location);
					} else if (left is DecimalConstant) {
						decimal res;

						if (ec.ConstantCheckState)
							res = checked (((DecimalConstant) left).Value /
								((DecimalConstant) right).Value);
						else
							res = unchecked (((DecimalConstant) left).Value /
								((DecimalConstant) right).Value);

						return new DecimalConstant (res, left.Location);
					} else {
						throw new Exception ( "Unexepected division input: " + left);
					}
				} catch (OverflowException){
					Error_CompileTimeOverflow (loc);

				} catch (DivideByZeroException) {
					Report.Error (020, loc, "Division by constant zero");
				}
				
				break;
				
			case Binary.Operator.Modulus:
				if (!DoBinaryNumericPromotions (ref left, ref right))
					return null;

				try {
					if (left is DoubleConstant){
						double res;
						
						if (ec.ConstantCheckState)
							res = checked (((DoubleConstant) left).Value %
								       ((DoubleConstant) right).Value);
						else
							res = unchecked (((DoubleConstant) left).Value %
									 ((DoubleConstant) right).Value);
						
						return new DoubleConstant (res, left.Location);
					} else if (left is FloatConstant){
						float res;
						
						if (ec.ConstantCheckState)
							res = checked (((FloatConstant) left).Value %
								       ((FloatConstant) right).Value);
						else
							res = unchecked (((FloatConstant) left).Value %
									 ((FloatConstant) right).Value);
						
						return new FloatConstant (res, left.Location);
					} else if (left is ULongConstant){
						ulong res;
						
						if (ec.ConstantCheckState)
							res = checked (((ULongConstant) left).Value %
								       ((ULongConstant) right).Value);
						else
							res = unchecked (((ULongConstant) left).Value %
									 ((ULongConstant) right).Value);
						
						return new ULongConstant (res, left.Location);
					} else if (left is LongConstant){
						long res;
						
						if (ec.ConstantCheckState)
							res = checked (((LongConstant) left).Value %
								       ((LongConstant) right).Value);
						else
							res = unchecked (((LongConstant) left).Value %
									 ((LongConstant) right).Value);
						
						return new LongConstant (res, left.Location);
					} else if (left is UIntConstant){
						uint res;
						
						if (ec.ConstantCheckState)
							res = checked (((UIntConstant) left).Value %
								       ((UIntConstant) right).Value);
						else
							res = unchecked (((UIntConstant) left).Value %
									 ((UIntConstant) right).Value);
						
						return new UIntConstant (res, left.Location);
					} else if (left is IntConstant){
						int res;

						if (ec.ConstantCheckState)
							res = checked (((IntConstant) left).Value %
								       ((IntConstant) right).Value);
						else
							res = unchecked (((IntConstant) left).Value %
									 ((IntConstant) right).Value);

						return new IntConstant (res, left.Location);
					} else {
						throw new Exception ( "Unexepected modulus input: " + left);
					}
				} catch (DivideByZeroException){
					Report.Error (020, loc, "Division by constant zero");
				} catch (OverflowException){
					Error_CompileTimeOverflow (loc);
				}
				break;

				//
				// There is no overflow checking on left shift
				//
			case Binary.Operator.LeftShift:
				IntConstant ic = right.ConvertImplicitly (TypeManager.int32_type) as IntConstant;
				if (ic == null){
					Binary.Error_OperatorCannotBeApplied (left, right, oper, loc);
					return null;
				}

				int lshift_val = ic.Value;
				if (left.Type == TypeManager.uint64_type)
					return new ULongConstant (((ULongConstant)left).Value << lshift_val, left.Location);
				if (left.Type == TypeManager.int64_type)
					return new LongConstant (((LongConstant)left).Value << lshift_val, left.Location);
				if (left.Type == TypeManager.uint32_type)
					return new UIntConstant (((UIntConstant)left).Value << lshift_val, left.Location);

				left = left.ConvertImplicitly (TypeManager.int32_type);
				if (left.Type == TypeManager.int32_type)
					return new IntConstant (((IntConstant)left).Value << lshift_val, left.Location);

				Binary.Error_OperatorCannotBeApplied (left, right, oper, loc);
				break;

				//
				// There is no overflow checking on right shift
				//
			case Binary.Operator.RightShift:
				IntConstant sic = right.ConvertImplicitly (TypeManager.int32_type) as IntConstant;
				if (sic == null){
					Binary.Error_OperatorCannotBeApplied (left, right, oper, loc); ;
					return null;
				}
				int rshift_val = sic.Value;
				if (left.Type == TypeManager.uint64_type)
					return new ULongConstant (((ULongConstant)left).Value >> rshift_val, left.Location);
				if (left.Type == TypeManager.int64_type)
					return new LongConstant (((LongConstant)left).Value >> rshift_val, left.Location);
				if (left.Type == TypeManager.uint32_type)
					return new UIntConstant (((UIntConstant)left).Value >> rshift_val, left.Location);

				left = left.ConvertImplicitly (TypeManager.int32_type);
				if (left.Type == TypeManager.int32_type)
					return new IntConstant (((IntConstant)left).Value >> rshift_val, left.Location);

				Binary.Error_OperatorCannotBeApplied (left, right, oper, loc);
				break;

			case Binary.Operator.Equality:
				if (left is NullLiteral){
					if (right is NullLiteral)
						return new BoolConstant (true, left.Location);
					else if (right is StringConstant)
						return new BoolConstant (
							((StringConstant) right).Value == null, left.Location);
				} else if (right is NullLiteral) {
					if (left is NullLiteral)
						return new BoolConstant (true, left.Location);
					else if (left is StringConstant)
						return new BoolConstant (
							((StringConstant) left).Value == null, left.Location);
				}
				if (left is StringConstant && right is StringConstant){
					return new BoolConstant (
						((StringConstant) left).Value ==
						((StringConstant) right).Value, left.Location);
					
				}

				if (!DoBinaryNumericPromotions (ref left, ref right))
					return null;

				bool_res = false;
				if (left is DoubleConstant)
					bool_res = ((DoubleConstant) left).Value ==
						((DoubleConstant) right).Value;
				else if (left is FloatConstant)
					bool_res = ((FloatConstant) left).Value ==
						((FloatConstant) right).Value;
				else if (left is ULongConstant)
					bool_res = ((ULongConstant) left).Value ==
						((ULongConstant) right).Value;
				else if (left is LongConstant)
					bool_res = ((LongConstant) left).Value ==
						((LongConstant) right).Value;
				else if (left is UIntConstant)
					bool_res = ((UIntConstant) left).Value ==
						((UIntConstant) right).Value;
				else if (left is IntConstant)
					bool_res = ((IntConstant) left).Value ==
						((IntConstant) right).Value;
				else
					return null;

				return new BoolConstant (bool_res, left.Location);

			case Binary.Operator.Inequality:
				if (left is NullLiteral) {
					if (right is NullLiteral)
						return new BoolConstant (false, left.Location);
					else if (right is StringConstant)
						return new BoolConstant (
							((StringConstant) right).Value != null, left.Location);
				} else if (right is NullLiteral) {
					if (left is NullLiteral)
						return new BoolConstant (false, left.Location);
					else if (left is StringConstant)
						return new BoolConstant (
							((StringConstant) left).Value != null, left.Location);
				}
				if (left is StringConstant && right is StringConstant){
					return new BoolConstant (
						((StringConstant) left).Value !=
						((StringConstant) right).Value, left.Location);
					
				}

				if (!DoBinaryNumericPromotions (ref left, ref right))
					return null;

				bool_res = false;
				if (left is DoubleConstant)
					bool_res = ((DoubleConstant) left).Value !=
						((DoubleConstant) right).Value;
				else if (left is FloatConstant)
					bool_res = ((FloatConstant) left).Value !=
						((FloatConstant) right).Value;
				else if (left is ULongConstant)
					bool_res = ((ULongConstant) left).Value !=
						((ULongConstant) right).Value;
				else if (left is LongConstant)
					bool_res = ((LongConstant) left).Value !=
						((LongConstant) right).Value;
				else if (left is UIntConstant)
					bool_res = ((UIntConstant) left).Value !=
						((UIntConstant) right).Value;
				else if (left is IntConstant)
					bool_res = ((IntConstant) left).Value !=
						((IntConstant) right).Value;
				else
					return null;

				return new BoolConstant (bool_res, left.Location);

			case Binary.Operator.LessThan:
				if (!DoBinaryNumericPromotions (ref left, ref right))
					return null;

				bool_res = false;
				if (left is DoubleConstant)
					bool_res = ((DoubleConstant) left).Value <
						((DoubleConstant) right).Value;
				else if (left is FloatConstant)
					bool_res = ((FloatConstant) left).Value <
						((FloatConstant) right).Value;
				else if (left is ULongConstant)
					bool_res = ((ULongConstant) left).Value <
						((ULongConstant) right).Value;
				else if (left is LongConstant)
					bool_res = ((LongConstant) left).Value <
						((LongConstant) right).Value;
				else if (left is UIntConstant)
					bool_res = ((UIntConstant) left).Value <
						((UIntConstant) right).Value;
				else if (left is IntConstant)
					bool_res = ((IntConstant) left).Value <
						((IntConstant) right).Value;
				else
					return null;

				return new BoolConstant (bool_res, left.Location);
				
			case Binary.Operator.GreaterThan:
				if (!DoBinaryNumericPromotions (ref left, ref right))
					return null;

				bool_res = false;
				if (left is DoubleConstant)
					bool_res = ((DoubleConstant) left).Value >
						((DoubleConstant) right).Value;
				else if (left is FloatConstant)
					bool_res = ((FloatConstant) left).Value >
						((FloatConstant) right).Value;
				else if (left is ULongConstant)
					bool_res = ((ULongConstant) left).Value >
						((ULongConstant) right).Value;
				else if (left is LongConstant)
					bool_res = ((LongConstant) left).Value >
						((LongConstant) right).Value;
				else if (left is UIntConstant)
					bool_res = ((UIntConstant) left).Value >
						((UIntConstant) right).Value;
				else if (left is IntConstant)
					bool_res = ((IntConstant) left).Value >
						((IntConstant) right).Value;
				else
					return null;

				return new BoolConstant (bool_res, left.Location);

			case Binary.Operator.GreaterThanOrEqual:
				if (!DoBinaryNumericPromotions (ref left, ref right))
					return null;

				bool_res = false;
				if (left is DoubleConstant)
					bool_res = ((DoubleConstant) left).Value >=
						((DoubleConstant) right).Value;
				else if (left is FloatConstant)
					bool_res = ((FloatConstant) left).Value >=
						((FloatConstant) right).Value;
				else if (left is ULongConstant)
					bool_res = ((ULongConstant) left).Value >=
						((ULongConstant) right).Value;
				else if (left is LongConstant)
					bool_res = ((LongConstant) left).Value >=
						((LongConstant) right).Value;
				else if (left is UIntConstant)
					bool_res = ((UIntConstant) left).Value >=
						((UIntConstant) right).Value;
				else if (left is IntConstant)
					bool_res = ((IntConstant) left).Value >=
						((IntConstant) right).Value;
				else
					return null;

				return new BoolConstant (bool_res, left.Location);

			case Binary.Operator.LessThanOrEqual:
				if (!DoBinaryNumericPromotions (ref left, ref right))
					return null;

				bool_res = false;
				if (left is DoubleConstant)
					bool_res = ((DoubleConstant) left).Value <=
						((DoubleConstant) right).Value;
				else if (left is FloatConstant)
					bool_res = ((FloatConstant) left).Value <=
						((FloatConstant) right).Value;
				else if (left is ULongConstant)
					bool_res = ((ULongConstant) left).Value <=
						((ULongConstant) right).Value;
				else if (left is LongConstant)
					bool_res = ((LongConstant) left).Value <=
						((LongConstant) right).Value;
				else if (left is UIntConstant)
					bool_res = ((UIntConstant) left).Value <=
						((UIntConstant) right).Value;
				else if (left is IntConstant)
					bool_res = ((IntConstant) left).Value <=
						((IntConstant) right).Value;
				else
					return null;

				return new BoolConstant (bool_res, left.Location);
			}
					
			return null;
		}
	}
}
