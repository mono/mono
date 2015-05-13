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
using System.Management.Internal.BaseDataTypes;

namespace System.Management.Internal
{
    /// <summary>
    /// 
    /// </summary>
    internal class CimPropertyReference : CimProperty
    {
        /* <!ELEMENT PROPERTY.REFERENCE (QUALIFIER*,VALUE.REFERENCE?)>
         * <!ATTLIST PROPERTY.REFERENCE
         *      %CIMName;
         *      %ReferenceClass;
         *      %ClassOrigin;   
         *      %Propagated;>
         * */
        private CimName _referenceClass = null;
        private CimValueReference _valueReference = null;
        
        //These three are inherited from base class CimProperty
        //private CimQualifiers _qualifiers;
        //private CimName classOrigin;
        //private bool isPropagated;

        //also has unused CimType property

        #region Constructors
        
        /// <summary>
        /// Creates a new CimPropertyReference with the given name
        /// </summary>
        /// <param name="name"></param>
        public CimPropertyReference(CimName name): base(name, CimType.REFERENCE)
        {
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the name of the Ref class
        /// </summary>
        public CimName ReferenceClass
        {
            get 
            {
                if (_referenceClass == null)
                    _referenceClass = new CimName(null);

                return _referenceClass; 
            }
            set { _referenceClass = value; }
        }

        /// <summary>
        /// Gets or sets the reference value
        /// </summary>
        public CimValueReference ValueReference
        {
            get 
            {
                if (_valueReference == null)
                    _valueReference = new CimValueReference(null);
                return _valueReference; 
            }
            set { _valueReference = value; }
        }

        #endregion

        #region Methods

        #endregion
        
    }
}
