//
// System.Data.ExpressionElement 
//
// Author:
//   Ville Palo <vi64pa@kolumbus.fi>
//
// Copyright (C) Ville Palo, 2003
//
// TODO: - Some functionelements and aggregates.
//       - New parsing style.
//       - Exceptions
//

using System;
using System.Data;
using System.Reflection;

using System.Collections;
using System.Text;

namespace System.Data
{
#if false
	/// <summary>
	/// The main element which includes whole expression
	/// </summary>
	internal class ExpressionMainElement : ExpressionElement
	{
		
		

		public ExpressionMainElement (string s)
		{
			s = ExpressionElement.ValidateExpression (s);
			ParseExpression (s);
		}
		
		public override bool Test (DataRow Row) 
		{

			foreach (ExpressionElement El in Elements) 
			{
				if (!El.Test (Row))
					return false;
			}
			
			return true;
		}

		
	}

	//
	// O_P_E_R_A_T_O_R_S
	//

	/// <summary>
	///  Class for =
	/// </summary>
	internal class ExpressionEquals : ExpressionElement
	{	

		public ExpressionEquals (string exp1, string exp2) 
		{	
			this.exp1 = exp1;
			this.exp2 = exp2;
			ParseExpression (exp1);
			ParseExpression (exp2);
		}

		public override bool Test (DataRow Row) 
		{
			
			ExpressionElement E1 = (ExpressionElement)Elements [0];
			ExpressionElement E2 = (ExpressionElement)Elements [1];

			return ExpressionElement.Compare (E1, E2, Row) == 0;
		}
	}

	/// <summary>
	///  Class for <
	/// </summary>
	internal class ExpressionLessThan : ExpressionElement
	{	

		public ExpressionLessThan (string exp1, string exp2) 
		{	
			this.exp1 = exp1;
			this.exp2 = exp2;
			ParseExpression (exp1);
			ParseExpression (exp2);
		}

		public override bool Test (DataRow Row) 
		{
								
			ExpressionElement E1 = (ExpressionElement)Elements [0];
			ExpressionElement E2 = (ExpressionElement)Elements [1];
					   
			return ExpressionElement.Compare (E1, E2, Row) < 0;
		}
	}

	/// <summary>
	///  Class for <=
	/// </summary>
	internal class ExpressionLessThanOrEqual : ExpressionElement
	{	

		public ExpressionLessThanOrEqual (string exp1, string exp2) 
		{	
			this.exp1 = exp1;
			this.exp2 = exp2;
			ParseExpression (exp1);
			ParseExpression (exp2);
		}

		public override bool Test (DataRow Row) 
		{

			ExpressionElement E1 = (ExpressionElement)Elements [0];
			ExpressionElement E2 = (ExpressionElement)Elements [1];

			return ExpressionElement.Compare (E1, E2, Row) <= 0;
		}
	}

	/// <summary>
	///  Class for >
	/// </summary>
	internal class ExpressionGreaterThan : ExpressionElement
	{	

		public ExpressionGreaterThan (string exp1, string exp2) 
		{	
			this.exp1 = exp1;
			this.exp2 = exp2;
			ParseExpression (exp1);
			ParseExpression (exp2);
		}

		public override bool Test (DataRow Row) 
		{
			
			ExpressionElement E1 = (ExpressionElement)Elements [0];
			ExpressionElement E2 = (ExpressionElement)Elements [1];

			return ExpressionElement.Compare (E1, E2, Row) > 0;
		}
	}

	/// <summary>
	///  Class for >=
	/// </summary>
	internal class ExpressionGreaterThanOrEqual : ExpressionElement
	{	

		public ExpressionGreaterThanOrEqual (string exp1, string exp2) 
		{	
			this.exp1 = exp1;
			this.exp2 = exp2;
			ParseExpression (exp1);
			ParseExpression (exp2);
		}

		public override bool Test (DataRow Row) 
		{

			ExpressionElement E1 = (ExpressionElement)Elements [0];
			ExpressionElement E2 = (ExpressionElement)Elements [1];

			return ExpressionElement.Compare (E1, E2, Row) >= 0;
		}
	}

	/// <summary>
	///  Class for <>
	/// </summary>
	internal class ExpressionUnequals : ExpressionElement
	{	

		public ExpressionUnequals (string exp1, string exp2) 
		{	
			this.exp1 = exp1;
			this.exp2 = exp2;
			ParseExpression (exp1);
			ParseExpression (exp2);
		}

		public override bool Test (DataRow Row) 
		{
			
			ExpressionElement E1 = (ExpressionElement)Elements [0];
			ExpressionElement E2 = (ExpressionElement)Elements [1];

			return ExpressionElement.Compare (E1, E2, Row) != 0;
		}
	}


	/// <summary>
	///  Class for LIKE-operator
	/// </summary>
	internal class ExpressionLike : ExpressionElement
	{	

		public ExpressionLike (string exp1, string exp2) 
		{
			ParseExpression (exp1);
			ParseExpression (exp2);
		}

		public override bool Test (DataRow Row) 
		{

			ExpressionElement E1 = (ExpressionElement)Elements [0];
			ExpressionElement E2 = (ExpressionElement)Elements [1];
			object value1 = E1.Result (Row);
			object value2 = E2.Result (Row);
			
			if (value1.GetType () != typeof (string) || value2.GetType () != typeof (string))
				throw new Exception (); // TODO: what exception
			
			string operand1 = value1.ToString ();
			string operand2 = value2.ToString ();

			// find out is there wildcards like * or %.
			while (operand2.EndsWith ("*") || operand2.EndsWith ("%")) 			       
				operand2 = operand2.Remove (operand2.Length - 1, 1);
			while (operand2.StartsWith ("*") || operand2.StartsWith ("%"))
				operand2 = operand2.Remove (0, 1);

			int oldIndex = 0;
			int indexOf = -1;

			indexOf = operand2.IndexOf ("*");
			while (indexOf != -1) 
			{

				oldIndex = indexOf + 1;
				if (operand2 [indexOf + 1] != ']' || operand2 [indexOf - 1] != '[')
					throw new EvaluateException ("Error in Like operator: ther string pattern " + operand1 + " is invalid");
				else 
				{
					operand2 = operand2.Remove (indexOf + 1, 1);
					operand2 = operand2.Remove (indexOf -1, 1);
					oldIndex--;
				}
					
				indexOf = operand2.IndexOf ("*", oldIndex);
			}

			oldIndex = 0;
			indexOf = operand2.IndexOf ("%");
			while (indexOf != -1) 
			{

				oldIndex = indexOf + 1;
				
				if (operand2 [indexOf + 1] != ']' || operand2 [indexOf - 1] != '[')
					throw new EvaluateException ("Error in Like operator: ther string pattern " + operand2 + " is invalid");
				else 
				{
					operand2 = operand2.Remove (indexOf + 1, 1);
					operand2 = operand2.Remove (indexOf -1, 1);					
					oldIndex--;
				}

				indexOf = operand2.IndexOf ("%", oldIndex);
			}

			int len2 = operand2.Length;
			int startIndex = 0;
			while ((startIndex + len2) <= operand1.Length) 
			{
				if (String.Compare (operand1.Substring (startIndex, len2), operand2, !Row.Table.CaseSensitive) == 0)
					return true;
				startIndex++;
			}

			return false;
		}
	}


	/// <summary>
	///  Class for OR
	/// </summary>
	internal class ExpressionOr : ExpressionElement
	{        	        	
		public ExpressionOr (string exp1, string exp2)
		{
			ParseExpression (exp1);
			ParseExpression (exp2);
		}

		public override bool Test (DataRow Row) 
		{			
			foreach (ExpressionElement El in Elements) 
			{
				if (El.Test (Row))
					return true;
			}
			
			return false;
		}		        	
	}
		
	/// <summary>
	///  Class for AND
	/// </summary>
	internal class ExpressionAnd : ExpressionElement
	{        	        	
		public ExpressionAnd (string exp1, string exp2)
		{
			ParseExpression (exp1);
			ParseExpression (exp2);
		}
	       
		public override object Result (DataRow Row) 
		{
			
			return Test(Row);
		}

		public override bool Test (DataRow Row) 
		{
			foreach (ExpressionElement El in Elements) 
			{
				if (!El.Test (Row))
					return false;
			}
			
			return true;
		}		        	
	}


	//
	// A_R_I_T_H_M_E_T_I_C  O_P_E_R_A_T_O_R_S
	//

	/// <summary>
	///  Class for +
	/// </summary>
	internal class ExpressionAddition : ExpressionElement
	{
		public ExpressionAddition (string exp1, string exp2)
		{        		
			this.exp1 = exp1;
			this.exp2 = exp2;
			ParseExpression (exp1);
			ParseExpression (exp2);
		}
        	
		public override Type ResultType (DataRow Row)
		{
			Type ResultType = typeof (string);
			ExpressionElement exp1Temp = ((ExpressionElement)Elements [0]);
			ExpressionElement exp2Temp = ((ExpressionElement)Elements [1]);

			if (exp1Temp.ResultType (Row) == typeof (string) || exp2Temp.ResultType (Row) == typeof (string))
				ResultType = typeof (string);

			else if (exp1Temp.ResultType (Row) == typeof (long) || exp2Temp.ResultType (Row) == typeof (long))
				ResultType = typeof (long);

			else if (exp1Temp.ResultType (Row) == typeof (int) || exp2Temp.ResultType (Row) == typeof (int))
				ResultType = typeof (int);

			return ResultType;
		}

		public override object Result (DataRow Row) 
		{
			return CalculateResult (Row);
		}
        	
		protected override object Calculate (object value1, object value2, Type TempType) 
		{
			object Result = null;			

			if (TempType == typeof (string))
				Result = (string)value1 + (string)value2;
			else if (TempType == typeof (long))
				Result = (long)value1 + (long)value2;
			else if (TempType == typeof (int))
				Result = (int)value1 + (int)value2;
			else if (TempType == typeof (short))
				Result = (short)value1 + (short)value2;
			else if (TempType == typeof (ulong))
				Result = (ulong)value1 + (ulong)value2;
			else if (TempType == typeof (uint))
				Result = (uint)value1 + (uint)value2;
			else if (TempType == typeof (ushort))
				Result = (ushort)value1 + (ushort)value2;
			else if (TempType == typeof (byte))
				Result = (byte)value1 + (byte)value2;
			else if (TempType == typeof (sbyte))
				Result = (sbyte)value1 + (sbyte)value2;
				// FIXME:
				//else if (TempType == typeof (bool))
				//	Result = (bool)value1 + (bool)value2;
			else if (TempType == typeof (float))
				Result = (float)value1 + (float)value2;
			else if (TempType == typeof (double))
				Result = (double)value1 + (double)value2;
			else if (TempType == typeof (decimal))
				Result = (decimal)value1 + (decimal)value2;
			// FIXME:
			//else if (TempType == typeof (DateTime))
			//	Result = (DateTime)value1 + (DateTime)value2;
			
			return Result;
		}


