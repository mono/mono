//
// CodeTypeReferenceCas.cs 
//	- CAS unit tests for System.CodeDom.CodeTypeReference
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
using System.CodeDom;
using System.Reflection;
using System.Security;
using System.Security.Permissions;

namespace MonoCasTests.System.CodeDom {

	[TestFixture]
	[Category ("CAS")]
	public class CodeTypeReferenceCas {

		[SetUp]
		public void SetUp ()
		{
			if (!SecurityManager.SecurityEnabled)
				Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
		}
#if NET_2_0
		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor0_Deny_Unrestricted ()
		{
			CodeTypeReference ctr = new CodeTypeReference ();
			Assert.IsNull (ctr.ArrayElementType, "ArrayElementType");
			ctr.ArrayElementType = new CodeTypeReference ();
			Assert.AreEqual (0, ctr.ArrayRank, "ArrayRank");
			ctr.ArrayRank = 1;
			Assert.AreEqual (String.Empty, ctr.BaseType, "BaseType");
			ctr.BaseType = "System.Void";
			Assert.AreEqual ((CodeTypeReferenceOptions)0, ctr.Options, "Options");
			ctr.Options = CodeTypeReferenceOptions.GlobalReference;
			Assert.AreEqual (0, ctr.TypeArguments.Count, "TypeArguments");
		}
#endif
		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor1_Deny_Unrestricted ()
		{
			CodeTypeReference ctr = new CodeTypeReference ("System.Int32");
			Assert.AreEqual ("System.Int32", ctr.BaseType, "BaseType");
			ctr.BaseType = String.Empty;
			Assert.IsNull (ctr.ArrayElementType, "ArrayElementType");
			ctr.ArrayElementType = new CodeTypeReference ("System.String");
			Assert.AreEqual (0, ctr.ArrayRank, "ArrayRank");
			ctr.ArrayRank = 1;
#if NET_2_0
			Assert.AreEqual ((CodeTypeReferenceOptions) 0, ctr.Options, "Options");
			ctr.Options = CodeTypeReferenceOptions.GenericTypeParameter;
			Assert.AreEqual (0, ctr.TypeArguments.Count, "TypeArguments");
#endif
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor2_Deny_Unrestricted ()
		{
			CodeTypeReference ctr = new CodeTypeReference (typeof (int));
			Assert.AreEqual ("System.Int32", ctr.BaseType, "BaseType");
			ctr.BaseType = String.Empty;
			Assert.IsNull (ctr.ArrayElementType, "ArrayElementType");
			ctr.ArrayElementType = new CodeTypeReference ("System.String");
			Assert.AreEqual (0, ctr.ArrayRank, "ArrayRank");
			ctr.ArrayRank = 1;
#if NET_2_0
			Assert.AreEqual ((CodeTypeReferenceOptions) 0, ctr.Options, "Options");
			ctr.Options = CodeTypeReferenceOptions.GenericTypeParameter;
			Assert.AreEqual (0, ctr.TypeArguments.Count, "TypeArguments");
#endif
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor3_Deny_Unrestricted ()
		{
			CodeTypeReference array = new CodeTypeReference ("System.Int32");
			CodeTypeReference ctr = new CodeTypeReference (array, 1);
			Assert.AreSame (array, ctr.ArrayElementType, "ArrayElementType");
			Assert.AreEqual ("System.Int32", ctr.BaseType, "BaseType");
			Assert.AreEqual (1, ctr.ArrayRank, "ArrayRank");
			ctr.ArrayElementType = new CodeTypeReference ("System.String");
			ctr.BaseType = String.Empty;
			ctr.ArrayRank = 0;
#if NET_2_0
			Assert.AreEqual ((CodeTypeReferenceOptions) 0, ctr.Options, "Options");
			ctr.Options = CodeTypeReferenceOptions.GenericTypeParameter;
			Assert.AreEqual (0, ctr.TypeArguments.Count, "TypeArguments");
#endif
		}
#if NET_2_0
		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor4_Deny_Unrestricted ()
		{
			CodeTypeParameter parameter = new CodeTypeParameter ("System.Int32");
			CodeTypeReference ctr = new CodeTypeReference (parameter);
			Assert.IsNull (ctr.ArrayElementType, "ArrayElementType");
			Assert.AreEqual ("System.Int32", ctr.BaseType, "BaseType");
			Assert.AreEqual (0, ctr.ArrayRank, "ArrayRank");
			ctr.ArrayElementType = new CodeTypeReference ();
			ctr.BaseType = String.Empty;
			ctr.ArrayRank = 1;
			Assert.AreEqual (CodeTypeReferenceOptions.GenericTypeParameter, ctr.Options, "Options");
			ctr.Options = CodeTypeReferenceOptions.GlobalReference;
			Assert.AreEqual (0, ctr.TypeArguments.Count, "TypeArguments");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor5_Deny_Unrestricted ()
		{
			CodeTypeReference ctr = new CodeTypeReference ("System.Int32", CodeTypeReferenceOptions.GlobalReference);
			Assert.IsNull (ctr.ArrayElementType, "ArrayElementType");
			Assert.AreEqual ("System.Int32", ctr.BaseType, "BaseType");
			Assert.AreEqual (0, ctr.ArrayRank, "ArrayRank");
			ctr.ArrayElementType = new CodeTypeReference ();
			ctr.BaseType = String.Empty;
			ctr.ArrayRank = 1;
			Assert.AreEqual (CodeTypeReferenceOptions.GlobalReference, ctr.Options, "Options");
			ctr.Options = CodeTypeReferenceOptions.GenericTypeParameter;
			Assert.AreEqual (0, ctr.TypeArguments.Count, "TypeArguments");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor6_Deny_Unrestricted ()
		{
			CodeTypeReference ctr = new CodeTypeReference (typeof (int), CodeTypeReferenceOptions.GlobalReference);
			Assert.IsNull (ctr.ArrayElementType, "ArrayElementType");
			Assert.AreEqual ("System.Int32", ctr.BaseType, "BaseType");
			Assert.AreEqual (0, ctr.ArrayRank, "ArrayRank");
			ctr.ArrayElementType = new CodeTypeReference ();
			ctr.BaseType = String.Empty;
			ctr.ArrayRank = 1;
			Assert.AreEqual (CodeTypeReferenceOptions.GlobalReference, ctr.Options, "Options");
			ctr.Options = CodeTypeReferenceOptions.GenericTypeParameter;
			Assert.AreEqual (0, ctr.TypeArguments.Count, "TypeArguments");
		}

		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void Constructor7_Deny_Unrestricted ()
		{
			CodeTypeReference[] arguments = new CodeTypeReference[1] { new CodeTypeReference ("System.Int32") };
			CodeTypeReference ctr = new CodeTypeReference ("System.Int32", arguments);
			Assert.IsNull (ctr.ArrayElementType, "ArrayElementType");
			Assert.AreEqual ("System.Int32`1", ctr.BaseType, "BaseType");
			Assert.AreEqual (0, ctr.ArrayRank, "ArrayRank");
			ctr.ArrayElementType = new CodeTypeReference ();
			ctr.BaseType = String.Empty;
			ctr.ArrayRank = 1;
			Assert.AreEqual ((CodeTypeReferenceOptions) 0, ctr.Options, "Options");
			ctr.Options = CodeTypeReferenceOptions.GenericTypeParameter;
			Assert.AreEqual (1, ctr.TypeArguments.Count, "TypeArguments");
		}
#endif
		[Test]
		[PermissionSet (SecurityAction.Deny, Unrestricted = true)]
		public void LinkDemand_Deny_Unrestricted ()
		{
			Type[] types = new Type[1] { typeof (string) };
			ConstructorInfo ci = typeof (CodeTypeReference).GetConstructor (types);
			Assert.IsNotNull (ci, ".ctor(string)");
			Assert.IsNotNull (ci.Invoke (new object[1] { String.Empty }), "invoke");
		}
	}
}
