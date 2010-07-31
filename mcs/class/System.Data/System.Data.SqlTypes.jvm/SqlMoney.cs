// System.Data.SqlTypes.SqlMoney
//
// Authors:
//	Konstantin Triger <kostat@mainsoft.com>
//	Boris Kirzner <borisk@mainsoft.com>
//	
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
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

namespace System.Data.SqlTypes
{

	/**
	 * <p>Title: </p>
	 * <p>Description: </p>
	 * <p>Copyright: Copyright (c) 2002</p>
	 * <p>Company: MainSoft</p>
	 * @author Pavel Sandler
	 * @version 1.0
	 */

	using System;


	/*
	* CURRENT LIMITATIONS:
	*/


	public struct SqlMoney : INullable
	{

		private Decimal _value;
		private bool _isNull;

		public static readonly SqlMoney MaxValue = new SqlMoney(922337203685475.5807m, true);
		public static readonly SqlMoney MinValue = new SqlMoney(-922337203685477.5808m, true);
		public static readonly SqlMoney Null = new SqlMoney(true);
		public static readonly SqlMoney Zero = new SqlMoney(0);

		// this ctor is to initailize max and min values, to avoid checks.
		private SqlMoney(Decimal value, bool dummy)
		{
			_value = value;
			_isNull = false;
		}
        
		private SqlMoney(bool isNull)
		{
			_isNull = true;
			_value = 0;
		}
		/**
		 * Initializes a new instance of the SqlMoney instance using the supplied Decimal value.
		 * @param value The Decimal value to be stored as a SqlMoney instance.
		 */
		public SqlMoney(Decimal value) 
		{
			if ((value.CompareTo(MaxValue.Value) > 0 || value.CompareTo(MinValue.Value) < 0))
				throw new OverflowException("overflow - the value is out of range " + value);

			_value = value;
			_isNull = false;
		}
        

		/**
		 * Initializes a new instance of the SqlMoney instance using the supplied double value.
		 * @param value The double value to be stored as a SqlMoney instance.
		 */
		public SqlMoney(double value) 
		{ 
			_value = new Decimal(value);
			if (_value.CompareTo(MaxValue.Value) > 0 || _value.CompareTo(MinValue.Value) < 0)
				throw new OverflowException("overflow - the value is out of range " + value);

			_isNull = false;
		}

		/**
		 * Initializes a new instance of the SqlMoney instance using the supplied int value.
		 * @param value The int value to be stored as a SqlMoney instance.
		 */
		public SqlMoney(int value) 
		{
			_value = new Decimal(value);
			_isNull = false;
		}

		/**
		 * Initializes a new instance of the SqlMoney instance using the supplied long value.
		 * @param value The long value to be stored as a SqlMoney instance.
		 */
		public SqlMoney(long value) 
		{
			_value = new Decimal(value);
			if (_value.CompareTo(MaxValue.Value) > 0 || _value.CompareTo(MinValue.Value) < 0)
				throw new OverflowException("overflow - the value is out of range " + value);
			_isNull = false;
		}

        
		/**
		 * Indicates whether or not Value is null.
		 * @return true if Value is null, otherwise false.
		 */
		public bool IsNull
		{
			get
			{
				return _isNull;
			}
		}

		/**
		 * Gets the value of the SqlMoney instance.
		 * @return the value of this instance
		 */
		public Decimal Value
		{
			get
			{
				if(IsNull)
					throw new SqlNullValueException();
				return _value;
			}
		}

		/**
		 * Calcuates the sum of the two SqlMoney operators.
		 * @param x A SqlMoney instance.
		 * @param y A SqlMoney instance.
		 * @return A new SqlMoney instance whose Value property contains the sum.
		 * If one of the parameters or their value is null return SqlMoney.Null.
		 */
		public static SqlMoney Add(SqlMoney x, SqlMoney y)
		{
			if (x.IsNull || y.IsNull)
				return SqlMoney.Null;

			Decimal res = Decimal.Add(x._value, y._value);

			if (res.CompareTo(MaxValue.Value) > 0 || res.CompareTo(MinValue.Value) < 0)
				throw new OverflowException("overflow - the sum of the 2 parameters can not be SqlMoney : " + res);

			return new SqlMoney(res);
		}

		/**
		 * Compares this instance to the supplied object and returns an indication of their relative values.
		 * @param obj The object to compare.
		 * @return A signed number indicating the relative values of the instance and the object.
		 * Less than zero This instance is less than object.
		 * Zero This instance is the same as object.
		 * Greater than zero This instance is greater than object -or-
		 * object is a null reference.
		 */
		public int CompareTo(Object obj)
		{
			if (obj == null)
				return 1;

			if (obj is SqlMoney)
			{
				SqlMoney value = (SqlMoney)obj;
                
				if(IsNull)
					return -1;
                
				if(value.IsNull)
					return 1;

				return Decimal.Compare(_value, value._value);
			}

			/** @todo throwArgumentException */
			throw new ArgumentException("parameter obj is not SqlMoney : " + obj.GetType().Name);


		}


