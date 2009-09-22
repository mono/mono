//
// System.Security.Policy.PolicyLevel_2_1.cs
//
// Authors:
//      Nick Drochak (ndrochak@gol.com)
//      Duncan Mak (duncan@ximian.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2001 Nick Drochak
// (C) 2003 Duncan Mak, Ximian Inc.
// Portions (C) 2004 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2005, 2009 Novell, Inc (http://www.novell.com)
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

#if NET_2_1 && !MONOTOUCH

using System.Reflection;

namespace System.Security.Policy {

	// the tuner will, later, internalize this type - but in order to be compiled it must be public
	public sealed class PolicyLevel {

                public static PolicyLevel CreateAppDomainLevel ()
		{
			return null;
		}

		internal bool IsFullTrustAssembly (Assembly a)
		{
			return false;
		}

		internal string ResolveClassName (string className)
		{
			return String.Empty;
		}
	}
}

#endif

