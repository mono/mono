//
// System.Data.ExpressionElement 
//
// Author:
//   Ville Palo <vi64pa@koti.soon.fi>
//
// Copyright (C) Ville Palo, 2003
//
// TODO: everything :)
// This just a sketch how this could be done (i hope).
//

using System;
using System.Data;


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

			if (Elements.Count == 0) {
								
				ExpressionElement E1 = (ExpressionElement)Singles [0];
				ExpressionElement E2 = (ExpressionElement)Singles [1];
					
				if (Row.Table.Columns.Contains (((string)E1.Result ()).Trim ())) {
				
					if (Row [((string)E1.Result ()).Trim ()].Equals (E2.Result ()))
						return true;
				}								
			}
			
			return false;
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
			
			if (Elements.Count == 0) {
				string ColumnName = "";
				string exp1Temp = exp1.Trim ();
				exp1Temp = exp1Temp.Trim ('\'');
				string exp2Temp = exp2.Trim ();
				exp2Temp = exp2Temp.Trim ('\'');

				if (exp1.IndexOf ('\'') == -1)
					ColumnName = exp1.Trim ();
				
				if (!Row [exp1].Equals (exp2))
					return true;
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
        	}
        	
        	public override object Result (DataRow Row) {
        		
			string exp1Temp = (string)((ExpressionElement)Singles [0]).Result ();
			string exp2Temp = (string)((ExpressionElement)Singles [1]).Result ();
			exp1Temp = exp1Temp.Trim ();
        		exp2Temp = exp2Temp.Trim ();
			exp1Temp = exp1Temp.Trim ('\'');
        		exp2Temp = exp2Temp.Trim ('\'');
        		
        		if (!Row.Table.Columns.Contains (exp1Temp.Trim ()) 
        			&& !Row.Table.Columns.Contains (exp2Temp.Trim ())) {
        				
       				return exp1Temp + exp2Temp;
        		}
        		
        		return null; 
        	}
        	
        	// This method is shouldnt never invoked
        	public override bool Test (DataRow Row)
        	{
	       		throw new EvaluateException ();
        	}
        }
        
        /// <summary>
        ///  Class for just one element for example string, int, ...
        /// </summary>
        internal class ExpressionSingleElement : ExpressionElement
        {        	
		private object Element = null;
        	
        	public ExpressionSingleElement (object s)
        	{
        		Element = s;
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

        /// <summary>
        ///  Parent class of all the elements of expression
        /// </summary>
        internal abstract class ExpressionElement
        {        	
		protected string exp1;
        	protected string exp2;

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

		protected void ParseExpression (string s)
		{							
			string inside = "";
			string s1 = "";
			string s2 = "";

			// Find ()
			if (s.IndexOf ("(") != -1) {
				int par = 1;
				int i = s.IndexOf ("(") + 1;
				int startIndex = i - 1;
				
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
		
			// Find or
			if (s.IndexOf ("or") != -1) {
				
				s1 = s.Substring (0, s.IndexOf ("or"));
				s2 = s.Substring (s.IndexOf ("or") + 2);
				if (s2 == string.Empty)
					s2 = inside;

				Elements.Add (new ExpressionOr (s1, s2));
			}
			
			// find and
			else if (s.IndexOf ("and") != -1) {
				s1 = s.Substring (0, s.IndexOf ("and"));
				s2 = s.Substring (s.IndexOf ("and") + 3);
				s1 = s1.Trim ();
				if (s1 == string.Empty && inside != string.Empty)
					s1 = inside;
				else if (s2 == string.Empty && inside != string.Empty)
				        s2 = inside;	
				
				Elements.Add (new ExpressionAnd (s1, s2));
			}
			
			// find =
			else if (s.IndexOf ("=") != -1) {
				s1 = s.Substring (0, s.IndexOf ("="));
				s2 = s.Substring (s.IndexOf ("=") + 2);
				Elements.Add (new ExpressionEquals (s1, s2));
			} 
			
			// if expression is like '(something someoperator something)'
			else if (inside.Trim () != string.Empty) {
				ParseExpression (inside);					
			}
			
			// if there wasn't any operators like 'and' or 'not' there still could be
			// arithmetic operators like '+' or '-' or functions like 'iif' or 'substring'
			
			else if (s.IndexOf ("+") != -1) {
				s1 = s.Substring (0, s.IndexOf ("+"));
				s2 = s.Substring (s.IndexOf ("+") + 1);
				Singles.Add (new ExpressionAdd (s1, s2));
			}
			
			// At least, if it wasnt any of the above it is just normat string or int
			// or....			
			else {
				Singles.Add (new ExpressionSingleElement (s));
			}
		}		
        }        
}