		// This method is shouldnt never invoked
		public override bool Test (DataRow Row)
		{
			throw new EvaluateException ();
		}
	}

	/// <summary>
	///  Class for -
	/// </summary>
	internal class ExpressionSubtraction : ExpressionElement
	{
		public ExpressionSubtraction (string exp1, string exp2)
		{        		
			this.exp1 = exp1;
			this.exp2 = exp2;
			ParseExpression (exp1);
			ParseExpression (exp2);
		}
        	
		public override object Result (DataRow Row) 
		{        		
			return CalculateResult (Row);
		}
        	
		// This method is shouldnt never invoked
		public override bool Test (DataRow Row)
		{
			throw new EvaluateException ();
		}

		protected override object Calculate (object value1, object value2, Type TempType) 
		{
			object Result = null;			

			// FIXME:
			//if (TempType == typeof (string))
			//	Result = (string)value1 - (string)value2;
			if (TempType == typeof (long))
				Result = (long)value1 - (long)value2;
			else if (TempType == typeof (int))
				Result = (int)value1 - (int)value2;
			else if (TempType == typeof (short))
				Result = (short)value1 - (short)value2;
			else if (TempType == typeof (ulong))
				Result = (ulong)value1 + (ulong)value2;
			else if (TempType == typeof (uint))
				Result = (uint)value1 - (uint)value2;
			else if (TempType == typeof (ushort))
				Result = (ushort)value1 - (ushort)value2;
			else if (TempType == typeof (byte))
				Result = (byte)value1 - (byte)value2;
			else if (TempType == typeof (sbyte))
				Result = (sbyte)value1 - (sbyte)value2;
				// FIXME:
				//else if (TempType == typeof (bool))
				//	Result = (bool)value1 - (bool)value2;
			else if (TempType == typeof (float))
				Result = (float)value1 - (float)value2;
			else if (TempType == typeof (double))
				Result = (double)value1 - (double)value2;
			else if (TempType == typeof (decimal))
				Result = (decimal)value1 - (decimal)value2;
			// FIXME:
			//else if (TempType == typeof (DateTime))
			//	Result = (DateTime)value1 - (DateTime)value2;
			
			return Result;
		}
	}

	/// <summary>
	///  Class for *
	/// </summary>
	internal class ExpressionMultiply : ExpressionElement
	{
		public ExpressionMultiply (string exp1, string exp2)
		{        		
			this.exp1 = exp1;
			this.exp2 = exp2;
			ParseExpression (exp1);
			ParseExpression (exp2);
		}
        	
		public override Type ResultType (DataRow Row)
		{
			Type ResultType = null;
			ExpressionElement E1 = ((ExpressionElement)Elements [0]);
			ExpressionElement E2 = ((ExpressionElement)Elements [1]);
			Type t1 = E1.ResultType (Row);
			Type t2 = E2.ResultType (Row);
				
			if (t1 == typeof (string) || t2 == typeof (string))
				throw new EvaluateException ("Cannon perform '*' operation on " + t1.ToString () + 
					" and " + t2.ToString ());

			else if (t1 == typeof (long) || t2 == typeof (long))
				ResultType = typeof (long);

			else if (t1 == typeof (int) || t2 == typeof (int))
				ResultType = typeof (int);

			return ResultType;
		}

		public override object Result (DataRow Row) 
		{
			return CalculateResult (Row);
		}
        	
		public override bool Test (DataRow Row)
		{
			throw new EvaluateException ();
		}

		protected override object Calculate (object value1, object value2, Type TempType) 
		{
			object Result = null;			

			if (TempType == typeof (long))
				Result = (long)value1 * (long)value2;
			else if (TempType == typeof (int))
				Result = (int)value1 * (int)value2;
			else if (TempType == typeof (short))
				Result = (short)value1 * (short)value2;
			else if (TempType == typeof (ulong))
				Result = (ulong)value1 * (ulong)value2;
			else if (TempType == typeof (uint))
				Result = (uint)value1 * (uint)value2;
			else if (TempType == typeof (ushort))
				Result = (ushort)value1 * (ushort)value2;
			else if (TempType == typeof (byte))
				Result = (byte)value1 * (byte)value2;
			else if (TempType == typeof (sbyte))
				Result = (sbyte)value1 * (sbyte)value2;
				// FIXME:
				//else if (TempType == typeof (bool))
				//	Result = (bool)value1 * (bool)value2;
			else if (TempType == typeof (float))
				Result = (float)value1 * (float)value2;
			else if (TempType == typeof (double))
				Result = (double)value1 * (double)value2;
			else if (TempType == typeof (decimal))
				Result = (decimal)value1 * (decimal)value2;
			// FIXME:
			//else if (TempType == typeof (DateTime))
			//	Result = (DateTime)value1 * (DateTime)value2;
			
			return Result;
		}

	}

	/// <summary>
	///  Class for *
	/// </summary>
	internal class ExpressionDivide : ExpressionElement
	{
		public ExpressionDivide (string exp1, string exp2)
		{        		
			this.exp1 = exp1;
			this.exp2 = exp2;
			ParseExpression (exp1);
			ParseExpression (exp2);
		}
        	
		public override object Result (DataRow Row) 
		{
			return CalculateResult (Row);
		}
        	
		// This method is shouldnt never invoked
		public override bool Test (DataRow Row)
		{
			throw new EvaluateException ();
		}

		protected  override object Calculate (object value1, object value2, Type TempType) 
		{
			object Result = null;			

			if (TempType == typeof (long))
				Result = (long)value1 / (long)value2;
				// FIXME: 
				//else if (TempType == typeof (int))
				//	Result = (string)value1 / (string)value2;
			else if (TempType == typeof (int))
				Result = (int)value1 / (int)value2;
			else if (TempType == typeof (short))
				Result = (short)value1 / (short)value2;
			else if (TempType == typeof (ulong))
				Result = (ulong)value1 / (ulong)value2;
			else if (TempType == typeof (uint))
				Result = (uint)value1 / (uint)value2;
			else if (TempType == typeof (ushort))
				Result = (ushort)value1 / (ushort)value2;
			else if (TempType == typeof (byte))
				Result = (byte)value1 / (byte)value2;
			else if (TempType == typeof (sbyte))
				Result = (sbyte)value1 / (sbyte)value2;
				// FIXME:
				//else if (TempType == typeof (bool))
				//	Result = (bool)value1 // (bool)value2;
			else if (TempType == typeof (float))
				Result = (float)value1 / (float)value2;
			else if (TempType == typeof (double))
				Result = (double)value1 / (double)value2;
			else if (TempType == typeof (decimal))
				Result = (decimal)value1 / (decimal)value2;
			// FIXME:
			//else if (TempType == typeof (DateTime))
			//	Result = (DateTime)value1 / (DateTime)value2;
			
			return Result;
		}
	}

	/// <summary>
	///  Class for *
	/// </summary>
	internal class ExpressionModulus : ExpressionElement
	{
		public ExpressionModulus (string exp1, string exp2)
		{        		
			this.exp1 = exp1;
			this.exp2 = exp2;
			ParseExpression (exp1);
			ParseExpression (exp2);
		}
        	
		public override object Result (DataRow Row) 
		{
			return CalculateResult (Row);
		}
        	
		// This method is shouldnt never invoked
		public override bool Test (DataRow Row)
		{
			throw new EvaluateException ();
		}

		protected  override object Calculate (object value1, object value2, Type TempType) 
		{
			object Result = null;			

			if (TempType == typeof (long))
				Result = (long)value1 % (long)value2;
				// FIXME: 
				//else if (TempType == typeof (int))
				//	Result = (string)value1 % (string)value2;
			else if (TempType == typeof (int))
				Result = (int)value1 % (int)value2;
			else if (TempType == typeof (short))
				Result = (short)value1 % (short)value2;
			else if (TempType == typeof (ulong))
				Result = (ulong)value1 % (ulong)value2;
			else if (TempType == typeof (uint))
				Result = (uint)value1 % (uint)value2;
			else if (TempType == typeof (ushort))
				Result = (ushort)value1 % (ushort)value2;
			else if (TempType == typeof (byte))
				Result = (byte)value1 % (byte)value2;
			else if (TempType == typeof (sbyte))
				Result = (sbyte)value1 % (sbyte)value2;
				// FIXME:
				//else if (TempType == typeof (bool))
				//	Result = (bool)value1 // (bool)value2;
			else if (TempType == typeof (float))
				Result = (float)value1 % (float)value2;
			else if (TempType == typeof (double))
				Result = (double)value1 % (double)value2;
			else if (TempType == typeof (decimal))
				Result = (decimal)value1 % (decimal)value2;
			// FIXME:
			//else if (TempType == typeof (DateTime))
			//	Result = (DateTime)value1 / (DateTime)value2;
			
			return Result;
		}
	}

	//
	// _____A_G_G_R_E_G_A_T_E_S_____
	//

	internal class ExpressionAggregate : ExpressionElement
	{
        	
		public ExpressionAggregate()
		{
		}

		public ExpressionAggregate(string s)
		{
			s = ExpressionElement.ValidateExpression (s);
			ParseExpression (s);
		}

        	
		public override bool Test (DataRow Row)
		{
			throw new EvaluateException ();
		}

		public override object Result(DataRow Row)
		{
			DataRow[] rows = new DataRow[Row.Table.Rows.Count];
			Row.Table.Rows.CopyTo(rows, 0);
			return ((ExpressionAggregate)Elements[0]).Result(rows);
		}

		public virtual object Result(DataRow[] rows)
		{
			return ((ExpressionAggregate)Elements[0]).Result(rows);
		}


		protected virtual void ParseParameters (string s)
		{
			string stemp = s.ToLower ();
			bool inString = false;
			string p1 = null;

			// find (
			while (!s.StartsWith ("("))
				s = s.Remove (0, 1);
			
			// remove (
			s = s.Remove (0, 1);

			int parentheses = 0;
			for (int i = 0; i < s.Length; i++) 
			{

				if (s [i] == '\'')
					inString = !inString;
				else if (s [i] == '(')
					parentheses++;
				else if (s [i] == ')')
					parentheses--;

				if ((s [i] == ',' ||  s [i] == ')') && !inString && parentheses == -1) 
				{ // Parameter changed

					if (p1 == null) 
					{
						p1 = s.Substring (0, i);
						break;
					}
				}
			}

			if (p1 == null)
				throw new Exception ();

			ParseExpression (p1);		
		}
		
	}

	/// <summary>
	///  Class for Sum (column_Name)
	/// </summary
	internal class ExpressionSum : ExpressionAggregate
	{
		public ExpressionSum (string exp1)
		{
			ParseParameters (exp1);
		}

