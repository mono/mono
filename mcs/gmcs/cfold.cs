//
// cfold.cs: Constant Folding
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) 2002, 2003 Ximian, Inc.
//

using System;

namespace Mono.CSharp {

	public class ConstantFold {

		//
		// Performs the numeric promotions on the left and right expresions
		// and desposits the results on `lc' and `rc'.
		//
		// On success, the types of `lc' and `rc' on output will always match,
		// and the pair will be one of:
		//
		//   (double, double)
		//   (float, float)
		//   (ulong, ulong)
		//   (long, long)
		//   (uint, uint)
		//   (int, int)
		//   (short, short)   (Happens with enumerations with underlying short type)
		//   (ushort, ushort) (Happens with enumerations with underlying short type)
		//
		static void DoConstantNumericPromotions (EmitContext ec, Binary.Operator oper,
							 ref Constant left, ref Constant right,
							 Location loc)
		{
			if (left is DoubleConstant || right is DoubleConstant){
				//
				// If either side is a double, convert the other to a double
				//
				if (!(left is DoubleConstant))
					left = left.ToDouble (loc);

				if (!(right is DoubleConstant))
					right = right.ToDouble (loc);
				return;
			} else if (left is FloatConstant || right is FloatConstant) {
				//
				// If either side is a float, convert the other to a float
				//
				if (!(left is FloatConstant))
					left = left.ToFloat (loc);

				if (!(right is FloatConstant))
					right = right.ToFloat (loc);
;				return;
			} else if (left is ULongConstant || right is ULongConstant){
				//
				// If either operand is of type ulong, the other operand is
				// converted to type ulong.  or an error ocurrs if the other
				// operand is of type sbyte, short, int or long
				//
#if WRONG
				Constant match, other;
#endif
					
				if (left is ULongConstant){
#if WRONG
					other = right;
					match = left;
#endif
					if (!(right is ULongConstant))
						right = right.ToULong (loc);
				} else {
#if WRONG
					other = left;
					match = right;
#endif
					left = left.ToULong (loc);
				}

#if WRONG
				if (other is SByteConstant || other is ShortConstant ||
				    other is IntConstant || other is LongConstant){
					Binary.Error_OperatorAmbiguous
						(loc, oper, other.Type, match.Type);
					left = null;
					right = null;
				}
#endif
				return;
			} else if (left is LongConstant || right is LongConstant){
				//
				// If either operand is of type long, the other operand is converted
				// to type long.
				//
				if (!(left is LongConstant))
					left = left.ToLong (loc);
				else if (!(right is LongConstant))
					right = right.ToLong (loc);
				return;
			} else if (left is UIntConstant || right is UIntConstant){
				//
				// If either operand is of type uint, and the other
				// operand is of type sbyte, short or int, the operands are
				// converted to type long.
				//
				Constant other;
				if (left is UIntConstant)
					other = right;
				else
					other = left;

				// Nothing to do.
				if (other is UIntConstant)
					return;

				IntConstant ic = other as IntConstant;
				if (ic != null){
					if (ic.Value >= 0){
						if (left == other)
							left = new UIntConstant ((uint) ic.Value, ic.Location);
						else
							right = new UIntConstant ((uint) ic.Value, ic.Location);
						return;
					}
				}
				
				if (other is SByteConstant || other is ShortConstant || ic != null){
					left = left.ToLong (loc);
					right = right.ToLong (loc);
				} else {
					left = left.ToUInt (loc);
					right = left.ToUInt (loc);
				}

				return;
			} else if (left is DecimalConstant || right is DecimalConstant) {
				if (!(left is DecimalConstant))
					left = left.ToDecimal (loc);
				else if (!(right is DecimalConstant))
					right = right.ToDecimal (loc);
				return;
			} else if (left is EnumConstant || right is EnumConstant){
				if (left is EnumConstant)
					left = ((EnumConstant) left).Child;
				if (right is EnumConstant)
					right = ((EnumConstant) right).Child;

				DoConstantNumericPromotions (ec, oper, ref left, ref right, loc);
				return;

			} else {
				//
				// Force conversions to int32
				//
				if (!(left is IntConstant))
					left = left.ToInt (loc);
				if (!(right is IntConstant))
					right = right.ToInt (loc);
			}
			return;
		}

		static void Error_CompileTimeOverflow (Location loc)
		{
			Report.Error (220, loc, "The operation overflows at compile time in checked mode");
		}
		
