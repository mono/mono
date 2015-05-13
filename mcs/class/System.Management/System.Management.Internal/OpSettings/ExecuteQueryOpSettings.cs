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
    internal class ExecuteQueryOpSettings : SingleRequest
    {
        string _queryLanguage;
        string _query;

        #region Constructors
        public ExecuteQueryOpSettings(string queryLanguage, string query)
        {
            ReqType = RequestType.ExecuteQuery;

            QueryLanguage = queryLanguage;
            Query = query;
        }
        #endregion

        #region Properties and Indexers
        /// <summary>
        /// <para>From DMTF Spec:</para>The QueryLanguage input parameter is used to uniquely identify the query language in which the Query parameter is expressed. In order to ensure uniqueness, valid values for QueryLanguage MUST conform to the following syntax: &lt;Vendor ID&gt;:&lt;Language ID&gt;. &lt;Vendor ID&gt; MUST NOT include a colon (":") and MUST include a copyrighted, trademarked or otherwise unique name that is owned by the entity that had defined query language. For DMTF defined query languages, the &lt;Vendor ID&gt; is "CIM". The &lt;Language ID&gt; MUST include a vendor-specified, unique identifier for the query language. "CIM:CQL" is the only DMTF defined value for this parameter. Refer to the CIM Query Language Specification, DSP0202 for details on the CIM:CQL query language.
        /// </summary>
        public string QueryLanguage
        {
            get { return _queryLanguage; }
            set { _queryLanguage = value; }
        }

        /// <summary>
        /// <para>From DMTF Spec:</para>The Query input parameter defines the query to be executed. The format of this string MUST comply with the QueryLanguage specification.
        /// </summary>
        public string Query
        {
            get { return _query; }
            set { _query = value; }
        }
        #endregion
    }
}
