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
	public class TextWriterTest : Assertion
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
				CoreNewLine = new char [] {'Z'};
			}

			public void UpdateLine2 ()
			{
				CoreNewLine [0] = 'Y';
			}
		}

		[Test]
		public void CoreNewLine ()
		{
			MyTextWriter w = new MyTextWriter ();
			AssertNotNull (w.NewLine);

			w.UpdateLine ();
			AssertEquals ('Z', w.NewLine [0]);

			w.UpdateLine2 ();
			AssertEquals ('Y', w.NewLine [0]);
		}
	}
}
