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
	internal enum tag_WMIQ_LANGUAGE_FEATURES
	{
		WMIQ_LF1_BASIC_SELECT = 1,
		WMIQ_LF2_CLASS_NAME_IN_QUERY = 2,
		WMIQ_LF3_STRING_CASE_FUNCTIONS = 3,
		WMIQ_LF4_PROP_TO_PROP_TESTS = 4,
		WMIQ_LF5_COUNT_STAR = 5,
		WMIQ_LF6_ORDER_BY = 6,
		WMIQ_LF7_DISTINCT = 7,
		WMIQ_LF8_ISA = 8,
		WMIQ_LF9_THIS = 9,
		WMIQ_LF10_COMPEX_SUBEXPRESSIONS = 10,
		WMIQ_LF11_ALIASING = 11,
		WMIQ_LF12_GROUP_BY_HAVING = 12,
		WMIQ_LF13_WMI_WITHIN = 13,
		WMIQ_LF14_SQL_WRITE_OPERATIONS = 14,
		WMIQ_LF15_GO = 15,
		WMIQ_LF16_SINGLE_LEVEL_TRANSACTIONS = 16,
		WMIQ_LF17_QUALIFIED_NAMES = 17,
		WMIQ_LF18_ASSOCIATONS = 18,
		WMIQ_LF19_SYSTEM_PROPERTIES = 19,
		WMIQ_LF20_EXTENDED_SYSTEM_PROPERTIES = 20,
		WMIQ_LF21_SQL89_JOINS = 21,
		WMIQ_LF22_SQL92_JOINS = 22,
		WMIQ_LF23_SUBSELECTS = 23,
		WMIQ_LF24_UMI_EXTENSIONS = 24,
		WMIQ_LF25_DATEPART = 25,
		WMIQ_LF26_LIKE = 26,
		WMIQ_LF27_CIM_TEMPORAL_CONSTRUCTS = 27,
		WMIQ_LF28_STANDARD_AGGREGATES = 28,
		WMIQ_LF29_MULTI_LEVEL_ORDER_BY = 29,
		WMIQ_LF30_WMI_PRAGMAS = 30,
		WMIQ_LF31_QUALIFIER_TESTS = 31,
		WMIQ_LF32_SP_EXECUTE = 32,
		WMIQ_LF33_ARRAY_ACCESS = 33,
		WMIQ_LF34_UNION = 34,
		WMIQ_LF35_COMPLEX_SELECT_TARGET = 35,
		WMIQ_LF36_REFERENCE_TESTS = 36,
		WMIQ_LF37_SELECT_INTO = 37,
		WMIQ_LF38_BASIC_DATETIME_TESTS = 38,
		WMIQ_LF39_COUNT_COLUMN = 39,
		WMIQ_LF_LAST = 40,
		WMIQ_LF40_BETWEEN = 40
	}
}