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
    /// A single property with an array type. 
    /// </summary>
    internal class CimPropertyArray : CimProperty
    {
        /* <!ELEMENT PROPERTY.ARRAY (QUALIFIER*,VALUE.ARRAY?)>
           <!ATTLIST PROPERTY.ARRAY 
                %CIMName;
                %CIMType;           #REQUIRED 
                %ArraySize;
                %ClassOrigin;
                %Propagated;
                xml:lang   NMTOKEN  #IMPLIED>
         * */
        private CimValueList _valueArray = null;
        private NullableInt32 _arraySize = null;
        
        #region Constructors
        /// <summary>
        /// Creates a new CimPropertyArray with the given name and type
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        public CimPropertyArray(CimName name, CimType type): base(name, type)
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

        /// <summary>
        /// Gets or sets the values of the property
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
        #endregion

        #region Methods
        #endregion
    }
}
