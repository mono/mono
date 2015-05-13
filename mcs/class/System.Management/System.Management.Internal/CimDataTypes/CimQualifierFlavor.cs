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
    /// Settings for a CimQualifier
    /// </summary>
    internal class CimQualifierFlavor
    {
        /* <!ENTITY % QualifierFlavor " OVERRIDABLE  (true|false)   'true'
                                        TOSUBCLASS   (true|false)   'true'
                                        TOINSTANCE   (true|false)   'false' //deprecated
                                        TRANSLATABLE (true|false)   'false'">
         *
         * DEPRECATION NOTE:  The attribute TOINSTANCE is DEPRECATED and MAY 
         * be removed from the QualifierFlavor entity in a future version of 
         * this document.  Use of this qualifier is discouraged.
         * 
         */

        private NullableBool _overridable = null;
        private NullableBool _tosubclass = null;
        private NullableBool _toinstance = null;
        private NullableBool _translatable = null;

        #region Constructors
        /// <summary>
        /// Creates a default CimQualifierFlavor object with the default settings
        /// </summary>
        public CimQualifierFlavor()
        {
        }
        #endregion

        #region Properties

        #region Overridable
        /// <summary>
        /// The flag which indicates if this qualifier is overridable. Defaults to true.
        /// </summary>
        public NullableBool Overridable
        {
            get 
            { 
                if (_overridable == null)
                    _overridable = new NullableBool(true);

                return _overridable; 
            }
            set { _overridable = value; }
        }
        #endregion

        #region ToSubClass
        /// <summary>
        /// Gets or sets the flag ToSubclass. Defaults to true.
        /// </summary>
        public NullableBool ToSubClass
        {
            get 
            {
                if (_tosubclass == null)
                    _tosubclass = new NullableBool(true);
                
                return _tosubclass;
            }
            set { _tosubclass = value; }
        }
        #endregion

        #region ToInstance
        /// <summary>
        /// Gets or sets the flag ToInstance. Defaults to false.
        /// </summary>
        [Obsolete]
        public NullableBool ToInstance
        {
            get 
            {
                if (_toinstance == null)
                    _toinstance = new NullableBool(false);
                
                return _toinstance; 
            }
            set { _toinstance = value; }
        }
        #endregion

        #region Translatable
        /// <summary>
        /// Gets or sets the flag indicating. Defaults to false.
        /// </summary>
        public NullableBool Translatable
        {
            get 
            {
                if (_translatable == null)
                    _translatable = new NullableBool(false);
                
                return _translatable; 
            }
            set { _translatable = value; }
        }
        #endregion

        /// <summary>
        /// Returns true if all flavors are set
        /// </summary>
        public bool IsSet
        {
            get { return (Overridable.IsSet && ToSubClass.IsSet && 
                           ToInstance.IsSet && Translatable.IsSet); }
        }
        #endregion

        #region Methods
        #endregion
    }
}
