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
using System.IO;

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

		[Test]
		public void IndentTest ()
		{
			StringWriter sw = new StringWriter ();
			IndentedTextWriter indentedTextWriter = new IndentedTextWriter (sw);
			Assert.AreEqual (0, indentedTextWriter.Indent, "#1");
			indentedTextWriter.Indent++;
			Assert.AreEqual (1, indentedTextWriter.Indent, "#2");
			indentedTextWriter.Indent = int.MaxValue;
			Assert.AreEqual (int.MaxValue, indentedTextWriter.Indent, "#3");
			indentedTextWriter.Indent = -1;
			Assert.AreEqual (0, indentedTextWriter.Indent, "#4");
			indentedTextWriter.Indent = int.MinValue;
			Assert.AreEqual (0, indentedTextWriter.Indent, "#5");
		}
	}
}
