//
// TextWriterTest.cs
//
// Author: 
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2004 Novell (http://www.novell.com)
//

using System;
using System.Globalization;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace MonoTests.System.IO
{
	[TestFixture]
	public class TextWriterTest
	{
		class MyTextWriter : TextWriter
		{
			public override Encoding Encoding { get { return Encoding.Default; } }

			internal MyTextWriter ()
				: base (CultureInfo.InvariantCulture)
			{
			}

			public void UpdateLine ()
			{
				NewLine = "Z";
			}

			public void UpdateLine2 ()
			{
				NewLine = "Y";
			}
		}

		[Test]
		public void CoreNewLine ()
		{
			MyTextWriter w = new MyTextWriter ();
			Assert.IsNotNull (w.NewLine);

			w.UpdateLine ();
			Assert.AreEqual ('Z', w.NewLine [0]);

			w.UpdateLine2 ();
			Assert.AreEqual ('Y', w.NewLine [0]);
		}

		class ArrayOrCharTester : TextWriter {
			public bool called_array;
			public override Encoding Encoding { get { return Encoding.UTF8; }}

			public override void Write (char [] x, int a, int b)
			{
				called_array = true;
			}
			public override void Write (char c)
			{
			}
		}

		[Test]
		public void TestCharArrayCallsArrayIntInt ()
		{
			ArrayOrCharTester x = new ArrayOrCharTester ();
			x.Write (new char [] {'a','b','c'});
			Assert.AreEqual (true, x.called_array);			
		}
	}
}
