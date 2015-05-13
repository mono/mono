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

namespace System.Management.Internal.Batch
{
    #region SingleRequest
    internal abstract class SingleRequest
    {
        #region Enum - RequestType
        public enum RequestType
        {
            GetClass,
            GetInstance,
            DeleteClass,
            DeleteInstance,
            CreateClass,
            CreateInstance,
            InvokeMethod,
            ModifyClass,
            ModifyInstance,
            EnumerateClasses,
            EnumerateLeafClasses,
            EnumerateClassNames,
            EnumerateInstances,
            EnumerateInstanceNames,
            ExecQuery,
            Associators,
            AssociatorNames,
            References,
            ReferenceNames,
            GetProperty,
            SetProperty,
            GetQualifier,
            SetQualifier,
            DeleteQualifier,
            EnumerateQualifiers,
            ExecuteQuery
        }
        #endregion

        CimName _namespace = null;
        RequestType _reqType;
        
        #region Constructors
        public SingleRequest()
        {
        }
        #endregion    

        #region Properties and Indexers
        /// <summary>
        /// Temporarily Overrides the default Namespace
        /// </summary>
        public CimName Namespace
        {
            get { return _namespace; }
            set { _namespace = value; }
        }

        /// <summary>
        /// Type of Operation
        /// </summary>
        protected internal RequestType ReqType
        {
            get { return _reqType; }
            set { _reqType = value; }
        }
        #endregion
    }
    #endregion
}