		public override object Result(DataRow[] rows)
		{
			ExpressionElement E1 = (ExpressionElement)Elements [0];
			object value1 = E1.Result (rows[0]);
			Type t1 = value1.GetType ();
			object result = 0;
			
			// This could be optimized. If E1 is single element (Not child or parent) the
			// result of Sum() aggregate is allways same

			if (E1 is ExpressionSingleElement) 
			{
				
				// This should be optimized somehow
				for (int i = 0; i < rows.Length; i++) 
				{

					// TODO: other types and exceptions
					object v = E1.Result (rows[i]);
					t1 = v.GetType ();

					if (v == null || v == DBNull.Value)
						continue;

					if (t1 == typeof (long)) 
					{
						result = (long)result + (long)v;
					}
					else if (t1 == typeof (int)) 
					{
						result = (int)result + (int)v;
					}
					else if (t1 == typeof (short)) 
					{
						result = (short)result + (short)v;
					}
					else if (t1 == typeof (double)) 
					{
						result = (double)result + (double)v;
					}
					else if (t1 == typeof (float)) 
					{
						result = (float)result + (float)v;
					}
					else
						throw new NotImplementedException ();
				}
			}
			
			return result;
		}

        	
		//
		// Copy: This method is copy-paste in every Aggregate class.
		//
	}

	/// <summary>
	///  Class for Avg (column_Name)
	/// </summary
	internal class ExpressionAvg : ExpressionAggregate
	{
		public ExpressionAvg (string exp1)
		{
			ParseParameters (exp1);
		}

		public override object Result(DataRow[] rows)
		{
			ExpressionElement E1 = (ExpressionElement)Elements [0];
			object value1 = E1.Result (rows[0]);
			Type original = value1.GetType ();
			object result = null;
			
			if (E1 is ExpressionSingleElement) 
			{
				
				Type t1 = null;
				// This should be optimized somehow
				for (int i = 0; i < rows.Length; i++) 
				{
				       
					// TODO: other types and exceptions
					object v = E1.Result (rows[i]);

					if (v == null || v == DBNull.Value)
						continue;

					t1 = v.GetType ();

					if (result == null)
						result = 0;
					
					if (t1 == typeof (long)) 
					{
						result = (long)result + (long)v;
					}
					else if (t1 == typeof (int)) 
					{
						result = (int)result + (int)v;
					}
					else if (t1 == typeof (short)) 
					{
						result = (short)result + (short)v;
					}
					else if (t1 == typeof (double)) 
					{
						result = (double)result + (double)v;
					}
					else if (t1 == typeof (float)) 
					{
						result = (float)result + (float)v;
					}
					else
						throw new NotImplementedException ();
				}

				// TODO: types

				if (t1 == typeof (long))
					result = (long)result / rows.Length;
				else if (t1 == typeof (int))
					result = (int)result / rows.Length;
				else if (t1 == typeof (short))
					result = (short)result / rows.Length;
				else if (t1 == typeof (double))
					result = (double)result / rows.Length;
			}
			
			return result;
		}

		/// <summary>
		///  This is used from ExpressionStdDev for evaluating avg.
		/// </summary>
		public ExpressionAvg (ExpressionElement E)
		{
			Elements.Add (E);
		}
      	
	}

	/// <summary>
	///  Class for Min (column_Name)
	/// </summary
	internal class ExpressionMin : ExpressionAggregate
	{
		public ExpressionMin (string exp1)
		{
			ParseParameters (exp1);
		}

		public override object Result(DataRow[] rows)
		{
			ExpressionElement E1 = (ExpressionElement)Elements [0];
			object value1 = E1.Result (rows[0]);
			Type original = value1.GetType ();
			object result = null;
			
			if (E1 is ExpressionSingleElement) 
			{
				
				Type t1 = null;
				// This should be optimized somehow
				for (int i = 0; i < rows.Length; i++)
				{
				       
					// TODO: other types and exceptions
					object v = E1.Result (rows[i]);

					if (v == null || v == DBNull.Value)
						continue;

					t1 = v.GetType ();

					if (result == null)
						result = 0;

					object CompResult = t1.InvokeMember ("CompareTo", BindingFlags.Default | 
						BindingFlags.InvokeMethod, null, 
						v, 
						new object [] {result});

					if ((int)CompResult < 0)
						result = v;

				}
			}
			
			return result;
		}

        	
	}

	/// <summary>
	///  Class for Max (column_Name)
	/// </summary
	internal class ExpressionMax : ExpressionAggregate
	{
		public ExpressionMax (string exp1)
		{
			ParseParameters (exp1);
		}

		public override object Result(DataRow[] rows)
		{
			ExpressionElement E1 = (ExpressionElement)Elements [0];
			object value1 = E1.Result (rows[0]);
			Type original = value1.GetType ();
			object result = null;
			
			if (E1 is ExpressionSingleElement) 
			{
				
				Type t1 = null;
				// This should be optimized somehow
				for (int i = 0; i < rows.Length; i++) 
				{
				       
					// TODO: other types and exceptions
					object v = E1.Result (rows[i]);

					if (v == null || v == DBNull.Value)
						continue;

					t1 = v.GetType ();

					if (result == null)
						result = 0;

					object CompResult = t1.InvokeMember ("CompareTo", BindingFlags.Default | 
						BindingFlags.InvokeMethod, null, 
						v, 
						new object [] {result});

					if ((int)CompResult > 0)
						result = v;

				}
			}
			
			return result;
		}

        	
	}


	/// <summary>
	///  Class for count (column)
	/// </summary>
	internal class ExpressionCount : ExpressionAggregate
	{
		public ExpressionCount (string exp1)
		{
			ParseParameters (exp1);
		}
		
		public override object Result(DataRow[] rows)
		{
			ExpressionElement E1 = (ExpressionElement)Elements [0];
			int count = 0;

			if (E1 is ExpressionSingleElement) 
				count = rows.Length;
			
			return count;
		}

		public override object Result (DataRow Row) 
		{
			ExpressionElement E1 = (ExpressionElement)Elements [0];
			int count = 0;

			if (E1 is ExpressionSingleElement) 
			{
				
				count = Row.Table.Rows.Count;
			}
			
			return count;
		}
	}


	/// <summary>
	///  Class for StdDev (column)
	/// </summary>
	internal class ExpressionStdev : ExpressionAggregate
	{
		public ExpressionStdev (string exp1)
		{		
			ParseParameters (exp1);
		}

		public override object Result(DataRow[] rows)
		{
			ExpressionElement E1 = (ExpressionElement)Elements [0];
			ExpressionAvg Avg = new ExpressionAvg (E1);

			object tempAvg = Avg.Result (rows[0]);
			double avg = 0;
			double sum = 0;
			double result = 0;

			if (tempAvg.GetType () == typeof (int))
				avg = (double)(int)tempAvg;
			
			if (E1 is ExpressionSingleElement) 
			{

				for (int i = 0; i <rows.Length; i++)
				{
					// (value - avg)Â²
					object v = E1.Result (rows[i]);

					if (v == null || v == DBNull.Value)
						continue;

					if (v.GetType () == typeof (long))
						sum = avg - (long)v;
					else if (v.GetType () == typeof (int))
						sum = avg - (int)v;
					else if (v.GetType () == typeof (short))
						sum = avg - (short)v;
					else
						throw new NotImplementedException ();

					result += Math.Pow (sum, 2);
				}
				
				result = result / (rows.Length - 1);
				result = Math.Sqrt (result);
			}

			return result;
		}
		
        	
	}

	/// <summary>
	///  Class for Var (column)
	/// </summary>
	internal class ExpressionVar : ExpressionAggregate
	{
		public ExpressionVar (string exp1)
		{
			ParseParameters (exp1);
		}

		public override object Result(DataRow[] rows)
		{
			ExpressionElement E1 = (ExpressionElement)Elements [0];
			ExpressionAvg Avg = new ExpressionAvg (E1);

			object tempAvg = Avg.Result (rows[0]);
			double avg = 0;
			double sum = 0;
			double result = 0;

			if (tempAvg.GetType () == typeof (int))
				avg = (double)(int)tempAvg;
			
			if (E1 is ExpressionSingleElement) 
			{

				for (int i = 0; i < rows.Length; i++)
				{

				       
					// (value - avg)Â²
					object v = E1.Result (rows[i]);

					if (v == null || v == DBNull.Value)
						continue;

					if (v.GetType () == typeof (long))
						sum = avg - (long)v;
					else if (v.GetType () == typeof (int))
						sum = avg - (int)v;
					else if (v.GetType () == typeof (short))
						sum = avg - (short)v;
					else
						throw new NotImplementedException ();

					result += Math.Pow (sum, 2);
				}
				
				result = result / (rows.Length - 1);
			}

			return result;
		}
		
        	
	}

	// 
	// _____F_U_ N_C_T_I_O_N_S_______
	//

	/// <summary>
	///  Class for len (string) function
	/// </summary>
	internal class ExpressionLen : ExpressionElement
	{
		public ExpressionLen (string exp1)
		{        		
			_ResultType = typeof (int);
			ParseParameters (exp1);
		}
        	
		public override object Result (DataRow Row) 
		{
			ExpressionElement E1 = ((ExpressionElement)Elements [0]);
			object value1 = E1.Result (Row);
			
			return value1.ToString ().Length;
		}
        	
		public override bool Test (DataRow Row)
		{
			throw new EvaluateException ();
		}

		public void ParseParameters (string s)
		{
			string stemp = s.ToLower ();
			bool inString = false;
			string p1 = null;

			// find (
			while (!s.StartsWith ("("))
				s = s.Remove (0, 1);

			// remove (
			s = s.Remove (0, 1);
			int parentheses = 0;
			for (int i = 0; i < s.Length; i++) 
			{

				if (s [i] == '\'')
					inString = !inString;
				else if (s [i] == '(')
					parentheses++;
				else if (s [i] == ')')
					parentheses--;

				if ((s [i] == ',' ||  s [i] == ')') && !inString && parentheses == -1) 
				{ // Parameter changed

					if (p1 == null) 
					{
						p1 = s.Substring (0, i);
						break;
					}
				}
			}

			if (p1 == null)
				throw new Exception ();

			ParseExpression (p1);		
		}
	}

	/// <summary>
	///  Class for iif (exp1, truepart, falsepart) function
	/// </summary>
	internal class ExpressionIif : ExpressionElement
	{
		public ExpressionIif (string exp)
		{       
			ParseParameters (exp);
		}

		public override object Result (DataRow Row) 
		{
			ExpressionElement E1 = ((ExpressionElement)Elements [0]);
			ExpressionElement E2 = ((ExpressionElement)Elements [1]);
			ExpressionElement E3 = ((ExpressionElement)Elements [2]);

			if (E1.Test (Row)) // expression
				return E2.Result (Row); // truepart
			else
				return E3.Result (Row); // false part			
		}
        	
		// This method is shouldnt never invoked
		public override bool Test (DataRow Row)
		{
			throw new EvaluateException ();
		}

		public override Type ResultType (DataRow Row)
		{						
			ExpressionElement E1 = ((ExpressionElement)Elements [0]);
			ExpressionElement E2 = ((ExpressionElement)Elements [1]);
			ExpressionElement E3 = ((ExpressionElement)Elements [2]);
			
			if (E1.Test (Row)) // expression
				return E2.Result (Row).GetType (); // truepart
			else
				return E3.Result (Row).GetType (); // false part			
		}

