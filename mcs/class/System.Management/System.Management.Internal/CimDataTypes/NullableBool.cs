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
    /// A value type of type bool that can also be set to null
    /// </summary>
    internal class NullableBool : BaseNullable<bool?>
    {
        bool? _defaultValue = null;

        #region Constructors
        /// <summary>
        /// Creates a default NullableBool
        /// </summary>
        public NullableBool()
        {
        }

        /// <summary>
        /// Allows you to set a default value. If the default value is set, then if the variable is null, 
        /// it is also equal to the default value.
        /// </summary>
        /// <param name="defaultValue"></param>
        public NullableBool(bool defaultValue)
        {
            DefaultValue = defaultValue;
        }

        /// <summary>
        /// Allows you to set a default value. If the default value is set, then if the variable is null, 
        /// it is also equal to the default value.
        /// </summary>
        /// <param name="defaultValue"></param>
        public NullableBool(string defaultValue)
        {
            if (defaultValue != string.Empty)
            {
                DefaultValue = Convert.ToBoolean(defaultValue);
            }
        }
        #endregion

        #region Properties and Indexers
        /// <summary>
        /// Gets or sets the default value of the NullableBool
        /// </summary>
        private bool? DefaultValue
        {
            get { return _defaultValue; }
            set { _defaultValue = value; }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Returns the string of the NullableBool object.
        /// </summary>
        /// <returns>string</returns>
        public override string ToString()
        {
            // Guaranteed not to return null.
            if (Value == null)
            {
                if (DefaultValue == null)
                    return string.Empty;
                else
                    return DefaultValue.ToString();    // Use default if available
            }

            return Value.ToString();
        }

        /// <summary>
        /// Converts to a regular bool type
        /// </summary>
        /// <returns></returns>
        public bool ToBool()
        {
            return Value.Value;
        }

		public bool ToBoolOrDefault()
		{
			return Value.GetValueOrDefault ();
		}

        /// <summary>
        /// Compares the values of two NullableBool objects
        /// </summary>
        /// <param name="obj">object to compare</param>
        /// <returns>True if the values of the NullableBool objects are equal</returns>
        public override bool Equals(object obj)
        {
            if (Value == null)
            {
                if (obj == null)
                    return true;            // both _value and obj are null, so true

                if (DefaultValue == null)
                    return false;           // both _value and _default are null, but obj isn't
                
                if (obj is bool)
                    return (DefaultValue == (bool)obj);  // See if obj is equal to _default

                if (obj is string)
                    return AreEqual(DefaultValue.ToString(), (string)obj);

                if (obj is NullableBool)
                    return (DefaultValue == ((NullableBool)obj).Value);
            }
            else
            {
                if (obj == null)
                    return false;

                if (obj is bool)
                    return (Value == (bool)obj);

                if (obj is string)
                    return AreEqual(Value.ToString(), (string)obj);

                if (obj is NullableBool)
                    return (Value == ((NullableBool)obj).Value);
            }

            throw new InvalidCastException("Can only compare NullableBool objects to other NullableBool objects, bools, or strings");
        }
        
        public override int GetHashCode()
        {
            return ((object)this).GetHashCode();
        }
        #endregion

        #region Operators
        /// <summary>
        /// Implicitly convert from a string to NullableBool.
        /// </summary>
        /// <param name="value">String to convert</param>
        /// <returns>NullableBool</returns>
        public static implicit operator NullableBool(string value)
        {
            NullableBool tmp = new NullableBool();
            tmp.Value = Convert.ToBoolean(value);
            return tmp;
        }

        /// <summary>
        /// Implicitly convert from a bool? to NullableBool.
        /// </summary>
        /// <param name="value">bool? to convert</param>
        /// <returns>NullableBool</returns>
        public static implicit operator NullableBool(bool? value)
        {
            NullableBool tmp = new NullableBool();
            tmp.Value = value;
            return tmp;
        }
        #endregion
    }
}
