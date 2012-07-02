// System.Security.AccessControl.AuditRule<T>
//
// Copyright 2012 James F. Bellinger
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

#if NET_4_0

using System;
using System.Security.Principal;

namespace System.Security.AccessControl
{
	public class AuditRule<T> : AuditRule where T : struct
	{
		public AuditRule (string identity, T rights, AuditFlags flags)
			: this (new NTAccount (identity), rights, flags)
		{

		}

		public AuditRule (IdentityReference identity, T rights, AuditFlags flags)
			: this (identity, rights, InheritanceFlags.None, PropagationFlags.None, flags)
		{

		}

		public AuditRule (string identity, T rights, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags)
			: this (new NTAccount (identity), rights, inheritanceFlags, propagationFlags, flags)
		{

		}

		public AuditRule (IdentityReference identity, T rights, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags)
			: this (identity, (int)(object)rights, false, inheritanceFlags, propagationFlags, flags)
		{

		}

		internal AuditRule (IdentityReference identity, int rights, bool isInherited, InheritanceFlags inheritanceFlags, PropagationFlags propagationFlags, AuditFlags flags)
			: base (identity, rights, isInherited, inheritanceFlags, propagationFlags, flags)
		{

		}

		public T Rights {
			get { return (T)(object)AccessMask; }
		}
	}
}

#endif

