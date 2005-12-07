//
// CacheCas.cs - CAS unit tests for System.Web.Caching.Cache
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using NUnit.Framework;

using System;
using System.Security;
using System.Security.Permissions;
using System.Web;
using System.Web.Caching;

namespace MonoCasTests.System.Web.Caching {

	[TestFixture]
	[Category ("CAS")]
	public class CacheCas : AspNetHostingMinimal {

		// LAMESPEC: using Cache also requires permission for UnmanagedCode
		// this shows up only for PermitOnly (expected) unless Level is None
		[SecurityPermission (SecurityAction.Assert, UnmanagedCode = true)]
		private object UnmanagedCreateControl (SecurityAction action, AspNetHostingPermissionLevel level)
		{
			return base.CreateControl (action, level);
		}

		public override object CreateControl (SecurityAction action, AspNetHostingPermissionLevel level)
		{
			if ((level != AspNetHostingPermissionLevel.None) && (action == SecurityAction.PermitOnly))
				return UnmanagedCreateControl (action, level);
			else
				return base.CreateControl (action, level);
		}

		public override Type Type {
			get { return typeof (Cache); }
		}
	}
}
