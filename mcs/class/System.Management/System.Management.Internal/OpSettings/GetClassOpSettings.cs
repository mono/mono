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
    /// <summary>
    /// 
    /// </summary>
    internal class GetClassOpSettings : SingleRequest
    {
        CimName _className;
        bool _localOnly;
        bool _includeQualifiers;
        bool _includeClassOrigin;
        string[] _propertyList;

        #region Constructors
        public GetClassOpSettings(string className)
            : this(new CimName(className))
        {
        }

        public GetClassOpSettings(CimName className)
        {
            ReqType = RequestType.GetClass;

            ClassName = className;
            LocalOnly = true;
            IncludeQualifiers = true;
            IncludeClassOrigin = false;
            PropertyList = null;
        }
        #endregion

        #region Properties and Indexers
        ///// <summary>
        ///// <para>From DMTF Spec:</para>The ClassName input parameter defines the name of the Class to be retrieved.
        ///// </summary>
        //public string ClassNameStr
        //{
        //    get
        //    {
        //        if (ClassName == null)
        //            return string.Empty;
        //        else
        //            return _className.ToString();
        //    }

        //    set { _className = new CimName(value); }
        //}

        /// <summary>
        /// <para>From DMTF Spec:</para>The ClassName input parameter defines the name of the Class to be retrieved.
        /// </summary>
        public CimName ClassName
        {
            get { return _className; }
            set { _className = value; }
        }

        /// <summary>
        /// <para>From DMTF Spec:</para>If the LocalOnly input parameter is true, this specifies that only CIM Elements (properties, methods and _qualifiers) defined or overridden within the definition of the Class  (as specified in the classname input parameter) are returned.   If false, all elements are returned.
        /// </summary>
        public bool LocalOnly
        {
            get { return _localOnly; }
            set { _localOnly = value; }
        }

        /// <summary>
        /// <para>From DMTF Spec:</para>If the IncludeQualifiers input parameter is true, this specifies that all Qualifiers for that Class (including Qualifiers on the Class and on any returned Properties, Methods or Method Parameters) MUST be included as &lt;QUALIFIER&gt; elements in the response.  If false no &lt;QUALIFIER&gt; elements are present in the returned Class.
        /// </summary>
        public bool IncludeQualifiers
        {
            get { return _includeQualifiers; }
            set { _includeQualifiers = value; }
        }

        /// <summary>
        /// <para>From DMTF Spec:</para>If the IncludeClassOrigin input parameter is true, this specifies that the CLASSORIGIN attribute MUST be present on all appropriate elements in the returned Class. If false, no CLASSORIGIN attributes are present in the returned Class.
        /// </summary>
        public bool IncludeClassOrigin
        {
            get { return _includeClassOrigin; }
            set { _includeClassOrigin = value; }
        }

        /// <summary>
        /// <para>From DMTF Spec:</para>If the PropertyList input parameter is not NULL, the members of the array define one or more Property names.  The returned Class MUST NOT include elements for any Properties missing from this list.  Note that if LocalOnly is specified as true this acts as an additional filter on the set of Properties returned (for example, if Property A is included in the PropertyList but LocalOnly is set to true and A is not local to the requested Class, then it will not be included in the response). If the PropertyList input parameter is an empty array this signifies that no Properties are included in the response. If the PropertyList input parameter is NULL this specifies that all Properties (subject to the conditions expressed by the other parameters) are included in the response.
        /// <para />If the PropertyList contains duplicate elements, the Server MUST ignore the duplicates but otherwise process the request normally.  If the PropertyList contains elements which are invalid Property names for the target Class, the Server MUST ignore such entries but otherwise process the request normally.
        /// </summary>
        public string[] PropertyList
        {
            get { return _propertyList; }
            set { _propertyList = value; }
        }
        #endregion
    }
}