		/// <summary>
		///  Parses expressions in parameters (exp, truepart, falsepart)
		/// </summary>
		private void ParseParameters (string s)
		{
			bool inString = false;
			string stemp = s.ToLower ();
			string p1 = null;
			string p2 = null;
			string p3 = null;
			s = s.Substring (stemp.IndexOf ("iif") + 3);

			// find (
			while (!s.StartsWith ("("))
				s = s.Remove (0, 1);

			// remove (
			s = s.Remove (0, 1);
			int parentheses = 0;
			for (int i = 0; i < s.Length; i++) 
			{

				if (s [i] == '\'')
					inString = !inString;
				else if (s [i] == '(')
					parentheses++;
				else if (s [i] == ')')
					parentheses--;

				if ((s [i] == ',' && !inString && parentheses == 0) || 
					(s [i] == ')' && i == (s.Length -1))) 
				{ // Parameter changed

					if (p1 == null) 
					{
						p1 = s.Substring (0, i);
						s = s.Substring (i + 1);
						i = 0;
					}

					else if (p2 == null) 
					{
						p2 = s.Substring (0, i);
						s = s.Substring (i + 1);
						i = 0;
					}

					else if (p3 == null) 
					{
						p3 = s.Substring (0, i);
						s = s.Substring (i + 1);
						i = 0;
					}

					else
						throw new Exception (); // FIXME: What exception
				}
			}

			if (p1 == null || p2 == null || p3 == null)
				throw new Exception ();

			ParseExpression (p1);
			ParseExpression (p2);
			ParseExpression (p3);
		}
	}

	/// <summary>
	///  Class for isnull (expression, returnvalue) function
	/// </summary>
	internal class ExpressionIsNull : ExpressionElement
	{
		public ExpressionIsNull (string exp)
		{        		
			ParseParameters (exp);
		}
        	
		public override object Result (DataRow Row) 
		{
			ExpressionElement E1 = (ExpressionElement)Elements [0];
			ExpressionElement E2 = (ExpressionElement)Elements [1];
			
			object R1 = E1.Result (Row);
			object value1 = null;
			if (R1 == null || R1 == DBNull.Value)
				return E2.Result (Row);
			else
				return R1;
		}

		public override Type ResultType (DataRow Row)
		{
			ExpressionElement E1 = (ExpressionElement)Elements [0];
			ExpressionElement E2 = (ExpressionElement)Elements [1];
			
			object R1 = E1.Result (Row);
			object value1 = null;
			if (R1 == null || R1 == DBNull.Value)
				return E2.Result (Row).GetType ();
			else
				return R1.GetType ();
		}
        	
		/// <summary>
		///  IsNull function does not return boolean value, so throw exception
		/// </summary>
		public override bool Test (DataRow Row)
		{
			throw new EvaluateException ();
		}

		/// <summary>
		///  Parses parameters of function and invoke ParseExpression methods
		/// </summary>
		private void ParseParameters (string s)
		{
			bool inString = false;
			string stemp = s.ToLower ();
			string p1 = null;
			string p2 = null;

			s = s.Substring (stemp.IndexOf ("isnull") + 6);

			// find (
			while (!s.StartsWith ("("))
				s = s.Remove (0, 1);

			// remove (
			s = s.Remove (0, 1);
			int parentheses = 0;
			for (int i = 0; i < s.Length; i++) 
			{

				if (s [i] == '\'')
					inString = !inString;
				else if (s [i] == '(')
					parentheses++;
				else if (s [i] == ')')
					parentheses--;

				if ((s [i] == ',' && !inString && parentheses == 0) || 
					(s [i] == ')' && i == (s.Length -1))) 
				{ // Parameter changed

					if (p1 == null) 
					{
						p1 = s.Substring (0, i);
						s = s.Substring (i + 1);
						i = 0;
					}

					else if (p2 == null) 
					{
						p2 = s.Substring (0, i);
						s = s.Substring (i + 1);
						i = 0;
					}

					else
						throw new Exception (); // FIXME: What exception
				}
			}

			if (p1 == null || p2 == null)
				throw new Exception ();

			ParseExpression (p1);
			ParseExpression (p2);
		}
	}

	/// <summary>
	///  Class for Substring (expression, start, length) function
	/// </summary>
	internal class ExpressionSubstring : ExpressionElement
	{
		public ExpressionSubstring (string exp)
		{        		
			ParseParameters (exp);
			_ResultType = typeof (string);
		}
        	
		public override object Result (DataRow Row) 
		{
			ExpressionElement E1 = (ExpressionElement)Elements [0];
			ExpressionElement E2 = (ExpressionElement)Elements [1];
			ExpressionElement E3 = (ExpressionElement)Elements [2];
			
			object value1 = E1.Result (Row);
			object value2 = E2.Result (Row);
			object value3 = E3.Result (Row);
			Type t1 = value1.GetType ();
			Type t2 = value2.GetType ();
			Type t3 = value3.GetType ();

			if (value1 == null || value2 == null || value3 == null 
				|| value1 == DBNull.Value || value2 == DBNull.Value || value3 == DBNull.Value)
				return string.Empty;

			if (t1 != typeof (string))
				throw new Exception (); // FIXME: what exception
			else if (t2 != typeof (int))
				throw new EvaluateException ("Type mismatch is function argument: Substring (), argument 2, excepted System.Int32");
			else if (t3 != typeof (int))
				throw new EvaluateException ("Type mismatch is function argument: Substring (), argument 3, excepted System.Int32");

			string str = value1.ToString ();
			int start = (int)value2;
			int length = (int)value3;

			if (str.Length < start)
				str =  string.Empty;
			else 
			{
				if ((start + length - 1) > str.Length)
					str = str.Substring (start - 1);
				else
					str = str.Substring (start - 1, length);
			}

			return str;
		}

		/// <summary>
		///  IsNull function does not return boolean value, so throw exception
		/// </summary>
		public override bool Test (DataRow Row)
		{
			throw new EvaluateException ();
		}

		/// <summary>
		///  Parses parameters of function and invoke ParseExpression methods
		/// </summary>
		private void ParseParameters (string s)
		{
			bool inString = false;
			string stemp = s.ToLower ();
			string p1 = null;
			string p2 = null;
			string p3 = null;

			s = s.Substring (stemp.IndexOf ("substring") + 9);

			// find (
			while (!s.StartsWith ("("))
				s = s.Remove (0, 1);

			// remove (
			s = s.Remove (0, 1);
			int parentheses = 0;
			for (int i = 0; i < s.Length; i++) 
			{

				if (s [i] == '\'')
					inString = !inString;
				else if (s [i] == '(')
					parentheses++;
				else if (s [i] == ')')
					parentheses--;


				if ((s [i] == ',' && !inString && parentheses == 0) || 
					(s [i] == ')' && i == (s.Length -1))) 
				{ // Parameter changed

					if (p1 == null) 
					{
						p1 = s.Substring (0, i);
						s = s.Substring (i + 1);
						i = 0;
					}

					else if (p2 == null) 
					{
						p2 = s.Substring (0, i);
						s = s.Substring (i + 1);
						i = 0;
					}

					else if (p3 == null) 
					{
						p3 = s.Substring (0, i);
						s = s.Substring (i + 1);
						i = 0;
					}

					else
						throw new Exception (); // FIXME: What exception
				}
			}

			if (p1 == null || p2 == null)
				throw new Exception ();

			ParseExpression (p1);
			ParseExpression (p2);
			ParseExpression (p3);			
		}
	}

	/// <summary>
	///  Class for In (exp, exp, exp, ...) function
	/// </summary>
	internal class ExpressionIn : ExpressionElement
	{
		public ExpressionIn (string exp1, string exp2)
		{       
			ParseExpression(exp1);
			ParseParameters (exp2);
		}

		/// <summary>
		///  IsNull function does not return boolean value, so throw exception
		/// </summary>
		public override bool Test (DataRow Row) 
		{
			ExpressionElement E;
			ExpressionElement columnElement = (ExpressionElement)Elements [0];

			ExpressionElementComparer comparer = new ExpressionElementComparer(columnElement, Row);

			for (int i = 1; i < Elements.Count; i++)
			{
				E = (ExpressionElement)Elements [i];
				if(comparer.CompareTo(E) == 0)
					return true;
			}
			return false;
		}

		/// <summary>
		///  Parses parameters of function and invoke ParseExpression methods
		/// </summary>
		private void ParseParameters (string s)
		{
			bool inString = false;
			ArrayList parameters = new ArrayList();
			
			// find (
			while (!s.StartsWith ("("))
				s = s.Remove (0, 1);

			// remove (
			s = s.Remove (0, 1);
			int parentheses = 0;
			for (int i = 0; i < s.Length; i++) 
			{

				if (s [i] == '\'')
					inString = !inString;
				else if (s [i] == '(')
					parentheses++;
				else if (s [i] == ')')
					parentheses--;


				if ((s [i] == ',' && !inString && parentheses == 0) || 
					(s [i] == ')' && i == (s.Length -1))) 
				{
					parameters.Add(s.Substring (0, i));
					s = s.Substring (i + 1);
					i = 0;
				}
			}
			
			for (int i = 0; i < parameters.Count; i++)
				ParseExpression((string)parameters[i]);
		}
	}

	/// <summary>
	///  Class for just one element for example string, int, ...
	/// </summary>
	internal class ExpressionSingleElement : ExpressionElement
	{        	
		private object Element = null;
        	
		public ExpressionSingleElement (string s)
		{
			// TODO: Every type should be checked
			if (s.StartsWith ("'") && s.EndsWith ("'")) 
			{
				Element = s.Substring (1, s.Length - 2);
				_ResultType = typeof (string);
			}
			else if (!Char.IsDigit (s [0]) && s [0] != '-' && s [0] != '+') 
			{
				Element = s;
				_ResultType = typeof (DataColumn);
			}
			else if (s.StartsWith ("#") && s.EndsWith ("#")) 
			{
				Element = DateTime.Parse (s.Substring (1, s.Length - 2));
				_ResultType = typeof (DateTime);
			}
			else 
			{
				try 
				{
					Element = int.Parse (s);
					_ResultType = typeof (int);
				} 
				catch 
				{
					Element = Decimal.Parse (s);
					_ResultType = typeof (Decimal);
				}
			}				
		}

		public override object Result (DataRow Row)
		{
			object Result = null;
			if (ResultType (Row) == typeof (DataColumn)) 
			{
				
				if (!Row.Table.Columns.Contains (Element.ToString ()))
					throw new EvaluateException ("Column name '" + Element.ToString () + "' not found.");
				else
				{
					DataRowVersion rowVersion = DataRowVersion.Default;
					// if this row is deleted we get the original version, or else we get an exception.
					if (Row.RowState == DataRowState.Deleted)
						rowVersion = DataRowVersion.Original;
					Result = Row [Element.ToString (), rowVersion];
				}
			}
			else
				Result = Element;
				
			return Result;
		}
        	
		public override bool Test (DataRow Row)
		{
			throw new EvaluateException ();
		}		
	}

