// SecurityPermissionFlag.cs
//
// This code was automatically generated from
// ECMA CLI XML Library Specification.
// Generator: libgen.xsl [1.0; (C) Sergey Chaban (serge@wildwestsoftware.com)]
// Created: Wed, 5 Sep 2001 06:30:18 UTC
// Source file: AllTypes.xml
// URL: http://msdn.microsoft.com/net/ecma/AllTypes.xml
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

namespace System.Security.Permissions {

#if NET_2_1 && !MONOTOUCH
	[Obsolete ("CAS support is not available with Silverlight applications.")]
#endif
	[ComVisible (true)]
	[Flags]
	[Serializable]
	public enum SecurityPermissionFlag {

		NoFlags = 0x00000000,

		Assertion = 0x00000001,

		UnmanagedCode = 0x00000002,

		SkipVerification = 0x00000004,

		Execution = 0x00000008,

		ControlThread = 0x00000010,

		ControlEvidence = 0x00000020,

		ControlPolicy = 0x00000040,

		SerializationFormatter = 0x00000080,

		ControlDomainPolicy = 0x00000100,

		ControlPrincipal = 0x00000200,

		ControlAppDomain = 0x00000400,

		RemotingConfiguration = 0x00000800,

		Infrastructure = 0x00001000,

		BindingRedirects = 0x00002000,

		AllFlags = Assertion | UnmanagedCode | SkipVerification | Execution | ControlThread
			| ControlAppDomain | ControlDomainPolicy | ControlEvidence | ControlPolicy
			| ControlPrincipal | Infrastructure | RemotingConfiguration | SerializationFormatter
			| BindingRedirects
	} // SecurityPermissionFlag

} // System.Security.Permissions
