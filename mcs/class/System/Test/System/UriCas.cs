//
// UriCas.cs - CAS unit tests for System.Uri
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
using System.Reflection;
using System.Security;
using System.Security.Permissions;

using MonoTests.System;

namespace MonoCasTests.System {

	[TestFixture]
	[Category ("CAS")]
	public class UriCas {

		[SetUp]
		public virtual void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void UnitTestReuse ()
		{
			UriTest unit = new UriTest ();
			unit.GetReady ();

			unit.Constructors ();
			unit.Constructor_DualHostPort ();
			unit.RelativeCtor ();
			unit.LeadingSlashes ();
			unit.ConstructorsRejectRelativePath ();
			unit.LocalPath ();
			unit.LocalPath_FileHost ();
			unit.LocalPath_Escape ();
			unit.UnixPath ();
			unit.Unc ();
			unit.FromHex ();
			unit.Unescape ();
			unit.HexEscape ();
			unit.HexUnescape ();
			unit.IsHexDigit ();
			unit.IsHexEncoding ();
			unit.GetLeftPart ();
			unit.NewsDefaultPort ();
			unit.FragmentEscape ();
			unit.CheckHostName2 ();
			unit.IsLoopback ();
			unit.Equals1 ();
			unit.TestEquals2 ();
			unit.CaseSensitivity ();
			unit.GetHashCodeTest ();
			unit.MakeRelative ();
			unit.RelativeUri ();
			unit.ToStringTest ();
			unit.CheckSchemeName ();
			unit.CheckSchemeName_FirstChar ();
			unit.CheckSchemeName_AnyOtherChar ();
			unit.Segments1 ();
			unit.Segments2 ();
			unit.Segments3 ();
			unit.Segments5 ();
			unit.UnixLocalPath ();
			unit.PortMax ();
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[Category ("NotDotNet")] // tests needs to be updated for 2.0
		public void UnitTest2Reuse ()
		{
			UriTest2 unit = new UriTest2 ();
			unit.AbsoluteUriFromFile ();
			unit.RelativeUriFromFile ();
			unit.MoreUnescape ();
			unit.UriScheme ();
		}
		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		[Category ("NotWorking")]
		public void UnitTest3Reuse ()
		{
			UriTest3 unit = new UriTest3 ();
			unit.Absolute_UriKind_Absolute ();
			unit.Relative_UriKind_Relative ();
			unit.TryCreate_String_UriKind_Uri ();
			unit.TryCreate_Uri_String_Uri ();
			unit.TryCreate_Uri_Uri_Uri ();
		}
		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void LinkDemand_Deny_Unrestricted ()
		{
			Type[] types = new Type[1] { typeof (string) };
			ConstructorInfo ci = typeof (Uri).GetConstructor (types);
			Assert.IsNotNull (ci, ".ctor(string)");
			Assert.IsNotNull (ci.Invoke (new object[1] { "http://www.example.com/" }), "invoke");
		}
	}
}
