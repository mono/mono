// System.Data.SqlTypes.SqlGuid
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

    using System;

    /**
     *
     */
    public struct SqlGuid : INullable, IComparable
    {
        private Guid _value;
        private bool _isNull;
        public static readonly SqlGuid Null = new SqlGuid();

        
        private SqlGuid(bool isNull)
        {
            _isNull = isNull;
            _value = Guid.Empty;
        }
    
        public SqlGuid(byte[] value) 
        {
            _value = new Guid(value);
            _isNull = false;
        }

        public SqlGuid(String s) 
        {
            _value = new Guid(s);
            _isNull = false;
        }

        public SqlGuid(Guid g) 
        {
            _value = g;
            _isNull = false;
        }

        public SqlGuid(int a, short b, short c, byte d, byte e, byte f, byte g, byte h, byte i, byte j, byte k)
        {
            _value = new Guid(a, b, c, d, e, f, g, h, i, j, k);
            _isNull = false;
        }

        
        public int CompareTo(Object obj)
        {
            if (obj == null)
                return 1;

            if (obj is SqlGuid)
            {
                SqlGuid g = (SqlGuid)obj;

                if (g.IsNull)
                    return 1;
                if (this.IsNull)
                    return -1;

                return this._value.CompareTo(g._value);
            }

            throw new ArgumentException("parameter obj is not SqlGuid : " + obj.GetType().Name);

        }

        public bool IsNull
        {
            get
            {
                return _isNull;
            }
        }

        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public override bool Equals(Object obj)
        {
            if (obj == null)
                return false;

            if (obj is SqlGuid)
            {
                SqlGuid g = (SqlGuid)obj;

                if (IsNull && g.IsNull)
                    return true;

                if (IsNull || g.IsNull)
                    return false;

                return _value == g._value;
            }

            return false;
        }

        public override String ToString()
        {
            if (IsNull)
                return "null";


            return _value.ToString();
        }

        public Guid Value
        {
            get
            {
                if(IsNull)
                    throw new SqlNullValueException();
                return _value;
            }
        }

        public static SqlGuid op_Implicit(Guid x)
        {
            return new SqlGuid(x);
        }

        public static Guid op_Explicit(SqlGuid x)
        {
            return x.Value;
        }

        public byte[] ToByteArray()
        {
            if(IsNull)
                throw new SqlNullValueException();
            return _value.ToByteArray();
        }

        public static SqlGuid Parse(String s)
        {
            return new SqlGuid(s);
        }

        public static SqlGuid op_Explicit(SqlString x)
        {
            return new SqlGuid(x.Value);
        }

        public static SqlGuid op_Explicit(SqlBinary x)
        {
            return new SqlGuid(x.Value);
        }

        public static SqlBoolean op_Equality(SqlGuid x, SqlGuid y)
        {
            return Equals(x, y);
        }

        public static SqlBoolean op_Inequality(SqlGuid x, SqlGuid y)
        {
            return NotEquals(x, y);
        }

        public static SqlBoolean op_LessThan(SqlGuid x, SqlGuid y)
        {
            return LessThan(x, y);
        }

        public static SqlBoolean op_GreaterThan(SqlGuid x, SqlGuid y)
        {
            return GreaterThan(x, y);
        }

        public static SqlBoolean op_LessThanOrEqual(SqlGuid x, SqlGuid y)
        {
            return LessThanOrEqual(x, y);
        }

        public static SqlBoolean op_GreaterThanOrEqual(SqlGuid x, SqlGuid y)
        {
            return GreaterThanOrEqual(x, y);
        }

        public static SqlBoolean Equals(SqlGuid x, SqlGuid y)
        {
            if(x.IsNull || y.IsNull)
                return SqlBoolean.Null;

            return new SqlBoolean(x.Value.Equals(y.Value));
        }

        public static SqlBoolean NotEquals(SqlGuid x, SqlGuid y)
        {
            SqlBoolean res = Equals(x, y);
            if(res.IsFalse)
                return SqlBoolean.True;
            return SqlBoolean.False;
        }

        public static SqlBoolean LessThan(SqlGuid x, SqlGuid y)
        {
            if(x.IsNull || y.IsNull)
                return SqlBoolean.Null;

            int res = x.CompareTo(y);
            if(res < 0)
                return SqlBoolean.True;
            return SqlBoolean.False;
        }

        public static SqlBoolean GreaterThan(SqlGuid x, SqlGuid y)
        {
            if(x.IsNull || y.IsNull)
                return SqlBoolean.Null;

            int res = x.CompareTo(y);
            if(res > 0)
                return SqlBoolean.True;
            return SqlBoolean.False;
        }

        public static SqlBoolean LessThanOrEqual(SqlGuid x, SqlGuid y)
        {
            if(x.IsNull || y.IsNull)
                return SqlBoolean.Null;

            int res = x.CompareTo(y);
            if(res <= 0)
                return SqlBoolean.True;
            return SqlBoolean.False;
        }

        public static SqlBoolean GreaterThanOrEqual(SqlGuid x, SqlGuid y)
        {
            if(x.IsNull || y.IsNull)
                return SqlBoolean.Null;

            int res = x.CompareTo(y);
            if(res >= 0)
                return SqlBoolean.True;
            return SqlBoolean.False;
        }

        public SqlString ToSqlString()
        {
            if(IsNull)
                return SqlString.Null;

            return new SqlString(ToString());
        }

        public SqlBinary ToSqlBinary()
        {
            if(IsNull)
                return SqlBinary.Null;

            return new SqlBinary(ToByteArray());
        }
    }
}