//
// IndentedTextWriterTest.cs:
// 		NUnit Test Cases for System.CodeDom.Compiler.IndentedTextWriter
//
// Authors:
//   Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2004 Novell
//

using System.CodeDom.Compiler;

using NUnit.Framework;

namespace MonoTests.System.CodeDom.Compiler
{
	[TestFixture]
	public class IndentedTextWriterTest
	{
		[Test]
		public void DefaultTabStringTest ()
		{
			Assert.AreEqual (new string (' ', 4), IndentedTextWriter.DefaultTabString);
		}
	}
}