	/// <summary>
	///  Parent class of all the elements of expression
	/// </summary>
	internal abstract class ExpressionElement
	{        
		enum OP {OPERATOR, OPERAND};
		enum OPERATOR_TYPE {SYMBOLIC, LITERAL, UNDEFINED};
		enum OPERAND_TYPE {NUMERIC, STRING, UNDEFINED};

		// 
		// TODO/FIXME: This class should be inherited more than once. I mean own subclass for operators, functions,...
		//

		protected string exp1;
		protected string exp2;
		protected  Type _ResultType;

		protected ArrayList Elements = new ArrayList ();

		enum AGGREGATE {SUM, AVG, MIN, MAX, COUNT, STDEV, VAR}
		//protected ArrayList Singles = new ArrayList ();
        	
		/// <summary>
		/// Tells does the current expressions match to current DataRow
		/// </summary>
		abstract public bool Test (DataRow Row);

		public virtual object Result (DataRow Row) {return null;}
                
		public virtual Type ResultType (DataRow Row)
		{
			return _ResultType;
		}

		protected object CalculateResult (DataRow Row)
		{
			ExpressionElement E1 = ((ExpressionElement)Elements [0]);
			ExpressionElement E2 = ((ExpressionElement)Elements [1]);
			object Result = null;
			object value1 = E1.Result (Row);
			object value2 = E2.Result (Row);
			Type t1 = value1.GetType ();
			Type t2 = value2.GetType ();
			
			// Check nulls
			if (value1 ==  DBNull.Value && value2 == DBNull.Value)
				return null;
			
			// TODO: More types
			
			if (t1 == typeof (string) || t2 == typeof (string)) 
			{
				
				if (t1 != typeof (string))
					value1 = Convert.ChangeType (value1, Type.GetTypeCode (t2));
				else if (t2 != typeof (string))
					value2 = Convert.ChangeType (value2, Type.GetTypeCode (t1));
			}
			
			if (t1 != t2)
				value2 = Convert.ChangeType (value2, Type.GetTypeCode (t1));
			
			Result = Calculate (value1, value2, t1);
			
			return Result; 
		}
		protected virtual object Calculate (object value1, object value2, Type TempType)
		{
			return null;
		}
		
		/// <summary>
		///  static method for comparing two ExpressionElement. This is used in =, <, >, <>, <=, >= elements.
		///  If elements are equal returns 0, if E1 is less that E2, return -1 else if E1 is greater 1 
		/// </summary>
		protected static int Compare (ExpressionElement E1, ExpressionElement E2, DataRow Row)
		{ 
			ExpressionElementComparer comparer = new ExpressionElementComparer(E1, Row);

			return comparer.CompareTo(E2);
		}

		/// <summary>
		///  Checks syntax of expression and throws exception if needed.
		///  Also removes whitespaces between operator elements for example: age < = 64 --> age <= 64
		/// </summary>
		internal static string ValidateExpression (string s)
		{			
			//
			// TODO: find out nice way to do this. This is NOT nice way :-P
			//
			//string temp = "";
			OP op = OP.OPERAND;
			OPERATOR_TYPE operatorType = OPERATOR_TYPE.UNDEFINED;

			StringBuilder strOperator = new StringBuilder();
			StringBuilder strOperand = new StringBuilder();
			int quotes = 0;
			int parentheses = 0;
			StringBuilder newExp = new StringBuilder();
			bool isDigit = false;
			//bool litOperator = false;
			s = s.Trim();
			
			for (int i = 0; i < s.Length; i++) 
			{

				char c = s [i];
				
				if (c == '\'')
					quotes++;

				if ((c == '\n' || c == '\t') && quotes == 0)
					c = ' ';

				if (op == OP.OPERAND && c == '(')
					parentheses++;
				else if (op == OP.OPERAND && c == ')')
					parentheses--;

				if (c == ' ' && op ==  OP.OPERAND && (quotes % 2) == 0 && parentheses == 0) 
				{
					
					op = OP.OPERATOR;
					int max = strOperand.Length;
					for (int it = 0; it < max; it++)
						newExp.Append(strOperand[it]);
					strOperand.Remove(0, max);
					strOperator.Append(' ');
				}

				if (op == OP.OPERAND) 
				{

					if (!Char.IsDigit (c) && isDigit && (quotes % 2) == 0) 
					{
						int max = strOperand.Length;
						for (int it = 0; it < max; it++)
							newExp.Append(strOperand[it]);
						strOperand.Remove(0, max);
						op = OP.OPERATOR;
						operatorType = OPERATOR_TYPE.UNDEFINED;
					}
					else
						strOperand.Append(c);
				}

				if (op == OP.OPERATOR) 
				{

					isDigit = false;
					if (operatorType == OPERATOR_TYPE.UNDEFINED) 
					{

						if (c == '<' || c == '=' || c == '>' || c == '*' || c == '/' || c == '%' 
							|| c == '-' || c == '+')

							operatorType = OPERATOR_TYPE.SYMBOLIC;
						else if (c != ' ')
							operatorType = OPERATOR_TYPE.LITERAL;
					}
					else if (operatorType == OPERATOR_TYPE.SYMBOLIC) 
					{
						
						//Checking for operators following one another
						if ((c == '=' || c== '<' || c== '>') &&
							(strOperator.Length == 2) &&
							(strOperator[0] == ' '))
						{
							char chSecond = strOperator[1];
							if (chSecond == '+' ||
								chSecond == '-' ||
								chSecond == '%' ||
								chSecond == '*')
							throw new SyntaxErrorException (
								"The operator " + strOperator.ToString() + c + " is not valid");
						}

						// this is COPY-PASTE
						op = OP.OPERAND;
						if (newExp[newExp.Length - 1] != ' '  && 
							strOperator[0] != ' ')
							newExp.Append(' ');

						int max = strOperator.Length;
						for (int it = 0; it < max; it++)
							newExp.Append(strOperator[it]);
						strOperator.Remove(0, max);

						if (Char.IsDigit (c))
							isDigit = true;
							
						strOperand.Remove(0, strOperand.Length);
						strOperand.Append(c);
						
						continue;

					}

					if (operatorType == OPERATOR_TYPE.LITERAL && c == ' ') 
					{
						op = OP.OPERAND;
						int max = strOperator.Length;
						for (int it = 0; it < max; it++)
							newExp.Append(strOperator[it]);
						strOperator.Remove(0, max);

						strOperand.Append(' ');
					}


					if (Char.IsDigit (c) && operatorType != OPERATOR_TYPE.LITERAL) 
					{

						op = OP.OPERAND;
						if (newExp[newExp.Length - 1] != ' '  && 
							strOperator[0] != ' ')
							newExp.Append(' ');

						int max = strOperator.Length;
						for (int it = 0; it < max; it++)
							newExp.Append(strOperator[it]);
						strOperator.Remove(0, max);

						if (Char.IsDigit (c))
							isDigit = true;
							
						strOperand.Remove(0, strOperand.Length);
						strOperand.Append(c);
					}

					else if (c != ' ')
						strOperator.Append(c);					
				}
			}

			if (op == OP.OPERATOR)
				throw new SyntaxErrorException (
					"Missing operand after '" + strOperator.ToString() + "' operator");
			else
			{
				int max = strOperand.Length;
				for (int it = 0; it < max; it++)
					newExp.Append(strOperand[it]);
			}

			return newExp.ToString();
		}

		/// <summary>
		///  Finds and creates Expression elements.
		///  This presumes that expression is valid.
		/// </summary>
		protected void ParseExpression (string s)
		{	
			//
			// TODO/FIXME: IMHO, this should be done with different kind of parsing:
			// char by char not operand by operand. 
			// fixed the easy places, should consider better parsing
			//

			string inside = String.Empty; // stores string betwee parentheses like a = 12 and (b = 1 or b = 2)
			string lower = s.ToLower();
			//string function = ""; // stores fuction paramters like substring (this, are, paramters)
			//string s1 = "";
			//string s2 = "";
			int startIndex = lower.IndexOf ('(');
			
			// Find parenthesis
			if (startIndex != -1) 
			{
				string functionName = String.Empty;
				if (startIndex > 0)
				{
					int functionStart = lower.LastIndexOf('=', startIndex-1, startIndex);
					if (functionStart < 0)
						functionStart++;
					functionName = lower.Substring(functionStart, startIndex);

					functionName = functionName.Trim ();
				}

				// check if previous element is a function
				if (functionName.Length == 0 || (!functionName.EndsWith ("convert") && !functionName.EndsWith ("len") &&
					!functionName.EndsWith ("isnull") && !functionName.EndsWith ("iif") &&
					!functionName.EndsWith ("trim") && !functionName.EndsWith ("substring") &&
					!functionName.EndsWith ("sum") && !functionName.EndsWith ("avg") &&
					!functionName.EndsWith ("min") && !functionName.EndsWith ("max") &&
					!functionName.EndsWith ("count") && !functionName.EndsWith ("stdev") &&
					!functionName.EndsWith ("var")&& !functionName.EndsWith ("in"))) 
				{
					
					
					int i = startIndex + 1;
					int max = lower.Length;
			
					for (int par = 1; par > 0 && i < max; i++) 
					{

						char c = s [i];
						if (c == '(')
							par++;
						else
						if (c == ')')
							par--;
					}
					
					inside = s.Substring(startIndex + 1, i - startIndex - 2); 
					lower = lower.Remove (startIndex, i - startIndex);
					s = s.Remove (startIndex, i - startIndex);
				}		
					     
			}
			
			string string1 = null;
			string string2 = null;
			if (FindOrElement (s, lower, ref string1, ref string2))		
				CreateOrElement (string1, string2, inside);

			else if (FindAndElement (s, lower, ref string1, ref string2))
				CreateAndElement (string1, string2, inside);

				// find LIKE
			else if (FindLikeElement (s, lower, ref string1, ref string2))
				CreateLikeElement (string1, string2, inside);
			
				// find IN
			else if (FindInElement (s, lower, ref string1, ref string2))
				CreateInElement (string1, string2, inside);

				// find =
			else if (FindEqualElement (s, lower, ref string1, ref string2))
				CreateEqualsElement (string1, string2, inside);

				// find <>
			else if (FindUnequalElement (s, lower, ref string1, ref string2))
				CreateUnequalsElement (string1, string2, inside);

				// find <=
			else if (FindLessThanOrEqualElement (s, lower, ref string1, ref string2))
				CreateLessThanOrEqualElement (string1, string2, inside);

				// find <
			else if (FindLessThanElement (s, lower, ref string1, ref string2))
				CreateLessThanElement (string1, string2, inside);

				// find >=
			else if (FindGreaterThanOrEqualElement (s, lower, ref string1, ref string2))
				CreateGreaterThanOrEqualElement (string1, string2, inside);

				// find >
			else if (FindGreaterThanElement (s, lower, ref string1, ref string2))
				CreateGreaterThanElement (string1, string2,  inside);

				// if there wasn't any operators like 'and' or 'not' there still could be
				// arithmetic operators like '+' or '-' or functions like 'iif' or 'substring'

				// find *
			else if (FindMultiplyElement (s, lower, ref string1, ref string2))
				CreateMultiplyElement (string1, string2, inside);
			
				// find /
			else if (FindDivideElement (s, lower, ref string1, ref string2))
				CreateDivideElement (string1, string2, inside);


				// find +
			else if (FindAdditionElement (s, lower, ref string1, ref string2))
				CreateAdditionElement (string1, string2, inside);

				// find -
			else if (FindSubtractElement (s, lower, ref string1, ref string2))
				CreateSubtractionElement (string1, string2, inside);

				// find %
			else if (FindModulusElement (s, lower, ref string1, ref string2))
				CreateModulusElement (string1, string2, inside);

				// find sum ()
			else if (FindAggregateElement (s, lower, AGGREGATE.SUM))
				Elements.Add (new ExpressionSum (s.Trim ()));

				// find avg ()
			else if (FindAggregateElement (s, lower, AGGREGATE.AVG))
				Elements.Add (new ExpressionAvg (s.Trim ()));

				// find min ()
			else if (FindAggregateElement (s, lower, AGGREGATE.MIN))
				Elements.Add (new ExpressionMin (s.Trim ()));

				// find max ()
			else if (FindAggregateElement (s, lower, AGGREGATE.MAX))
				Elements.Add (new ExpressionMax (s.Trim ()));

				// find count ()
			else if (FindAggregateElement (s, lower, AGGREGATE.COUNT))
				Elements.Add (new ExpressionCount (s.Trim ()));				   

				// find stdev ()
			else if (FindAggregateElement (s, lower, AGGREGATE.STDEV))
				Elements.Add (new ExpressionStdev (s.Trim ()));

				// find var ()
			else if (FindAggregateElement (s, lower, AGGREGATE.VAR))
				Elements.Add (new ExpressionVar (s.Trim ()));

				// find len
			else if (FindLenElement (s, lower))
				Elements.Add (new ExpressionLen (s.Trim ()));

				// find iif
			else if (FindIifElement (s, lower))
				Elements.Add (new ExpressionIif (s.Trim ()));

				// find isnull
			else if (FindIsNullElement (s, lower))
				Elements.Add (new ExpressionIsNull (s.Trim ()));

				// find substring
			else if (FindSubstringElement (s, lower))
				Elements.Add (new ExpressionSubstring (s.Trim ()));

				// if expression is like '(something someoperator something)'
			else if (inside.Trim () != string.Empty)
				ParseExpression (inside);

				// At least, if it wasnt any of the above it is just normat string or int
				// or....			
			else
				Elements.Add (new ExpressionSingleElement (s.Trim ()));			
		}

