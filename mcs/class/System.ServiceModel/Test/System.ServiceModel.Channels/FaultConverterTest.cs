//
// FaultConverterTest.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//
// Copyright (C) 2006 Novell, Inc.  http://www.novell.com
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
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel.Channels
{
	[TestFixture]
	public class FaultConverterTest
	{
		static readonly FaultConverter s11 = FaultConverter.GetDefaultFaultConverter (MessageVersion.Soap11WSAddressing10);
		static readonly FaultConverter s12 = FaultConverter.GetDefaultFaultConverter (MessageVersion.Default);

		XmlWriterSettings GetWriterSettings ()
		{
			XmlWriterSettings s = new XmlWriterSettings ();
			s.OmitXmlDeclaration = true;
			return s;
		}

		[Test]
		public void TryCreateExceptionDefault ()
		{
			Message msg;
			Assert.IsFalse (s12.TryCreateFaultMessage (new Exception ("error happened"), out msg), "#1-1");

			Assert.IsFalse (s12.TryCreateFaultMessage (new FaultException ("fault happened", FaultCode.CreateSenderFaultCode (new FaultCode ("IAmBroken"))), out msg), "#1-2");

			Assert.IsFalse (s12.TryCreateFaultMessage (new FaultException<string> ("fault happened."), out msg), "#1-3");

			Assert.IsFalse (s11.TryCreateFaultMessage (new Exception ("error happened"), out msg), "#2-1");

			Assert.IsFalse (s11.TryCreateFaultMessage (new FaultException ("fault happened", FaultCode.CreateSenderFaultCode (new FaultCode ("IAmBroken"))), out msg), "#2-2");

			Assert.IsFalse (s11.TryCreateFaultMessage (new FaultException<string> ("fault happened."), out msg), "#2-3");
		}
	}
}
