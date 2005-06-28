//
// CodeTypeReferenceTest.cs - NUnit Test Cases for System.CodeDom.CodeTypeReference
//
// Authors:
//   Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2005 Novell
//
using System;
using System.CodeDom;

using NUnit.Framework;

namespace MonoTests.System.CodeDom
{
	[TestFixture]
	public class CodeTypeReferenceTest
	{
		[Test]
		public void EmptyTypeName ()
		{
			CodeTypeReference reference = new CodeTypeReference (string.Empty);
			Assert.AreEqual (typeof (void).FullName, reference.BaseType);
			Assert.AreEqual (0, reference.ArrayRank);
			Assert.IsNull (reference.ArrayElementType);
		}

		[Test]
		public void NullTypeName ()
		{
			CodeTypeReference reference = new CodeTypeReference ((string) null);
			Assert.AreEqual (typeof (void).FullName, reference.BaseType);
			Assert.AreEqual (0, reference.ArrayRank);
			Assert.IsNull (reference.ArrayElementType);
		}

		[Test]
#if NET_2_0
		[ExpectedException (typeof(ArgumentNullException))]
#else
		[ExpectedException (typeof (NullReferenceException))]
#endif
		public void NullType ()
		{
			new CodeTypeReference ((Type) null);
		}
	}
}