		/**
		 * The division operator divides the first SqlMoney operand by the second.
		 * @param x A SqlMoney instance.
		 * @param y A SqlMoney instance.
		 * @return A SqlMoney instance containing the results of the division operation.
		 * If one of the parameters is null or null value - return SqlDouble.Null.
		 */
		public static SqlMoney Divide(SqlMoney x, SqlMoney y)
		{
			if (x.IsNull || y.IsNull)
				return SqlMoney.Null;

			Decimal res = Decimal.Divide(x._value, y._value);

			return new SqlMoney(res);
		}

		public override bool Equals(Object obj)
		{
			if (obj == null)
				return false;

			if (obj is SqlMoney)
			{
				SqlMoney dec = (SqlMoney)obj;

				return Decimal.Equals(_value, dec._value);
			}

			return false;
		}

        
		/**
		 * Performs a logical comparison on two instances of SqlMoney to determine if they are equal.
		 * @param x A SqlMoney instance.
		 * @param y A SqlMoney instance.
		 * @return true if the two values are equal, otherwise false.
		 * If one of the parameters is null or null value return SqlBoolean.Null.
		 */
		public static SqlBoolean Equals(SqlMoney x, SqlMoney y)
		{
			if (x.IsNull || y.IsNull)
				return SqlBoolean.Null;

			if (x.Equals(y))
				return SqlBoolean.True;

			return SqlBoolean.False;
		}


		/**
		 * Compares two instances of SqlMoney to determine if the first is greater than the second.
		 * @param x A SqlMoney instance
		 * @param y A SqlMoney instance
		 * @return A SqlBoolean that is True if the first instance is greater than the second instance, otherwise False.
		 * If either instance of SqlDouble is null, the Value of the SqlBoolean will be Null.
		 */
		public static SqlBoolean GreaterThan(SqlMoney x, SqlMoney y)
		{
			if (x.IsNull || y.IsNull)
				return SqlBoolean.Null;

			if (x.CompareTo(y) > 0)
				return SqlBoolean.True;

			return SqlBoolean.False;
		}

		/**
		 * Compares two instances of SqlMoney to determine if the first is greater than or equal to the second.
		 * @param x A SqlMoney instance
		 * @param y A SqlMoney instance
		 * @return A SqlBoolean that is True if the first instance is greaater than or equal to the second instance, otherwise False.
		 * If either instance of SqlDouble is null, the Value of the SqlBoolean will be Null.
		 */
		public static SqlBoolean GreaterThanOrEqual(SqlMoney x, SqlMoney y)
		{
			if (x.IsNull || y.IsNull)
				return SqlBoolean.Null;

			if (x.CompareTo(y) >= 0)
				return SqlBoolean.True;

			return SqlBoolean.False;
		}

		/**
		 * Compares two instances of SqlMoney to determine if the first is less than the second.
		 * @param x A SqlMoney instance
		 * @param y A SqlMoney instance
		 * @return A SqlBoolean that is True if the first instance is less than the second instance, otherwise False.
		 * If either instance of SqlDouble is null, the Value of the SqlBoolean will be Null.
		 */
		public static SqlBoolean LessThan(SqlMoney x, SqlMoney y)
		{
			if (x.IsNull || y.IsNull)
				return SqlBoolean.Null;

			if (x.CompareTo(y) < 0)
				return SqlBoolean.True;

			return SqlBoolean.False;
		}

		/**
		 * Compares two instances of SqlMoney to determine if the first is less than the second.
		 * @param x A SqlMoney instance
		 * @param y A SqlMoney instance
		 * @return A SqlBoolean that is True if the first instance is less than the second instance, otherwise False.
		 * If either instance of SqlDouble is null, the Value of the SqlBoolean will be Null.
		 */
		public static SqlBoolean LessThanOrEqual(SqlMoney x, SqlMoney y)
		{
			if (x.IsNull || y.IsNull)
				return SqlBoolean.Null;

			if (x.CompareTo(y) <= 0)
				return SqlBoolean.True;

			return SqlBoolean.False;
		}

		/**
		 * The multiplication operator computes the product of the two SqlMoney operands.
		 * @param x A SqlMoney instance
		 * @param y A SqlMoney instance
		 * @return The product of the two SqlMoney operands.
		 */
		public static SqlMoney Multiply(SqlMoney x, SqlMoney y)
		{
			if (x.IsNull || y.IsNull)
				return SqlMoney.Null;

			Decimal res = Decimal.Multiply(x._value, y._value);

			if (res.CompareTo(MaxValue.Value) > 0 || res.CompareTo(MinValue.Value) < 0)
				throw new OverflowException("overflow - the multiply value is out of range " + res);

			return new SqlMoney(res);
		}

