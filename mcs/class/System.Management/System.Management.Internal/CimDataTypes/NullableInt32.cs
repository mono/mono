/******************************************************************************
* The MIT License
* Copyright (c) 2007 Novell Inc.,  www.novell.com
*
* Permission is hereby granted, free of charge, to any person obtaining  a copy
* of this software and associated documentation files (the Software), to deal
* in the Software without restriction, including  without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to  permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED AS IS, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
* SOFTWARE.
*******************************************************************************/

// Authors:
// 		Thomas Wiest (twiest@novell.com)
//		Rusty Howell  (rhowell@novell.com)
//
// (C)  Novell Inc.


using System;
using System.Management.Internal.BaseDataTypes;

namespace System.Management.Internal
{
    /// <summary>
    /// A value of type int that can also be set to null
    /// </summary>
    internal class NullableInt32 : BaseNullable<System.Int32?>
    {
        #region Constructors
        /// <summary>
        /// Creates a default NullableInt32
        /// </summary>
        public NullableInt32()
        {
        }
        #endregion

        #region Methods
        /// <summary>
        /// Returns the integer value of the NullableInt object. Returns 0 if null.
        /// </summary>
        /// <returns>int</returns>
        public int ToInt()
        {
            if (Value == null)
                return 0;

            return (int)Value;
        }

        /// <summary>
        /// Returns the string value of the ToInt() call.
        /// </summary>
        /// <returns>string</returns>
        public override string ToString()
        {
            return ToInt().ToString();
        }

        /// <summary>
        /// Compares the values of two NullableInt objects
        /// </summary>
        /// <param name="obj">object to compare</param>
        /// <returns>True if the values of the NullableInt objects are equal</returns>
        public override bool Equals(object obj)
        {
            if (Value == null)
            {
                if (obj == null)
                    return true;            // both _value and obj are null, so true
                else
                    return false;           // only _value is null, so false
            }
            else
            {
                if (obj == null)
                    return false;           // only obj is null, so false

                if (obj is int)
                    return (Value == (int)obj);

                if (obj is string)
                    return AreEqual(Value.ToString(), (string)obj);

                if (obj is NullableInt32)
                    return (Value == ((NullableInt32)obj).Value);
            }

            throw new InvalidCastException("Can only compare NullableInt objects to other NullableInt objects, ints, or strings");
        }

        public override int GetHashCode()
        {
            return ((object)this).GetHashCode();
        }
        #endregion

        #region Operators
        /// <summary>
        /// Implicitly convert from a string to NullableInt.
        /// </summary>
        /// <param name="value">String to convert</param>
        /// <returns>NullableInt</returns>
        public static implicit operator NullableInt32(string value)
        {
            NullableInt32 tmp = new NullableInt32();

            if (value.ToLower() == "null")
                tmp.Value = null;
            else
                tmp.Value = Convert.ToInt32(value);

            return tmp;
        }

        /// <summary>
        /// Implicitly convert from a int to NullableInt.
        /// </summary>
        /// <param name="value">int to convert</param>
        /// <returns>NullableInt</returns>
        public static implicit operator NullableInt32(int value)
        {
            NullableInt32 tmp = new NullableInt32();

            tmp.Value = value;

            return tmp;
        }
        #endregion
    }
}
