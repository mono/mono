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
    /// A single Cim refernce property value
    /// </summary>
    internal class CimValueReference
    {
        /* <!ELEMENT VALUE.REFERENCE 
         *      (CLASSPATH|LOCALCLASSPATH|CLASSNAME|INSTANCEPATH|LOCALINSTANCEPATH|INSTANCENAME)>
         * */
        public enum RefType { ClassNamePath, ClassName, InstanceNamePath, InstanceName };

        #region Members
        private object _cimObject = null;
        private RefType _type;
        #endregion

        #region Constructors
        public CimValueReference()
        {
        }

        /// <summary>
        /// Creates a new CimValueReference with the given object
        /// </summary>
        /// <param name="cimObject"></param>
        public CimValueReference(object cimObject)
        {
            CimObject = cimObject;
        }

        #endregion

        #region Properties and Indexers
        /// <summary>
        /// Gets or sets the CimObject
        /// </summary>
        public object CimObject
        {
            get { return _cimObject; }
            set 
            {
                _cimObject = value;
                if (_cimObject is CimClassNamePath)
                {
                    this._type = RefType.ClassNamePath;
                }
                else if (_cimObject is CimName)
                {
                    this._type = RefType.ClassName;
                }
                else if (_cimObject is CimInstanceNamePath)
                {
                    this._type = RefType.InstanceNamePath;
                }
                else if (_cimObject is CimInstanceName)
                {
                    this._type = RefType.InstanceName;
                }
                else
                {
                    throw new Exception("Invalid type for CimValueReference");
                }
            }
        }

        /// <summary>
        /// Gets the type of the reference
        /// </summary>
        public RefType Type
        {
            get { return _type; }
        }

        /// <summary>
        /// Returns true if the the CimObject property is not null
        /// </summary>
        public bool IsSet
        {
            get { return (CimObject != null); }
        }
        #endregion

        #region Methods and Operators

        #region Equals, operator== , operator!=

        public override bool Equals(object obj)
        {
            if ((obj == null) || !(obj is CimValueReference))
            {
                return false;
            }
            return (this == (CimValueReference)obj);
        }
        public static bool operator ==(CimValueReference val1, CimValueReference val2)
        {

            if (((object)val1 == null) || ((object)val2 == null))
            {
                if (((object)val1 == null) && ((object)val2 == null))
                {
                    return true;
                }
                return false;
            }
            throw new Exception("Not implemented yet");

            //Add code here

        }
        public static bool operator !=(CimValueReference val1, CimValueReference val2)
        {
            if (val1 != null)
                return !(val1.Equals(val2));

            return ((object)val2) == null;
        }
        #endregion
        
        
        #endregion

    }
}