		/**
		 * Compares two instances of SqlMoney to determine if they are equal.
		 * @param x A SqlMoney instance
		 * @param y A SqlMoney instance
		 * @return A SqlBoolean that is True if the two instances are not equal or False if the two instances are equal.
		 * If either instance of SqlDouble is null, the Value of the SqlBoolean will be Null.
		 */
		public static SqlBoolean NotEquals(SqlMoney x, SqlMoney y)
		{
			SqlBoolean eVal = Equals(x, y);

			if (eVal.IsNull)
				return eVal;
			if (eVal.IsTrue)
				return SqlBoolean.False;

			return SqlBoolean.True;
		}

		/**
		 * Converts the String representation of a number to its Decimal number equivalent.
		 * @param s The String to be parsed.
		 * @return A SqlMoney containing the value represented by the String.
		 */
		public static SqlMoney Parse(String s)
		{
			Decimal val = Decimal.Parse(s);
			SqlMoney retVal = new SqlMoney(val);

			if (GreaterThan(retVal, MaxValue).IsTrue || LessThan(retVal, MinValue).IsTrue)
				throw new OverflowException("the value is out of range : " + retVal);

			return retVal;
		}

		/**
		 * The subtraction operator the second SqlMoney operand from the first.
		 * @param x A SqlMoney instance
		 * @param y A SqlMoney instance
		 * @return The results of the subtraction operation.
		 */
		public static SqlMoney Subtract(SqlMoney x, SqlMoney y)
		{
			Decimal val = Decimal.Subtract(x._value, y._value);
			SqlMoney retVal = new SqlMoney(val);

			if (GreaterThan(retVal, MaxValue).IsTrue || LessThan(retVal, MinValue).IsTrue)
				throw new OverflowException("the subtract result is out of range : " + retVal);

			return retVal;

		}

		/**
		 * Returns the a double equal to the contents of the Value property of this instance.
		 * @return The decimal representation of the Value property.
		 */
		public double ToDouble()
		{
			return Decimal.ToDouble(_value);
		}

		/**
		 * Returns the a decimal equal to the contents of the Value property of this instance.
		 * @return The decimal representation of the Value property.
		 */
		public Decimal ToDecimal()
		{
			return _value;
		}

		/**
		 * Converts this SqlMoney instance to SqlBoolean.
		 * @return A SqlBoolean instance whose Value will be True if the SqlMoney instance's Value is non-zero,
		 * False if the SqlMoney is zero
		 * and Null if the SqlMoney instance is Null.
		 */
		public SqlBoolean ToSqlBoolean()
		{
			if (IsNull)
				return SqlBoolean.Null;

			int val = Decimal.ToInt32(_value);

			return new SqlBoolean(val);
		}

		/**
		 * Converts this SqlMoney instance to SqlByte.
		 * @return A SqlByte instance whose Value equals the Value of this SqlDouble instance.
		 */
		public SqlByte ToSqlByte()
		{
			if (IsNull)
				return SqlByte.Null;

			return new SqlByte(Decimal.ToByte(_value));
		}

		/**
		 * Converts this SqlMoney instance to SqlDouble.
		 * @return A SqlDouble instance whose Value equals the Value of this SqlMoney instance.
		 */
		public SqlDouble ToSqlDouble()
		{
			if (IsNull)
				return SqlDouble.Null;

			return new SqlDouble(Decimal.ToDouble(_value));
		}

		/**
		 * Converts this SqlMoney instance to SqlSingle.
		 * @return A SqlSingle instance whose Value equals the Value of this SqlMoney instance.
		 */
		public SqlSingle ToSqlSingle()
		{
			if (IsNull)
				return SqlSingle.Null;

			return new SqlSingle(Decimal.ToSingle(_value));
		}

		/**
		 * Converts this SqlMoney instance to SqlDecimal.
		 * @return A SqlDecimal instance whose Value equals the Value of this SqlMoney instance.
		 */
		public SqlDecimal ToSqlDecimal()
		{
			if (IsNull)
				return SqlDecimal.Null;

			return new SqlDecimal(_value);
		}


		/**
		 * Converts this SqlDouble structure to SqlInt16.
		 * @return A SqlInt16 structure whose Value equals the Value of this SqlDouble structure.
		 */
		public SqlInt16 ToSqlInt16()
		{
			if (IsNull)
				return SqlInt16.Null;

			return new SqlInt16(Decimal.ToInt16(_value));
		}

