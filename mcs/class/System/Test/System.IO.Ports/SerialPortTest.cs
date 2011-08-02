//
// SerialPortTest.cs: Test cases for SerialPort.
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
// Copyright (c) 2010 Novell, Inc. (http://www.novell.com)
//
// Author:
//   	Carlos Alberto Cortez <calberto.cortez@gmail.com>
//	Konrad M. Kruczynski
//

using System;
using System.IO;
using System.IO.Ports;

using NUnit.Framework;

namespace MonoTests.System.IO.Ports
{
	[TestFixture]
	public class SerialPortTest
	{
		[Category ("NotWorking")]
		[Test]
		public void DiscardNull ()
		{
			SerialPort sp = new SerialPort ();
			Assert.AreEqual (false, sp.IsOpen, "#A1");
			Assert.AreEqual (false, sp.DiscardNull, "#A2");

			sp.DiscardNull = true;
			Assert.AreEqual (true, sp.DiscardNull, "#B1");

			sp.DiscardNull = false;
			Assert.AreEqual (false, sp.DiscardNull, "#C1");
		}

		[Test]
		public void NonstandardBaudRate ()
		{
			int platform = (int) Environment.OSVersion.Platform;
			// we are testing on Unix only
			if ((platform != 4) && (platform != 128)) return;
			SerialPort sp = new SerialPort ();
			sp.BaudRate = 1234;
			var exceptionCatched = false;
			try {
				sp.Open();
			} catch(ArgumentOutOfRangeException) {
				exceptionCatched = true;
			}
			Assert.IsTrue(exceptionCatched,
				"Exception not thrown despite wrong baud rate");
		}

		/// <summary>
		/// This test is related to bug #635971
		/// </summary>
		[Test]
		public void ZeroTimeout ()
		{
			var sp = new SerialPort ();
			var exceptionThrown = false;
			try {
				sp.ReadTimeout = 0;
			} catch(ArgumentOutOfRangeException) {
				exceptionThrown = true;
			}
			Assert.IsFalse(exceptionThrown,
				"Exception thrown despite proper timeout (0)");
		}

	}
}

