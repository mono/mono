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
    /// 
    /// </summary>
    internal class CimMethod : CimClassMember
    {
        /* <!ELEMENT METHOD 
         * (QUALIFIER*,(PARAMETER|PARAMETER.REFERENCE|PARAMETER.ARRAY|PARAMETER.REFARRAY)*)> 
         * <!ATTLIST METHOD 
         *      %CIMName; 
         *      %CIMType; #IMPLIED 
         *      %ClassOrigin; 
         *      %Propagated;> 
         * */

        //private CimQualifierList _qualifiers;        
        //private CimName _name;
        //private CimType _type;
        //private CimName _classOrigin;
        //private bool _isPropagated;
        private CimParameterList _parameters = null;

        #region Constructors
        public CimMethod(string name)
            : this(new CimName(name))
        {

        }
        public CimMethod(CimName name)
        {
            Name = name;
        }
        public CimMethod(CimName name, CimType type)
            : this(name)
        {
            Type = type;            
        }

        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the CimParameters of the CimMethod object
        /// </summary>
        public CimParameterList Parameters
        {
            get
            {
                if (_parameters == null)
                    _parameters = new CimParameterList();

                return _parameters;
            }
            private set { _parameters = value; }
        }

        ///// <summary>
        ///// Gets or sets the name of the CimMethod object
        ///// </summary>
        //public CimName Name
        //{
        //    get
        //    {
        //        if (_name == null)
        //            _name = new CimName(string.Empty);
        //        return _name;
        //    }
        //    set { _name = value; }
        //}
        ///// <summary>
        ///// Gets or sets the return type of the CimMethod object
        ///// </summary>
        //public CimType Type
        //{
        //    get { return _type; }
        //    set { _type = value; }
        //}
        ///// <summary>
        ///// Gets or sets the CimQualifiers of the CimMethod object
        ///// </summary>
        //public CimQualifierList Qualifiers
        //{
        //    get 
        //    {
        //        if (_qualifiers == null)
        //            _qualifiers = new CimQualifierList();

        //        return _qualifiers; 
        //    }
        //    set { _qualifiers = value; }
        //}

        ///// <summary>
        ///// Gets or sets the name of the class that the method belongs to
        ///// </summary>
        //public CimName ClassOrigin
        //{
        //    get 
        //    {
        //        if (_classOrigin == null)
        //            _classOrigin = new CimName(string.Empty);
        //        return _classOrigin; 
        //    }
        //    set { _classOrigin = value; }
        //}
        ///// <summary>
        ///// Gets or sets the flag
        ///// </summary>
        //public bool IsPropagated
        //{
        //    get { return _isPropagated; }
        //    set { _isPropagated = value; }
        //}

        #endregion

        #region Methods
        ///// <summary>
        ///// Finds the CimParameter in the current CimMethod object
        ///// </summary>
        ///// <param name="parameterName">Name of the parameter</param>
        ///// <returns>Returns the CimParameter if found, otherwise null</returns>
        //public CimParameter GetParameter(CimName parameterName)
        //{
        //    return CimDataTypeUtils.GetParameter(_parameters, parameterName);
        //}
        ///// <summary>
        ///// Adds a CimParamter to the object array of parameters
        ///// </summary>
        ///// <param name="parameter">CimParameter to add</param>
        //public void AddParameter(CimParameter parameter)
        //{
        //    CimDataTypeUtils.AddParameter(ref _parameters, parameter);
        //}

        public bool IsSet
        {
            get { return (Name.IsSet && Type.IsSet); }
        }
        #endregion
    }
}
