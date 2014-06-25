//
// cfold.cs: Constant Folding
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//   Marek Safar (marek.safar@seznam.cz)
//
// Copyright 2002, 2003 Ximian, Inc.
// Copyright 2003-2011, Novell, Inc.
// 
using System;

namespace Mono.CSharp {

	public static class ConstantFold
	{
		public static TypeSpec[] CreateBinaryPromotionsTypes (BuiltinTypes types)
		{
			return new TypeSpec[] { 
				types.Decimal, types.Double, types.Float,
				types.ULong, types.Long, types.UInt
			};
		}

		//
		// Performs the numeric promotions on the left and right expresions
		// and deposits the results on `lc' and `rc'.
		//
		// On success, the types of `lc' and `rc' on output will always match,
		// and the pair will be one of:
		//
		// TODO: BinaryFold should be called as an optimization step only,
		// error checking here is weak
		//		
		static bool DoBinaryNumericPromotions (ResolveContext rc, ref Constant left, ref Constant right)
		{
			TypeSpec ltype = left.Type;
			TypeSpec rtype = right.Type;

			foreach (TypeSpec t in rc.BuiltinTypes.BinaryPromotionsTypes) {
				if (t == ltype)
					return t == rtype || ConvertPromotion (rc, ref right, ref left, t);

				if (t == rtype)
					return t == ltype || ConvertPromotion (rc, ref left, ref right, t);
			}

			left = left.ConvertImplicitly (rc.BuiltinTypes.Int);
			right = right.ConvertImplicitly (rc.BuiltinTypes.Int);
			return left != null && right != null;
		}

		static bool ConvertPromotion (ResolveContext rc, ref Constant prim, ref Constant second, TypeSpec type)
		{
			Constant c = prim.ConvertImplicitly (type);
			if (c != null) {
				prim = c;
				return true;
			}

			if (type.BuiltinType == BuiltinTypeSpec.Type.UInt) {
				type = rc.BuiltinTypes.Long;
				prim = prim.ConvertImplicitly (type);
				second = second.ConvertImplicitly (type);
				return prim != null && second != null;
			}

			return false;
		}

		internal static void Error_CompileTimeOverflow (ResolveContext rc, Location loc)
		{
			rc.Report.Error (220, loc, "The operation overflows at compile time in checked mode");
		}
		
