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

namespace System.Management.Internal
{
    /// <summary>
    /// Specifies the value returned from an extrinsic method call. 
    /// </summary>
    internal class CimReturnValue
    {
        #region Members
       
        string _value;
        CimValueReference _valueReference;
        NullableCimType _type;
        
        #endregion

        #region Constructors
        /// <summary>
        /// Creates an empty CimReturnValue object
        /// </summary>
        public CimReturnValue()
        {
            _value = null;
            _type = null;
        }

        /// <summary>
        /// Creates a CimReturnValue with the given type and value
        /// </summary>
        /// <param name="type"></param>
        /// <param name="val"></param>
        public CimReturnValue(NullableCimType type, string val)
        {
            _value = val;
            _type = type;
        }
        #endregion

        #region Properties and Indexers
        /// <summary>
        /// Gets or sets the type of the parameter
        /// </summary>
        public NullableCimType Type
        {
            get
            {
                if (_type == null)
                    _type = new NullableCimType();

                return _type;
            }
            set { _type = value; }
        }

        /// <summary>
        /// Gets or sets the value
        /// </summary>
        public string Value
        {
            get
            {
                if (_value == null)
                    return string.Empty;

                return _value;
            }
            set { _value = value; }
        }

        /// <summary>
        /// Gets or sets the value reference
        /// </summary>
        public CimValueReference ValueReference
        {
            get
            {
                if (_valueReference == null)
                    _valueReference = new CimValueReference();

                return _valueReference;
            }
            set { _valueReference = value; }
        }

        public bool IsValueReference
        {
            get { return ValueReference.IsSet; }
        }
        #endregion

        #region Methods and Operators

        #endregion

    }
}
