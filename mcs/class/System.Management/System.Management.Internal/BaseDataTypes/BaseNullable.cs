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
using System.Collections.Generic;
using System.Text;

namespace System.Management.Internal.BaseDataTypes
{
    internal abstract class BaseNullable<T>
    {
        #region Members
        T _value = default(T);
        #endregion

        #region Properties and Indexers
        /// <summary>
        /// Gets or sets the Value
        /// </summary>
        protected T Value
        {
            get { return _value; }
            set { _value = value; }
        }

        /// <summary>
        /// Returns true if the value is not null
        /// </summary>
        public bool IsSet
        {
            get { return (Value != null); }
        }
        #endregion

        #region Methods
        protected static bool AreEqual(string lhs, string rhs)
        {
            return (lhs.ToLower() == rhs.ToLower());
        }
        #endregion

        #region Operators
        /// <summary>
        /// Compare a NullableInt object and an object
        /// </summary>
        /// <param name="lhs">NullableInt object</param>
        /// <param name="rhs">object value</param>
        /// <returns>Returns true if the values are the same</returns>
        public static bool operator ==(BaseNullable<T> lhs, object rhs)
        {
            if ((object)lhs == null)
            {
                if (rhs == null)
                    return true;
                else
                    return false;
            }
            else
                return lhs.Equals(rhs);
        }

        /// <summary>
        /// Compare a NullableInt object and an object
        /// </summary>
        /// <param name="lhs">NullableInt object</param>
        /// <param name="rhs">object value</param>
        /// <returns>Returns true if the values are not the same</returns>
        public static bool operator !=(BaseNullable<T> lhs, object rhs)
        {
            if ((object)lhs == null)
            {
                if (rhs != null)
                    return true;
                else
                    return false;
            }
            else
                return !lhs.Equals(rhs);
        }
        #endregion
    }
}
