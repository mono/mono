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
using System.Text;
using System.Management.Internal.Batch;

namespace System.Management.Internal
{
    #region ReferenceNamesOpSettings
    internal abstract class ReferenceNamesOpSettings : SingleRequest, ICimObjectNameSettings
    {
        ICimObjectName _objectName;
        CimName _resultClass;
        string _role;

        #region Constructors
        public ReferenceNamesOpSettings()//ICimObjectName objectName)
        {
            ReqType = RequestType.ReferenceNames;

            //ObjectName = objectName;
            ResultClass = null;
            Role = null;
        }
        #endregion

        #region Properties and Indexers
        /// <summary>
        /// <para>From DMTF Spec:</para>The ObjectName input parameter defines the target CIM Object whose referring object names are to be returned. It may be either a Class name or an Instance name (model path).
        /// </summary>
        public ICimObjectName ObjectName
        {
            get { return _objectName; }
            set { _objectName = value; }
        }

        /// <summary>
        /// <para>From DMTF Spec:</para>The ResultClass input parameter, if not NULL, MUST be a valid CIM Class name. It acts as a filter on the returned set of Object Names by mandating that each returned Object Name MUST identify an Instance of this Class (or one of its subclasses), or this Class (or one of its subclasses).
        /// </summary>
        public CimName ResultClass
        {
            get { return _resultClass; }
            set { _resultClass = value; }
        }

        /// <summary>
        /// <para>From DMTF Spec:</para>The Role input parameter, if not NULL, MUST be a valid Property name. It acts as a filter on the returned set of Object Names by mandating that each returned Object Name MUST identify an Object that refers to the target Instance via a Property whose name matches the value of this parameter.
        /// </summary>
        public string Role
        {
            get { return _role; }
            set { _role = value; }
        }
        #endregion
    }
    #endregion

    #region ReferenceNamesWithClassNameOpSettings
    internal class ReferenceNamesWithClassNameOpSettings : ReferenceNamesOpSettings   
    {
        //CimName _className;
        //CimName _resultClass;
        //string _role;

        #region Constructors
        //public ReferenceNamesWithClassNameOpSettings(ICimObjectName objectName)
        public ReferenceNamesWithClassNameOpSettings(CimName className)
        {
            //ReqType = RequestType.ReferenceNames;

            ClassName = className;
            //ResultClass = null;
            //Role = null;
        }
        #endregion

        #region Properties and Indexers
        /// <summary>
        /// <para>From DMTF Spec:</para>The ObjectName input parameter defines the target CIM Object whose referring object names are to be returned. It may be either a Class name or an Instance name (model path).
        /// </summary>
        public CimName ClassName
        {
            get { return (CimName)ObjectName; }
            set { ObjectName = value; }
        }
        #endregion
    }
    #endregion

    #region ReferenceNamesWithInstanceNameOpSettings
    internal class ReferenceNamesWithInstanceNameOpSettings : ReferenceNamesOpSettings
    {
        //ICimObjectName _objectName;
        //CimInstanceName _instanceName;
        //CimName _resultClass;
        //string _role;

        #region Constructors
        //public ReferenceNamesWithClassNameOpSettings(ICimObjectName objectName)
        public ReferenceNamesWithInstanceNameOpSettings(CimInstanceName instanceName)
        {
            //ReqType = RequestType.ReferenceNames;

            //ObjectName = objectName;
            InstanceName = instanceName;
            //ResultClass = null;
            //Role = null;
        }
        #endregion

        #region Properties and Indexers
        /// <summary>
        /// <para>From DMTF Spec:</para>The ObjectName input parameter defines the target CIM Object whose referring object names are to be returned. It may be either a Class name or an Instance name (model path).
        /// </summary>
        public CimInstanceName InstanceName
        {
            get { return (CimInstanceName)ObjectName; }
            set { ObjectName = value; }
        }

        /// <summary>
        /// <para>From DMTF Spec:</para>The ResultClass input parameter, if not NULL, MUST be a valid CIM Class name. It acts as a filter on the returned set of Object Names by mandating that each returned Object Name MUST identify an Instance of this Class (or one of its subclasses), or this Class (or one of its subclasses).
        /// </summary>
        //public CimName ResultClass
        //{
        //    get { return _resultClass; }
        //    set { _resultClass = value; }
        //}

        ///// <summary>
        ///// <para>From DMTF Spec:</para>The Role input parameter, if not NULL, MUST be a valid Property name. It acts as a filter on the returned set of Object Names by mandating that each returned Object Name MUST identify an Object that refers to the target Instance via a Property whose name matches the value of this parameter.
        ///// </summary>
        //public string Role
        //{
        //    get { return _role; }
        //    set { _role = value; }
        //}
        #endregion
    }
    #endregion

}
