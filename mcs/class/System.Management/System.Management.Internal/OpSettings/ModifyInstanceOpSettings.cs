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
    internal class ModifyInstanceOpSettings : SingleRequest
    {
        CimInstance _modifiedInstance;
        bool _includeQualifiers;
        string[] _propertyList;


        #region Constructors
        public ModifyInstanceOpSettings(CimInstance modifiedInstance)
        {
            ReqType = RequestType.ModifyInstance;

            ModifiedInstance = modifiedInstance;
            IncludeQualifiers = true;
            PropertyList = null;
        }
        #endregion

        #region Properties and Indexers
        /// <summary>
        /// <para>From DMTF Spec:</para>The ModifiedInstance input parameter identifies the name of the Instance to be modified, and defines the set of changes (which MUST be correct amendments to the Instance as defined by the CIM Specification [1]) to be made to the current Instance definition.
        /// </summary>
        public CimInstance ModifiedInstance
        {
            get { return _modifiedInstance; }
            set { _modifiedInstance = value; }
        }

        /// <summary>
        /// <para>From DMTF Spec:</para>DEPRECATION NOTE: The use of the IncludeQualifiers parameter is DEPRECATED and it may be removed in a future version of this specification. The behavior of the IncludeQualifiers parameter is not specified. A CIM Client can not rely on the IncludeQualifiers to have any impact on the operation. It is RECOMMENDED that the CIM Server ignore any _qualifiers included in the instance. If the IncludeQualifiers input parameter is true, this specifies that the Qualifiers are modified as specified in the ModifiedInstance. If false, Qualifiers in the ModifiedInstance are ignored and no Qualifiers are explicitly modified in the specified Instance.
        /// </summary>
        [Obsolete]
        public bool IncludeQualifiers
        {
            get { return _includeQualifiers; }
            set { _includeQualifiers = value; }
        }

        /// <summary>
        /// <para>From DMTF Spec:</para>If the PropertyList input parameter is not NULL, the members of the array define one or more Property names. Only those properties specified in the PropertyList are modified as specified in the ModifiedInstance. Properties of the ModifiedInstance that are missing from the PropertyList are ignored. If the PropertyList input parameter is an empty array this signifies that no Properties are explicitly modified in the specified Instance. If the PropertyList input parameter is NULL this specifies that all Properties are updated in the specified Instance.
        /// <para />If the PropertyList contains duplicate elements, the Server MUST ignore the duplicates but otherwise process the request normally. If the PropertyList contains elements which are invalid Property names for the target Instance, the Server MUST ignore such entries but otherwise process the request normally.
        /// </summary>
        public string[] PropertyList
        {
            get { return _propertyList; }
            set { _propertyList = value; }
        }
        #endregion
    }
}
