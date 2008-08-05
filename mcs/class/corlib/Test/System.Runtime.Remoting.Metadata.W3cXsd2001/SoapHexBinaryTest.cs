//
// SoapHexBinaryTest.cs - Unit tests for System.Runtime.Remoting.Metadata.
// W3cXsd2001.SoapHexBinary
//
// Author:
//      Gert Driesen  <drieseng@users.sourceforge.net>
//
// Copyright (C) 2008 Gert Driesen
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

using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Metadata.W3cXsd2001;

using NUnit.Framework;

namespace MonoTests.System.Runtime.Remoting.Metadata.W3cXsd2001
{
	[TestFixture]
	public class SoapHexBinaryTest
	{
		[Test] // ctor ()
		public void Constructor1 ()
		{
			SoapHexBinary shb = new SoapHexBinary ();
			Assert.AreEqual ("hexBinary", shb.GetXsdType (), "#1");
			try {
				shb.ToString ();
				Assert.Fail ("#2");
			} catch (NullReferenceException) {
			}
			Assert.IsNull (shb.Value, "#3");
		}

		[Test] // ctor (Byte [])
		public void Constructor2 ()
		{
			byte [] bytes;
			SoapHexBinary shb;
			
			bytes = new byte [] { 2, 3, 5, 7, 11 };
			shb = new SoapHexBinary (bytes);
			Assert.AreEqual ("hexBinary", shb.GetXsdType (), "#A1");
			Assert.AreEqual ("020305070B", shb.ToString (), "#A2");
			Assert.AreSame (bytes, shb.Value, "#A3");

			bytes = new byte [0];
			shb = new SoapHexBinary (bytes);
			Assert.AreEqual ("hexBinary", shb.GetXsdType (), "#B1");
			Assert.AreEqual ("", shb.ToString (), "#B2");
			Assert.AreSame (bytes, shb.Value, "#B3");

			bytes = null;
			shb = new SoapHexBinary (bytes);
			Assert.AreEqual ("hexBinary", shb.GetXsdType (), "#C1");
			try {
				shb.ToString ();
				Assert.Fail ("#C2");
			} catch (NullReferenceException) {
			}
			Assert.IsNull (shb.Value, "#C3");
		}

		[Test]
		public void Parse ()
		{
			string xsdHexBinary;
			SoapHexBinary shb;

			xsdHexBinary = "3f789ABC";
			shb = SoapHexBinary.Parse (xsdHexBinary);
			Assert.AreEqual ("hexBinary", shb.GetXsdType (), "#A1");
			Assert.AreEqual ("3F789ABC", shb.ToString (), "#A2");
			Assert.AreEqual (new byte [] { 63, 120, 154, 188 }, shb.Value, "#A3");

			xsdHexBinary = string.Empty;
			shb = SoapHexBinary.Parse (xsdHexBinary);
			Assert.AreEqual ("hexBinary", shb.GetXsdType (), "#B1");
			Assert.AreEqual (string.Empty, shb.ToString (), "#B2");
			Assert.AreEqual (new byte [0], shb.Value, "#B3");
		}

