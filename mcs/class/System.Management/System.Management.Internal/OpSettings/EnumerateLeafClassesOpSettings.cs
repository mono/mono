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
    internal class EnumerateLeafClassesOpSettings : SingleRequest
    {
        CimName _className;
        bool _localOnly;
        bool _includeQualifiers;
        bool _includeClassOrigin;

        #region Constructors
        public EnumerateLeafClassesOpSettings()
        {
            ReqType = RequestType.EnumerateLeafClasses;

            ClassName = null;
            LocalOnly = true;
            IncludeQualifiers = true;
            IncludeClassOrigin = false;
        }
        #endregion

        #region Properties and Indexers
        /// <summary>
        /// <para>From DMTF Spec:</para>The ClassName input parameter defines the Class that is the basis for the enumeration.
        /// </summary>
        public CimName ClassName
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
        /// <para>From DMTF Spec:</para>If the LocalOnly input parameter is true, this specifies that only CIM Elements (properties, methods and _qualifiers) defined or overridden within the definition of the Class (as specified in the classname input parameter) are returned. If false, all elements are returned.
        /// </summary>
        public bool LocalOnly
        {
            get { return _localOnly; }
            set { _localOnly = value; }
        }

        /// <summary>
        /// <para>From DMTF Spec:</para>If the IncludeQualifiers input parameter is true, this specifies that all Qualifiers for each Class (including Qualifiers on the Class and on any returned Properties, Methods or Method Parameters) MUST be included as &lt;QUALIFIER&gt; elements in the response. If false no &lt;QUALIFIER&gt; elements are present in each returned Class.
        /// </summary>
        public bool IncludeQualifiers
        {
            get { return _includeQualifiers; }
            set { _includeQualifiers = value; }
        }

        /// <summary>
        /// <para>From DMTF Spec:</para>If the IncludeClassOrigin input parameter is true, this specifies that the CLASSORIGIN attribute MUST be present on all appropriate elements in each returned Class. If false, no CLASSORIGIN attributes are present in each returned Class.
        /// </summary>
        public bool IncludeClassOrigin
        {
            get { return _includeClassOrigin; }
            set { _includeClassOrigin = value; }
        }
        #endregion
    }
}
