//
// OperationDescriptionTest.cs
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
using System.Collections.ObjectModel;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel.Description
{
	[TestFixture]
	public class OperationDescriptionTest
	{
		[Test]
		public void Messages ()
		{
			ContractDescription cd =
				ContractDescription.GetContract (typeof (IFoo));
			OperationDescription od = cd.Operations [0];
			Assert.IsNull (od.Messages.Find ("Echo"), "#1");
			MessageDescription md = od.Messages.Find ("http://tempuri.org/IFoo/Echo");
			Assert.IsNotNull (md, "#2");

			Assert.AreEqual ("http://tempuri.org/IFoo/Echo", md.Action, "#3");
			Assert.AreEqual (MessageDirection.Input, md.Direction, "#4");
			Assert.IsFalse (md.HasProtectionLevel, "#5");
			Assert.IsNotNull (md.Headers, "#6");
			Assert.AreEqual (0, md.Headers.Count, "#7");
			Assert.IsNull (md.MessageType, "#8");
			Assert.IsNotNull (md.Properties, "#9");
			Assert.AreEqual (0, md.Properties.Count, "#10");

			MessageBodyDescription mb = md.Body;
			Assert.IsNotNull (mb, "#11");
			Assert.AreEqual ("Echo", mb.WrapperName, "#12");
			Assert.AreEqual ("http://tempuri.org/", mb.WrapperNamespace, "#13");
			Assert.IsNotNull (mb.Parts, "#14");
			Assert.AreEqual (0, mb.Parts.Count, "#15");
			Assert.IsNull (mb.ReturnValue, "#16"); // void Echo()
		}

		[Test]
		public void MessagesNameSpace ()
		{
			ContractDescription cd =
				ContractDescription.GetContract (typeof (IFoo2));
			OperationDescription od = cd.Operations [0];
			Assert.IsNull (od.Messages.Find ("Echo"), "#1");
			MessageDescription md = od.Messages.Find ("http://MonoTests.System.ServiceModel.Description/IFoo2/Echo");
			Assert.IsNotNull (md, "#2");

			Assert.AreEqual ("http://MonoTests.System.ServiceModel.Description/IFoo2/Echo", md.Action, "#3");
			Assert.AreEqual (MessageDirection.Input, md.Direction, "#4");
			Assert.IsFalse (md.HasProtectionLevel, "#5");
			Assert.IsNotNull (md.Headers, "#6");
			Assert.AreEqual (0, md.Headers.Count, "#7");
			Assert.IsNull (md.MessageType, "#8");
			Assert.IsNotNull (md.Properties, "#9");
			Assert.AreEqual (0, md.Properties.Count, "#10");

			MessageBodyDescription mb = md.Body;
			Assert.IsNotNull (mb, "#11");
			Assert.AreEqual ("Echo", mb.WrapperName, "#12");
			Assert.AreEqual ("http://MonoTests.System.ServiceModel.Description", mb.WrapperNamespace, "#13");
			Assert.IsNotNull (mb.Parts, "#14");
			Assert.AreEqual (0, mb.Parts.Count, "#15");
			Assert.IsNull (mb.ReturnValue, "#16"); // void Echo()
		}

		[Test]
		public void Parts ()
		{
			ContractDescription cd =
				ContractDescription.GetContract (typeof (IFoo3));

			MessagePartDescriptionCollection parts =
				cd.Operations [0].Messages [0].Body.Parts;

			Assert.AreEqual (1, parts.Count, "#1");
			MessagePartDescription part = parts [0];
			Assert.AreEqual ("intValue", part.Name, "#2");
			Assert.AreEqual ("http://tempuri.org/", part.Namespace, "#3");
			Assert.AreEqual (typeof (int), part.Type, "#4");
			Assert.AreEqual (0, part.Index, "#5");
			Assert.AreEqual (false, part.Multiple, "#5");
		}

		[Test]
		public void PartsNamespace ()
		{
			ContractDescription cd =
				ContractDescription.GetContract (typeof (IFoo4));

			MessagePartDescriptionCollection parts =
				cd.Operations [0].Messages [0].Body.Parts;

			Assert.AreEqual (1, parts.Count, "#1");
			MessagePartDescription part = parts [0];
			Assert.AreEqual ("intValue", part.Name, "#2");
			Assert.AreEqual ("http://MonoTests.System.ServiceModel.Description", part.Namespace, "#3");
			Assert.AreEqual (typeof (int), part.Type, "#4");
			Assert.AreEqual (0, part.Index, "#5");
			Assert.AreEqual (false, part.Multiple, "#5");
		}
		
		[ServiceContract]
		public interface IFoo
		{
			[OperationContract]
			void Echo ();
		}
		
		[ServiceContract(Namespace="http://MonoTests.System.ServiceModel.Description")]
		public interface IFoo2
		{
			[OperationContract]
			void Echo ();
		}

		[ServiceContract]
		public interface IFoo3
		{
			[OperationContract]
			int Echo (int intValue);
		}

		[ServiceContract (Namespace = "http://MonoTests.System.ServiceModel.Description")]
		public interface IFoo4
		{
			[OperationContract]
			int Echo (int intValue);
		}
	}
}