		[Test]
		public void Parse_Value_Invalid ()
		{
			string xsdHexBinary;
			SoapHexBinary shb;
			
			xsdHexBinary = "3f789ABG";
#if NET_2_0
			try {
				SoapHexBinary.Parse (xsdHexBinary);
				Assert.Fail ("#A1");
			} catch (RemotingException ex) {
				// Soap Parse error, xsd:type xsd:hexBinary
				// invalid 3f789ABCZ
				Assert.AreEqual (typeof (RemotingException), ex.GetType (), "#A2");
				Assert.IsNull (ex.InnerException, "#A3");
				Assert.IsNotNull (ex.Message, "#A4");
				Assert.IsTrue (ex.Message.IndexOf ("xsd:hexBinary") != -1, "#A5");
				Assert.IsTrue (ex.Message.IndexOf (xsdHexBinary) != -1, "#A6");
			}
#else
			shb = SoapHexBinary.Parse (xsdHexBinary);
			Assert.AreEqual ("hexBinary", shb.GetXsdType (), "#A1");
			Assert.AreEqual ("3F789AB0", shb.ToString (), "#A2");
			Assert.AreEqual (new byte [] { 63, 120, 154, 176 }, shb.Value, "#A3");

#endif

			xsdHexBinary = "3f789AbCE";
			try {
				shb = SoapHexBinary.Parse (xsdHexBinary);
				Assert.Fail ("#B1");
			} catch (RemotingException ex) {
				// Soap Parse error, xsd:type xsd:hexBinary
				// invalid 3f789AbCE
				Assert.AreEqual (typeof (RemotingException), ex.GetType (), "#B2");
				Assert.IsNull (ex.InnerException, "#B3");
				Assert.IsNotNull (ex.Message, "#B4");
				Assert.IsTrue (ex.Message.IndexOf ("xsd:hexBinary") != -1, "#B5");
				Assert.IsTrue (ex.Message.IndexOf (xsdHexBinary) != -1, "#B6");
			}

			xsdHexBinary = "3f789GbC";
#if NET_2_0
			try {
				shb = SoapHexBinary.Parse (xsdHexBinary);
				Assert.Fail ("#C1");
			} catch (RemotingException ex) {
				// Soap Parse error, xsd:type xsd:hexBinary
				// invalid 3f789GbC
				Assert.AreEqual (typeof (RemotingException), ex.GetType (), "#C2");
				Assert.IsNull (ex.InnerException, "#C3");
				Assert.IsNotNull (ex.Message, "#C4");
				Assert.IsTrue (ex.Message.IndexOf ("xsd:hexBinary") != -1, "#C5");
				Assert.IsTrue (ex.Message.IndexOf (xsdHexBinary) != -1, "#C6");
			}
#else
			shb = SoapHexBinary.Parse (xsdHexBinary);
			Assert.AreEqual ("hexBinary", shb.GetXsdType (), "#C1");
			Assert.AreEqual ("3F7890BC", shb.ToString (), "#C2");
			Assert.AreEqual (new byte [] { 63, 120, 144, 188 }, shb.Value, "#C3");
#endif

			xsdHexBinary = "3f-89ABC";
#if NET_2_0
			try {
				shb = SoapHexBinary.Parse (xsdHexBinary);
				Assert.Fail ("#D1");
			} catch (RemotingException ex) {
				// Soap Parse error, xsd:type xsd:hexBinary
				// invalid 3f-89ABC
				Assert.AreEqual (typeof (RemotingException), ex.GetType (), "#D2");
				Assert.IsNull (ex.InnerException, "#D3");
				Assert.IsNotNull (ex.Message, "#D4");
				Assert.IsTrue (ex.Message.IndexOf ("xsd:hexBinary") != -1, "#D5");
				Assert.IsTrue (ex.Message.IndexOf (xsdHexBinary) != -1, "#D6");
			}
#else
			shb = SoapHexBinary.Parse (xsdHexBinary);
			Assert.AreEqual ("hexBinary", shb.GetXsdType (), "#D1");
			Assert.AreEqual ("3F089ABC", shb.ToString (), "#D2");
			Assert.AreEqual (new byte [] { 63, 8, 154, 188 }, shb.Value, "#D3");
#endif
		}

		[Test]
		public void Parse_Value_Null ()
		{
			try {
				SoapHexBinary.Parse ((string) null);
				Assert.Fail ("#1");
			} catch (NullReferenceException) {
			}
		}

		[Test]
		public void Value ()
		{
			byte [] bytes;
			SoapHexBinary shb = new SoapHexBinary ();

			bytes = new byte [] { 2, 3, 5, 7, 11 };
			shb.Value = bytes;
			Assert.AreSame (bytes, shb.Value, "#1");

			bytes = null;
			shb.Value = bytes;
			Assert.IsNull (shb.Value, "#2");

			bytes = new byte [0];
			shb.Value = bytes;
			Assert.AreSame (bytes, shb.Value, "#3");
		}

		[Test]
		public void XsdType ()
		{
			Assert.AreEqual ("hexBinary", SoapHexBinary.XsdType);
		}
	}
}
