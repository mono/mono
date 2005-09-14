//
// CacheDependencyCas.cs 
//	- CAS unit tests for System.Web.Caching.CacheDependency
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
using System.IO;
using System.Reflection;
using System.Security.Permissions;
using System.Web;
using System.Web.Caching;

namespace MonoCasTests.System.Web.Caching {

	[TestFixture]
	[Category ("CAS")]
	public class CacheDependencyCas : AspNetHostingMinimal {

		private string tempFile;

		[TestFixtureSetUp]
		public void FixtureSetUp ()
		{
			// that requires both FileIOPermission and EnvironmentPermission
			// so we do it before setting the stack with PermitOnly and Deny
			tempFile = Path.GetTempFileName ();
		}

		// note: CacheDependency still requires some file access
		[FileIOPermission (SecurityAction.Assert, Unrestricted = true)]
#if ONLY_1_1
		[SecurityPermission (SecurityAction.Assert, UnmanagedCode = true)]
#endif
		private object FileIOPermissionCreateControl (SecurityAction action, AspNetHostingPermissionLevel level)
		{
			return CreateControlStringCtor (action, level);
		}

		public override object CreateControl (SecurityAction action, AspNetHostingPermissionLevel level)
		{
			if ((level != AspNetHostingPermissionLevel.None) && (action == SecurityAction.PermitOnly)) {
				try {
					return FileIOPermissionCreateControl (action, level);
				}
				catch (TargetInvocationException tie) {
#if ONLY_1_1
					// hide this error (occurs with ms 1.x)
					if ((tie.InnerException is NullReferenceException) &&
						(level == AspNetHostingPermissionLevel.Unrestricted)) {
						return String.Empty;
					}
#endif
					throw tie;
				}
			} 
			else
				return CreateControlStringCtor (action, level);
		}

		private object CreateControlStringCtor (SecurityAction action, AspNetHostingPermissionLevel level)
		{
			// not public empty (default) ctor - at least not before 2.0
			ConstructorInfo ci = this.Type.GetConstructor (new Type[1] { typeof (string) });
			Assert.IsNotNull (ci, ".ctor(string)");
			return ci.Invoke (new object[1] { tempFile });
		}

		public override Type Type {
			get { return typeof (CacheDependency); }
		}
	}
}
