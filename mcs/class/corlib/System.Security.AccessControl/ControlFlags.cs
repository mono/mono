//
// System.Security.AccessControl.ControlFlags enum
//
// Author:
//	Dick Porter  <dick@ximian.com>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

namespace System.Security.AccessControl {
	[Flags]
	public enum ControlFlags {
		None					= 0x0000,
		OwnerDefaulted				= 0x0001,
		GroupDefaulted				= 0x0002,
		DiscretionaryAclPresent			= 0x0004,
		DiscretionaryAclDefaulted		= 0x0008,
		SystemAclPresent			= 0x0010,
		SystemAclDefaulted			= 0x0020,
		DiscretionaryAclUntrusted		= 0x0040,
		ServerSecurity				= 0x0080,
		DiscretionaryAclAutoInheritRequired	= 0x0100,
		SystemAclAutoInheritRequired		= 0x0200,
		DiscretionaryAclAutoInherited		= 0x0400,
		SystemAclAutoInherited			= 0x0800,
		DiscretionaryAclProtected		= 0x1000,
		SystemAclProtected			= 0x2000,
		RMControlValid				= 0x4000,
		SelfRelative				= 0x8000,
	}
}

#endif