		/// <summary>
		///   Constant expression folder for binary operations.
		///
		///   Returns null if the expression can not be folded.
		/// </summary>
		static public Constant BinaryFold (ResolveContext ec, Binary.Operator oper,
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

			TypeSpec lt = left.Type;
			TypeSpec rt = right.Type;
			bool bool_res;

			if (lt.BuiltinType == BuiltinTypeSpec.Type.Bool && lt == rt) {
				bool lv = (bool) left.GetValue ();
				bool rv = (bool) right.GetValue ();			
				switch (oper) {
				case Binary.Operator.BitwiseAnd:
				case Binary.Operator.LogicalAnd:
					return new BoolConstant (ec.BuiltinTypes, lv && rv, left.Location);
				case Binary.Operator.BitwiseOr:
				case Binary.Operator.LogicalOr:
					return new BoolConstant (ec.BuiltinTypes, lv || rv, left.Location);
				case Binary.Operator.ExclusiveOr:
					return new BoolConstant (ec.BuiltinTypes, lv ^ rv, left.Location);
				case Binary.Operator.Equality:
					return new BoolConstant (ec.BuiltinTypes, lv == rv, left.Location);
				case Binary.Operator.Inequality:
					return new BoolConstant (ec.BuiltinTypes, lv != rv, left.Location);
				}
				return null;
			}

			//
			// During an enum evaluation, none of the rules are valid
			// Not sure whether it is bug in csc or in documentation
			//
			if (ec.HasSet (ResolveContext.Options.EnumScope)){
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
						result = BinaryFold (ec, oper, ((EnumConstant)left).Child, ((EnumConstant)right).Child, loc);
						if (result != null)
							result = result.Reduce (ec, lt);
						return result;

					///
					/// U operator -(E x, E y);
					/// 
					case Binary.Operator.Subtraction:
						result = BinaryFold (ec, oper, ((EnumConstant)left).Child, ((EnumConstant)right).Child, loc);
						if (result != null)
							result = result.Reduce (ec, EnumSpec.GetUnderlyingType (lt));
						return result;

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
				//
				// bool? operator |(bool? x, bool? y);
				//
				if ((lt.BuiltinType == BuiltinTypeSpec.Type.Bool && right is NullLiteral) ||
					(rt.BuiltinType == BuiltinTypeSpec.Type.Bool && left is NullLiteral)) {
					var b = new Binary (oper, left, right).ResolveOperator (ec);

					// false | null => null
					// null | false => null
					if ((right is NullLiteral && left.IsDefaultValue) || (left is NullLiteral && right.IsDefaultValue))
						return Nullable.LiftedNull.CreateFromExpression (ec, b);

					// true | null => true
					// null | true => true
					return ReducedExpression.Create (new BoolConstant (ec.BuiltinTypes, true, loc), b);					
				}

				if (!DoBinaryNumericPromotions (ec, ref left, ref right))
					return null;

				if (left is IntConstant){
					int res = ((IntConstant) left).Value | ((IntConstant) right).Value;

					return new IntConstant (ec.BuiltinTypes, res, left.Location);
				}
				if (left is UIntConstant){
					uint res = ((UIntConstant)left).Value | ((UIntConstant)right).Value;

					return new UIntConstant (ec.BuiltinTypes, res, left.Location);
				}
				if (left is LongConstant){
					long res = ((LongConstant)left).Value | ((LongConstant)right).Value;

					return new LongConstant (ec.BuiltinTypes, res, left.Location);
				}
				if (left is ULongConstant){
					ulong res = ((ULongConstant)left).Value |
						((ULongConstant)right).Value;

					return new ULongConstant (ec.BuiltinTypes, res, left.Location);
				}
				break;
				
			case Binary.Operator.BitwiseAnd:
				//
				// bool? operator &(bool? x, bool? y);
				//
				if ((lt.BuiltinType == BuiltinTypeSpec.Type.Bool && right is NullLiteral) ||
					(rt.BuiltinType == BuiltinTypeSpec.Type.Bool && left is NullLiteral)) {
					var b = new Binary (oper, left, right).ResolveOperator (ec);

					// false & null => false
					// null & false => false
					if ((right is NullLiteral && left.IsDefaultValue) || (left is NullLiteral && right.IsDefaultValue))
						return ReducedExpression.Create (new BoolConstant (ec.BuiltinTypes, false, loc), b);

					// true & null => null
					// null & true => null
					return Nullable.LiftedNull.CreateFromExpression (ec, b);
				}

				if (!DoBinaryNumericPromotions (ec, ref left, ref right))
					return null;
				
				///
				/// int operator &(int x, int y);
				/// uint operator &(uint x, uint y);
				/// long operator &(long x, long y);
				/// ulong operator &(ulong x, ulong y);
				///
				if (left is IntConstant){
					int res = ((IntConstant) left).Value & ((IntConstant) right).Value;
					return new IntConstant (ec.BuiltinTypes, res, left.Location);
				}
				if (left is UIntConstant){
					uint res = ((UIntConstant)left).Value & ((UIntConstant)right).Value;
					return new UIntConstant (ec.BuiltinTypes, res, left.Location);
				}
				if (left is LongConstant){
					long res = ((LongConstant)left).Value & ((LongConstant)right).Value;
					return new LongConstant (ec.BuiltinTypes, res, left.Location);
				}
				if (left is ULongConstant){
					ulong res = ((ULongConstant)left).Value &
						((ULongConstant)right).Value;

					return new ULongConstant (ec.BuiltinTypes, res, left.Location);
				}
				break;

			case Binary.Operator.ExclusiveOr:
				if (!DoBinaryNumericPromotions (ec, ref left, ref right))
					return null;
				
				if (left is IntConstant){
					int res = ((IntConstant) left).Value ^ ((IntConstant) right).Value;
					return new IntConstant (ec.BuiltinTypes, res, left.Location);
				}
				if (left is UIntConstant){
					uint res = ((UIntConstant)left).Value ^ ((UIntConstant)right).Value;

					return new UIntConstant (ec.BuiltinTypes, res, left.Location);
				}
				if (left is LongConstant){
					long res = ((LongConstant)left).Value ^ ((LongConstant)right).Value;

					return new LongConstant (ec.BuiltinTypes, res, left.Location);
				}
				if (left is ULongConstant){
					ulong res = ((ULongConstant)left).Value ^
						((ULongConstant)right).Value;

					return new ULongConstant (ec.BuiltinTypes, res, left.Location);
				}
				break;

			case Binary.Operator.Addition:
				//
				// If both sides are strings, then concatenate
				//
				// string operator + (string x, string y)
				//
				if (lt.BuiltinType == BuiltinTypeSpec.Type.String || rt.BuiltinType == BuiltinTypeSpec.Type.String){
					if (lt == rt)
						return new StringConstant (ec.BuiltinTypes, (string)left.GetValue () + (string)right.GetValue (),
							left.Location);

					if (lt == InternalType.NullLiteral)
						return new StringConstant (ec.BuiltinTypes, "" + right.GetValue (), left.Location);

					if (rt == InternalType.NullLiteral)
						return new StringConstant (ec.BuiltinTypes, left.GetValue () + "", left.Location);

					return null;
				}

				//
				// string operator + (string x, object y)
				//
				if (lt == InternalType.NullLiteral) {
					if (rt.BuiltinType == BuiltinTypeSpec.Type.Object)
						return new StringConstant (ec.BuiltinTypes, "" + right.GetValue (), left.Location);

					if (lt == rt) {
						ec.Report.Error (34, loc, "Operator `{0}' is ambiguous on operands of type `{1}' and `{2}'",
							"+", lt.GetSignatureForError (), rt.GetSignatureForError ());
						return null;
					}

					return right;
				}

				//
				// string operator + (object x, string y)
				//
				if (rt == InternalType.NullLiteral) {
					if (lt.BuiltinType == BuiltinTypeSpec.Type.Object)
						return new StringConstant (ec.BuiltinTypes, right.GetValue () + "", left.Location);
	
					return left;
				}

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

					result = result.Reduce (ec, lt);
					if (result == null || lt.IsEnum)
						return result;

					return new EnumConstant (result, lt);
				}

				if (!DoBinaryNumericPromotions (ec, ref left, ref right))
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

						return new DoubleConstant (ec.BuiltinTypes, res, left.Location);
					}
					if (left is FloatConstant){
						double a, b, res;
						a = ((FloatConstant) left).DoubleValue;
						b = ((FloatConstant) right).DoubleValue;

						if (ec.ConstantCheckState)
							res = checked (a + b);
						else
							res = unchecked (a + b);

						result = new FloatConstant (ec.BuiltinTypes, res, left.Location);
					} else if (left is ULongConstant){
						ulong res;
						
						if (ec.ConstantCheckState)
							res = checked (((ULongConstant) left).Value +
								       ((ULongConstant) right).Value);
						else
							res = unchecked (((ULongConstant) left).Value +
									 ((ULongConstant) right).Value);

						result = new ULongConstant (ec.BuiltinTypes, res, left.Location);
					} else if (left is LongConstant){
						long res;
						
						if (ec.ConstantCheckState)
							res = checked (((LongConstant) left).Value +
								       ((LongConstant) right).Value);
						else
							res = unchecked (((LongConstant) left).Value +
									 ((LongConstant) right).Value);

						result = new LongConstant (ec.BuiltinTypes, res, left.Location);
					} else if (left is UIntConstant){
						uint res;
						
						if (ec.ConstantCheckState)
							res = checked (((UIntConstant) left).Value +
								       ((UIntConstant) right).Value);
						else
							res = unchecked (((UIntConstant) left).Value +
									 ((UIntConstant) right).Value);

						result = new UIntConstant (ec.BuiltinTypes, res, left.Location);
					} else if (left is IntConstant){
						int res;

						if (ec.ConstantCheckState)
							res = checked (((IntConstant) left).Value +
								       ((IntConstant) right).Value);
						else
							res = unchecked (((IntConstant) left).Value +
									 ((IntConstant) right).Value);

						result = new IntConstant (ec.BuiltinTypes, res, left.Location);
					} else if (left is DecimalConstant) {
						decimal res;

						if (ec.ConstantCheckState)
							res = checked (((DecimalConstant) left).Value +
								((DecimalConstant) right).Value);
						else
							res = unchecked (((DecimalConstant) left).Value +
								((DecimalConstant) right).Value);

						result = new DecimalConstant (ec.BuiltinTypes, res, left.Location);
					}
				} catch (OverflowException){
					Error_CompileTimeOverflow (ec, loc);
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

					result = result.Reduce (ec, lt);
					if (result == null)
						return null;

					return new EnumConstant (result, lt);
				}

