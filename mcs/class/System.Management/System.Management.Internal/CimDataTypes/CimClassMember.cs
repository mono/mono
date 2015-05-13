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
    /// Abstract parent class of a CimProperty or a CimMethod, and include the common attributes of both sub classes
    /// </summary>
    internal abstract class CimClassMember
    {
        /* <!ELEMENT PROPERTY (QUALIFIER*,VALUE?)> 
         * <!ATTLIST PROPERTY 
         *      %CIMName; 
         *      %CIMType; #REQUIRED 
         *      %ClassOrigin; 
         *      %Propagated; 
         *      xml:lang NMTOKEN #IMPLIED>
         * */

        /* <!ELEMENT METHOD 
         * (QUALIFIER*,(PARAMETER|PARAMETER.REFERENCE|PARAMETER.ARRAY|PARAMETER.REFARRAY)*)> 
         * <!ATTLIST METHOD 
         *      %CIMName; 
         *      %CIMType; #IMPLIED 
         *      %ClassOrigin; 
         *      %Propagated;> 
         * */
        private CimQualifierList _qualifiers = null;
        private CimName _name = null;
        private NullableCimType _type = null;
        private CimName _classOrigin = null;
        private NullableBool _isPropagated = null;        
        

        #region Constructors
        #endregion

        #region Properties
        /// <summary>
        /// Gets or set the name of the property
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
        /// Gets or sets the name of the class that the CimMember object belongs to.
        /// </summary>
        public CimName ClassOrigin
        {
            get 
            {
                if (_classOrigin == null)
                    _classOrigin = new CimName(null);
                return _classOrigin; 
            }
            set { _classOrigin = value; }
        }

        /// <summary>
        /// Gets or sets the NullableCimType of the CimMember object
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
        /// Gets or sets the CimQualifiers of the CimProperty object
        /// </summary>
        public CimQualifierList Qualifiers
        {
            // Guaranteed not to be null
            get 
            {
                // Guaranteed not to be null
                if (_qualifiers == null)
                    _qualifiers = new CimQualifierList();   // Alloc on demand

                return _qualifiers; 
            }
            private set { _qualifiers = value; }
        }

        /// <summary>
        /// Gets or sets the flag indicating whether or not this property is propagated
        /// </summary>
        public NullableBool IsPropagated
        {
            get 
            {
                if (_isPropagated == null)
                    _isPropagated = new NullableBool(false);
                return _isPropagated; 
            }
            set { _isPropagated = value; }
        }

        //public bool IsSet
        //{
        //    get { return (Name.IsSet && Type.IsSet); }
        //}
        #endregion

        #region Methods
        #endregion
    }
}
