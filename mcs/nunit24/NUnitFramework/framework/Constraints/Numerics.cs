using System;

namespace NUnit.Framework.Constraints
{
	/// <summary>
	/// The Numerics class contains common operations on numeric values.
	/// </summary>
	public class Numerics
	{
		#region Numeric Type Recognition
		/// <summary>
		/// Checks the type of the object, returning true if
		/// the object is a numeric type.
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <returns>true if the object is a numeric type</returns>
		public static bool IsNumericType(Object obj)
		{
			return IsFloatingPointNumeric( obj ) || IsFixedPointNumeric( obj );
		}

		/// <summary>
		/// Checks the type of the object, returning true if
		/// the object is a floating point numeric type.
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <returns>true if the object is a floating point numeric type</returns>
		public static bool IsFloatingPointNumeric(Object obj)
		{
			if (null != obj)
			{
				if (obj is double) return true;
				if (obj is float) return true;

				if (obj is System.Double) return true;
				if (obj is System.Single) return true;
			}
			return false;
		}
		/// <summary>
		/// Checks the type of the object, returning true if
		/// the object is a fixed point numeric type.
		/// </summary>
		/// <param name="obj">The object to check</param>
		/// <returns>true if the object is a fixed point numeric type</returns>
		public static bool IsFixedPointNumeric(Object obj)
		{
			if (null != obj)
			{
				if (obj is byte) return true;
				if (obj is sbyte) return true;
				if (obj is decimal) return true;
				if (obj is int) return true;
				if (obj is uint) return true;
				if (obj is long) return true;
				if (obj is short) return true;
				if (obj is ushort) return true;

				if (obj is System.Byte) return true;
				if (obj is System.SByte) return true;
				if (obj is System.Decimal) return true;
				if (obj is System.Int32) return true;
				if (obj is System.UInt32) return true;
				if (obj is System.Int64) return true;
				if (obj is System.UInt64) return true;
				if (obj is System.Int16) return true;
				if (obj is System.UInt16) return true;
			}
			return false;
		}
		#endregion

		#region Numeric Equality
        /// <summary>
        /// Test two numeric values for equality, performing the usual numeric 
        /// conversions and using a provided or default tolerance. If the value 
        /// referred to by tolerance is null, this method may set it to a default.
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The actual value</param>
        /// <param name="tolerance">A reference to the numeric tolerance in effect</param>
        /// <returns>True if the values are equal</returns>
		public static bool AreEqual( object expected, object actual, ref object tolerance )
		{
            if (IsFloatingPointNumeric(expected) || IsFloatingPointNumeric(actual))
                return AreEqual(Convert.ToDouble(expected), Convert.ToDouble(actual), ref tolerance);

			if ( expected is decimal || actual is decimal )
				return AreEqual( Convert.ToDecimal(expected), Convert.ToDecimal(actual), Convert.ToDecimal(tolerance) );
			
			if ( expected is ulong || actual is ulong )
				return AreEqual( Convert.ToUInt64(expected), Convert.ToUInt64(actual), Convert.ToUInt64(tolerance) );
		
			if ( expected is long || actual is long )
				return AreEqual( Convert.ToInt64(expected), Convert.ToInt64(actual), Convert.ToInt64(tolerance) );
			
			if ( expected is uint || actual is uint )
				return AreEqual( Convert.ToUInt32(expected), Convert.ToUInt32(actual), Convert.ToUInt32(tolerance) );

			return AreEqual( Convert.ToInt32(expected), Convert.ToInt32(actual), Convert.ToInt32(tolerance) );
		}

		private static bool AreEqual( double expected, double actual, ref object tolerance )
		{
            if (double.IsNaN(expected) && double.IsNaN(actual))
                return true;
            // handle infinity specially since subtracting two infinite values gives 
            // NaN and the following test fails. mono also needs NaN to be handled
            // specially although ms.net could use either method.
            if (double.IsInfinity(expected) || double.IsNaN(expected) || double.IsNaN(actual))
                return expected.Equals(actual);

            if (tolerance != null)
                return Math.Abs(expected - actual) <= Convert.ToDouble(tolerance);

            if (GlobalSettings.DefaultFloatingPointTolerance > 0.0d
                && !double.IsNaN(expected) && !double.IsInfinity(expected))
            {
                tolerance = GlobalSettings.DefaultFloatingPointTolerance;
                return Math.Abs(expected - actual) <= GlobalSettings.DefaultFloatingPointTolerance;
            }

			return expected.Equals( actual );
		}

		private static bool AreEqual( decimal expected, decimal actual, decimal tolerance )
		{
			if ( tolerance > 0m )
				return Math.Abs(expected - actual) <= tolerance;
				
			return expected.Equals( actual );
		}

		private static bool AreEqual( ulong expected, ulong actual, ulong tolerance )
		{
			if ( tolerance > 0ul )
			{
				ulong diff = expected >= actual ? expected - actual : actual - expected;
				return diff <= tolerance;
			}

			return expected.Equals( actual );
		}

		private static bool AreEqual( long expected, long actual, long tolerance )
		{
			if ( tolerance > 0L )
				return Math.Abs(expected - actual) <= tolerance;

			return expected.Equals( actual );
		}

		private static bool AreEqual( uint expected, uint actual, uint tolerance )
		{
			if ( tolerance > 0 )
			{
				uint diff = expected >= actual ? expected - actual : actual - expected;
				return diff <= tolerance;
			}
				
			return expected.Equals( actual );
		}

		private static bool AreEqual( int expected, int actual, int tolerance )
		{
			if ( tolerance > 0 )
				return Math.Abs(expected - actual) <= tolerance;
				
			return expected.Equals( actual );
		}
		#endregion

		#region Numeric Comparisons 
        /// <summary>
        /// Compare two numeric values, performing the usual numeric conversions.
        /// </summary>
        /// <param name="expected">The expected value</param>
        /// <param name="actual">The actual value</param>
        /// <returns></returns>
		public static int Compare( IComparable expected, object actual )
		{
			if ( expected == null )
				throw new ArgumentException( "Cannot compare using a null reference", "expected" );

			if ( actual == null )
				throw new ArgumentException( "Cannot compare to null reference", "actual" );

			if( IsNumericType( expected ) && IsNumericType( actual ) )
			{
				if ( IsFloatingPointNumeric(expected) || IsFloatingPointNumeric(actual) )
					return Convert.ToDouble(expected).CompareTo(Convert.ToDouble(actual));

				if ( expected is decimal || actual is decimal )
					return Convert.ToDecimal(expected).CompareTo(Convert.ToDecimal(actual));
			
				if ( expected is ulong || actual is ulong )
					return Convert.ToUInt64(expected).CompareTo(Convert.ToUInt64(actual));
		
				if ( expected is long || actual is long )
					return Convert.ToInt64(expected).CompareTo(Convert.ToInt64(actual));
			
				if ( expected is uint || actual is uint )
					return Convert.ToUInt32(expected).CompareTo(Convert.ToUInt32(actual));

				return Convert.ToInt32(expected).CompareTo(Convert.ToInt32(actual));
			}
			else
				return expected.CompareTo(actual);
		}
		#endregion

		private Numerics()
		{
		}
	}
}
