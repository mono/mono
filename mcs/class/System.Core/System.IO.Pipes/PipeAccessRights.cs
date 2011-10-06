//
// PipeAccessRights.cs
//
// Authors:
//    Marek Safar  <marek.safar@gmail.com>
//
// Copyright 2011 Xamarin Inc (http://www.xamarin.com).
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//

namespace System.IO.Pipes
{
	[Flags]
	public enum PipeAccessRights
	{
		ReadData = 1,
		WriteData = 1 << 1,
		CreateNewInstance = 1 << 2,
		ReadExtendedAttributes = 1 << 3,
		WriteExtendedAttributes = 1 << 4,
		ReadAttributes = 1 << 7,
		WriteAttributes = 1 << 8,

		Delete = 1 << 16,
		ReadPermissions = 1 << 17,
		ChangePermissions = 1 << 18,
		TakeOwnership = 1 << 19,
		Synchronize = 1 << 20,

		AccessSystemSecurity = 1 << 24,

		Read = ReadData | ReadAttributes | ReadExtendedAttributes | ReadPermissions,
		Write = WriteData | WriteAttributes | WriteExtendedAttributes,
		ReadWrite = Read | Write,
		FullControl = ReadWrite | CreateNewInstance | Delete | ChangePermissions | TakeOwnership | Synchronize
	}
}
