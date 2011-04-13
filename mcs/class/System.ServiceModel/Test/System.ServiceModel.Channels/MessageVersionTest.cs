//
// MessageVersionTest.cs
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
	public class MessageVersionTest
	{
		[Test]
		public void Equality ()
		{
			MessageVersion v = MessageVersion.CreateVersion (
				EnvelopeVersion.Soap12, AddressingVersion.WSAddressing10);
			Assert.AreEqual (MessageVersion.Default, v, "#1");

			v = MessageVersion.CreateVersion (
				EnvelopeVersion.Soap11, AddressingVersion.WSAddressing10);
			Assert.AreEqual (MessageVersion.Soap11WSAddressing10, v, "#2");

			v = MessageVersion.CreateVersion (
				EnvelopeVersion.Soap12, AddressingVersion.WSAddressing10);
			Assert.AreEqual (MessageVersion.Soap12WSAddressing10, v, "#3");
			
			v = MessageVersion.CreateVersion (
				EnvelopeVersion.None, AddressingVersion.None);
			Assert.AreEqual (MessageVersion.None, v, "#4");

		}

		[Test]
		[ExpectedException (typeof (ArgumentException))]
		public void InvalidCombinationThrowsException ()
		{
			MessageVersion.CreateVersion (EnvelopeVersion.None, AddressingVersion.WSAddressing10);
		}

		[Test]
		public void CreateVersionReturnsSameObjectReference ()
		{
			Assert.AreSame (
				MessageVersion.CreateVersion (EnvelopeVersion.None, AddressingVersion.None),
				MessageVersion.CreateVersion (EnvelopeVersion.None, AddressingVersion.None)
			, "#1");

			Assert.AreSame (
				MessageVersion.CreateVersion (EnvelopeVersion.Soap11, AddressingVersion.None),
				MessageVersion.CreateVersion (EnvelopeVersion.Soap11, AddressingVersion.None)
			, "#2");

			Assert.AreSame (
				MessageVersion.CreateVersion (EnvelopeVersion.Soap12, AddressingVersion.WSAddressing10),
				MessageVersion.CreateVersion (EnvelopeVersion.Soap12, AddressingVersion.WSAddressing10)
			, "#3");

			Assert.AreSame (
				MessageVersion.CreateVersion (EnvelopeVersion.Soap11, AddressingVersion.WSAddressing10),
				MessageVersion.CreateVersion (EnvelopeVersion.Soap11, AddressingVersion.WSAddressing10)
			, "#4");

			Assert.AreSame (
				MessageVersion.CreateVersion (EnvelopeVersion.Soap12, AddressingVersion.None),
				MessageVersion.CreateVersion (EnvelopeVersion.Soap12, AddressingVersion.None)
			, "#5");
		}
	}
}
