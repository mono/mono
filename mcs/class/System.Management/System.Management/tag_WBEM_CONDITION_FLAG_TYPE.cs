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
	internal enum tag_WBEM_CONDITION_FLAG_TYPE
	{
		WBEM_FLAG_ALWAYS = 0,
		WBEM_FLAG_ONLY_IF_TRUE = 1,
		WBEM_FLAG_ONLY_IF_FALSE = 2,
		WBEM_FLAG_ONLY_IF_IDENTICAL = 3,
		WBEM_MASK_PRIMARY_CONDITION = 3,
		WBEM_FLAG_KEYS_ONLY = 4,
		WBEM_FLAG_REFS_ONLY = 8,
		WBEM_FLAG_LOCAL_ONLY = 16,
		WBEM_FLAG_PROPAGATED_ONLY = 32,
		WBEM_FLAG_SYSTEM_ONLY = 48,
		WBEM_FLAG_NONSYSTEM_ONLY = 64,
		WBEM_MASK_CONDITION_ORIGIN = 112,
		WBEM_FLAG_CLASS_OVERRIDES_ONLY = 256,
		WBEM_FLAG_CLASS_LOCAL_AND_OVERRIDES = 512,
		WBEM_MASK_CLASS_CONDITION = 768
	}
}