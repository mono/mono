//
// Functions.cs
//
// Author:
//   Juraj Skripsky (juraj@hotfeet.ch)
//
// (C) 2004 HotFeet GmbH (http://www.hotfeet.ch)
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections;
using System.Data;

namespace Mono.Data.SqlExpressions {
	internal class IifFunction : UnaryExpression {
		IExpression trueExpr, falseExpr;
		public IifFunction (IExpression e, IExpression trueExpr, IExpression falseExpr) : base (e)
		{
			this.trueExpr = trueExpr;
			this.falseExpr = falseExpr;
		}
		
		override public object Eval (DataRow row)
		{
			object o = expr.Eval (row);
			if (o == DBNull.Value)
				return o;
			bool val = (bool)o;
			return (val ? trueExpr.Eval (row) : falseExpr.Eval (row));
		}
	}
	
	internal class IsNullFunction : UnaryExpression {
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
	
	internal class ConvertFunction : UnaryExpression {
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
			
			if (val == DBNull.Value || val.GetType () == targetType)
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