		#region CheckElement methods

		//
		// These methods are temporary for now
		//

		private bool FindOrElement (string s, string lower, ref string s1, ref string s2)
		{
			string stemp = lower;
			int indexOf = stemp.IndexOf("or");

			if (indexOf == -1)
				return false;

			// Test if or is between ''
			int oldIndex = -1;			
			while ((indexOf = stemp.IndexOf ("or", oldIndex + 1)) != -1 && indexOf > oldIndex) 
			{
				
				oldIndex = indexOf;

				// check is the 'or' element part of string element
				if (IsPartOfStringElement (stemp, indexOf))
					continue;
				
				// Check is or part of something else for example column name
				if (indexOf != 0) 
				{
					
					if (stemp [indexOf - 1] != ' ' && stemp [indexOf - 1] != '\'')
						continue;
				}
				
				if (indexOf < s.Length + 2) 
				{
					
					if (stemp [indexOf + 2] != ' ' && stemp [indexOf + 2] != '\'')
						continue;
				}

				if (IsPartOfFunction (stemp, indexOf))
					continue;

				s1 = s.Substring (0, indexOf).Trim ();
				s2 = s.Substring (indexOf + 2).Trim ();

				return true;
			}

			return false;
		}
		
		private bool FindAndElement (string s, string lower, ref string s1, ref string s2)
		{
			string stemp = lower;
			int indexOf = stemp.IndexOf("and");

			if (indexOf == -1)
				return false;

			// Test if or is between ''
			int oldIndex = -1;
			while ((indexOf = stemp.IndexOf ("and", oldIndex + 1)) != -1 && indexOf > oldIndex) 
			{
				
				oldIndex = indexOf;
				
				// check is the 'and' element part of string element
				if (IsPartOfStringElement (stemp, indexOf))
					continue;


				// Check is or part of something else for example column name
				if (indexOf != 0) 
				{
					
					if (stemp [indexOf - 1] != ' ' && stemp [indexOf - 1] != '\'')
						continue;
				}
				
				if (indexOf < stemp.Length + 3) 
				{
					
					if (stemp [indexOf + 3] != ' ' && stemp [indexOf + 3] != '\'')
						continue;
				}

				if (IsPartOfFunction (stemp, indexOf))
					continue;


				s1 = s.Substring (0, indexOf).Trim ();
				s2 = s.Substring (indexOf + 3).Trim ();
				return true;
			}

			return false;
		}

		private bool FindLikeElement (string s, string lower, ref string s1, ref string s2)
		{
			string stemp = lower;
			int indexOf = stemp.IndexOf("like");

			if (indexOf == -1)
				return false;

			// Test if or is between ''
			int oldIndex = -1;
			while ((indexOf = stemp.IndexOf ("like", oldIndex + 1)) != -1 && indexOf > oldIndex) 
			{
				
				oldIndex = indexOf;
				
				// check is the 'and' element part of string element
				if (IsPartOfStringElement (stemp, indexOf))
					continue;


				// Check is or part of something else for example column name
				if (indexOf != 0) 
				{
					
					if (stemp [indexOf - 1] != ' ' && stemp [indexOf - 1] != '\'')
						continue;
				}
				
				if (indexOf < stemp.Length + 4) 
				{
					
					if (stemp [indexOf + 4] != ' ' && stemp [indexOf + 4] != '\'')
						continue;
				}

				if (IsPartOfFunction (stemp, indexOf))
					continue;


				s1 = s.Substring (0, indexOf).Trim ();
				s2 = s.Substring (indexOf + 4).Trim ();
				return true;
			}

			return false;
		}

		private bool FindEqualElement (string s, string lower, ref string s1, ref string s2)
		{
			string stemp = lower;
			int indexOf = stemp.IndexOf ("=");

			if (indexOf == -1)
				return false;
			
			int oldIndex = -1;

			while ((indexOf = stemp.IndexOf ("=", oldIndex + 1)) != -1 && indexOf > oldIndex) 
			{

				oldIndex = indexOf;

				// Check is the = part of <= or >=
				if (stemp [indexOf - 1] == '<' || stemp [indexOf - 1] == '>')
					continue;

				// Check is the = element part of string element
				if (IsPartOfStringElement (stemp, indexOf))
					continue;

				// Check is or part of column name
				if (IsPartOfColumnName (stemp, indexOf))
					continue;
					
				if (IsPartOfFunction (stemp, indexOf))
					continue;

				s1 = s.Substring (0, indexOf).Trim ();
				s2 = s.Substring (indexOf + 1).Trim ();
				
				return true;
			}

			return false;
		}

		private bool FindUnequalElement (string s, string lower, ref string s1, ref string s2)
		{
			string stemp = lower;
			int indexOf = stemp.IndexOf ("<>");

			if (stemp.IndexOf ("<>") == -1)
				return false;
		       
			int oldIndex = -1;
			while ((indexOf = stemp.IndexOf ("<>", oldIndex + 1)) != -1 && indexOf > oldIndex) 
			{

				oldIndex = indexOf;

				// test if next charachter is something else than ' '
				bool failed = false;

				// Check is the <> element part of string element
				if (IsPartOfStringElement (stemp, indexOf))
					continue;

				// Check is or part of column name
				if (IsPartOfColumnName (stemp, indexOf))
					continue;
					
				if (IsPartOfFunction (stemp, indexOf))
					continue;

				s1 = s.Substring (0, indexOf).Trim ();
				s2 = s.Substring (indexOf + 2).Trim ();
				
				return true;
			}

			return false;
			
		}


		private bool FindLessThanElement (string s, string lower, ref string s1, ref string s2)
		{
			string stemp = lower;
			int indexOf = stemp.IndexOf ("<");

			if (indexOf == -1)
				return false;

			int oldIndex = -1;
			while ((indexOf = stemp.IndexOf ("<", oldIndex + 1)) != -1 && indexOf > oldIndex) 
			{

				oldIndex = indexOf;

				// if < is part of <> or <=
				if (stemp [indexOf + 1] == '>' || stemp [indexOf + 1] == '=')
					continue;

				// Test is < element part of string element
				if (IsPartOfStringElement (stemp, indexOf))
					continue;

				// Check is or part of column name
				if (IsPartOfColumnName (stemp, indexOf))
					continue;

				if (IsPartOfFunction (stemp, indexOf))
					continue;

				s1 = s.Substring (0, indexOf).Trim ();
				s2 = s.Substring (indexOf + 1).Trim ();

				return true;
			}
		
			return false;			
		}

		private bool FindLessThanOrEqualElement (string s, string lower, ref string s1, ref string s2)
		{
			string stemp = lower;
			int indexOf = stemp.IndexOf ("<=");

			if (indexOf == -1)
				return false;

			int oldIndex = -1;
			while ((indexOf = stemp.IndexOf ("<=", oldIndex + 1)) != -1 && indexOf > oldIndex) 
			{

				oldIndex = indexOf;
				// Test is <= element part of string element
				if (IsPartOfStringElement (stemp, indexOf))
					continue;

				// Check is or part of column name
				if (IsPartOfColumnName (stemp, indexOf))
					continue;

				if (IsPartOfFunction (stemp, indexOf))
					continue;

				s1 = s.Substring (0, indexOf).Trim ();
				s2 = s.Substring (indexOf + 2).Trim ();

				return true;
			}
		
			return false;			
		}

		private bool FindGreaterThanElement (string s, string lower, ref string s1, ref string s2)
		{
			string stemp = lower;
			int indexOf = stemp.IndexOf (">");

			if (indexOf == -1)
				return false;

			int oldIndex = -1;
			while ((indexOf = stemp.IndexOf (">", oldIndex + 1)) != -1 && indexOf > oldIndex) 
			{

				oldIndex = indexOf;

				// if < is part of <> or <=
				if (stemp [indexOf - 1] == '<' || stemp [indexOf + 1] == '=')
					continue;

				// Test is < element part of string element
				if (IsPartOfStringElement (stemp, indexOf))
					continue;

				// Check is or part of column name
				if (IsPartOfColumnName (stemp, indexOf))
					continue;

				if (IsPartOfFunction (stemp, indexOf))
					continue;

				s1 = s.Substring (0, indexOf).Trim ();
				s2 = s.Substring (indexOf + 1).Trim ();
				return true;
			}
		
			return false;			
		}

