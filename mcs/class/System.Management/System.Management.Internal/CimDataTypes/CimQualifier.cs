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
    /// A qualifer can have an array or non-array type, and corresponding value array or single value
    /// </summary>
    internal class CimQualifier
    {
        /* <!ELEMENT QUALIFIER ((VALUE|VALUE.ARRAY)?)>
         * <!ATTLIST QUALIFIER 
         *      %CIMName; 
         *      %CIMType; #REQUIRED 
         *      %Propagated; 
         *      %QualifierFlavor; 
         *      xml:lang NMTOKEN #IMPLIED>
         * */
        private CimName _name = null;
        private NullableCimType _type = null;
        private NullableBool _isPropagated = null;
        private CimQualifierFlavor _flavor = null;
        private CimValueList _values = null;
        private bool _hasValueArray;

        #region Constructors
        /// <summary>
        /// Creates a CimQualifier object with the type and name
        /// </summary>
        /// <param name="type">CimType of the qualifier</param>
        /// <param name="name">Name of the qualifier</param>
        public CimQualifier(CimType type, string name)
            : this(type, new CimName(name))
        {
        }

        /// <summary>
        /// Creates a CimQualifier object with the type and name
        /// </summary>
        /// <param name="type">CimType of the qualifier</param>
        /// <param name="name">Name of the qualifier</param>
        public CimQualifier(CimType type, CimName name)
        {
            Type = type;
            Name = name;
        }
        #endregion

        #region Properties
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

        /// <summary>
        /// Gets or sets the flag that designates this qualifier as propagated
        /// </summary>
        public NullableBool IsPropagated
        {
            get 
            {
                if (_isPropagated == null)
                    _isPropagated = new NullableBool();
                return _isPropagated; 
            }
            set { _isPropagated = value; }
        }

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
        /// Gets or sets the values of the property
        /// </summary>
        public CimValueList Values
        {
            get 
            {
                if (_values == null)
                    _values = new CimValueList();   // Alloc on demand

                return _values;
            }
            private set { _values = value; }
        }

        /// <summary>
        /// Gets or sets the qualifier flavor for the qualifier
        /// </summary>
        public CimQualifierFlavor Flavor
        {
            get 
            {
                if (_flavor == null)
                    _flavor = new CimQualifierFlavor(); // Alloc on demand

                return _flavor; 
            }
            private set { _flavor = value; }
        }

        /// <summary>
        /// Returns true if the name and type are set
        /// </summary>
        public bool IsSet
        {
            get { return ((Name.IsSet) &&  (Type != null)); }
        }
        /// <summary>
        /// Returns true if the qualifier has a ValueArray, else just a single Value
        /// </summary>
        public bool HasValueArray
        {
            get { return _hasValueArray; }
            set { _hasValueArray = value; }
        }
        #endregion

        #region Methods

        #endregion
    }
}
