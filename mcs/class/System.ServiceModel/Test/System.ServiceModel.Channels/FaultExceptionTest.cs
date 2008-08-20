using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Xml;

using NUnit.Framework;

namespace MonoTests.System.ServiceModel
{
        [TestFixture]
        public class FaultExceptionTest
        {
		[Test]
		public void TestDefaults ()
		{
		        FaultException<int> e = new FaultException<int> (0);
		        Assert.AreEqual (0, e.Detail, "#1");
			Assert.IsNull (e.Action, "#2");
		}

		[Test]
		public void TestMessage ()
		{
			FaultException<int> e = new FaultException<int> (0);
			Assert.AreEqual (e.Message, e.Reason.GetMatchingTranslation ().Text);
		}
		[Test]
		public void TestCode ()
		{
			// default Code is a SenderFault with a null SubCode
			FaultException<int> e = new FaultException<int> (0);
			Assert.IsTrue (e.Code.IsSenderFault);
			Assert.IsNull (e.Code.SubCode);
		}

		[Test]
		public void TestAction ()
		{
			FaultException<int> e = new FaultException<int> (0);
			Assert.IsNull (e.Action);
		}

		static void AreMessageFaultEqual (MessageFault a, MessageFault b, string label)
		{
			Assert.AreEqual (a.Actor, b.Actor, label + ".Actor");
			Assert.AreEqual (a.Code, b.Code, label + ".Code");
			Assert.AreEqual (a.HasDetail, b.HasDetail, label + ".HasDetail");
			Assert.AreEqual (a.Node, b.Node, label + ".Node");
			Assert.AreEqual (a.Reason, b.Reason, label + ".Reason");
		}

		[Test]
		public void TestCreateMessageFault ()
		{
			FaultException<int> e = new FaultException<int> (0);				Assert.IsFalse (
				(object) MessageFault.CreateFault (e.Code, e.Reason, e.Detail)
				== e.CreateMessageFault (), "#1");
			AreMessageFaultEqual (
				MessageFault.CreateFault (e.Code, e.Reason, e.Detail), 
				e.CreateMessageFault (), "#2");
		}

		[Test]
		[Ignore ("this test is old")]
		public void TestGetObjectData ()
		{
			FaultException<int> e = new FaultException<int> (0);

			if (true) {
				XmlWriterSettings s = new XmlWriterSettings ();
				s.Indent = true;
				s.ConformanceLevel = ConformanceLevel.Fragment;
				XmlWriter w = XmlWriter.Create (TextWriter.Null, s);
				XmlObjectSerializer formatter = new DataContractSerializer (typeof (int));
				formatter.WriteObject (w, e);
				w.Close ();
			}
		}

		[Test]
		[Ignore ("This test premises English.")]
		public void TestToString ()
		{
			FaultException<int> e = new FaultException<int> (0);			
			Assert.AreEqual (
				String.Format ("{0}: {1} (Fault Detail is equal to {2}).", e.GetType (), e.Message, e.Detail),
				e.ToString ());
		}
        }
}


