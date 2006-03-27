//
// Aggregation.cs
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
	internal enum AggregationFunction {
		Count, Sum, Min, Max, Avg, StDev, Var
	}

	internal class Aggregation : BaseExpression {
		bool cacheResults;
		DataRow[] rows;
		ColumnReference column;
		AggregationFunction function;
		int count;
		IConvertible result;
		DataRowChangeEventHandler RowChangeHandler;
		DataTable table ;

		public Aggregation (bool cacheResults, DataRow[] rows, AggregationFunction function, ColumnReference column)
		{
			this.cacheResults = cacheResults;
			this.rows = rows;
			this.column = column;
			this.function = function;
			this.result = null;
			if (cacheResults)
				RowChangeHandler = new DataRowChangeEventHandler (InvalidateCache);
		}

		public override bool Equals(object obj)
		{
			if (!base.Equals (obj))
				return false;

			if (!(obj is Aggregation))
				return false;

			Aggregation other = (Aggregation) obj;
			if (!other.function.Equals( function))
				return false;

			if (!other.column.Equals (column))
				return false;		

			if (other.rows != null && rows != null) {
			if (other.rows.Length != rows.Length)
				return false;

			for (int i=0; i < rows.Length; i++)
				if (other.rows [i] != rows [i])
					return false;

			}
			else if (!(other.rows == null && rows == null))
				return false;
		
			return true;
		}

		public override int GetHashCode()
		{
			int hashCode = base.GetHashCode ();
			hashCode ^= function.GetHashCode ();
			hashCode ^= column.GetHashCode ();
			for (int i=0; i < rows.Length; i++)
				hashCode ^= rows [i].GetHashCode ();
			
			return hashCode;
		}
		
	
		public override object Eval (DataRow row)
		{
			//TODO: implement a better caching strategy and a mechanism for cache invalidation.
			//for now only aggregation over the table owning 'row' (e.g. 'sum(parts)'
			//in constrast to 'sum(child.parts)') is cached.
			if (cacheResults && result != null && column.ReferencedTable == ReferencedTable.Self)
				return result;
				
			count = 0;
			result = null;
			
			object[] values;
			if (rows == null)
				values = column.GetValues (column.GetReferencedRows (row));
			else
				values = column.GetValues (rows);
			
			foreach (object val in values) {
				if (val == null)
					continue;
					
				count++;
				Aggregate ((IConvertible)val);
			}

			switch (function) {
			case AggregationFunction.StDev:
			case AggregationFunction.Var:
				result = CalcStatisticalFunction (values);
				break;
					
			case AggregationFunction.Avg:
				result = ((count == 0) ? DBNull.Value : Numeric.Divide (result, count));
				break;
			
			case AggregationFunction.Count:
				result = count;
				break;
			}
			
			if (result == null)
				result = DBNull.Value;
			
			if (cacheResults && column.ReferencedTable == ReferencedTable.Self) 
			{
				table = row.Table;
				row.Table.RowChanged += RowChangeHandler;
			}	
			return result;
		}

		override public bool DependsOn(DataColumn other)
		{
			return column.DependsOn(other);
		}
		
		private void Aggregate (IConvertible val)
		{
			switch (function) {
			case AggregationFunction.Min:
				result = (result != null ? Numeric.Min (result, val) : val);
				return;
			
			case AggregationFunction.Max:
				result = (result != null ? Numeric.Max (result, val) : val);
				return;

			case AggregationFunction.Sum:
			case AggregationFunction.Avg:
			case AggregationFunction.StDev:
 			case AggregationFunction.Var:
				result = (result != null ? Numeric.Add (result, val) : val);
				return;
			}
		}
		
		private IConvertible CalcStatisticalFunction (object[] values)
		{
			if (count < 2)
				return DBNull.Value;

			double average = (double)Convert.ChangeType(result, TypeCode.Double) / count;
			double res = 0.0;
						
			foreach (object val in values) {
				if (val == null)
					continue;
					
				double diff = average - (double)Convert.ChangeType(val, TypeCode.Double);
				res += System.Math.Pow (diff, 2);
			}
			res /= (count - 1);
			
			if (function == AggregationFunction.StDev)
				res = System.Math.Sqrt (res);

			return res;
		}

		public override void ResetExpression ()
		{
			if (table != null)
				InvalidateCache (table, null);
		}

		private void InvalidateCache (Object sender, DataRowChangeEventArgs args)
		{
			result = null; 
			((DataTable)sender).RowChanged -= RowChangeHandler;
		}
	}
}
