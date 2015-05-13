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
using System.Management.Internal;
using System.Management.Internal.BaseDataTypes;

namespace System.Management.Internal
{
    /// <summary>
    /// A value of type CimType that can also be set to null
    /// </summary>
    internal class NullableCimType : BaseNullable<CimType?>
    {
        #region Constructors
        /// <summary>
        /// Creates a default NullabelCimType
        /// </summary>
        public NullableCimType()
        {
        }
        #endregion

        #region Methods
        /// <summary>
        /// Returns the integer value of the NullableCimType object. Returns 0 if null.
        /// </summary>
        /// <returns>int</returns>
        public CimType ToCimType()
        {
            if (Value == null)
                return CimType.CIMNULL;

            return (CimType)Value;
        }

        /// <summary>
        /// Returns the string value of the ToInt() call.
        /// </summary>
        /// <returns>string</returns>
        public override string ToString()
        {
            return CimTypeUtils.CimTypeToStr(ToCimType());
        }

        /// <summary>
        /// Compares the values of two NullableCimType objects
        /// </summary>
        /// <param name="obj">object to compare</param>
        /// <returns>True if the values of the NullableCimType objects are equal</returns>
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

                if (obj is CimType)
                    return (Value == (CimType)obj);
                
                if (obj is string)
                    return AreEqual(Value.ToString(), (string)obj);

                if (obj is NullableCimType)
                    return (Value == ((NullableCimType)obj).Value);
            }

            throw new InvalidCastException("Can only compare NullableCimType objects to other NullableCimType objects, ints, or strings");
        }

        public override int GetHashCode()
        {
            return ((object)this).GetHashCode();
        }
        #endregion

        #region Operators
        /// <summary>
        /// Implicitly convert from a string to NullableCimType.
        /// </summary>
        /// <param name="value">String to convert</param>
        /// <returns>NullableCimType</returns>
        public static implicit operator NullableCimType(string value)
        {
            NullableCimType tmp = new NullableCimType();

            if (value.ToLower() == "null")
                tmp.Value = null;
            else
                tmp.Value = CimTypeUtils.StrToCimType(value);

            return tmp;
        }

        /// <summary>
        /// Implicitly convert from a int to NullableCimType.
        /// </summary>
        /// <param name="value">int to convert</param>
        /// <returns>NullableCimType</returns>
        public static implicit operator NullableCimType(CimType value)
        {
            NullableCimType tmp = new NullableCimType();

            tmp.Value = value;

            return tmp;
        }
        #endregion
    }
}
