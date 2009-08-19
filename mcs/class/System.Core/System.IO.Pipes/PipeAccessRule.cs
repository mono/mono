//
// PipeAccessRule.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2009 Novell, Inc.  http://www.novell.com
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
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Security.Permissions;
using System.Security.Principal;

namespace System.IO.Pipes
{
	[HostProtection (SecurityAction.LinkDemand, MayLeakOnAbort = true)]
	public sealed class PipeAccessRule : AccessRule
	{
		[MonoNotSupported ("ACL is not supported in Mono")]
		public PipeAccessRule (IdentityReference identity, PipeAccessRights rights, AccessControlType type)
			: base (identity, 0, false, InheritanceFlags.None, PropagationFlags.None, type)
		{
			throw new NotImplementedException ("ACL is not supported in Mono");
		}

		[MonoNotSupported ("ACL is not supported in Mono")]
		public PipeAccessRule (string identity, PipeAccessRights rights, AccessControlType type)
			: this ((IdentityReference) null, rights, type)
		{
			throw new NotImplementedException ("ACL is not supported in Mono");
		}

		[MonoNotSupported ("ACL is not supported in Mono")]
		public PipeAccessRights PipeAccessRights { get; private set; }
	}
}