				if (left is NullLiteral && right is NullLiteral) {
					var lifted_int = new Nullable.NullableType (ec.BuiltinTypes.Int, loc);
					lifted_int.ResolveAsType (ec);
					return (Constant) new Binary (oper, lifted_int, right).ResolveOperator (ec);
				}

				if (!DoBinaryNumericPromotions (ec, ref left, ref right))
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

						result = new DoubleConstant (ec.BuiltinTypes, res, left.Location);
					} else if (left is FloatConstant){
						double a, b, res;
						a = ((FloatConstant) left).DoubleValue;
						b = ((FloatConstant) right).DoubleValue;

						if (ec.ConstantCheckState)
							res = checked (a - b);
						else
							res = unchecked (a - b);

						result = new FloatConstant (ec.BuiltinTypes, res, left.Location);
					} else if (left is ULongConstant){
						ulong res;
						
						if (ec.ConstantCheckState)
							res = checked (((ULongConstant) left).Value -
								       ((ULongConstant) right).Value);
						else
							res = unchecked (((ULongConstant) left).Value -
									 ((ULongConstant) right).Value);

						result = new ULongConstant (ec.BuiltinTypes, res, left.Location);
					} else if (left is LongConstant){
						long res;
						
						if (ec.ConstantCheckState)
							res = checked (((LongConstant) left).Value -
								       ((LongConstant) right).Value);
						else
							res = unchecked (((LongConstant) left).Value -
									 ((LongConstant) right).Value);

						result = new LongConstant (ec.BuiltinTypes, res, left.Location);
					} else if (left is UIntConstant){
						uint res;
						
						if (ec.ConstantCheckState)
							res = checked (((UIntConstant) left).Value -
								       ((UIntConstant) right).Value);
						else
							res = unchecked (((UIntConstant) left).Value -
									 ((UIntConstant) right).Value);

						result = new UIntConstant (ec.BuiltinTypes, res, left.Location);
					} else if (left is IntConstant){
						int res;

						if (ec.ConstantCheckState)
							res = checked (((IntConstant) left).Value -
								       ((IntConstant) right).Value);
						else
							res = unchecked (((IntConstant) left).Value -
									 ((IntConstant) right).Value);

						result = new IntConstant (ec.BuiltinTypes, res, left.Location);
					} else if (left is DecimalConstant) {
						decimal res;

						if (ec.ConstantCheckState)
							res = checked (((DecimalConstant) left).Value -
								((DecimalConstant) right).Value);
						else
							res = unchecked (((DecimalConstant) left).Value -
								((DecimalConstant) right).Value);

						return new DecimalConstant (ec.BuiltinTypes, res, left.Location);
					} else {
						throw new Exception ( "Unexepected subtraction input: " + left);
					}
				} catch (OverflowException){
					Error_CompileTimeOverflow (ec, loc);
				}

				return result;
				
			case Binary.Operator.Multiply:
				if (left is NullLiteral && right is NullLiteral) {
					var lifted_int = new Nullable.NullableType (ec.BuiltinTypes.Int, loc);
					lifted_int.ResolveAsType (ec);
					return (Constant) new Binary (oper, lifted_int, right).ResolveOperator (ec);
				}

				if (!DoBinaryNumericPromotions (ec, ref left, ref right))
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

						return new DoubleConstant (ec.BuiltinTypes, res, left.Location);
					} else if (left is FloatConstant){
						double a, b, res;
						a = ((FloatConstant) left).DoubleValue;
						b = ((FloatConstant) right).DoubleValue;

						if (ec.ConstantCheckState)
							res = checked (a * b);
						else
							res = unchecked (a * b);

						return new FloatConstant (ec.BuiltinTypes, res, left.Location);
					} else if (left is ULongConstant){
						ulong res;
						
						if (ec.ConstantCheckState)
							res = checked (((ULongConstant) left).Value *
								((ULongConstant) right).Value);
						else
							res = unchecked (((ULongConstant) left).Value *
								((ULongConstant) right).Value);

						return new ULongConstant (ec.BuiltinTypes, res, left.Location);
					} else if (left is LongConstant){
						long res;
						
						if (ec.ConstantCheckState)
							res = checked (((LongConstant) left).Value *
								((LongConstant) right).Value);
						else
							res = unchecked (((LongConstant) left).Value *
								((LongConstant) right).Value);

						return new LongConstant (ec.BuiltinTypes, res, left.Location);
					} else if (left is UIntConstant){
						uint res;
						
						if (ec.ConstantCheckState)
							res = checked (((UIntConstant) left).Value *
								((UIntConstant) right).Value);
						else
							res = unchecked (((UIntConstant) left).Value *
								((UIntConstant) right).Value);

						return new UIntConstant (ec.BuiltinTypes, res, left.Location);
					} else if (left is IntConstant){
						int res;

						if (ec.ConstantCheckState)
							res = checked (((IntConstant) left).Value *
								((IntConstant) right).Value);
						else
							res = unchecked (((IntConstant) left).Value *
								((IntConstant) right).Value);

						return new IntConstant (ec.BuiltinTypes, res, left.Location);
					} else if (left is DecimalConstant) {
						decimal res;

						if (ec.ConstantCheckState)
							res = checked (((DecimalConstant) left).Value *
								((DecimalConstant) right).Value);
						else
							res = unchecked (((DecimalConstant) left).Value *
								((DecimalConstant) right).Value);

						return new DecimalConstant (ec.BuiltinTypes, res, left.Location);
					} else {
						throw new Exception ( "Unexepected multiply input: " + left);
					}
				} catch (OverflowException){
					Error_CompileTimeOverflow (ec, loc);
				}
				break;

			case Binary.Operator.Division:
				if (left is NullLiteral && right is NullLiteral) {
					var lifted_int = new Nullable.NullableType (ec.BuiltinTypes.Int, loc);
					lifted_int.ResolveAsType (ec);
					return (Constant) new Binary (oper, lifted_int, right).ResolveOperator (ec);
				}

				if (!DoBinaryNumericPromotions (ec, ref left, ref right))
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

						return new DoubleConstant (ec.BuiltinTypes, res, left.Location);
					} else if (left is FloatConstant){
						double a, b, res;
						a = ((FloatConstant) left).DoubleValue;
						b = ((FloatConstant) right).DoubleValue;

						if (ec.ConstantCheckState)
							res = checked (a / b);
						else
							res = unchecked (a / b);

						return new FloatConstant (ec.BuiltinTypes, res, left.Location);
					} else if (left is ULongConstant){
						ulong res;
						
						if (ec.ConstantCheckState)
							res = checked (((ULongConstant) left).Value /
								((ULongConstant) right).Value);
						else
							res = unchecked (((ULongConstant) left).Value /
								((ULongConstant) right).Value);

						return new ULongConstant (ec.BuiltinTypes, res, left.Location);
					} else if (left is LongConstant){
						long res;
						
						if (ec.ConstantCheckState)
							res = checked (((LongConstant) left).Value /
								((LongConstant) right).Value);
						else
							res = unchecked (((LongConstant) left).Value /
								((LongConstant) right).Value);

						return new LongConstant (ec.BuiltinTypes, res, left.Location);
					} else if (left is UIntConstant){
						uint res;
						
						if (ec.ConstantCheckState)
							res = checked (((UIntConstant) left).Value /
								((UIntConstant) right).Value);
						else
							res = unchecked (((UIntConstant) left).Value /
								((UIntConstant) right).Value);

						return new UIntConstant (ec.BuiltinTypes, res, left.Location);
					} else if (left is IntConstant){
						int res;

						if (ec.ConstantCheckState)
							res = checked (((IntConstant) left).Value /
								((IntConstant) right).Value);
						else
							res = unchecked (((IntConstant) left).Value /
								((IntConstant) right).Value);

						return new IntConstant (ec.BuiltinTypes, res, left.Location);
					} else if (left is DecimalConstant) {
						decimal res;

						if (ec.ConstantCheckState)
							res = checked (((DecimalConstant) left).Value /
								((DecimalConstant) right).Value);
						else
							res = unchecked (((DecimalConstant) left).Value /
								((DecimalConstant) right).Value);

						return new DecimalConstant (ec.BuiltinTypes, res, left.Location);
					} else {
						throw new Exception ( "Unexepected division input: " + left);
					}
				} catch (OverflowException){
					Error_CompileTimeOverflow (ec, loc);

				} catch (DivideByZeroException) {
					ec.Report.Error (20, loc, "Division by constant zero");
				}
				
				break;
				
			case Binary.Operator.Modulus:
				if (left is NullLiteral && right is NullLiteral) {
					var lifted_int = new Nullable.NullableType (ec.BuiltinTypes.Int, loc);
					lifted_int.ResolveAsType (ec);
					return (Constant) new Binary (oper, lifted_int, right).ResolveOperator (ec);
				}

				if (!DoBinaryNumericPromotions (ec, ref left, ref right))
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

						return new DoubleConstant (ec.BuiltinTypes, res, left.Location);
					} else if (left is FloatConstant){
						double a, b, res;
						a = ((FloatConstant) left).DoubleValue;
						b = ((FloatConstant) right).DoubleValue;
						
						if (ec.ConstantCheckState)
							res = checked (a % b);
						else
							res = unchecked (a % b);

						return new FloatConstant (ec.BuiltinTypes, res, left.Location);
					} else if (left is ULongConstant){
						ulong res;
						
						if (ec.ConstantCheckState)
							res = checked (((ULongConstant) left).Value %
								       ((ULongConstant) right).Value);
						else
							res = unchecked (((ULongConstant) left).Value %
									 ((ULongConstant) right).Value);

						return new ULongConstant (ec.BuiltinTypes, res, left.Location);
					} else if (left is LongConstant){
						long res;
						
						if (ec.ConstantCheckState)
							res = checked (((LongConstant) left).Value %
								       ((LongConstant) right).Value);
						else
							res = unchecked (((LongConstant) left).Value %
									 ((LongConstant) right).Value);

						return new LongConstant (ec.BuiltinTypes, res, left.Location);
					} else if (left is UIntConstant){
						uint res;
						
						if (ec.ConstantCheckState)
							res = checked (((UIntConstant) left).Value %
								       ((UIntConstant) right).Value);
						else
							res = unchecked (((UIntConstant) left).Value %
									 ((UIntConstant) right).Value);

						return new UIntConstant (ec.BuiltinTypes, res, left.Location);
					} else if (left is IntConstant){
						int res;

						if (ec.ConstantCheckState)
							res = checked (((IntConstant) left).Value %
								       ((IntConstant) right).Value);
						else
							res = unchecked (((IntConstant) left).Value %
									 ((IntConstant) right).Value);

						return new IntConstant (ec.BuiltinTypes, res, left.Location);
					} else {
						throw new Exception ( "Unexepected modulus input: " + left);
					}
				} catch (DivideByZeroException){
					ec.Report.Error (20, loc, "Division by constant zero");
				} catch (OverflowException){
					Error_CompileTimeOverflow (ec, loc);
				}
				break;

				//
				// There is no overflow checking on left shift
				//
			case Binary.Operator.LeftShift:
				if (left is NullLiteral && right is NullLiteral) {
					var lifted_int = new Nullable.NullableType (ec.BuiltinTypes.Int, loc);
					lifted_int.ResolveAsType (ec);
					return (Constant) new Binary (oper, lifted_int, right).ResolveOperator (ec);
				}

				IntConstant ic = right.ConvertImplicitly (ec.BuiltinTypes.Int) as IntConstant;
				if (ic == null){
					return null;
				}

				int lshift_val = ic.Value;
				switch (left.Type.BuiltinType) {
				case BuiltinTypeSpec.Type.ULong:
					return new ULongConstant (ec.BuiltinTypes, ((ULongConstant) left).Value << lshift_val, left.Location);
				case BuiltinTypeSpec.Type.Long:
					return new LongConstant (ec.BuiltinTypes, ((LongConstant) left).Value << lshift_val, left.Location);
				case BuiltinTypeSpec.Type.UInt:
					return new UIntConstant (ec.BuiltinTypes, ((UIntConstant) left).Value << lshift_val, left.Location);
				}

				// null << value => null
				if (left is NullLiteral)
					return (Constant) new Binary (oper, left, right).ResolveOperator (ec);

				left = left.ConvertImplicitly (ec.BuiltinTypes.Int);
				if (left.Type.BuiltinType == BuiltinTypeSpec.Type.Int)
					return new IntConstant (ec.BuiltinTypes, ((IntConstant) left).Value << lshift_val, left.Location);

				return null;

				//
				// There is no overflow checking on right shift
				//
			case Binary.Operator.RightShift:
				if (left is NullLiteral && right is NullLiteral) {
					var lifted_int = new Nullable.NullableType (ec.BuiltinTypes.Int, loc);
					lifted_int.ResolveAsType (ec);
					return (Constant) new Binary (oper, lifted_int, right).ResolveOperator (ec);
				}

				IntConstant sic = right.ConvertImplicitly (ec.BuiltinTypes.Int) as IntConstant;
				if (sic == null){
					return null;
				}
				int rshift_val = sic.Value;
				switch (left.Type.BuiltinType) {
				case BuiltinTypeSpec.Type.ULong:
					return new ULongConstant (ec.BuiltinTypes, ((ULongConstant) left).Value >> rshift_val, left.Location);
				case BuiltinTypeSpec.Type.Long:
					return new LongConstant (ec.BuiltinTypes, ((LongConstant) left).Value >> rshift_val, left.Location);
				case BuiltinTypeSpec.Type.UInt:
					return new UIntConstant (ec.BuiltinTypes, ((UIntConstant) left).Value >> rshift_val, left.Location);
				}

				// null >> value => null
				if (left is NullLiteral)
					return (Constant) new Binary (oper, left, right).ResolveOperator (ec);

				left = left.ConvertImplicitly (ec.BuiltinTypes.Int);
				if (left.Type.BuiltinType == BuiltinTypeSpec.Type.Int)
					return new IntConstant (ec.BuiltinTypes, ((IntConstant) left).Value >> rshift_val, left.Location);

				return null;

			case Binary.Operator.Equality:
				if (TypeSpec.IsReferenceType (lt) && TypeSpec.IsReferenceType (rt) ||
					(left is Nullable.LiftedNull && right.IsNull) ||
					(right is Nullable.LiftedNull && left.IsNull)) {
					if (left.IsNull || right.IsNull) {
						return ReducedExpression.Create (
							new BoolConstant (ec.BuiltinTypes, left.IsNull == right.IsNull, left.Location),
							new Binary (oper, left, right));
					}

					if (left is StringConstant && right is StringConstant)
						return new BoolConstant (ec.BuiltinTypes,
							((StringConstant) left).Value == ((StringConstant) right).Value, left.Location);

					return null;
				}

				if (!DoBinaryNumericPromotions (ec, ref left, ref right))
					return null;

				bool_res = false;
				if (left is DoubleConstant)
					bool_res = ((DoubleConstant) left).Value ==
						((DoubleConstant) right).Value;
				else if (left is FloatConstant)
					bool_res = ((FloatConstant) left).DoubleValue ==
						((FloatConstant) right).DoubleValue;
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

				return new BoolConstant (ec.BuiltinTypes, bool_res, left.Location);

			case Binary.Operator.Inequality:
				if (TypeSpec.IsReferenceType (lt) && TypeSpec.IsReferenceType (rt) ||
					(left is Nullable.LiftedNull && right.IsNull) ||
					(right is Nullable.LiftedNull && left.IsNull)) {
					if (left.IsNull || right.IsNull) {
						return ReducedExpression.Create (
							new BoolConstant (ec.BuiltinTypes, left.IsNull != right.IsNull, left.Location),
							new Binary (oper, left, right));
					}

					if (left is StringConstant && right is StringConstant)
						return new BoolConstant (ec.BuiltinTypes,
							((StringConstant) left).Value != ((StringConstant) right).Value, left.Location);

					return null;
				}

				if (!DoBinaryNumericPromotions (ec, ref left, ref right))
					return null;

				bool_res = false;
				if (left is DoubleConstant)
					bool_res = ((DoubleConstant) left).Value !=
						((DoubleConstant) right).Value;
				else if (left is FloatConstant)
					bool_res = ((FloatConstant) left).DoubleValue !=
						((FloatConstant) right).DoubleValue;
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

				return new BoolConstant (ec.BuiltinTypes, bool_res, left.Location);

			case Binary.Operator.LessThan:
				if (right is NullLiteral) {
					if (left is NullLiteral) {
						var lifted_int = new Nullable.NullableType (ec.BuiltinTypes.Int, loc);
						lifted_int.ResolveAsType (ec);
						return (Constant) new Binary (oper, lifted_int, right).ResolveOperator (ec);
					}
				}

				if (!DoBinaryNumericPromotions (ec, ref left, ref right))
					return null;

				bool_res = false;
				if (left is DoubleConstant)
					bool_res = ((DoubleConstant) left).Value <
						((DoubleConstant) right).Value;
				else if (left is FloatConstant)
					bool_res = ((FloatConstant) left).DoubleValue <
						((FloatConstant) right).DoubleValue;
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

				return new BoolConstant (ec.BuiltinTypes, bool_res, left.Location);
				
			case Binary.Operator.GreaterThan:
				if (right is NullLiteral) {
					if (left is NullLiteral) {
						var lifted_int = new Nullable.NullableType (ec.BuiltinTypes.Int, loc);
						lifted_int.ResolveAsType (ec);
						return (Constant) new Binary (oper, lifted_int, right).ResolveOperator (ec);
					}
				}

				if (!DoBinaryNumericPromotions (ec, ref left, ref right))
					return null;

				bool_res = false;
				if (left is DoubleConstant)
					bool_res = ((DoubleConstant) left).Value >
						((DoubleConstant) right).Value;
				else if (left is FloatConstant)
					bool_res = ((FloatConstant) left).DoubleValue >
						((FloatConstant) right).DoubleValue;
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

				return new BoolConstant (ec.BuiltinTypes, bool_res, left.Location);

			case Binary.Operator.GreaterThanOrEqual:
				if (right is NullLiteral) {
					if (left is NullLiteral) {
						var lifted_int = new Nullable.NullableType (ec.BuiltinTypes.Int, loc);
						lifted_int.ResolveAsType (ec);
						return (Constant) new Binary (oper, lifted_int, right).ResolveOperator (ec);
					}
				}

				if (!DoBinaryNumericPromotions (ec, ref left, ref right))
					return null;

				bool_res = false;
				if (left is DoubleConstant)
					bool_res = ((DoubleConstant) left).Value >=
						((DoubleConstant) right).Value;
				else if (left is FloatConstant)
					bool_res = ((FloatConstant) left).DoubleValue >=
						((FloatConstant) right).DoubleValue;
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

				return new BoolConstant (ec.BuiltinTypes, bool_res, left.Location);

			case Binary.Operator.LessThanOrEqual:
				if (right is NullLiteral) {
					if (left is NullLiteral) {
						var lifted_int = new Nullable.NullableType (ec.BuiltinTypes.Int, loc);
						lifted_int.ResolveAsType (ec);
						return (Constant) new Binary (oper, lifted_int, right).ResolveOperator (ec);
					}
				}

				if (!DoBinaryNumericPromotions (ec, ref left, ref right))
					return null;

				bool_res = false;
				if (left is DoubleConstant)
					bool_res = ((DoubleConstant) left).Value <=
						((DoubleConstant) right).Value;
				else if (left is FloatConstant)
					bool_res = ((FloatConstant) left).DoubleValue <=
						((FloatConstant) right).DoubleValue;
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

				return new BoolConstant (ec.BuiltinTypes, bool_res, left.Location);
			}

			return null;
		}
	}
}