		private bool FindGreaterThanOrEqualElement (string s, string lower, ref string s1, ref string s2)
		{
			string stemp = lower;
			int indexOf = stemp.IndexOf (">=");

			if (indexOf == -1)
				return false;

			int oldIndex = -1;
			while ((indexOf = stemp.IndexOf (">=", oldIndex + 1)) != -1 && indexOf > oldIndex) 
			{

				oldIndex = indexOf;
				bool failed = false;
				// Test is <= element part of string element

				// Check is or part of column name
				if (IsPartOfColumnName (stemp, indexOf))
					continue;

				// is the element part of string element
				if (IsPartOfStringElement (stemp, indexOf))
					continue;

				if (IsPartOfFunction (stemp, indexOf))
					continue;

				s1 = s.Substring (0, indexOf).Trim ();
				s2 = s.Substring (indexOf + 2).Trim ();

				return true;
			}
		
			return false;			
		}

		private bool FindAdditionElement (string s, string lower, ref string s1, ref string s2)
		{
			string stemp = lower;
			int indexOf = stemp.IndexOf ("+");

			if (indexOf == -1)
				return false;

			int oldIndex = -1;
			while ((indexOf = stemp.IndexOf ("+", oldIndex + 1)) != -1 && indexOf > oldIndex) 
			{

				// FIXME: if '+' represents sign of integer

				oldIndex = indexOf;
				bool failed = false;

				// Check is or part of column name
				if (IsPartOfColumnName (stemp, indexOf))
					continue;

				// is the element part of string element
				if (IsPartOfStringElement (stemp, indexOf))
					continue;

				if (IsPartOfFunction (stemp, indexOf))
					continue;

				s1 = s.Substring (0, indexOf).Trim ();
				s2 = s.Substring (indexOf + 1).Trim ();

				return true;
			}
		
			return false;			
		}

		private bool FindSubtractElement (string s, string lower, ref string s1, ref string s2)
		{
			string stemp = lower;
			int indexOf = stemp.IndexOf ("-");

			if (indexOf == -1)
				return false;

			int oldIndex = -1;
			while ((indexOf = stemp.IndexOf ("-", oldIndex + 1)) != -1 && indexOf > oldIndex) 
			{

				oldIndex = indexOf;
				bool failed = false;

				// check is this lonely element		
				failed = true;
				for (int i = indexOf - 1; i >= 0; i--) 
				{
					if (stemp [i] != ' ') 
					{
						failed = false;
						break;
					}
				}
					
				if (failed)
					continue;

				// Check is or part of column name
				if (IsPartOfColumnName (stemp, indexOf))
					continue;

				// is the element part of string element
				if (IsPartOfStringElement (stemp, indexOf))
					continue;

				if (IsPartOfFunction (stemp, indexOf))
					continue;

				s1 = s.Substring (0, indexOf).Trim ();
				s2 = s.Substring (indexOf + 1).Trim ();

				return true;
			}
		
			return false;			
		}

		private bool FindMultiplyElement (string s, string lower, ref string s1, ref string s2)
		{
			string stemp = lower;
			int indexOf = stemp.IndexOf ("*");

			if (indexOf == -1)
				return false;

			int oldIndex = -1;
			while ((indexOf = stemp.IndexOf ("*", oldIndex + 1)) != -1 && indexOf > oldIndex) 
			{


				oldIndex = indexOf;
				bool failed = false;

				// FIXME: If there is a divide operator before multiply operator.

				// Check is or part of column name
				if (IsPartOfColumnName (stemp, indexOf))
					continue;

				// is the element part of string element
				if (IsPartOfStringElement (stemp, indexOf))
					continue;

				if (IsPartOfFunction (stemp, indexOf))
					continue;

				s1 = s.Substring (0, indexOf).Trim ();
				s2 = s.Substring (indexOf + 1).Trim ();

				return true;
			}
		
			return false;			
		}

		private bool FindDivideElement (string s, string lower, ref string s1, ref string s2)
		{
			string stemp = lower;
			int indexOf = stemp.IndexOf ("/");

			if (indexOf == -1)
				return false;

			int oldIndex = -1;
			while ((indexOf = stemp.IndexOf ("/", oldIndex + 1)) != -1 && indexOf > oldIndex) 
			{


				oldIndex = indexOf;
				bool failed = false;

				// FIXME: If there is a multiply operator before divide operator.

				// Check is or part of column name
				if (IsPartOfColumnName (stemp, indexOf))
					continue;

				// is the element part of string element
				if (IsPartOfStringElement (stemp, indexOf))
					continue;

				if (IsPartOfFunction (stemp, indexOf))
					continue;
				    
				s1 = s.Substring (0, indexOf).Trim ();
				s2 = s.Substring (indexOf + 1).Trim ();

				return true;
			}
		
			return false;			
		}

		private bool FindModulusElement (string s, string lower, ref string s1, ref string s2)
		{
			string stemp = lower;
			int indexOf = stemp.IndexOf ("%");

			if (indexOf == -1)
				return false;

			int oldIndex = -1;
			while ((indexOf = stemp.IndexOf ("%", oldIndex + 1)) != -1 && indexOf > oldIndex) 
			{


				oldIndex = indexOf;
				bool failed = false;

				// FIXME: If there is a multiply operator before divide operator.

				// Check is or part of column name
				if (IsPartOfColumnName (stemp, indexOf))
					continue;

				// is the element part of string element
				if (IsPartOfStringElement (stemp, indexOf))
					continue;

				s1 = s.Substring (0, indexOf).Trim ();
				s2 = s.Substring (indexOf + 1).Trim ();

				return true;
			}
		
			return false;			
		}

		private bool FindAggregateElement (string s, string lower, AGGREGATE aggregate)
		{
			string agg = null;

			switch (aggregate) 
			{

				case AGGREGATE.SUM:
					agg = "sum";
					break;
				case AGGREGATE.AVG:
					agg = "avg";
					break;
				case AGGREGATE.MIN:
					agg = "min";
					break;
				case AGGREGATE.MAX:
					agg = "max";
					break;
				case AGGREGATE.COUNT:
					agg = "count";
					break;
				case AGGREGATE.STDEV:
					agg = "stdev";
					break;
				case AGGREGATE.VAR:
					agg = "var";
					break;
				default:
					throw new NotImplementedException ();
			}
			       
				
			string stemp = lower;
			int indexOf = stemp.IndexOf (agg);

			if (indexOf == -1)
				return false;

			int oldIndex = -1;
			while ((indexOf = stemp.IndexOf (agg, oldIndex + 1)) != -1 && indexOf > oldIndex) 
			{

				oldIndex = indexOf;
				bool failed = false;

				// Check is or part of column name
				if (indexOf != 0 && stemp [indexOf - 1] != ' ')
					continue;

				// is the element part of string element
				if (IsPartOfStringElement (stemp, indexOf))
					continue;


				return true;
			}
		
			return false;			

		}
		
		private bool FindSumElement (string s, string lower)
		{
			string stemp = lower;
			int indexOf = stemp.IndexOf ("sum");

			if (indexOf == -1)
				return false;

			int oldIndex = -1;
			while ((indexOf = stemp.IndexOf ("sum", oldIndex + 1)) != -1 && indexOf > oldIndex) 
			{

				oldIndex = indexOf;
				bool failed = false;

				// Check is or part of column name
				if (indexOf != 0 && stemp [indexOf - 1] != ' ')
					continue;

				// is the element part of string element
				if (IsPartOfStringElement (stemp, indexOf))
					continue;


				return true;
			}
		
			return false;			
		}

		private bool FindAvgElement (string s, string lower)
		{
			string stemp = lower;
			int indexOf = stemp.IndexOf ("avg");

			if (indexOf == -1)
				return false;

			int oldIndex = -1;
			while ((indexOf = stemp.IndexOf ("avg", oldIndex + 1)) != -1 && indexOf > oldIndex) 
			{

				oldIndex = indexOf;
				bool failed = false;

				// Check is or part of column name
				if (indexOf != 0 && stemp [indexOf - 1] != ' ')
					continue;

				// is the element part of string element
				if (IsPartOfStringElement (stemp, indexOf))
					continue;

				return true;
			}
		
			return false;			
		}

		private bool FindMinElement (string s, string lower)
		{
			string stemp = lower;
			int indexOf = stemp.IndexOf ("min");

			if (indexOf == -1)
				return false;

			int oldIndex = -1;
			while ((indexOf = stemp.IndexOf ("min", oldIndex + 1)) != -1 && indexOf > oldIndex) 
			{

				oldIndex = indexOf;
				bool failed = false;

				// Check is or part of column name
				if (indexOf != 0 && stemp [indexOf - 1] != ' ')
					continue;

				// is the element part of string element
				if (IsPartOfStringElement (stemp, indexOf))
					continue;

				return true;
			}
		
			return false;			
		}

		private bool FindMaxElement (string s, string lower)
		{
			string stemp = lower;
			int indexOf = stemp.IndexOf ("max");

			if (indexOf == -1)
				return false;

			int oldIndex = -1;
			while ((indexOf = stemp.IndexOf ("max", oldIndex + 1)) != -1 && indexOf > oldIndex) 
			{

				oldIndex = indexOf;
				bool failed = false;

				// Check is or part of column name
				if (indexOf != 0 && stemp [indexOf - 1] != ' ')
					continue;

				// is the element part of string element
				if (IsPartOfStringElement (stemp, indexOf))
					continue;

				return true;
			}
		
			return false;			
		}

		private bool FindCountElement (string s, string lower)
		{
			string stemp = lower;
			int indexOf = stemp.IndexOf ("count");

			if (indexOf == -1)
				return false;

			int oldIndex = -1;
			while ((indexOf = stemp.IndexOf ("count", oldIndex + 1)) != -1 && indexOf > oldIndex) 
			{

				oldIndex = indexOf;
				bool failed = false;

				// Check is or part of column name
				if (indexOf != 0 && stemp [indexOf - 1] != ' ')
					continue;

				// is the element part of string element
				if (IsPartOfStringElement (stemp, indexOf))
					continue;

				return true;
			}
		
			return false;			
		}

		private bool FindStdevElement (string s, string lower)
		{
			string stemp = lower;
			int indexOf = stemp.IndexOf ("stdev");

			if (indexOf == -1)
				return false;

			int oldIndex = -1;
			while ((indexOf = stemp.IndexOf ("stdev", oldIndex + 1)) != -1 && indexOf > oldIndex) 
			{

				oldIndex = indexOf;
				bool failed = false;

				// Check is or part of column name
				if (indexOf != 0 && stemp [indexOf - 1] != ' ')
					continue;

				// is the element part of string element
				if (IsPartOfStringElement (stemp, indexOf))
					continue;

				return true;
			}
		
			return false;			
		}

		private bool FindVarElement (string s, string lower)
		{
			string stemp = lower;
			int indexOf = stemp.IndexOf ("var");

			if (indexOf == -1)
				return false;

			int oldIndex = -1;
			while ((indexOf = stemp.IndexOf ("var", oldIndex + 1)) != -1 && indexOf > oldIndex) 
			{

				oldIndex = indexOf;
				bool failed = false;

				// Check is or part of column name
				if (indexOf != 0 && stemp [indexOf - 1] != ' ')
					continue;

				// is the element part of string element
				if (IsPartOfStringElement (stemp, indexOf))
					continue;

				return true;
			}
		
			return false;			
		}

