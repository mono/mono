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
    internal class CimQualifierDeclaration
    {
        /* <!ELEMENT QUALIFIER.DECLARATION (SCOPE?,(VALUE|VALUE.ARRAY)?)>
         * <!ATTLIST QUALIFIER.DECLARATION 
         *      %CIMName;
         *      %CIMType;                       #REQUIRED 
         *      ISARRAY        (true|false)     #IMPLIED
         *      %ArraySize;
         *      %QualifierFlavor;>
         * */

        private CimScope _scope = null;
        private CimValueList _values = null;
        private CimName _name = null;
        private NullableCimType _type = null;
        private NullableBool _isArray = null;
        private NullableInt32 _arraySize = null;
        private CimQualifierFlavor _qualifierFlavor = null;
        
        #region Constructors
        //public CimQualifierDeclaration(CimType type)
        //{
        //    _type = type;
        //}
        public CimQualifierDeclaration(CimType type, CimName name)
        {
            _type = type;
            Name = name;
        }
        public CimQualifierDeclaration(CimType type, string name)
            : this(type, new CimName(name))
        {
        }

        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the CimType of this qualifier
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
        /// Gets or sets the name of the CimQualifier object
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
        /// Gets or sets: Returns true if the Qualifier is an array
        /// </summary>
        public NullableBool IsArray
        {
            get 
            {
                if (_isArray == null)
                    _isArray = new NullableBool();
                return _isArray; 
            }
            set { _isArray = value; }
        }

        public CimQualifierFlavor QualifierFlavor
        {
            get 
            {
                if (_qualifierFlavor == null)
                    _qualifierFlavor = new CimQualifierFlavor();

                return _qualifierFlavor; 
            }
            private set { _qualifierFlavor = value; }
        }

        public CimScope Scope
        {
            get { return _scope; }
            set { _scope = value; }
        }

        /// <summary>
        /// Gets or set the values of the qualifier declaration
        /// </summary>
        public CimValueList Values
        {
            get 
            {
                if (_values == null)
                    _values = new CimValueList();

                return _values; 
            }

            private set { _values = value; }
        }

        public bool IsSet
        {
            get { return ((Name.IsSet) && (Type.IsSet) && (IsArray.IsSet)); }
        }
        #endregion
    }

}
