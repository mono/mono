using System;
using System.Collections;

namespace Mono.Data.SqlExpressions {
	internal class Numeric {
		internal static bool IsNumeric (object o) {
			if (o is IConvertible) {
				TypeCode tc = ((IConvertible)o).GetTypeCode();
				if(TypeCode.Char <= tc && tc <= TypeCode.Decimal)
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
		internal static void ToSameType (ref IConvertible o1, ref IConvertible o2)
		{
			TypeCode tc1 = o1.GetTypeCode();
			TypeCode tc2 = o2.GetTypeCode();
			
			if (tc1 == tc2)
				return;

			// is it ok to make such assumptions about the order of an enum?
			if (tc1 < tc2)
				Convert.ChangeType (o1, tc2);
			else
				Convert.ChangeType (o2, tc1);
		}
		
		internal static IConvertible Add (IConvertible o1, IConvertible o2)
		{
			ToSameType (ref o1, ref o2);
			switch (o1.GetTypeCode()) {
			case TypeCode.Int32:
			default:
				return (int)o1 + (int)o2;
			case TypeCode.Int64:
				return (long)o1 + (long)o2;
			case TypeCode.Double:
				return (double)o1 + (double)o2;
			case TypeCode.Decimal:
				return (decimal)o1 + (decimal)o2;
			}
		}
		
		internal static IConvertible Subtract (IConvertible o1, IConvertible o2)
		{
			ToSameType (ref o1, ref o2);
			switch (o1.GetTypeCode()) {
			case TypeCode.Int32:
			default:
				return (int)o1 - (int)o2;
			case TypeCode.Int64:
				return (long)o1 - (long)o2;
			case TypeCode.Double:
				return (double)o1 - (double)o2;
			case TypeCode.Decimal:
				return (decimal)o1 - (decimal)o2;
			}
		}
		
		internal static IConvertible Multiply (IConvertible o1, IConvertible o2)
		{
			ToSameType (ref o1, ref o2);
			switch (o1.GetTypeCode()) {
			case TypeCode.Int32:
			default:
				return (int)o1 * (int)o2;
			case TypeCode.Int64:
				return (long)o1 * (long)o2;
			case TypeCode.Double:
				return (double)o1 * (double)o2;
			case TypeCode.Decimal:
				return (decimal)o1 * (decimal)o2;
			}
		}
		
		internal static IConvertible Divide (IConvertible o1, IConvertible o2)
		{
			ToSameType (ref o1, ref o2);
			switch (o1.GetTypeCode()) {
			case TypeCode.Int32:
			default:
				return (int)o1 / (int)o2;
			case TypeCode.Int64:
				return (long)o1 / (long)o2;
			case TypeCode.Double:
				return (double)o1 / (double)o2;
			case TypeCode.Decimal:
				return (decimal)o1 / (decimal)o2;
			}
		}
		
		internal static IConvertible Modulo (IConvertible o1, IConvertible o2)
		{
			ToSameType (ref o1, ref o2);
			switch (o1.GetTypeCode()) {
			case TypeCode.Int32:
			default:
				return (int)o1 % (int)o2;
			case TypeCode.Int64:
				return (long)o1 % (long)o2;
			case TypeCode.Double:
				return (double)o1 % (double)o2;
			case TypeCode.Decimal:
				return (decimal)o1 % (decimal)o2;
			}
		}
		
		internal static IConvertible Negative (IConvertible o)
		{
			switch (o.GetTypeCode()) {
			case TypeCode.Int32:
			default:
				return -((int)o);
			case TypeCode.Int64:
				return -((long)o);
			case TypeCode.Double:
				return -((double)o);
			case TypeCode.Decimal:
				return -((decimal)o);
			}
		}
		
		internal static IConvertible Min (IConvertible o1, IConvertible o2)
		{
			switch (o1.GetTypeCode()) {
			case TypeCode.Int32:
			default:
				return System.Math.Min ((int)o1, (int)o2);
			case TypeCode.Int64:
				return System.Math.Min ((long)o1, (long)o2);
			case TypeCode.Double:
				return System.Math.Min ((double)o1, (double)o2);
			case TypeCode.Decimal:
				return System.Math.Min ((decimal)o1, (decimal)o2);
			}
		}

		internal static IConvertible Max (IConvertible o1, IConvertible o2)
		{
			switch (o1.GetTypeCode()) {
			case TypeCode.Int32:
			default:
				return System.Math.Max ((int)o1, (int)o2);
			case TypeCode.Int64:
				return System.Math.Max ((long)o1, (long)o2);
			case TypeCode.Double:
				return System.Math.Max ((double)o1, (double)o2);
			case TypeCode.Decimal:
				return System.Math.Max ((decimal)o1, (decimal)o2);
			}
		}
	}
}