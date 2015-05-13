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
    internal abstract class CimObject
    {
        /*
         * <!ELEMENT INSTANCE (QUALIFIER*,(PROPERTY|PROPERTY.ARRAY|PROPERTY.REFERENCE)*)>
<!ATTLIST INSTANCE
         %ClassName;
         xml:lang   NMTOKEN  #IMPLIED>
         * */
        private CimQualifierList _qualifiers = null;
        private CimPropertyList _properties = null;

        #region Properties
        public abstract CimName ClassName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the properties of this CimClass object
        /// </summary>
        public CimPropertyList Properties
        {
            get
            {
                if (_properties == null)
                    _properties = new CimPropertyList();
                return _properties;
            }
            set { _properties = value; }
        }

        /// <summary>
        /// Gets the _qualifiers of this CimClass object
        /// </summary>
        public CimQualifierList Qualifiers
        {
            get
            {
                if (_qualifiers == null)
                    _qualifiers = new CimQualifierList();
                return _qualifiers;
            }
            set { _qualifiers = value; }
        }

        /// <summary>
        /// Returns true if the class has a property that is a key property
        /// </summary>
        public bool HasKeyProperty
        {
            get
            {
                //Changed for MONO
                for (int i = 0; i < Properties.Count; ++i)
                {
                    if (Properties[i].IsKeyProperty)
                        return true;
                }
                return false;
            }
        }

        #endregion

    }
}
