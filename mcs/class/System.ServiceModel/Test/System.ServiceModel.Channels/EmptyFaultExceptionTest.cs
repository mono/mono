#if USE_DEPRECATED
// EmptyFaultException does not exist anymore
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Xml;

using NUnit.Framework;

namespace MonoTests.System.ServiceModel.Channels
{
	[TestFixture]
	public class EmptyFaultExceptionTest
	{
		[Test]
		public void TestDefaults ()
		{
			EmptyFaultException e = new EmptyFaultException ();
			Assert.AreEqual (MessageFault.DefaultAction, e.Action);
			Assert.IsTrue (e.Code.IsSenderFault);
			Assert.IsNull (e.Code.SubCode);
			Assert.AreEqual ("Unspecified ServiceModel Fault.", e.Reason.GetMatchingTranslation ().Text);
		}

		[Test]
		[Ignore ("bad English-oriented test")]
		public void TestToString ()
		{
			EmptyFaultException e = new EmptyFaultException ();
			Assert.AreEqual (
				String.Format ("{0}: {1} (Fault Detail is equal to null).", e.GetType (), e.Message),
				e.ToString ());
		}

		bool AreEqual (MessageFault a, MessageFault b)
		{
			return a.Actor == b.Actor && a.Code == b.Code && a.HasDetail == b.HasDetail && a.Node == b.Node && a.Reason == b.Reason;
		}

		[Test]
		public void TestCreateMessageFault ()
		{
			EmptyFaultException e = new EmptyFaultException ();
			Assert.IsTrue (
				AreEqual (MessageFault.CreateFault (e.Code, e.Reason), e.CreateMessageFault ()));
		}
	}
}
#endif
