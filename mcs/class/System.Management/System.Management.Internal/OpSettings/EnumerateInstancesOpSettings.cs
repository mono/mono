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
    internal class EnumerateInstancesOpSettings : SingleRequest
    {
        CimName _className;
        bool _localOnly;
        bool _deepInheritance;
        bool _includeQualifiers;
        bool _includeClassOrigin;
        string[] _propertyList;

        #region Constructors
        public EnumerateInstancesOpSettings(string className)
            : this(new CimName(className))
        {
        }

        public EnumerateInstancesOpSettings(CimName className)
        {
            ReqType = RequestType.EnumerateInstances;

            ClassName = className;
            DeepInheritance = true;
            LocalOnly = true;
            IncludeQualifiers = false;
            IncludeClassOrigin = false;
            PropertyList = null;
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
        /// <para>From DMTF Spec:</para>DEPRECATION NOTE: With the 1.2 release of this specification, the LocalOnly parameter is DEPRECATED. LocalOnly filtering, as defined in 1.1, will not be supported in the next major revision of this specification. In the 1.1 version of this specification, the definition of the LocalOnly parameter was incorrectly modified. This change introduced a number of interoperability and backward compatibility problems for CIM Clients using the LocalOnly parameter to filter the set of Properties returned. The DMTF strongly recommends that CIM Clients set LocalOnly = false and do not rely on the use of this parameter to filter the set of Properties returned. To minimize the impact of implementing this recommendation on CIM Clients, a CIM Server MAY choose to treat the value of the LocalOnly parameter as FALSE for all requests. A CIM Server MUST consistently support a single interpretation of the LocalOnly parameter. Refer to Appendix C for additional details.
        /// </summary>
        [Obsolete]
        public bool LocalOnly
        {
            get { return _localOnly; }
            set { _localOnly = value; }
        }

        /// <summary>
        /// <para>From DMTF Spec:</para>If the DeepInheritance input parameter is true, this specifies that, for each returned Instance of the Class, all properties of the Instance MUST be present (subject to constraints imposed by the other parameters), including any which were added by subclassing the specified Class. If false, each returned Instance includes only properties defined for the specified Class.
        /// </summary>
        public bool DeepInheritance
        {
            get { return _deepInheritance; }
            set { _deepInheritance = value; }
        }

        /// <summary>
        /// <para>From DMTF Spec:</para>DEPRECATION NOTE: The use of the IncludeQualifiers parameter is DEPRECATED and it may be removed in a future version of this specification. The IncludeQualifiers parameter definition is ambiguous and when set to true CIM Clients can not be assured of any _qualifiers being returned. A CIM Client SHOULD always set this parameter to false. To minimize the impact of implementing this recommendation on CIM Clients, a CIM Server MAY choose to treat the value of the IncludeQualifiers parameter as FALSE for all requests. The preferred behavior is to use the class operations to receive qualifier information and not depend on any _qualifiers existing in this response. If the IncludeQualifiers input parameter is true, this specifies that all Qualifiers for that Instance (including Qualifiers on the Instance and on any returned Properties) MUST be included as &lt;QUALIFIER&gt; elements in the response. If false no &lt;QUALIFIER&gt; elements are present in the returned Instance.
        /// </summary>
        [Obsolete]
        public bool IncludeQualifiers
        {
            get { return _includeQualifiers; }
            set { _includeQualifiers = value; }
        }

        /// <summary>
        /// <para>From DMTF Spec:</para>If the IncludeClassOrigin input parameter is true, this specifies that the CLASSORIGIN attribute MUST be present on all appropriate elements in each returned Instance. If false, no CLASSORIGIN attributes are present in each returned Instance.
        /// </summary>
        public bool IncludeClassOrigin
        {
            get { return _includeClassOrigin; }
            set { _includeClassOrigin = value; }
        }

        /// <summary>
        /// <para>From DMTF Spec:</para>If the PropertyList input parameter is not NULL, the members of the array define one or more Property names of the designated class. This may include inherited Property names or Property names explicitly defined in the designated class, but MAY NOT include Property names defined in subclasses of the designated class. Each returned Instance MUST NOT include elements for any Properties missing from this list. Note that the PropertyList parameter acts as an additional filter on the Properties defined by the LocalOnly and DeepInheritance input parameters; if the PropertyList includes a Property that is not in the set defined by the LocalOnly and DeepInheritance combination, the element for the Property MUST NOT be returned. If the PropertyList input parameter is an empty array this signifies that no elements for Properties are included in the returned Instances. If the PropertyList input parameter is NULL, no additional filtering is defined by this parameter.
        /// <para />If the PropertyList contains duplicate elements, the Server MUST ignore the duplicates but otherwise process the request normally. If the PropertyList contains elements which are invalid Property names for any target Instance, the Server MUST ignore such entries but otherwise process the request normally.
        /// </summary>
        public string[] PropertyList
        {
            get { return _propertyList; }
            set { _propertyList = value; }
        }
        #endregion
    }
}
