//------------------------------------------------------------------------------
// 
// System.IO.FileAttributes.cs 
//
// Copyright (C) 2001 Moonlight Enterprises, All Rights Reserved
// 
// Author:         Jim Richardson, develop@wtfo-guru.com
// Created:        Monday, August 13, 2001 
//
//------------------------------------------------------------------------------

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System.Runtime.InteropServices;

namespace System.IO
{
	[Flags]
	[Serializable]
	[ComVisible (true)]
	public enum FileAttributes
	{
		Archive = 0x00020,
		Compressed = 0x00800, 
		Device = 0x00040, // Reserved for future use (NOT the w32 value). 
		Directory = 0x00010,
		Encrypted = 0x04000, // NOT the w32 value
		Hidden = 0x00002,
		Normal = 0x00080,
		NotContentIndexed = 0x02000,
		Offline = 0x01000,
		ReadOnly = 0x00001,
		ReparsePoint = 0x00400,
		SparseFile = 0x00200,
		System = 0x00004,
		Temporary = 0x00100,
#if NET_4_5
		IntegrityStream = 0x8000,
		NoScrubData = 0x20000,
#endif
		//
		// This flag is used internall by Mono to make it Executable
		//
		// Executable = 0x80000000
	}

}
