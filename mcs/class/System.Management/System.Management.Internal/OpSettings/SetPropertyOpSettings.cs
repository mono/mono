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
    internal class SetPropertyOpSettings : SingleRequest
    {
        CimInstanceName _instanceName;
        string _propertyName;
        string _newValue;

        #region Constructors
        public SetPropertyOpSettings(CimInstanceName instanceName, string propertyName)
            : this(instanceName, propertyName, null)
        {

        }
        public SetPropertyOpSettings(CimInstanceName instanceName, string propertyName, string value)
        {
            ReqType = RequestType.SetProperty;

            InstanceName = instanceName;
            PropertyName = propertyName;
            NewValue = value;
        }
        #endregion

        #region Properties and Indexers
        /// <summary>
        /// <para>From DMTF Spec:</para>The InstanceName input parameter specifies the name of the Instance (model path) for which the Property value is to be updated.
        /// </summary>
        public CimInstanceName InstanceName
        {
            get { return _instanceName; }
            set { _instanceName = value; }
        }

        /// <summary>
        /// <para>From DMTF Spec:</para>The PropertyName input parameter specifies the name of the Property whose value is to be updated.
        /// </summary>
        public string PropertyName
        {
            get { return _propertyName; }
            set { _propertyName = value; }
        }

        /// <summary>
        /// <para>From DMTF Spec:</para>The NewValue input parameter specifies the new value for the Property (which may be NULL).
        /// </summary>
        public string NewValue
        {
            get { return _newValue; }
            set { _newValue = value; }
        }
        #endregion
    }
}