		private bool FindLenElement (string s, string lower)
		{
			string stemp = lower;
			int indexOf = stemp.IndexOf ("len");

			if (indexOf == -1)
				return false;

			int oldIndex = -1;
			while ((indexOf = stemp.IndexOf ("len", oldIndex + 1)) != -1 && indexOf > oldIndex) 
			{

				oldIndex = indexOf;
				bool failed = false;

				// Check is or part of column name
				if (indexOf != 0 && stemp [indexOf - 1] != ' ')
					continue;

				// is the element part of string element
				if (IsPartOfStringElement (stemp, indexOf))
					continue;


				return true;
			}
		
			return false;			
		}

		private bool FindIifElement (string s, string lower)
		{
			string stemp = lower;
			int indexOf = stemp.IndexOf ("iif");

			if (indexOf == -1)
				return false;

			int oldIndex = -1;
			while ((indexOf = stemp.IndexOf ("iif", oldIndex + 1)) != -1 && indexOf > oldIndex) 
			{

				oldIndex = indexOf;
				bool failed = false;

				// Check is or part of column name
				if (indexOf != 0 && stemp [indexOf - 1] != ' ')
					continue;

				// is the element part of string element
				if (IsPartOfStringElement (stemp, indexOf))
					continue;

				return true;
			}
		
			return false;			
		}

		private bool FindIsNullElement (string s, string lower)
		{
			string stemp = lower;
			int indexOf = stemp.IndexOf ("isnull");

			if (indexOf == -1)
				return false;

			int oldIndex = -1;
			while ((indexOf = stemp.IndexOf ("isnull", oldIndex + 1)) != -1 && indexOf > oldIndex) 
			{

				oldIndex = indexOf;
				bool failed = false;

				// Check is or part of column name
				if (indexOf != 0 && stemp [indexOf - 1] != ' ')
					continue;

				// is the element part of string element
				if (IsPartOfStringElement (stemp, indexOf))
					continue;
				
				return true;
			}
		
			return false;			
		}

		private bool FindSubstringElement (string s, string lower)
		{
			string stemp = lower;
			int indexOf = stemp.IndexOf ("substring");

			if (indexOf == -1)
				return false;

			int oldIndex = -1;
			while ((indexOf = stemp.IndexOf ("substring", oldIndex + 1)) != -1 && indexOf > oldIndex) 
			{

				oldIndex = indexOf;
				bool failed = false;

				// Check is or part of column name
				if (indexOf != 0 && stemp [indexOf - 1] != ' ')
					continue;

				// is the element part of string element
				if (IsPartOfStringElement (stemp, indexOf))
					continue;

				return true;
			}
		
			return false;			
		}

		private bool FindInElement (string s, string lower, ref string s1, ref string s2)
		{
			string stemp = lower;
			int indexOf = stemp.IndexOf ("in");

			if (indexOf == -1)
				return false;

			int oldIndex = -1;
			while ((indexOf = stemp.IndexOf ("in", oldIndex + 1)) != -1 && indexOf > oldIndex) 
			{
				oldIndex = indexOf;
				
				// check is the 'and' element part of string element
				if (IsPartOfStringElement (stemp, indexOf))
					continue;


				// Check is or part of something else for example column name
				if (indexOf != 0) 
				{	
					if (stemp [indexOf - 1] != ' ' && stemp [indexOf - 1] != '\'')
						continue;
				}
				
				if (indexOf < stemp.Length + 2) 
				{
					if (stemp [indexOf + 2] != ' ' && stemp [indexOf + 2] != '\'')
						continue;
				}

				if (IsPartOfFunction (stemp, indexOf))
					continue;

				s1 = s.Substring (0, indexOf).Trim ();
				s2 = s.Substring (indexOf + 2).Trim ();
				return true;
			}
		
			return false;			
		}

		
		#endregion // CheckElement methods

		#region CreateElement methods

		// 
		// These methods are going to be removed when way of parsing is changed
		//

		private void CreateOrElement (string s1, string s2, string inside) 
		{
			CheckParenthesis (inside, ref s1, ref s2);
			Elements.Add (new ExpressionOr (s1.Trim (), s2.Trim ()));
		}

		private void CreateAndElement (string s1, string s2, string inside)
		{
			CheckParenthesis (inside, ref s1, ref s2);
			Elements.Add (new ExpressionAnd (s1.Trim (), s2.Trim ()));
		}

		private void CreateLikeElement (string s1, string s2, string inside)
		{
			CheckParenthesis (inside, ref s1, ref s2);
			Elements.Add (new ExpressionLike (s1.Trim (), s2.Trim ()));
		}

		private void CreateInElement (string s1, string s2, string inside)
		{
			CheckParenthesis (inside, ref s1, ref s2);
			Elements.Add (new ExpressionIn (s1.Trim (), s2.Trim ()));
		}

		private void CreateEqualsElement (string s1, string s2, string inside)
		{
			CheckParenthesis (inside, ref s1, ref s2);
			Elements.Add (new ExpressionEquals (s1.Trim (), s2.Trim ()));			
		}

		private void CreateUnequalsElement (string s1, string s2, string inside)
		{
			CheckParenthesis (inside, ref s1, ref s2);
			Elements.Add (new ExpressionUnequals (s1.Trim (), s2.Trim ()));
		}

		private void CreateLessThanElement (string s1, string s2, string inside)
		{
			CheckParenthesis (inside, ref s1, ref s2);
			Elements.Add (new ExpressionLessThan (s1.Trim (), s2.Trim ()));
		}

		private void CreateLessThanOrEqualElement (string s1, string s2, string inside)
		{
			CheckParenthesis (inside, ref s1, ref s2);
			Elements.Add (new ExpressionLessThanOrEqual (s1.Trim (), s2.Trim ()));
		}

		private void CreateGreaterThanElement (string s1, string s2, string inside)
		{
			CheckParenthesis (inside, ref s1, ref s2);
			Elements.Add (new ExpressionGreaterThan (s1.Trim (), s2.Trim ()));
		}


		private void CreateGreaterThanOrEqualElement (string s1, string s2, string inside)
		{
			CheckParenthesis (inside, ref s1, ref s2);
			Elements.Add (new ExpressionGreaterThanOrEqual (s1.Trim (), s2.Trim ()));
		}

		private void CreateAdditionElement (string s1, string s2,  string inside)
		{
			CheckParenthesis (inside, ref s1, ref s2);			
			Elements.Add (new ExpressionAddition (s1.Trim (), s2.Trim ()));
		}

		private void CreateSubtractionElement (string s1, string s2,  string inside)
		{
			CheckParenthesis (inside, ref s1, ref s2);			
			Elements.Add (new ExpressionSubtraction (s1.Trim (), s2.Trim ()));
		}

		private void CreateMultiplyElement (string s1, string s2, string inside)
		{
			CheckParenthesis (inside, ref s1, ref s2);
			Elements.Add (new ExpressionMultiply (s1.Trim (), s2.Trim ()));
		}

		private void CreateDivideElement (string s1, string s2, string inside)
		{
			CheckParenthesis (inside, ref s1, ref s2);
			Elements.Add (new ExpressionDivide (s1.Trim (), s2.Trim ()));
		}

		private void CreateModulusElement (string s1, string s2, string inside)
		{
			CheckParenthesis (inside, ref s1, ref s2);
			Elements.Add (new ExpressionModulus (s1.Trim (), s2.Trim ()));
		}			

		#endregion // CreateElemnt methods

		#region Little helppers

		private void CheckParenthesis (string inside, ref string s1, ref string s2)
		{
			if (s1 == string.Empty && inside != string.Empty)
				s1 = inside;
			else if (s2 == string.Empty && inside != string.Empty)
				s2 = inside;	
		}


		/// <summary>
		///  Checks is the element part of stringelement
		/// </summary>
		private bool IsPartOfStringElement (string s, int indexOf)
		{
			// count how many '-charachters are before or. If count is odd it means or IS between quotes
			int quotes = 0;
			for (int i = indexOf - 1; i >= 0; i--) 
			{
				if (s [i] == '\'')
					quotes++;
			}
			
			if ((quotes & 1) != 0)
				return true;
			else 
				return false;
		}

		/// <summary>
		///  Checks is the element part of column table
		/// </summary>
		private bool IsPartOfColumnName (string s, int indexOf)
		{
			for (int i = indexOf; i >= 0; i--) 
			{
				
				// If the element is between [] it is part of columnname
				if (s [i] == '\'' || s [i] == ']') 
				{
					break;
				}
				else if (s [i] == '[') 
				{
					return true;
				}
			}

			return false;
		}


		/// <summary>
		///  Checks are element part of function
		/// </summary>
		private bool IsPartOfFunction (string s, int indexOf)
		{

			// 
			// If ',' or '\''  comes before '(' this element is not part of function's parameters
			//
			
			for (int i = indexOf; i >= 0; i--) 
			{
				
				if (s [i] == '(' || s [i] == ',') 
				{
					return true;
				}
				else if (s [i] == ')') 
				{
					break;
				}
			}

			return false;
		}

		#endregion // Little helppers

		#region ExpressionElementComparer
		protected sealed class ExpressionElementComparer
		{
			object _value1 = null;
			Type _t1 = null;
			Type _RT1 = null;
			DataRow _row = null;

			public ExpressionElementComparer(ExpressionElement E1, DataRow Row)
			{
				_value1 = E1.Result (Row);

				_row = Row;

				if (_value1 == null || _value1 == DBNull.Value)
					return;

				_t1 = _value1.GetType ();
				_RT1 = E1.ResultType (Row);
			}

			public int CompareTo (ExpressionElement E2)
			{
				object value1 = _value1;
				object value2 = E2.Result (_row);

				if ((value1 == null || value1 == DBNull.Value) && (value2 == null || value2 == DBNull.Value))
					return 0;
				else if (value2 == null || value2 == DBNull.Value)
					return 1;
				else if (value1 == null || value1 == DBNull.Value)
					return -1;

				Type t2 = value2.GetType ();
			
				Type RT2 = E2.ResultType (_row);

				if (_t1 == typeof (string) || t2 == typeof (string)) 
				{
					// FIXME: If one of elements are string they both should be???
					return String.Compare(value1.ToString(), value2.ToString(), !_row.Table.CaseSensitive);
				}

				if (_t1 != t2) 
				{
					value2 = Convert.ChangeType (value2, Type.GetTypeCode (_t1));
				}

				if (value1 is IComparable)
					return ((IComparable)value1).CompareTo(value2);

				return (int) _t1.InvokeMember ("CompareTo", BindingFlags.Default | 
					BindingFlags.InvokeMethod, null, 
					value1, 
					new object [] {value2});
			}
		}
		#endregion
	}    
	#endif    
}
