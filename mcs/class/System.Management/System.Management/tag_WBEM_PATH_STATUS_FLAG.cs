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
	internal enum tag_WBEM_PATH_STATUS_FLAG
	{
		WBEMPATH_INFO_ANON_LOCAL_MACHINE = 1,
		WBEMPATH_INFO_HAS_MACHINE_NAME = 2,
		WBEMPATH_INFO_IS_CLASS_REF = 4,
		WBEMPATH_INFO_IS_INST_REF = 8,
		WBEMPATH_INFO_HAS_SUBSCOPES = 16,
		WBEMPATH_INFO_IS_COMPOUND = 32,
		WBEMPATH_INFO_HAS_V2_REF_PATHS = 64,
		WBEMPATH_INFO_HAS_IMPLIED_KEY = 128,
		WBEMPATH_INFO_CONTAINS_SINGLETON = 256,
		WBEMPATH_INFO_V1_COMPLIANT = 512,
		WBEMPATH_INFO_V2_COMPLIANT = 1024,
		WBEMPATH_INFO_CIM_COMPLIANT = 2048,
		WBEMPATH_INFO_IS_SINGLETON = 4096,
		WBEMPATH_INFO_IS_PARENT = 8192,
		WBEMPATH_INFO_SERVER_NAMESPACE_ONLY = 16384,
		WBEMPATH_INFO_NATIVE_PATH = 32768,
		WBEMPATH_INFO_WMI_PATH = 65536,
		WBEMPATH_INFO_PATH_HAD_SERVER = 131072
	}
}