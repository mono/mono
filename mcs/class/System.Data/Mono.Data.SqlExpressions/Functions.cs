//
// Functions.cs
//
// Author:
//   Juraj Skripsky (juraj@hotfeet.ch)
//
// (C) 2004 HotFeet GmbH (http://www.hotfeet.ch)
//

using System;
using System.Collections;
using System.Data;

namespace Mono.Data.SqlExpressions {
	public class IifFunction : UnaryExpression {
		IExpression trueExpr, falseExpr;
		public IifFunction (IExpression e, IExpression trueExpr, IExpression falseExpr) : base (e)
		{
			this.trueExpr = trueExpr;
			this.falseExpr = falseExpr;
		}
		
		override public object Eval (DataRow row)
		{
			bool val = (bool)expr.Eval (row);
			return (val ? trueExpr.Eval (row) : falseExpr.Eval (row));
		}
	}
	
	public class IsNullFunction : UnaryExpression {
		IExpression defaultExpr;
		public IsNullFunction (IExpression e, IExpression defaultExpr) : base (e)
		{
			this.defaultExpr = defaultExpr;
		}
		
		override public object Eval (DataRow row)
		{
			object val = expr.Eval (row);
			return (val != null ? val : defaultExpr.Eval (row));
		}
	}
	
	public class ConvertFunction : UnaryExpression {
		Type targetType;
		public ConvertFunction (IExpression e, string targetType) : base (e)
		{
			try {
				this.targetType = Type.GetType (targetType, true);
			} catch (TypeLoadException) {
				throw new EvaluateException (String.Format ("Invalid type name '{0}'.", targetType));
			}
		}
		
		override public object Eval (DataRow row)
		{
			object val = expr.Eval (row);
			
			if (val.GetType () == targetType)
				return val;

			//--> String is always allowed			
			if (targetType == typeof (string))
				return val.ToString();
				
			//only TimeSpan <--> String is allowed
			if (targetType == typeof (TimeSpan)) {
				if (val is string)
					return TimeSpan.Parse ((string)val);
				else
					ThrowInvalidCastException (val);
			}
			
			if (val is TimeSpan)
				ThrowInvalidCastException (val);
			
			//only Char <--> String/Int32/UInt32 is allowed
			if (val is Char && !(targetType == typeof (Int32) || targetType == typeof (UInt32)))
				ThrowInvalidCastException (val);
				
			if (targetType == typeof (Char) && !(val is Int32 || val is UInt32))
				ThrowInvalidCastException (val);

			//bool <--> Char/Single/Double/Decimal/TimeSpan/DateTime is not allowed
			if (val is Boolean && (targetType == typeof (Single) || targetType == typeof (Double) || targetType == typeof (Decimal)))
				ThrowInvalidCastException (val);
				
			if (targetType == typeof(Boolean) && (val is Single || val is Double || val is Decimal))
				ThrowInvalidCastException (val);

			//Convert throws the remaining invalid casts
			return Convert.ChangeType (val, targetType);

		}
		
		private void ThrowInvalidCastException (object val) {
			throw new InvalidCastException (String.Format ("Type '{0}' cannot be converted to '{1}'.", val.GetType(), targetType));
		}
	}
}
