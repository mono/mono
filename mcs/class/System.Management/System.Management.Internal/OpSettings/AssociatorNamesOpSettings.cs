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

    #region AssociatorNamesOpSettings
    internal abstract class AssociatorNamesOpSettings : SingleRequest, ICimObjectNameSettings
    {
        ICimObjectName _objectName;
        CimName _assocClass;
        CimName _resultClass;
        string _role;
        string _resultRole;

        #region Constructors
        public AssociatorNamesOpSettings()
        {
            ReqType = RequestType.AssociatorNames;

            AssocClass = null;
            ResultClass = null;
            Role = null;
            ResultRole = null;
        }
        #endregion

        #region Properties and Indexers
        /// <summary>
        /// <para>From DMTF Spec:</para>The ObjectName input parameter defines the source CIM Object whose associated names are to be returned. This is either a Class name or Instance name (model path).
        /// </summary>
        public ICimObjectName ObjectName
        {
            get { return _objectName; }
            set { _objectName = value; }
        }

        /// <summary>
        /// <para>From DMTF Spec:</para>The AssocClass input parameter, if not NULL, MUST be a valid CIM Association Class name. It acts as a filter on the returned set of names by mandating that each returned name identifies an Object that MUST be associated to the source Object via an Instance of this Class or one of its subclasses.
        /// </summary>
        public CimName AssocClass
        {
            get { return _assocClass; }
            set { _assocClass = value; }
        }

        /// <summary>
        /// <para>From DMTF Spec:</para>The ResultClass input parameter, if not NULL, MUST be a valid CIM Class name. It acts as a filter on the returned set of names by mandating that each returned name identifies an Object that MUST be either an Instance of this Class (or one of its subclasses) or be this Class (or one of its subclasses).
        /// </summary>
        public CimName ResultClass
        {
            get { return _resultClass; }
            set { _resultClass = value; }
        }

        /// <summary>
        /// <para>From DMTF Spec:</para>The Role input parameter, if not NULL, MUST be a valid Property name. It acts as a filter on the returned set of names by mandating that each returned name identifies an Object that MUST be associated to the source Object via an Association in which the source Object plays the specified role (i.e. the name of the Property in the Association Class that refers to the source Object MUST match the value of this parameter).
        /// </summary>
        public string Role
        {
            get { return _role; }
            set { _role = value; }
        }

        /// <summary>
        /// <para>From DMTF Spec:</para>The ResultRole input parameter, if not NULL, MUST be a valid Property name. It acts as a filter on the returned set of names by mandating that each returned name identifies an Object that MUST be associated to the source Object via an Association in which the named returned Object plays the specified role (i.e. the name of the Property in the Association Class that refers to the returned Object MUST match the value of this parameter).
        /// </summary>
        public string ResultRole
        {
            get { return _resultRole; }
            set { _resultRole = value; }
        }
        #endregion
    }
    #endregion

    #region AssociatorNamesWithClassNameOpSettings
    internal class AssociatorNamesWithClassNameOpSettings : AssociatorNamesOpSettings
    {
        //CimName _className; 	

        #region Constructors
        public AssociatorNamesWithClassNameOpSettings(CimName className)
            : base()
        {
            ClassName = className;
        }
        #endregion

        #region Properties and Indexers
        /// <summary>
        /// <para>From DMTF Spec:</para>The ObjectName input parameter defines the source CIM Object whose associated names are to be returned. This is either a Class name or Instance name (model path).
        /// </summary>
        public CimName ClassName
        {
            get { return (CimName)ObjectName; }
            set { ObjectName = value; }
        }

        #endregion

    }
    #endregion

    #region AssociatorNamesWithInstanceNameOpSettings
    internal class AssociatorNamesWithInstanceNameOpSettings : AssociatorNamesOpSettings
    {
        //CimInstanceName _instanceName;

        #region Constructors
        public AssociatorNamesWithInstanceNameOpSettings(CimInstanceName instanceName)
            : base()
        {
            InstanceName = instanceName;
        }
        #endregion

        #region Properties and Indexers
        /// <summary>
        /// <para>From DMTF Spec:</para>The ObjectName input parameter defines the source CIM Object whose associated names are to be returned. This is either a Class name or Instance name (model path).
        /// </summary>
        public CimInstanceName InstanceName
        {
            get { return (CimInstanceName)ObjectName; }
            set { ObjectName = value; }
        }

        #endregion
    }
    #endregion

}