		/**
		 * Converts this SqlDouble structure to SqlInt32.
		 * @return A SqlInt32 structure whose Value equals the Value of this SqlDouble structure.
		 */
		public SqlInt32 ToSqlInt32()
		{
			if (IsNull)
				return SqlInt32.Null;

			return new SqlInt32(Decimal.ToInt32(_value));
		}

		/**
		 * Converts this SqlMoney structure to SqlInt64.
		 * @return A SqlInt64 structure whose Value equals the Value of this SqlMoney structure.
		 */
		public SqlInt64 ToSqlInt64()
		{
			if (IsNull)
				return SqlInt64.Null;
			return new SqlInt64(Decimal.ToInt64(_value));
		}


		/**
		 * Converts this SqlMoney structure to SqlString.
		 * @return A SqlString structure whose value is a string representing the date and time contained in this SqlMoney structure.
		 */
		public SqlString ToSqlString()
		{
			if (IsNull)
				return SqlString.Null;
			return new SqlString(ToString());
		}

		public override String ToString()
		{
			if (IsNull)
				return "null";
			return _value.ToString();
		}

		public override int GetHashCode()
		{
			if (IsNull)
				return 0;
			return _value.GetHashCode();
		}

		public static SqlMoney operator + (SqlMoney x, SqlMoney y)
		{
			checked 
			{
				return new SqlMoney (x.Value + y.Value);
			}
		}

		public static SqlMoney operator / (SqlMoney x, SqlMoney y)
		{
			checked 
			{
				return new SqlMoney (x.Value / y.Value);
			}
		}

		public static SqlBoolean operator == (SqlMoney x, SqlMoney y)
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.Value == y.Value);
		}

		public static SqlBoolean operator > (SqlMoney x, SqlMoney y)
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.Value > y.Value);
		}

		public static SqlBoolean operator >= (SqlMoney x, SqlMoney y)
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.Value >= y.Value);
		}

		public static SqlBoolean operator != (SqlMoney x, SqlMoney y)
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;
			else
				return new SqlBoolean (!(x.Value == y.Value));
		}

		public static SqlBoolean operator < (SqlMoney x, SqlMoney y)
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;
			else
				return new SqlBoolean (x.Value < y.Value);
		}

		public static SqlBoolean operator <= (SqlMoney x, SqlMoney y)
		{
			if (x.IsNull || y.IsNull) 
				return SqlBoolean.Null;
			return new SqlBoolean (x.Value <= y.Value);
		}

		public static SqlMoney operator * (SqlMoney x, SqlMoney y)
		{
			checked 
			{
				return new SqlMoney (x.Value * y.Value);
			}
		}

		public static SqlMoney operator - (SqlMoney x, SqlMoney y)
		{
			checked 
			{
				return new SqlMoney (x.Value - y.Value);
			}
		}

		public static SqlMoney operator - (SqlMoney n)
		{
			return new SqlMoney (-(n.Value));
		}

		public static explicit operator SqlMoney (SqlBoolean x)
		{			
			if (x.IsNull) 
				return Null;
			else 
			{
				checked 
				{
					return new SqlMoney ((decimal)x.ByteValue);
				}
			}
		}

		public static explicit operator SqlMoney (SqlDecimal x)
		{
			if (x.IsNull) 
				return Null;
			else 
			{
				checked 
				{
					return new SqlMoney (x.Value);
				}
			}
		}

		public static explicit operator SqlMoney (SqlDouble x)
		{
			if (x.IsNull) 
				return Null;
			else 
			{
				checked 
				{
					return new SqlMoney ((decimal)x.Value);
				}
			}
		}

		public static explicit operator decimal (SqlMoney x)
		{
			return x.Value;
		}

		public static explicit operator SqlMoney (SqlSingle x)
		{
			if (x.IsNull) 
				return Null;
			else 
			{
				checked 
				{
					return new SqlMoney ((decimal)x.Value);
				}
			}
		}

		public static explicit operator SqlMoney (SqlString x)
		{
			checked 
			{
				return SqlMoney.Parse (x.Value);
			}
		}

		public static implicit operator SqlMoney (decimal x)
		{
			return new SqlMoney (x);
		}

		public static implicit operator SqlMoney (SqlByte x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SqlMoney ((decimal)x.Value);
		}

		public static implicit operator SqlMoney (SqlInt16 x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SqlMoney ((decimal)x.Value);
		}

		public static implicit operator SqlMoney (SqlInt32 x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SqlMoney (x.Value);
		}

		public static implicit operator SqlMoney (SqlInt64 x)
		{
			if (x.IsNull) 
				return Null;
			else
				return new SqlMoney (x.Value);
		}


	}
}