		/// <summary>
		///   Constant expression folder for binary operations.
		///
		///   Returns null if the expression can not be folded.
		/// </summary>
		static public Expression BinaryFold (EmitContext ec, Binary.Operator oper,
						     Constant left, Constant right, Location loc)
		{
			if (left is NullCast)
				return BinaryFold (ec, oper, ((NullCast)left).child, right, loc);

			if (right is NullCast)
				return BinaryFold (ec, oper, left, ((NullCast)right).child, loc);

			Type lt = left.Type;
			Type rt = right.Type;
			Type result_type = null;
			bool bool_res;

			//
			// Enumerator folding
			//
			if (rt == lt && left is EnumConstant)
				result_type = lt;

			//
			// During an enum evaluation, we need to unwrap enumerations
			//
			if (ec.InEnumContext){
				if (left is EnumConstant)
					left = ((EnumConstant) left).Child;
				
				if (right is EnumConstant)
					right = ((EnumConstant) right).Child;
			}

			if (left is BoolConstant && right is BoolConstant) {
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
				default:
					throw new InternalErrorException ("Invalid operator on booleans: " + oper);
				}
			}

			Type wrap_as;
			Constant result = null;
			switch (oper){
			case Binary.Operator.BitwiseOr:
				DoConstantNumericPromotions (ec, oper, ref left, ref right, loc);
				if (left == null || right == null)
					return null;

				if (left is IntConstant){
					IntConstant v;
					int res = ((IntConstant) left).Value | ((IntConstant) right).Value;
					
					v = new IntConstant (res, left.Location);
					if (result_type == null)
						return v;
					else
						return new EnumConstant (v, result_type);
				} else if (left is UIntConstant){
					UIntConstant v;
					uint res = ((UIntConstant)left).Value | ((UIntConstant)right).Value;
					
					v = new UIntConstant (res, left.Location);
					if (result_type == null)
						return v;
					else
						return new EnumConstant (v, result_type);
				} else if (left is LongConstant){
					LongConstant v;
					long res = ((LongConstant)left).Value | ((LongConstant)right).Value;
					
					v = new LongConstant (res, left.Location);
					if (result_type == null)
						return v;
					else
						return new EnumConstant (v, result_type);
				} else if (left is ULongConstant){
					ULongConstant v;
					ulong res = ((ULongConstant)left).Value |
						((ULongConstant)right).Value;
					
					v = new ULongConstant (res, left.Location);
					if (result_type == null)
						return v;
					else
						return new EnumConstant (v, result_type);
				} else if (left is UShortConstant){
					UShortConstant v;
					ushort res = (ushort) (((UShortConstant)left).Value |
							       ((UShortConstant)right).Value);
					
					v = new UShortConstant (res, left.Location);
					if (result_type == null)
						return v;
					else
						return new EnumConstant (v, result_type);
				} else if (left is ShortConstant){
					ShortConstant v;
					short res = (short) (((ShortConstant)left).Value |
							     ((ShortConstant)right).Value);
					
					v = new ShortConstant (res, left.Location);
					if (result_type == null)
						return v;
					else
						return new EnumConstant (v, result_type);
				}
				break;
				
			case Binary.Operator.BitwiseAnd:
				DoConstantNumericPromotions (ec, oper, ref left, ref right, loc);
				if (left == null || right == null)
					return null;
				
				if (left is IntConstant){
					IntConstant v;
					int res = ((IntConstant) left).Value & ((IntConstant) right).Value;
					
					v = new IntConstant (res, left.Location);
					if (result_type == null)
						return v;
					else
						return new EnumConstant (v, result_type);
				} else if (left is UIntConstant){
					UIntConstant v;
					uint res = ((UIntConstant)left).Value & ((UIntConstant)right).Value;
					
					v = new UIntConstant (res, left.Location);
					if (result_type == null)
						return v;
					else
						return new EnumConstant (v, result_type);
				} else if (left is LongConstant){
					LongConstant v;
					long res = ((LongConstant)left).Value & ((LongConstant)right).Value;
					
					v = new LongConstant (res, left.Location);
					if (result_type == null)
						return v;
					else
						return new EnumConstant (v, result_type);
				} else if (left is ULongConstant){
					ULongConstant v;
					ulong res = ((ULongConstant)left).Value &
						((ULongConstant)right).Value;
					
					v = new ULongConstant (res, left.Location);
					if (result_type == null)
						return v;
					else
						return new EnumConstant (v, result_type);
				} else if (left is UShortConstant){
					UShortConstant v;
					ushort res = (ushort) (((UShortConstant)left).Value &
							       ((UShortConstant)right).Value);
					
					v = new UShortConstant (res, left.Location);
					if (result_type == null)
						return v;
					else
						return new EnumConstant (v, result_type);
				} else if (left is ShortConstant){
					ShortConstant v;
					short res = (short) (((ShortConstant)left).Value &
							     ((ShortConstant)right).Value);
					
					v = new ShortConstant (res, left.Location);
					if (result_type == null)
						return v;
					else
						return new EnumConstant (v, result_type);
				}
				break;

			case Binary.Operator.ExclusiveOr:
				DoConstantNumericPromotions (ec, oper, ref left, ref right, loc);
				if (left == null || right == null)
					return null;
				
				if (left is IntConstant){
					IntConstant v;
					int res = ((IntConstant) left).Value ^ ((IntConstant) right).Value;
					
					v = new IntConstant (res, left.Location);
					if (result_type == null)
						return v;
					else
						return new EnumConstant (v, result_type);
				} else if (left is UIntConstant){
					UIntConstant v;
					uint res = ((UIntConstant)left).Value ^ ((UIntConstant)right).Value;
					
					v = new UIntConstant (res, left.Location);
					if (result_type == null)
						return v;
					else
						return new EnumConstant (v, result_type);
				} else if (left is LongConstant){
					LongConstant v;
					long res = ((LongConstant)left).Value ^ ((LongConstant)right).Value;
					
					v = new LongConstant (res, left.Location);
					if (result_type == null)
						return v;
					else
						return new EnumConstant (v, result_type);
				} else if (left is ULongConstant){
					ULongConstant v;
					ulong res = ((ULongConstant)left).Value ^
						((ULongConstant)right).Value;
					
					v = new ULongConstant (res, left.Location);
					if (result_type == null)
						return v;
					else
						return new EnumConstant (v, result_type);
				} else if (left is UShortConstant){
					UShortConstant v;
					ushort res = (ushort) (((UShortConstant)left).Value ^
							       ((UShortConstant)right).Value);
					
					v = new UShortConstant (res, left.Location);
					if (result_type == null)
						return v;
					else
						return new EnumConstant (v, result_type);
				} else if (left is ShortConstant){
					ShortConstant v;
					short res = (short)(((ShortConstant)left).Value ^
							    ((ShortConstant)right).Value);
					
					v = new ShortConstant (res, left.Location);
					if (result_type == null)
						return v;
					else
						return new EnumConstant (v, result_type);
				}
				break;

			case Binary.Operator.Addition:
				bool left_is_string = left is StringConstant;
				bool right_is_string = right is StringConstant;

				//
				// If both sides are strings, then concatenate, if
				// one is a string, and the other is not, then defer
				// to runtime concatenation
				//
				wrap_as = null;
				if (left_is_string || right_is_string){
					if (left_is_string && right_is_string)
						return new StringConstant (
							((StringConstant) left).Value +
							((StringConstant) right).Value, left.Location);
					
					return null;
				}

				//
				// handle "E operator + (E x, U y)"
				// handle "E operator + (Y y, E x)"
				//
				// note that E operator + (E x, E y) is invalid
				//
				if (left is EnumConstant){
					if (right is EnumConstant){
						return null;
					}

					right = right.ToType (((EnumConstant) left).Child.Type, loc);
					if (right == null)
						return null;

					wrap_as = left.Type;
				} else if (right is EnumConstant){
					left = left.ToType (((EnumConstant) right).Child.Type, loc);
					if (left == null)
						return null;

					wrap_as = right.Type;
				}

				result = null;
				DoConstantNumericPromotions (ec, oper, ref left, ref right, loc);
				if (left == null || right == null)
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
						
						result = new DoubleConstant (res, left.Location);
					} else if (left is FloatConstant){
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

				if (wrap_as != null)
					return result.TryReduce (ec, wrap_as, loc);
				else
					return result;

			case Binary.Operator.Subtraction:
				//
				// handle "E operator - (E x, U y)"
				// handle "E operator - (Y y, E x)"
				// handle "U operator - (E x, E y)"
				//
				wrap_as = null;
				if (left is EnumConstant){
					if (right is EnumConstant){
						if (left.Type != right.Type) {
							Binary.Error_OperatorCannotBeApplied (loc, "-", left.Type, right.Type);
							return null;
						}

						wrap_as = TypeManager.EnumToUnderlying (left.Type);
						right = ((EnumConstant) right).Child.ToType (wrap_as, loc);
						if (right == null)
							return null;

						left = ((EnumConstant) left).Child.ToType (wrap_as, loc);
						if (left == null)
							return null;
					}
					else {
						right = right.ToType (((EnumConstant) left).Child.Type, loc);
						if (right == null)
							return null;

						wrap_as = left.Type;
					}
				} else if (right is EnumConstant){
					left = left.ToType (((EnumConstant) right).Child.Type, loc);
					if (left == null)
						return null;

					wrap_as = right.Type;
				}

				DoConstantNumericPromotions (ec, oper, ref left, ref right, loc);
				if (left == null || right == null)
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

				if (wrap_as != null)
					return result.TryReduce (ec, wrap_as, loc);

				return result;
				
			case Binary.Operator.Multiply:
				DoConstantNumericPromotions (ec, oper, ref left, ref right, loc);
				if (left == null || right == null)
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
				DoConstantNumericPromotions (ec, oper, ref left, ref right, loc);
				if (left == null || right == null)
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
				DoConstantNumericPromotions (ec, oper, ref left, ref right, loc);
				if (left == null || right == null)
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
				IntConstant ic = right.ToInt (loc);
				if (ic == null){
					Binary.Error_OperatorCannotBeApplied (loc, "<<", lt, rt);
					return null;
				}
				int lshift_val = ic.Value;

				IntConstant lic;
				if ((lic = left.ConvertToInt ()) != null)
					return new IntConstant (lic.Value << lshift_val, left.Location);

				UIntConstant luic;
				if ((luic = left.ConvertToUInt ()) != null)
					return new UIntConstant (luic.Value << lshift_val, left.Location);

				LongConstant llc;
				if ((llc = left.ConvertToLong ()) != null)
					return new LongConstant (llc.Value << lshift_val, left.Location);

				ULongConstant lulc;
				if ((lulc = left.ConvertToULong ()) != null)
					return new ULongConstant (lulc.Value << lshift_val, left.Location);

				Binary.Error_OperatorCannotBeApplied (loc, "<<", lt, rt);
				break;

				//
				// There is no overflow checking on right shift
				//
			case Binary.Operator.RightShift:
				IntConstant sic = right.ToInt (loc);
				if (sic == null){
					Binary.Error_OperatorCannotBeApplied (loc, ">>", lt, rt);
					return null;
				}
				int rshift_val = sic.Value;

				IntConstant ric;
				if ((ric = left.ConvertToInt ()) != null)
					return new IntConstant (ric.Value >> rshift_val, left.Location);

				UIntConstant ruic;
				if ((ruic = left.ConvertToUInt ()) != null)
					return new UIntConstant (ruic.Value >> rshift_val, left.Location);

				LongConstant rlc;
				if ((rlc = left.ConvertToLong ()) != null)
					return new LongConstant (rlc.Value >> rshift_val, left.Location);

				ULongConstant rulc;
				if ((rulc = left.ConvertToULong ()) != null)
					return new ULongConstant (rulc.Value >> rshift_val, left.Location);

				Binary.Error_OperatorCannotBeApplied (loc, ">>", lt, rt);
				break;

			case Binary.Operator.Equality:
				if (left is BoolConstant && right is BoolConstant){
					return new BoolConstant (
						((BoolConstant) left).Value ==
						((BoolConstant) right).Value, left.Location);
				
				}
				if (left is NullLiteral){
					if (right is NullLiteral)
						return new BoolConstant (true, left.Location);
					else if (right is StringConstant)
						return new BoolConstant (
							((StringConstant) right).Value == null, left.Location);
				} else if (right is NullLiteral){
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

				DoConstantNumericPromotions (ec, oper, ref left, ref right, loc);
				if (left == null || right == null)
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
				if (left is BoolConstant && right is BoolConstant){
					return new BoolConstant (
						((BoolConstant) left).Value !=
						((BoolConstant) right).Value, left.Location);
				}
				if (left is NullLiteral){
					if (right is NullLiteral)
						return new BoolConstant (false, left.Location);
					else if (right is StringConstant)
						return new BoolConstant (
							((StringConstant) right).Value != null, left.Location);
				} else if (right is NullLiteral){
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
				DoConstantNumericPromotions (ec, oper, ref left, ref right, loc);
				if (left == null || right == null)
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
				DoConstantNumericPromotions (ec, oper, ref left, ref right, loc);
				if (left == null || right == null)
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
				DoConstantNumericPromotions (ec, oper, ref left, ref right, loc);
				if (left == null || right == null)
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
				DoConstantNumericPromotions (ec, oper, ref left, ref right, loc);
				if (left == null || right == null)
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
				DoConstantNumericPromotions (ec, oper, ref left, ref right, loc);
				if (left == null || right == null)
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
