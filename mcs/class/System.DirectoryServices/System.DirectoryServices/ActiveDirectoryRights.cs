/******************************************************************************
* The MIT License
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

using System;

namespace System.DirectoryServices
{
	[Flags]
	public enum ActiveDirectoryRights
	{
		Delete = 65536,
		ReadControl = 131072,
		WriteDacl = 262144,
		WriteOwner = 524288,
		Synchronize = 1048576,
		AccessSystemSecurity = 16777216,
		GenericRead = 131220,
		GenericWrite = 131112,
		GenericExecute = 131076,
		GenericAll = 983551,
		CreateChild = 1,
		DeleteChild = 2,
		ListChildren = 4,
		Self = 8,
		ReadProperty = 16,
		WriteProperty = 32,
		DeleteTree = 64,
		ListObject = 128,
		ExtendedRight = 256
	}
}
