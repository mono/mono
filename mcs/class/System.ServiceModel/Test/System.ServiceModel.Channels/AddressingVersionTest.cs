//
// AddressingVersionTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2005 Novell, Inc.  http://www.novell.com
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
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.Text;
using NUnit.Framework;

using Element = System.ServiceModel.Channels.TextMessageEncodingBindingElement;

namespace MonoTests.System.ServiceModel.Channels
{
	[TestFixture]
	public class AddressingVersionTest
	{
		[Test]
		public void Equality ()
		{
			Assert.AreEqual (AddressingVersion.WSAddressing10, 
				MessageVersion.Default.Addressing, "#1");
			Assert.IsTrue (AddressingVersion.WSAddressing10 == 
				MessageVersion.Default.Addressing, "#2");
			Assert.IsTrue (Object.ReferenceEquals (
				AddressingVersion.WSAddressing10,
				MessageVersion.Default.Addressing), "#3");
		}

		[Test]
		public void Constants ()
		{
			Assert.AreEqual ("Addressing10 (http://www.w3.org/2005/08/addressing)",
				AddressingVersion.WSAddressing10.ToString (), "#1");
			Assert.AreEqual ("Addressing200408 (http://schemas.xmlsoap.org/ws/2004/08/addressing)",
				AddressingVersion.WSAddressingAugust2004.ToString (), "#2");
		}
	}
}
