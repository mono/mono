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
	public class CodeTypeDelegateTest
	{
		[Test]
		public void EmptyTypeName ()
		{
			CodeTypeDelegate delegateType = new CodeTypeDelegate (string.Empty);
			Assert.AreEqual (string.Empty, delegateType.Name);
		}

		[Test]
		public void NullTypeName ()
		{
			CodeTypeDelegate delegateType = new CodeTypeDelegate ((string) null);
			Assert.AreEqual (string.Empty, delegateType.Name);
		}

		[Test]
		public void BaseTypes ()
		{
			CodeTypeDelegate delegateType = new CodeTypeDelegate ((string) null);
			Assert.AreEqual (1, delegateType.BaseTypes.Count);
			Assert.AreEqual ("System.Delegate", delegateType.BaseTypes[0].BaseType);
		}

		[Test]
		public void DefaultReturnType ()
		{
			CodeTypeDelegate delegateType = new CodeTypeDelegate ((string) null);
			Assert.AreEqual (typeof(void).FullName, delegateType.ReturnType.BaseType);
		}
	}
}
