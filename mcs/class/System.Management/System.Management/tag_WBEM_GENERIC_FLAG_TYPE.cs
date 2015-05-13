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
	internal enum tag_WBEM_GENERIC_FLAG_TYPE
	{
		WBEM_FLAG_DONT_SEND_STATUS = 0,
		WBEM_FLAG_RETURN_WBEM_COMPLETE = 0,
		WBEM_FLAG_BIDIRECTIONAL = 0,
		WBEM_FLAG_RETURN_ERROR_OBJECT = 0,
		WBEM_FLAG_SEND_ONLY_SELECTED = 0,
		WBEM_RETURN_WHEN_COMPLETE = 0,
		WBEM_FLAG_RETURN_IMMEDIATELY = 16,
		WBEM_RETURN_IMMEDIATELY = 16,
		WBEM_FLAG_FORWARD_ONLY = 32,
		WBEM_FLAG_NO_ERROR_OBJECT = 64,
		WBEM_FLAG_SEND_STATUS = 128,
		WBEM_FLAG_ENSURE_LOCATABLE = 256,
		WBEM_FLAG_DIRECT_READ = 512,
		WBEM_MASK_RESERVED_FLAGS = 126976,
		WBEM_FLAG_USE_AMENDED_QUALIFIERS = 131072,
		WBEM_FLAG_STRONG_VALIDATION = 1048576
	}
}