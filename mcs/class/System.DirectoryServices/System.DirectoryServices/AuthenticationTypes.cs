/******************************************************************************
* The MIT License
* Copyright (c) 2003 Novell Inc.,  www.novell.com
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

//
// System.DirectoryServices.AuthenticationTypes.cs
//
// Author:
//   Sunil Kumar (sunilk@novell.com)
//
// (C)  Novell Inc.
//

namespace System.DirectoryServices
{
	
	/// <summary>
	/// Specifies the types of authentication used in 
	/// System.DirectoryServices
	/// This enumeration has a FlagsAttribute attribute
	///  that allows a bitwise combination of its member values.
	/// </summary>
	[Flags]
	[Serializable]
	public enum AuthenticationTypes
	{
		Anonymous = 16,
		Delegation = 256,
	    Encryption = 2,
	    FastBind = 32,
	    None = 0,
	    ReadonlyServer = 4,
	    Sealing = 128,
	    Secure = 1,
	    SecureSocketsLayer = 2,
	    ServerBind = 512,
	    Signing = 64
	}

}

