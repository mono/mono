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
    /// Identifies the scope of a qualifier declaration in the case that there are restrictions on the scope of the qualifier declaration
    /// </summary>
    internal class CimScope
    {
        /* <!ELEMENT SCOPE EMPTY>
         * <!ATTLIST SCOPE 
         *      CLASS        (true|false)      "false"
         *      ASSOCIATION  (true|false)      "false"
         *      REFERENCE    (true|false)      "false"
         *      PROPERTY     (true|false)      "false"
         *      METHOD       (true|false)      "false"
         *      PARAMETER    (true|false)      "false"
         *      INDICATION   (true|false)      "false">
         * */

        //default values
        private NullableBool _isClass = null;
        private NullableBool _isAssociation = null;
        private NullableBool _isReference = null;
        private NullableBool _isProperty = null;
        private NullableBool _isMethod = null;
        private NullableBool _isParameter = null;
        private NullableBool _isIndication = null;

        #region Constructors

        #endregion

        #region Properties

        #region IsClass
        public NullableBool IsClass
        {
            get 
            {
                if (_isClass == null)
                    _isClass = new NullableBool(false);
                return _isClass; 
            }
            set { _isClass = value; }
        }
        #endregion

        #region IsAssociation
        public NullableBool IsAssociation
        {
            get 
            {
                if (_isAssociation == null)
                    _isAssociation = new NullableBool(false);
                return _isAssociation; 
            }
            set { _isAssociation = value; }
        }
        #endregion

        #region IsReference
        public NullableBool IsReference
        {
            get 
            {
                if (_isReference == null)
                    _isReference = new NullableBool(false);
                return _isReference; 
            }
            set { _isReference = value; }
        }
        #endregion

        #region IsProperty
        public NullableBool IsProperty
        {
            get 
            {
                if (_isProperty == null)
                    _isProperty = new NullableBool(false);
                return _isProperty; 
            }
            set { _isProperty = value; }
        }
        #endregion

        #region IsMethod
        public NullableBool IsMethod
        {
            get 
            {
                if (_isMethod == null)
                    _isMethod = new NullableBool(false);
                return _isMethod; 
            }
            set { _isMethod = value; }
        }
        #endregion

        #region IsParameter
        public NullableBool IsParameter
        {
            get 
            {
                if (_isParameter == null)
                    _isParameter = new NullableBool(false);
                return _isParameter; 
            }
            set { _isParameter = value; }
        }
        #endregion

        #region IsIndictaion
        public NullableBool IsIndication
        {
            get 
            {
                if (_isIndication == null)
                    _isIndication = new NullableBool(false);
                return _isIndication; 
            }
            set { _isIndication = value; }
        }
        #endregion

        /// <summary>
        /// Returns true if at least one of the scopes is set
        /// </summary>
        public bool IsSet
        {
            get { return (IsClass.IsSet || IsAssociation.IsSet || IsReference.IsSet || 
                          IsProperty.IsSet || IsMethod.IsSet || IsParameter.IsSet || 
                          IsIndication.IsSet); }
        }
        #endregion

        #region Methods

        #endregion

    }
}
