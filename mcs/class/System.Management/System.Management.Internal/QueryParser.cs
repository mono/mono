//
// AssemblyRef
//
// Author:
//	Bruno Lauze     (brunolauze@msn.com)
//	Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2015 Microsoft (http://www.microsoft.com)
//

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
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq.Dynamic;
using System.Text.RegularExpressions;

namespace System.Management
{

	public abstract class QueryParser
	{
		public static string GetQueryFromPath (string path)
		{
			string[] paths = path.Split (new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
			//string server = paths.ElementAtOrDefault (0);
			//string namespace1 = paths.ElementAtOrDefault (1);
			//string namepsace2 = paths.ElementAtOrDefault (2);
			string className = path.Contains ("=") ? paths.ElementAtOrDefault (paths.Count () - 2) : paths.LastOrDefault ();
			string filter = path.Contains ("=") ? paths.LastOrDefault ().Replace ("=", " = ") : (string)null;

			if (string.IsNullOrEmpty (filter))
				return string.Format ("SELECT * FROM {0}", className);
			return string.Format ("SELECT * FROM {0} WHERE {1}", className, filter);
		}

		public abstract IDictionary<string, object> GetWhereClauses (string strQuery, IDictionary<string, string> propMap);

		protected static void GetWhereClause (string statement, IDictionary<string, string> propMap, IDictionary<string, object> ret, Type targetType)
		{
			string[] andStatments = Regex.Split(statement, " AND ", RegexOptions.IgnoreCase).Where (x => !string.IsNullOrEmpty (x)).ToArray ();
			foreach (var strQuery in andStatments) { 
				if (!string.IsNullOrEmpty (strQuery.Trim ())) {
					int indexLike = strQuery.IndexOf ("LIKE", StringComparison.OrdinalIgnoreCase);
					
					int indexGreaterEqual = strQuery.IndexOf (">=", StringComparison.OrdinalIgnoreCase);
					
					int indexSmallerEqual = strQuery.IndexOf ("<=", StringComparison.OrdinalIgnoreCase);
					
					int indexGreater = strQuery.IndexOf (">", StringComparison.OrdinalIgnoreCase);
					
					int indexSmaller = strQuery.IndexOf ("<", StringComparison.OrdinalIgnoreCase);
					
					int indexEqual = strQuery.IndexOf ("=", StringComparison.OrdinalIgnoreCase);
					
					
					if (indexLike != -1) {
						var t = Extract (strQuery, indexLike, indexLike + "LIKE".Length);
						ret.Add (t.Item1, GetValue(t.Item2, GetPropertyType (t.Item1, propMap, targetType)));
					}
					
					if (indexGreaterEqual != -1) {
						var t = Extract (strQuery, indexGreaterEqual, indexGreaterEqual + 2);
						ret.Add (t.Item1, GetValue(t.Item2, GetPropertyType (t.Item1, propMap, targetType)));
					}
					
					if (indexSmallerEqual != -1) {
						var t = Extract (strQuery, indexSmallerEqual, indexSmallerEqual + 2);
						ret.Add (t.Item1, GetValue(t.Item2, GetPropertyType (t.Item1, propMap, targetType)));
					}
					
					
					if (indexGreater != -1) {
						var t = Extract (strQuery, indexGreater, indexGreater+ 1);
						ret.Add (t.Item1, GetValue(t.Item2, GetPropertyType (t.Item1, propMap, targetType)));
					}
					
					if (indexSmaller != -1) {
						var t = Extract (strQuery, indexSmaller, indexSmaller + 1);
						ret.Add (t.Item1, GetValue(t.Item2, GetPropertyType (t.Item1, propMap, targetType)));
					}
					
					if (indexEqual != -1) {
						var t = Extract (strQuery, indexEqual, indexEqual + 1);
						ret.Add (t.Item1, GetValue(t.Item2, GetPropertyType (t.Item1, propMap, targetType)));
					}
				}
			}
		}

		
		protected static Tuple<string ,string> Extract(string strQuery, int indexStart, int indexEnd)
		{
			string fieldName = strQuery.Substring (0, indexStart).Trim ();
			string value = strQuery.Substring (indexEnd, strQuery.Length - indexEnd).Trim ();
			return new Tuple<string, string>(fieldName, value);
		}

		
		protected static Type GetPropertyType (string fieldName, IDictionary<string, string> propMap, Type targetType)
		{
			if (propMap != null) {
				string key = propMap.Keys.Where (x => x.Equals (fieldName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault ();
				if (!string.IsNullOrEmpty (key)) fieldName = propMap[key];
			}
			var pProp = targetType.GetProperty (fieldName); 
			
			return pProp == null ? typeof(string) : pProp.PropertyType;
		}
		
		
		protected static Expression GetConstant (string value, Type type)
		{
			return Expression.Constant(GetValue (value, type), type);;
		}
		
		protected static object GetValue(string value, Type  type) {
			object obj = value;
			var indexSep = value.IndexOf ("\"", StringComparison.OrdinalIgnoreCase);
			if (indexSep != -1)
				value = value.Replace ("\"", "");
			else
				value = value.Replace ("'", "");

			if (type == typeof(string))
				obj = value;
			
			if (type == typeof(int)) {
				int i = 0;
				int.TryParse (value, out i);
				obj = i;
			}
			
			if (type == typeof(long)) {
				long i = 0;
				long.TryParse (value, out i);
				obj = i;
			}
			
			
			if (type == typeof(double)) {
				double i = 0;
				double.TryParse (value, out i);
				obj = i;
			}
			
			
			if (type == typeof(decimal)) {
				decimal i = 0;
				decimal.TryParse (value, out i);
				obj = i;
			}
			
			
			if (type == typeof(short)) {
				short i = 0;
				short.TryParse (value, out i);
				obj = i;
			}
			
			
			if (type == typeof(uint)) {
				uint i = 0;
				uint.TryParse (value, out i);
				obj = i;
			}
			
			if (type == typeof(ushort)) {
				ushort i = 0;
				ushort.TryParse (value, out i);
				obj = i;
			}
			
			if (type == typeof(ulong)) {
				ulong i = 0;
				ulong.TryParse (value, out i);
				obj = i;
			}
			if (type == typeof(Guid)) {
				Guid i = Guid.Empty;
				Guid.TryParse (value, out i);
				obj = i;
			}
			
			if (type == typeof(bool)) {
				obj = value.Equals("TRUE", StringComparison.OrdinalIgnoreCase) ? true : (value.Equals("1", StringComparison.OrdinalIgnoreCase) ? true : false);
			}
			return obj;
		}

		public abstract IQueryable Parse(IEnumerable<object> queryable, string strQuery);
	}
	
	public class QueryParser<T> : QueryParser
	{
		public override IQueryable Parse (IEnumerable<object> queryable, string strQuery)
		{
			return ParseInternal(queryable.OfType<T>().AsQueryable (), strQuery);
		}

		protected static IQueryable<T> ParseInternal(IQueryable<T> obj, string strQuery)
		{
			string whereClause = "";
			string orderClause = "";
			var indexWhere = strQuery.IndexOf (" WHERE ", StringComparison.OrdinalIgnoreCase);
			if (indexWhere == -1)
				return obj;
			var indexOrder = strQuery.IndexOf (" ORDER BY ", StringComparison.OrdinalIgnoreCase);
			if (indexOrder == -1) {
				whereClause = strQuery.Substring (indexWhere + " WHERE ".Length);
			} else {
				orderClause = strQuery.Substring (indexOrder + " ORDER BY ".Length);;
				whereClause = strQuery.Substring (indexWhere + " WHERE ".Length, indexOrder - (indexWhere + " WHERE ".Length));
			}
			if (!string.IsNullOrEmpty (whereClause)) {
				obj = obj.Where (whereClause);
			}
			if (!string.IsNullOrEmpty (orderClause)) {
				obj = obj.OrderBy (orderClause);
			}
			return obj;
		}

		public override IDictionary<string, object> GetWhereClauses (string strQuery, IDictionary<string, string> propMap)
		{
			var ret = new Dictionary<string, object>();
			var indexWhere = strQuery.IndexOf (" WHERE ", StringComparison.OrdinalIgnoreCase);
			if (indexWhere == -1)
				return null;
			var indexOrder = strQuery.IndexOf (" ORDER BY ", StringComparison.OrdinalIgnoreCase);
			if (indexOrder == -1) {
				strQuery = strQuery.Substring (indexWhere + " WHERE ".Length);
			} else {
				strQuery = strQuery.Substring (indexWhere + " WHERE ".Length, indexOrder - (indexWhere + " WHERE ".Length));
			}
			string[] lines = strQuery.Replace ("(", "").Split (new char[] { ')' }, StringSplitOptions.RemoveEmptyEntries);
			foreach(var line in lines) {
				string[] orStatments = Regex.Split(line, " OR ", RegexOptions.IgnoreCase).Where (x => !string.IsNullOrEmpty (x)).ToArray ();
				foreach (var statement in orStatments) {
					if (!string.IsNullOrEmpty (statement.Trim()))
					{
						GetWhereClause(statement, propMap, ret, typeof(T));
					}
				}
			}
			return ret;
		}

		/*
		protected static Expression GetWhereExpression(string strQuery, ParameterExpression pe, IDictionary<string, string> propMap, Type targetType)
		{
			var indexWhere = strQuery.IndexOf (" WHERE ", StringComparison.OrdinalIgnoreCase);
			if (indexWhere == -1)
				return null;
			var indexOrder = strQuery.IndexOf (" ORDER BY ", StringComparison.OrdinalIgnoreCase);
			if (indexOrder == -1) {
				strQuery = strQuery.Substring (indexWhere + " WHERE ".Length);
			} else {
				strQuery = strQuery.Substring (indexWhere + " WHERE ".Length, indexOrder - (indexWhere + " WHERE ".Length));
			}
			Expression ex = null;
			
			string[] lines = strQuery.Replace ("(", "").Split (new char[] { ')' }, StringSplitOptions.RemoveEmptyEntries);
			foreach(var line in lines) {
				string[] orStatments = Regex.Split(line, " OR ", RegexOptions.IgnoreCase).Where (x => !string.IsNullOrEmpty (x)).ToArray ();
				foreach (var statement in orStatments) {
					if (!string.IsNullOrEmpty (statement.Trim()))
					{
						if (ex == null) 
						{
							ex = GetExpression(statement.Trim (), pe, propMap, targetType);
						}
						else {
							ex = Expression.Or(ex, GetExpression(statement.Trim (), pe, propMap, targetType));
						}
					}
				}
			}
			return ex;
		}
		
		protected static Expression GetExpression (string strQuery, ParameterExpression pe, IDictionary<string, string> propMap, Type targetType)
		{
			Expression ex = null;
			string[] andStatments = Regex.Split(strQuery, " AND ", RegexOptions.IgnoreCase).Where (x => !string.IsNullOrEmpty (x)).ToArray ();
			foreach (var statement in andStatments) { 
				if (!string.IsNullOrEmpty (statement.Trim()))
				{
					if (ex == null) 
					{
						ex = GetBinaryExpression(statement, pe, propMap, targetType);
					}
					else {
						ex = Expression.And(ex, GetBinaryExpression(statement, pe, propMap, targetType));
					}
				}
			}
			return ex;
		}
		
		protected static Expression GetBinaryExpression (string strQuery, ParameterExpression pe, IDictionary<string, string> propMap, Type targetType)
		{
			int indexLike = strQuery.IndexOf ("LIKE", StringComparison.OrdinalIgnoreCase);
			
			int indexGreaterEqual = strQuery.IndexOf (">=", StringComparison.OrdinalIgnoreCase);
			
			int indexSmallerEqual = strQuery.IndexOf ("<=", StringComparison.OrdinalIgnoreCase);
			
			int indexGreater = strQuery.IndexOf (">", StringComparison.OrdinalIgnoreCase);
			
			int indexSmaller = strQuery.IndexOf ("<", StringComparison.OrdinalIgnoreCase);
			
			int indexEqual = strQuery.IndexOf ("=", StringComparison.OrdinalIgnoreCase);
			
			
			if (indexLike != -1) {
				var t = Extract (strQuery, indexLike, indexLike + "LIKE".Length);
				return GetBinaryExpression (t.Item1, t.Item2, pe, ComparisonOperator.Contains, propMap, targetType);
			}
			
			if (indexGreaterEqual != -1) {
				var t = Extract (strQuery, indexGreaterEqual, indexGreaterEqual + 2);
				return GetBinaryExpression (t.Item1, t.Item2, pe, ComparisonOperator.GreaterOrEqual, propMap, targetType);
			}
			
			if (indexSmallerEqual != -1) {
				var t = Extract (strQuery, indexSmallerEqual, indexSmallerEqual + 2);
				return GetBinaryExpression (t.Item1, t.Item2, pe, ComparisonOperator.SmallerOrEqual, propMap, targetType);
			}
			
			
			if (indexGreater != -1) {
				var t = Extract (strQuery, indexGreater, indexGreater+ 1);
				return GetBinaryExpression (t.Item1, t.Item2, pe, ComparisonOperator.Greater, propMap, targetType);
			}
			
			if (indexSmaller != -1) {
				var t = Extract (strQuery, indexSmaller, indexSmaller + 1);
				return GetBinaryExpression (t.Item1, t.Item2, pe, ComparisonOperator.Smaller, propMap, targetType);
			}
			
			if (indexEqual != -1) {
				var t = Extract (strQuery, indexEqual, indexEqual + 1);
				return GetBinaryExpression (t.Item1, t.Item2, pe, ComparisonOperator.Equal, propMap, targetType);
			}
			
			
			return null;
		}

		protected static Expression GetBinaryExpression (string fieldName, string value, ParameterExpression pe, ComparisonOperator op, IDictionary<string, string> propMap, Type targetType)
		{
			Expression ex = null;
			if (propMap != null) {
				string key = propMap.Keys.Where (x => x.Equals (fieldName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault ();
				if (!string.IsNullOrEmpty (key))
					fieldName = propMap [key];
			}
			var pProp = targetType.GetProperty (fieldName);
			if (pProp != null) {
				var valueEx = GetConstant (value, pProp.PropertyType);
				Expression p = Expression.Property (pe, pProp);
				switch (op) {
				case ComparisonOperator.Contains:
					ex = Expression.Call (p, "Contains", null, new Expression[] { valueEx });
					break;
				case ComparisonOperator.Equal:
					ex = Expression.Equal (p, valueEx);
					break;
				case ComparisonOperator.GreaterOrEqual:
					ex = Expression.GreaterThanOrEqual (p, valueEx);
					break;
				case ComparisonOperator.SmallerOrEqual:
					ex = Expression.LessThanOrEqual (p, valueEx);
					break;
				case ComparisonOperator.Greater:
					ex = Expression.GreaterThan (p, valueEx);
					break;
				case ComparisonOperator.Smaller:
					ex = Expression.LessThan (p, valueEx);
					break;
				}
			}
			return ex;
		}
		*/

		protected enum ComparisonOperator
		{
			Contains,
			GreaterOrEqual,
			SmallerOrEqual,
			Greater,
			Smaller,
			Equal
		}

	}
}
