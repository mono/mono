//
// System.Data.ExpressionElement 
//
// Author:
//   Ville Palo <vi64pa@koti.soon.fi>
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

namespace System.Data
{
        /// <summary>
        /// The main element which includes whole expression
        /// </summary>
        internal class ExpressionMainElement : ExpressionElement
	{
		public ExpressionMainElement (string s)
		{
			ParseExpression (s);
		}
		
		public override bool Test (DataRow Row) {
			
			foreach (ExpressionElement El in Elements) {
				
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

		public override bool Test (DataRow Row) {
								
			ExpressionElement E1 = (ExpressionElement)Singles [0];
			ExpressionElement E2 = (ExpressionElement)Singles [1];

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

		public override bool Test (DataRow Row) {
								
			ExpressionElement E1 = (ExpressionElement)Singles [0];
			ExpressionElement E2 = (ExpressionElement)Singles [1];
					   
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

		public override bool Test (DataRow Row) {

			ExpressionElement E1 = (ExpressionElement)Singles [0];
			ExpressionElement E2 = (ExpressionElement)Singles [1];

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

		public override bool Test (DataRow Row) {
			
			ExpressionElement E1 = (ExpressionElement)Singles [0];
			ExpressionElement E2 = (ExpressionElement)Singles [1];

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

		public override bool Test (DataRow Row) {

			ExpressionElement E1 = (ExpressionElement)Singles [0];
			ExpressionElement E2 = (ExpressionElement)Singles [1];

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

		public override bool Test (DataRow Row) {
			
			ExpressionElement E1 = (ExpressionElement)Singles [0];
			ExpressionElement E2 = (ExpressionElement)Singles [1];

			return ExpressionElement.Compare (E1, E2, Row) != 0;
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
			foreach (ExpressionElement El in Elements) {
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
	       
		public override object Result (DataRow Row) {
			
			return Test(Row);
		}

		public override bool Test (DataRow Row) 
		{
			foreach (ExpressionElement El in Elements) {
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
        internal class ExpressionAdd : ExpressionElement
        {
        	public ExpressionAdd (string exp1, string exp2)
        	{        		
        		this.exp1 = exp1;
        		this.exp2 = exp2;
        		ParseExpression (exp1);
        		ParseExpression (exp2);

			ExpressionElement exp1Temp = ((ExpressionElement)Singles [0]);
			ExpressionElement exp2Temp = ((ExpressionElement)Singles [1]);

			if (exp1Temp.ResultType == typeof (string) || exp2Temp.ResultType == typeof (string))
				_ResultType = typeof (string);

			else if (exp1Temp.ResultType == typeof (long) || exp2Temp.ResultType == typeof (long))
				_ResultType = typeof (long);

			else if (exp1Temp.ResultType == typeof (int) || exp2Temp.ResultType == typeof (int))
				_ResultType = typeof (int);

        	}
        	
		public override Type ResultType 
		{
			get {
				return _ResultType;
			}
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

			ExpressionElement exp1Temp = ((ExpressionElement)Singles [0]);
			ExpressionElement exp2Temp = ((ExpressionElement)Singles [1]);

			if (exp1Temp.ResultType == typeof (string) || exp2Temp.ResultType == typeof (string))
				throw new EvaluateException ("Cannon perform '*' operation on " + exp1Temp.ResultType.ToString () + 
							     " and " + exp2Temp.ResultType.ToString ());

			else if (exp1Temp.ResultType == typeof (long) || exp2Temp.ResultType == typeof (long))
				_ResultType = typeof (long);

			else if (exp1Temp.ResultType == typeof (int) || exp2Temp.ResultType == typeof (int))
				_ResultType = typeof (int);

        	}
        	
		public override Type ResultType 
		{
			get {
				return _ResultType;
			}
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
        ///  Class for just one element for example string, int, ...
        /// </summary>
        internal class ExpressionSingleElement : ExpressionElement
        {        	
		private object Element = null;
        	
        	public ExpressionSingleElement (string s)
        	{

			// TODO: Every type should be checked

			if (s.StartsWith ("'") && s.EndsWith ("'")) {
				Element = s.Substring (1, s.Length - 2);
				_ResultType = typeof (string);
			}
			else if (!Char.IsDigit (s [0])) {
				Element = s;
				_ResultType = typeof (DataColumn);
			}
			else {
				_ResultType = typeof (int);
				Element = int.Parse (s);
			}				
        	}

        	public override Type ResultType
		{
			get {
				return _ResultType;
			}
		}

        	public override object Result (DataRow Row)
        	{
        		return Element;
        	}
        	
        	public override bool Test (DataRow Row)
        	{
        		throw new EvaluateException ();
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
        		this.exp1 = exp1;
			_ResultType = typeof (int);
			
        		ParseExpression (exp1);
        	}
        	
        	public override object Result (DataRow Row) 
		{
			ExpressionElement exp1Temp = ((ExpressionElement)Singles [0]);
			
			if (exp1Temp.ResultType == typeof (DataColumn)) {

				return ((string)Row [(string)exp1Temp.Result ()]).Length;
			}

			return null;
        	}
        	
        	public override bool Test (DataRow Row)
        	{
	       		throw new EvaluateException ();
        	}
        }

        /// <summary>
        ///  Class for isnull (expression, returnvalue) function
        /// </summary>
        internal class ExpressionIsNull : ExpressionElement
        {
        	public ExpressionIsNull (string exp)
        	{        		
			exp1 = exp.Substring (exp.IndexOf ("(") + 1, exp.IndexOf (",") - exp.IndexOf ("(") - 1); 
			exp2 = exp.Substring (exp.IndexOf (",") + 1, exp.IndexOf (")") - exp.IndexOf (",") - 1);
			
        		ParseExpression (exp1);
			ParseExpression (exp2);
			_ResultType = ((ExpressionElement)Singles [0]).ResultType;
        	}
        	
        	public override object Result (DataRow Row) 
		{
			ExpressionElement E1 = (ExpressionElement)Singles [0];
			ExpressionElement E2 = (ExpressionElement)Singles [1];
			
			object R1 = E1.Result (Row);
			object value1 = null;

			Type t1 = E1.ResultType;

			if (t1 == typeof (DataColumn))
				value1 = Row [R1.ToString ()];
			else 
				value1 = R1;

			if (value1 == null || value1 == DBNull.Value)
				return E2.Result (Row);

			return value1;
        	}
        	

        	// This method is shouldnt never invoked
        	public override bool Test (DataRow Row)
        	{
	       		throw new EvaluateException ();
        	}
        }

        /// <summary>
        ///  Class for iif (exp1, truepart, falsepart) function
        /// </summary>
        internal class ExpressionIif : ExpressionElement
        {
        	public ExpressionIif (string exp)
        	{        		
			_ResultType = typeof (bool);
			
        		ParseExpression (exp1);
        	}
        	
        	public override object Result (DataRow Row) 
		{
			ExpressionElement exp1Temp = ((ExpressionElement)Singles [0]);
			
			if (exp1Temp.ResultType == typeof (DataColumn)) {

				return ((string)Row [(string)exp1Temp.Result ()]).Length;
			}

			return null;
        	}
        	
        	public override Type ResultType
		{
			get {
				return _ResultType;
			}
		}

        	// This method is shouldnt never invoked
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
		// 
		// TODO/FIXME: This class should be inherited more than once. I mean own subclass for operators, functions,...
		//

		protected string exp1;
        	protected string exp2;
		protected  Type _ResultType;

        	protected ArrayList Elements = new ArrayList ();
        	protected ArrayList Singles = new ArrayList ();
        	
		/// <summary>
		/// Tells does the current expressions match to current DataRow
		/// </summary>
        	abstract public bool Test (DataRow Row);

		/// <summary>
		/// Return result of the current expression
		/// </summary>
		public object Result () 
		{
			return Result (null);
		}

		public virtual object Result (DataRow Row) {return null;}
                
        	public virtual Type ResultType
		{
			get {
				return null;
			}
		}

		protected object CalculateResult (DataRow Row)
		{
			ExpressionElement E1 = ((ExpressionElement)Singles [0]);
			ExpressionElement E2 = ((ExpressionElement)Singles [1]);
			Type t1 = E1.ResultType;
			Type t2 = E2.ResultType;
			object Result = null;
			
			if (t1 == typeof(DataColumn) && t2 == typeof (DataColumn)) {

				object value1 = Row [E1.Result ().ToString ()];
				object value2 = Row [E2.Result ().ToString ()];

 				// Check nulls
 				if (value1 ==  DBNull.Value || value2 == DBNull.Value)
 					return null;
				
				Type TempType = value1.GetType ();				
				if (TempType != value2.GetType ()) {
					value2 = Convert.ChangeType (value2, Type.GetTypeCode (TempType));
				}
				
				Result = Calculate (value1, value2, TempType);
			}

			else if (t1 == typeof (DataColumn)) {

				object value1 = Row [E1.Result (Row).ToString ()];
				object value2 = E2.Result (Row);
				
 				// Check nulls
 				if (value1 ==  DBNull.Value || value2 == DBNull.Value)
					return null;

				Type TempType = value2.GetType ();				
				if (TempType != value1.GetType ()) {
					value1 = Convert.ChangeType (value1, Type.GetTypeCode (TempType));
				}

				Result = Calculate (value1, value2, TempType);


			}
				 
			else if (t2 == typeof (DataColumn)) {
				
				object value1 = E1.Result (Row);
				object value2 = Row [E2.Result (Row).ToString ()];
				
 				// Check nulls
 				if (value1 ==  DBNull.Value || value2 == DBNull.Value)
					return null;

				Type TempType = value1.GetType ();				
				if (TempType != value2.GetType ()) {
					value2 = Convert.ChangeType (value2, Type.GetTypeCode (TempType));
				}
				
				Result =  Calculate (value1, value2, TempType);
			}

			else {
				object value1 = E1.Result ();
				object value2 = E2.Result ();
				
 				// Check nulls
 				if (value1 ==  DBNull.Value && value2 == DBNull.Value)
					return null;

				Type TempType = value2.GetType ();				
				if (TempType != value1.GetType ()) {
					value1 = Convert.ChangeType (value1, Type.GetTypeCode (TempType));
				}
				
				Result = Calculate (value1, value2, TempType);
			}
				 				 			      			
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
			int ReturnValue = 0;

			Type t1 = E1.ResultType;
			Type t2 = E2.ResultType;
			
			Type TempType = null;
			if (t1 == typeof (DataColumn) && t2 == typeof (DataColumn)) {
				
				object value1 = Row [E1.Result ().ToString ()];
				object value2 = Row [E2.Result ().ToString ()];
				
 				// Check nulls
 				if (value1 ==  DBNull.Value && value2 == DBNull.Value)
 					return 0;
 				else if (value1 ==  DBNull.Value)
 					return -1;
 				else if (value2 == DBNull.Value)
 					return 1;
				
				TempType = Row.Table.Columns [E1.Result ().ToString ()].DataType;
				if (TempType != value2.GetType ()) {
					value2 = Convert.ChangeType (value2, Type.GetTypeCode (TempType));
				}
				// TODO: Exceptions if column dont exists
				
				object Result = TempType.InvokeMember ("CompareTo", BindingFlags.Default | 
								       BindingFlags.InvokeMethod, null, 
								       value1, new object [] {value2});

				ReturnValue = (int)Result;
			}
			
			else if (t1 == typeof (DataColumn)) {
				
				object value1 = Row [E1.Result ().ToString ()];
				object value2 = E2.Result ();
				
				
 				if (value1 == DBNull.Value && value2 == null)
 					return 0;
 				else if (value1 == DBNull.Value)
 					return -1;
 				else if (value2 == null)
 					return 1;
				
				//TempType = Row.Table.Columns [E1.Result (Row).ToString ()].DataType;
				TempType = value2.GetType ();
				if (TempType != value1.GetType ()) {
					value1 = Convert.ChangeType (value1, Type.GetTypeCode (TempType));
				}
				
				// TODO: Exceptions if column dont exists
				object Result = TempType.InvokeMember ("CompareTo", BindingFlags.Default | 
								       BindingFlags.InvokeMethod, null, 
								       value1, new object [] {value2});

				ReturnValue = (int)Result;
				
			}
			
			else if (t2 == typeof (DataColumn)) {
				
				object value1 = E1.Result ();
				object value2 = Row [E2.Result ().ToString ()];
				
 				if (value1 == null && value2 == DBNull.Value)
 					return 0;
 				else if (value2 == DBNull.Value)
 					return 1;
 				else if (value1 == null)
 					return -1;
				
				TempType = Row.Table.Columns [E2.Result (Row).ToString ()].DataType;
				
				if (TempType  != value1.GetType ()) {
					value1 = Convert.ChangeType (value1, Type.GetTypeCode (TempType));
				}
				
				object Result = TempType.InvokeMember ("CompareTo", BindingFlags.Default | 
								       BindingFlags.InvokeMethod, null, 
								       value1,
								       new object [] {value2});
				ReturnValue = (int)Result;
			}
			else {
				

				object value1 = E1.Result (Row);
				object value2 = E2.Result (Row);

 				if (value1 == null && value2 == null)
 					return 0;
 				else if (value2 == null)
 					return 1;
 				else if (value1 == null)
 					return -1;

				TempType = value1.GetType ();
				if (TempType != value2.GetType ())
					value2 = Convert.ChangeType (value2, Type.GetTypeCode (TempType));

				object Result = TempType.InvokeMember ("CompareTo", BindingFlags.Default | 
								       BindingFlags.InvokeMethod, null, 
								       value1, 
								       new object [] {value2});

				ReturnValue = (int)Result;
			}

			return ReturnValue;
		}

		protected void ParseExpression (string s)
		{	
			//
			// TODO/FIXME: IMHO, this should be done with different kind of parsing:
			// char by char not operand by operand. 
			//

			string inside = "";
			string s1 = "";
			string s2 = "";

			
			// Find ()
			if (s.IndexOf ("(") != -1) {
			       
				//
				// finds firs parenthesis and put it and whats inside of it to
				// variable inside and removes it from s
				//

				int par = 1;
				int i = s.IndexOf ("(") + 1;				
				int startIndex = i - 1;
				int indexOfFunction = -1;
				
				if (s.IndexOf ("len") != -1)
					indexOfFunction = s.IndexOf ("len");					
				if (s.IndexOf ("isnull") != -1 && indexOfFunction > s.IndexOf ("isnull") || indexOfFunction == -1)
					indexOfFunction = s.IndexOf ("isnull");					
				if (s.IndexOf ("iif") != -1 && indexOfFunction > s.IndexOf ("iif") || indexOfFunction == -1)
					indexOfFunction = s.IndexOf ("iif");					
						
				if (s.IndexOf ("(") < indexOfFunction || indexOfFunction == -1) {

					char c;				
					while (par > 0) {
						
						c = s [i];
						if (c == '(')
							par++;
						if (c == ')')
							par--;
						
						if (par > 0)
							inside += c;
						i++;
					}
					
					s = s.Remove (startIndex, i - startIndex);
				}
			}
		
			// Find or
			if (s.IndexOf ("or") != -1)
				CreateOrElement (s, inside);
							
			// find and
			else if (s.IndexOf ("and") != -1 || s.IndexOf ("AND") != -1)
				CreateAndElement (s, inside);

			// find <>
			else if (s.IndexOf ("<>") != -1)
				CreateUnequalsElement (s, inside);
						
			// find <=
			else if (s.IndexOf ("<=") != -1)
				CreateLessThanOrEqualElement (s, inside);

			// find <
			else if (s.IndexOf ("<") != -1)
				CreateLessThanElement (s, inside);


			// find >=
			else if (s.IndexOf (">=") != -1)
				CreateGreaterThanOrEqualElement (s, inside);

			// find >
			else if (s.IndexOf (">") != -1)
				CreateGreaterThanElement (s, inside);

			// find =
			else if (s.IndexOf ("=") != -1)
				CreateEqualsElement (s, inside);


			// if there wasn't any operators like 'and' or 'not' there still could be
			// arithmetic operators like '+' or '-' or functions like 'iif' or 'substring'

			else if (s.IndexOf ("*") != -1)
				CreateMultiplyElement (s, inside);
			
			else if (s.IndexOf ("/") != -1)
				CreateDivideElement (s, inside);

			else if (s.IndexOf ("+") != -1)
				CreateAddElement (s, inside);

			else if (s.IndexOf ("-") != -1)
				CreateSubtractionElement (s, inside);

			// TODO: Find out that len is not a string in for example name = 'allen'
			else if (s.IndexOf ("len") != -1)
				CreateLenElement (s);

			else if (s.IndexOf ("isnull") != -1)
				CreateIsNullElement (s);

			else if (s.IndexOf ("iif") != -1)
				CreateIifElement (s);

			// if expression is like '(something someoperator something)'
			else if (inside.Trim () != string.Empty)
				ParseExpression (inside);

			
			// At least, if it wasnt any of the above it is just normat string or int
			// or....			
			else
				Singles.Add (new ExpressionSingleElement (s.Trim ()));
		}	       
	       
		#region CreateElement methods

		//
		// These methods are going to be removed when way of parsing is changed
		//

		private void CreateOrElement (string s, string inside) 
		{
			string s1 = s.Substring (0, s.IndexOf ("or"));
			string s2 = s.Substring (s.IndexOf ("or") + 2);

			CheckParenthesis (inside, ref s1, ref s2);
			
			Elements.Add (new ExpressionOr (s1.Trim (), s2.Trim ()));
		}

		private void CreateAndElement (string s, string inside)
		{
			string temp = s.ToLower ();
			
			string s1 = s.Substring (0, temp.IndexOf ("and")); 
			string s2 = s.Substring (temp.IndexOf ("and") + 3);

			s1 = s1.Trim ();


			CheckParenthesis (inside, ref s1, ref s2);

			Elements.Add (new ExpressionAnd (s1.Trim (), s2.Trim ()));
		}

		private void CreateEqualsElement (string s, string inside)
		{
			string s1 = s.Substring (0, s.IndexOf ("="));
			string s2 = s.Substring (s.IndexOf ("=") + 1);

			CheckParenthesis (inside, ref s1, ref s2);

			Elements.Add (new ExpressionEquals (s1.Trim (), s2.Trim ()));			
		}

		private void CreateUnequalsElement (string s, string inside)
		{
			string s1 = s.Substring (0, s.IndexOf ("<>"));
			string s2 = s.Substring (s.IndexOf ("<>") + 2);

			CheckParenthesis (inside, ref s1, ref s2);
			
			Elements.Add (new ExpressionUnequals (s1.Trim (), s2.Trim ()));
		}

		private void CreateLessThanElement (string s, string inside)
		{
			string s1 = s.Substring (0, s.IndexOf ("<"));
			string s2 = s.Substring (s.IndexOf ("<") + 1);

			CheckParenthesis (inside, ref s1, ref s2);

			Elements.Add (new ExpressionLessThan (s1.Trim (), s2.Trim ()));
		}

		private void CreateLessThanOrEqualElement (string s, string inside)
		{
			string s1 = s.Substring (0, s.IndexOf ("<="));
			string s2 = s.Substring (s.IndexOf ("<=") + 2);

			CheckParenthesis (inside, ref s1, ref s2);

			Elements.Add (new ExpressionLessThanOrEqual (s1.Trim (), s2.Trim ()));
		}

		private void CreateGreaterThanElement (string s, string inside)
		{
			string s1 = s.Substring (0, s.IndexOf (">"));
			string s2 = s.Substring (s.IndexOf (">") + 1);

			CheckParenthesis (inside, ref s1, ref s2);

			Elements.Add (new ExpressionGreaterThan (s1.Trim (), s2.Trim ()));
		}


		private void CreateGreaterThanOrEqualElement (string s, string inside)
		{
			string s1 = s.Substring (0, s.IndexOf (">="));
			string s2 = s.Substring (s.IndexOf (">=") + 2);

			CheckParenthesis (inside, ref s1, ref s2);

			Elements.Add (new ExpressionGreaterThanOrEqual (s1.Trim (), s2.Trim ()));
		}

		private void CreateMultiplyElement (string s, string inside)
		{
			string s1 = s.Substring (0, s.IndexOf ("*"));
			string s2 = s.Substring (s.IndexOf ("*") + 1);

			CheckParenthesis (inside, ref s1, ref s2);
			
			Singles.Add (new ExpressionMultiply (s1.Trim (), s2.Trim ()));
		}


		private void CreateDivideElement (string s, string inside)
		{
			string s1 = s.Substring (0, s.IndexOf ("+"));
			string s2 = s.Substring (s.IndexOf ("+") + 1);

			CheckParenthesis (inside, ref s1, ref s2);
			
			Singles.Add (new ExpressionDivide (s1.Trim (), s2.Trim ()));
		}

		private void CreateAddElement (string s, string inside)
		{
			string s1 = s.Substring (0, s.IndexOf ("+"));
			string s2 = s.Substring (s.IndexOf ("+") + 1);

			CheckParenthesis (inside, ref s1, ref s2);
			
			Singles.Add (new ExpressionAdd (s1.Trim (), s2.Trim ()));
		}

		private void CreateSubtractionElement (string s, string inside)
		{
			string s1 = s.Substring (0, s.IndexOf ("+"));
			string s2 = s.Substring (s.IndexOf ("+") + 1);

			CheckParenthesis (inside, ref s1, ref s2);
			
			Singles.Add (new ExpressionSubtraction (s1.Trim (), s2.Trim ()));
		}

		private void CreateLenElement (string s)
		{
			string s1 = s.Substring (s.IndexOf ("len") + 3);			
			Singles.Add (new ExpressionLen (s1.Trim ()));
		}

		private void CreateIsNullElement (string s)
		{
			// FIXME: If isnull ("test,fd", true) i.e. parsing sucks
			string temp = s.Substring (s.IndexOf ("isnull") + 5);
			Singles.Add (new ExpressionIsNull (temp));
		}
		
		private void CreateIifElement (string s)
		{
			string s1 = s.Substring (s.IndexOf ("iif") + 3);
			Singles.Add (new ExpressionLen (s1.Trim ()));
		}

		#endregion // CreateElemnt methods

		private void CheckParenthesis (string inside, ref string s1, ref string s2)
		{
			if (s1 == string.Empty && inside != string.Empty)
				s1 = inside;
			else if (s2 == string.Empty && inside != string.Empty)
				s2 = inside;	
		}
        }        
}
