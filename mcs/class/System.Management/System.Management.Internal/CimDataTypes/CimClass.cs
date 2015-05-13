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
using System.Xml.Serialization;

namespace System.Management.Internal
{
    /// <summary>
    /// Represents a CIM class definition
    /// </summary>
    internal class CimClass : CimObject
    {
        /* 
         *       
         * <!ELEMENT CLASS (QUALIFIER*,(PROPERTY|PROPERTY.ARRAY|PROPERTY.REFERENCE)*,METHOD*)> 
         * <!ATTLIST CLASS 
         *      %CIMName; 
         *      %SuperClass;>
         */

        private CimMethodList _methods = null;
        //private CimName _name = null;
        private CimName _superClass = null;
        private CimName _className = null;
        private NullableBool _isKeyed = null;
        private NullableBool _isAssociation = null;
        

        #region Constructors
        /// <summary>
        /// Creates an empty CimClass object
        /// </summary>
        public CimClass()
        {
        }
        
        /// <summary>
        /// Creates a new CimClass with the class name
        /// </summary>
        /// <param name="className">Name of the class</param>
        public CimClass(CimName className)
        {
            ClassName = className;
        }

        /// <summary>
        /// Creates a new CimClass with a class name, and parent class
        /// </summary>
        /// <param name="className">Name of the class</param>
        /// <param name="superClass">Name of the parent class</param>
        public CimClass(CimName className, CimName superClass):this(className)
        {
            SuperClass = superClass;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the methods of this CimClass object
        /// </summary>
        /// <example>
        /// <code>
        /// CimClass curClass = new CimClass();
        /// Console.WriteLine(curClass.Methods["meth1"].Name);
        /// </code>
        /// </example>
        public CimMethodList Methods
        {
            get 
            {
                if (_methods == null)
                    _methods = new CimMethodList();
                return _methods; 
            }
            set { _methods = value; }
        }

        /// <summary>
        /// Gets or sets the name of the CimClass object
        /// </summary>
        public override CimName ClassName
        {
            get
            {
                if (_className == null)
                    _className = new CimName(null);
                return _className;
            }
            set { _className = value; }
        }

        /// <summary>
        /// Gets or sets the name of the super class
        /// </summary>
        public CimName SuperClass
        {
            get
            {
                if (_superClass == null)
                    _superClass = new CimName(null);

                return _superClass;
            }
            set { _superClass = value; }
        }

        /// <summary>
        /// Gets or sets the flag indicating whether this class has keys
        /// </summary>
        public NullableBool IsKeyed
        {
            get 
            {
                if (_isKeyed == null)
                    _isKeyed = new NullableBool();

                return _isKeyed; 
            }
            set { _isKeyed = value; }
        }

        /// <summary>
        /// Gets or sets the flag that indicates this class is an association
        /// </summary>
        public NullableBool IsAssociation
        {
            get 
            {
                if (_isAssociation == null)
                    _isAssociation = new NullableBool();

                return _isAssociation; 
            }
            set { _isAssociation = value; }
        }
        
        /// <summary>
        /// Returns true is the class name is set for the class
        /// </summary>    
        public bool IsSet
        {
            get { return (ClassName.IsSet); }
        }
        #endregion

        #region Methods
        


        #endregion

    }
}
