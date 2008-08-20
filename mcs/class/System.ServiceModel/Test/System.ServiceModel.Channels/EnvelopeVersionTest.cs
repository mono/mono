//
// MessageVersionTest.cs
//
// Author:
//	Duncan Mak <duncan@ximian.com>
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
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel.Channels
{
	[TestFixture]
	public class EnvelopeVersionTest
	{
		[Test]
		public void GetUltimateDestinationActorValuesTest ()
		{
			// SOAP 1.1
			Assert.AreEqual (2, EnvelopeVersion.Soap11.GetUltimateDestinationActorValues ().Length, "#1");
			Assert.AreEqual (String.Empty, EnvelopeVersion.Soap11.GetUltimateDestinationActorValues () [0], "#2");
			Assert.AreEqual ("http://schemas.xmlsoap.org/soap/actor/next", EnvelopeVersion.Soap11.GetUltimateDestinationActorValues () [1], "#3");

			// SOAP 1.2
			Assert.AreEqual (3, EnvelopeVersion.Soap12.GetUltimateDestinationActorValues ().Length, "#4");
			Assert.AreEqual (String.Empty,
			    EnvelopeVersion.Soap12.GetUltimateDestinationActorValues () [0], "#5");
			Assert.AreEqual ("http://www.w3.org/2003/05/soap-envelope/role/ultimateReceiver",
					 EnvelopeVersion.Soap12.GetUltimateDestinationActorValues () [1], "#6");
			Assert.AreEqual ("http://www.w3.org/2003/05/soap-envelope/role/next",
					 EnvelopeVersion.Soap12.GetUltimateDestinationActorValues () [2], "#7");
		}

		[Test]
		public void Equality ()
		{
			Assert.AreEqual (EnvelopeVersion.Soap12, MessageVersion.Default.Envelope, "#1");
			Assert.IsTrue (EnvelopeVersion.Soap12 == MessageVersion.Default.Envelope, "#2");
			Assert.IsTrue (Object.ReferenceEquals (EnvelopeVersion.Soap12, MessageVersion.Default.Envelope), "#3");
		}
	}
}
