//
// StringFunctions.cs
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
	public abstract class StringFunction : UnaryExpression {
		protected StringFunction (IExpression e) : base (e) {}

		override public object Eval (DataRow row)
		{
			object val = expr.Eval (row);
			if(val == null)
				return null;
				
			if (!(val is string)) {
				string fnct = this.GetType ().ToString ();
				int start = fnct.LastIndexOf('.') + 1;
				fnct = fnct.Substring (start, fnct.Length - start - "Function".Length);
				throw new EvaluateException (String.Format ("'{0}' can be applied only to strings.", fnct));
			}
				
			return val;
		}
	}
	
	public class SubstringFunction : StringFunction {
		int start, len;
		public SubstringFunction (IExpression e, int start, int len) : base (e)
		{
			this.start = start;
			this.len = len;
		}
		
		override public object Eval (DataRow row)
		{
			string str = (string)base.Eval (row);
			if(str == null)
				return null;
				
			if (start > str.Length)
				return String.Empty;
			
			return str.Substring (start - 1, System.Math.Min (len, str.Length - (start - 1)));
		}
	}
	
	public class LenFunction : StringFunction {
		public LenFunction (IExpression e) : base (e) {}
		
		override public object Eval (DataRow row)
		{
			string str = (string)base.Eval (row);
			if(str == null)
				return 0;
				
			return str.Length;
		}
	}

	public class TrimFunction : StringFunction {
		public TrimFunction (IExpression e) : base (e) {}
		
		override public object Eval (DataRow row)
		{
			string str = (string)base.Eval (row);
			if(str == null)
				return null;
				
			return str.Trim();
		}
	}
}
