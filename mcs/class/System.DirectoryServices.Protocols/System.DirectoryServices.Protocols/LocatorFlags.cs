//
// LocatorFlags.cs
//
// Author:
//   Atsushi Enomoto  <atsushi@ximian.com>
//
// Copyright (C) 2009 Novell, Inc.
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

using System;

namespace System.DirectoryServices.Protocols
{
	[Flags]
	public enum LocatorFlags
	{
		None = 0,
		ForceRediscovery = 1,
		DirectoryServicesRequired = 0x10,
		DirectoryServicesPreferred = 0x20,
		GCRequired = 0x40,
		PdcRequired = 0x80,
		IPRequired = 0x200,
		KdcRequired = 0x400,
		TimeServerRequired = 0x800,
		WriteableRequired = 0x1000,
		GoodTimeServerPreferred = 0x2000,
		AvoidSelf = 0x4000,
		OnlyLdapNeeded = 0x8000,
		IsFlatName = 0x10000,
		IsDnsName = 0x20000,
		ReturnDnsName = 0x40000000,
		ReturnFlatName = -0x7FFFFFFF
	}
}
