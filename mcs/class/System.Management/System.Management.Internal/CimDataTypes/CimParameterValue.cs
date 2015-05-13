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
    /// An extrinsic method named parameter value
    /// </summary>
    internal class CimParameterValue
    {
        CimName _name = null;
        NullableCimType _type = null;
        CimValueList _valueArray = null;

        #region Constructors
        /// <summary>
        /// Creates a new CimParameterValue with the given name and type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        public CimParameterValue(NullableCimType type, CimName name)
        {
            Type = type;
            Name = name;

        }

        /// <summary>
        /// Creates a new CimParameter with the given name, type, and values
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="values"></param>
        public CimParameterValue(NullableCimType type, CimName name, params string[] values)
            : this(type,name)
        {
            ValueArray = new CimValueList(values);

        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the name of the parameter
        /// </summary>
        public CimName Name
        {
            get
            {
                if (_name == null)
                    _name = new CimName(null);

                return _name;
            }
            set { _name = value; }
        }

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
        /// Gets or sets the 
        /// </summary>
        public CimValueList ValueArray
        {
            get
            {
                // Guaranteed not to be null
                if (_valueArray == null)
                    _valueArray = new CimValueList();   // Alloc on demand

                return _valueArray;
            }
            private set { _valueArray = value; }
        }

        /// <summary>
        /// Returns true name, type, and value array are all set
        /// </summary>
        public bool IsSet
        {
            get { return Name.IsSet && Type.IsSet && ValueArray.IsSet; }
        }
        #endregion
    }
}
