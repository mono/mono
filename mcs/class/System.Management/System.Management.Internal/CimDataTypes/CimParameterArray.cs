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
    /// A single parameter to a CimMethod that has an array type
    /// </summary>
    internal class CimParameterArray : CimParameter
    {
        /* <!ELEMENT PARAMETER.ARRAY (QUALIFIER*)>
         * <!ATTLIST PARAMETER.ARRAY
         *      %CIMName;
         *      %CIMType;           #REQUIRED
         *      %ArraySize;>
         * */
        private NullableInt32 _arraySize = null;//null if _arraySize isn't used

        #region Constructors
        /// <summary>
        /// Creates a new CimParameterArray with the given name and type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        public CimParameterArray(CimType type, string name):this(type, new CimName(name))
        {

        }

        /// <summary>
        /// Creates a new CimParameterArray with the given name and type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        public CimParameterArray(CimType type, CimName name)
            : base(type, name)
        {

        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the size of the array
        /// </summary>
        public NullableInt32 ArraySize
        {
            get 
            {
                if (_arraySize == null)
                    _arraySize = new NullableInt32();
                return _arraySize; 
            }
            set { _arraySize = value; }
        }
        #endregion

        #region Methods

        #endregion
    }
}
