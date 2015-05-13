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
    /// A single CIM parameter
    /// </summary>
    internal class CimParameter
    {
        /* <!ELEMENT PARAMETER (QUALIFIER*)> 
         * <!ATTLIST PARAMETER 
         *      %CIMName; 
         *      %CIMType; #REQUIRED>
         * */
        private CimQualifierList _qualifiers = null;
        private CimName _name = null;
        private NullableCimType _type = null;

        #region Constructors
        /// <summary>
        /// Creates a new CIMParameter with the given name and type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        public CimParameter(CimType type, CimName name)
        {
            _type = type;
            _name = name;
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
        /// Gets or set the _qualifiers for this parameter
        /// </summary>
        public CimQualifierList Qualifiers
        {
            get 
            {
                if (_qualifiers == null)
                    _qualifiers = new CimQualifierList();   // Alloc on demand

                return _qualifiers; 
            }
            private set { _qualifiers = value; }
        }

        /// <summary>
        /// Returns true if the name property is set
        /// </summary>
        public bool IsSet
        {
            get { return (Name.IsSet); }
        }
        #endregion

        #region Methods
        
        #region Equals, operator== , operator!=
        /// <summary>
        /// Deep compare of two CimParamter objects
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if ((obj == null) || !(obj is CimParameter))
            {
                return false;
            }
            return (this == (CimParameter)obj);
        }
        /// <summary>
        /// Deep compare of two CimParameter objects
        /// </summary>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <returns>Returns true if the parameters have the same values</returns>
        public static bool operator ==(CimParameter val1, CimParameter val2)
        {

            if (((object)val1 == null) || ((object)val2 == null))
            {
                if (((object)val1 == null) && ((object)val2 == null))
                {
                    return true;
                }
                return false;
            }

            if ((val1.Name != val2.Name)||(val1.Type != val2.Type))
                return false;

            return (val1.Qualifiers.IsEqualTo(val2.Qualifiers));


        }
        /// <summary>
        /// Deep compare of two CimParameter objects
        /// </summary>
        /// <param name="val1"></param>
        /// <param name="val2"></param>
        /// <returns>Returns true if the parameters do not have the same values</returns>
        public static bool operator !=(CimParameter val1, CimParameter val2)
        {
            return !(val1 == val2);
        }
        #endregion

        #endregion

    }
}
