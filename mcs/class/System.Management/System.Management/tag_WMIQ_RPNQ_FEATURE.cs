//
// System.Management.AuthenticationLevel
//
// Author:
//	Bruno Lauze     (brunolauze@msn.com)
//	Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2015 Microsoft (http://www.microsoft.com)
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
namespace System.Management
{
	internal enum tag_WMIQ_RPNQ_FEATURE
	{
		WMIQ_RPNF_WHERE_CLAUSE_PRESENT = 1,
		WMIQ_RPNF_QUERY_IS_CONJUNCTIVE = 2,
		WMIQ_RPNF_QUERY_IS_DISJUNCTIVE = 4,
		WMIQ_RPNF_PROJECTION = 8,
		WMIQ_RPNF_FEATURE_SELECT_STAR = 16,
		WMIQ_RPNF_EQUALITY_TESTS_ONLY = 32,
		WMIQ_RPNF_COUNT_STAR = 64,
		WMIQ_RPNF_QUALIFIED_NAMES_IN_SELECT = 128,
		WMIQ_RPNF_QUALIFIED_NAMES_IN_WHERE = 256,
		WMIQ_RPNF_PROP_TO_PROP_TESTS = 512,
		WMIQ_RPNF_ORDER_BY = 1024,
		WMIQ_RPNF_ISA_USED = 2048,
		WMIQ_RPNF_ISNOTA_USED = 4096,
		WMIQ_RPNF_GROUP_BY_HAVING = 8192,
		WMIQ_RPNF_WITHIN_INTERVAL = 16384,
		WMIQ_RPNF_WITHIN_AGGREGATE = 32768,
		WMIQ_RPNF_SYSPROP_CLASS = 65536,
		WMIQ_RPNF_REFERENCE_TESTS = 131072,
		WMIQ_RPNF_DATETIME_TESTS = 262144,
		WMIQ_RPNF_ARRAY_ACCESS = 524288,
		WMIQ_RPNF_QUALIFIER_FILTER = 1048576,
		WMIQ_RPNF_SELECTED_FROM_PATH = 2097152
	}
}