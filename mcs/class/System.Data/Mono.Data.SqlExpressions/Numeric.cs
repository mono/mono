
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

namespace Mono.Data.SqlExpressions {
	internal class Numeric {
		internal static bool IsNumeric (object o) {
			if (o is IConvertible) {
				TypeCode tc = ((IConvertible)o).GetTypeCode();
				if(TypeCode.Char < tc && tc <= TypeCode.Decimal)
					return true;
			}
			return false;
		}

		//extends to Int32/Int64/Decimal/Double
		internal static IConvertible Unify (IConvertible o)
		{
			switch (o.GetTypeCode()) {
			case TypeCode.Char:
			case TypeCode.SByte:
			case TypeCode.Byte:
			case TypeCode.Int16:
			case TypeCode.UInt16:
				return (IConvertible)Convert.ChangeType (o, TypeCode.Int32);
			
			case TypeCode.UInt32:
				return (IConvertible)Convert.ChangeType (o, TypeCode.Int64);
				
			case TypeCode.UInt64:
				return (IConvertible)Convert.ChangeType (o, TypeCode.Decimal);
				
			case TypeCode.Single:
				return (IConvertible)Convert.ChangeType (o, TypeCode.Double);
			
			default:
				return o;
			}
		}
		
		//(note: o1 and o2 must both be of type Int32/Int64/Decimal/Double)
		internal static TypeCode ToSameType (ref IConvertible o1, ref IConvertible o2)
		{
			TypeCode tc1 = o1.GetTypeCode();
			TypeCode tc2 = o2.GetTypeCode();
			
			if (tc1 == tc2)
				return tc1;

			if (tc1 == TypeCode.DBNull || tc2 == TypeCode.DBNull)
				return TypeCode.DBNull;


			// is it ok to make such assumptions about the order of an enum?
			if (tc1 < tc2)
			{
				o1 = (IConvertible)Convert.ChangeType (o1, tc2);
				return tc2;
			}
			else
			{
				o2 = (IConvertible)Convert.ChangeType (o2, tc1);
				return tc1;
			}
		}
		
		internal static IConvertible Add (IConvertible o1, IConvertible o2)
		{
			switch (ToSameType (ref o1, ref o2)) {
			case TypeCode.Int32:
				return (long)((int)o1 + (int)o2);
			case TypeCode.Int64:
				return (long)o1 + (long)o2;
			case TypeCode.Double:
				return (double)o1 + (double)o2;
			case TypeCode.Decimal:
				return (decimal)o1 + (decimal)o2;
			default:
				return DBNull.Value;
			}
		}
		
		internal static IConvertible Subtract (IConvertible o1, IConvertible o2)
		{
			switch (ToSameType (ref o1, ref o2)) {
			case TypeCode.Int32:
				return (int)o1 - (int)o2;
			case TypeCode.Int64:
				return (long)o1 - (long)o2;
			case TypeCode.Double:
				return (double)o1 - (double)o2;
			case TypeCode.Decimal:
				return (decimal)o1 - (decimal)o2;
			default:
				return DBNull.Value;
			}
		}
		
		internal static IConvertible Multiply (IConvertible o1, IConvertible o2)
		{
			switch (ToSameType (ref o1, ref o2)) {
			case TypeCode.Int32:
				return (int)o1 * (int)o2;
			case TypeCode.Int64:
				return (long)o1 * (long)o2;
			case TypeCode.Double:
				return (double)o1 * (double)o2;
			case TypeCode.Decimal:
				return (decimal)o1 * (decimal)o2;
			default:
				return DBNull.Value;
			}
		}
		
		internal static IConvertible Divide (IConvertible o1, IConvertible o2)
		{
			switch (ToSameType (ref o1, ref o2)) {
			case TypeCode.Int32:
				return (int)o1 / (int)o2;
			case TypeCode.Int64:
				return (long)o1 / (long)o2;
			case TypeCode.Double:
				return (double)o1 / (double)o2;
			case TypeCode.Decimal:
				return (decimal)o1 / (decimal)o2;
			default:
				return DBNull.Value;
			}
		}
		
		internal static IConvertible Modulo (IConvertible o1, IConvertible o2)
		{
			switch (ToSameType (ref o1, ref o2)) {
			case TypeCode.Int32:
				return (int)o1 % (int)o2;
			case TypeCode.Int64:
				return (long)o1 % (long)o2;
			case TypeCode.Double:
				return (double)o1 % (double)o2;
			case TypeCode.Decimal:
				return (decimal)o1 % (decimal)o2;
			default:
				return DBNull.Value;
			}
		}
		
		internal static IConvertible Negative (IConvertible o)
		{
			switch (o.GetTypeCode()) {
			case TypeCode.Int32:
				return -((int)o);
			case TypeCode.Int64:
				return -((long)o);
			case TypeCode.Double:
				return -((double)o);
			case TypeCode.Decimal:
				return -((decimal)o);
			default:
				return DBNull.Value;
			}
		}
		
		internal static IConvertible Min (IConvertible o1, IConvertible o2)
		{
			switch (ToSameType (ref o1, ref o2)) {
			case TypeCode.Int32:
				return System.Math.Min ((int)o1, (int)o2);
			case TypeCode.Int64:
				return System.Math.Min ((long)o1, (long)o2);
			case TypeCode.Double:
				return System.Math.Min ((double)o1, (double)o2);
			case TypeCode.Decimal:
				return System.Math.Min ((decimal)o1, (decimal)o2);
			case TypeCode.String:
				int result = String.Compare ((string)o1, (string)o2);
				if (result <= 0)
					return o1;
				return o2;
			default:
				return DBNull.Value;
			}
		}

		internal static IConvertible Max (IConvertible o1, IConvertible o2)
		{
			switch (ToSameType (ref o1, ref o2)) {
			case TypeCode.Int32:
				return System.Math.Max ((int)o1, (int)o2);
			case TypeCode.Int64:
				return System.Math.Max ((long)o1, (long)o2);
			case TypeCode.Double:
				return System.Math.Max ((double)o1, (double)o2);
			case TypeCode.Decimal:
				return System.Math.Max ((decimal)o1, (decimal)o2);
			case TypeCode.String:
				int result = String.Compare ((string)o1, (string)o2);
				if (result >= 0)
					return o1;
				return o2;
			default:
				return DBNull.Value;
			}
		}
	}
}
