using System;
using System.Xml;
using NUnit.Framework;

namespace MonoTests.System.ServiceModel
{
	[TestFixture]
	public class UniqueIdTest
	{
		[Test]
		public void TestDefault ()
		{
			UniqueId id = new UniqueId ();
			Assert.IsTrue (id.IsGuid, "#1");

			Guid g = Guid.NewGuid ();

			UniqueId a = new UniqueId (g);
			UniqueId b = new UniqueId (g.ToByteArray ());

			Assert.AreEqual (a, b, "#2");
			Assert.AreEqual ("urn:uuid:", a.ToString ().Substring (0, 9), "#3");

			a = new UniqueId ("foo");
			Assert.AreEqual ("foo", a.ToString (), "#4");
		}

		[Test]
		[ExpectedException (typeof (FormatException))]
		public void ZeroLengthCtor ()
		{
			new UniqueId ("");
		}

		[Test]
		[ExpectedException (typeof (ArgumentNullException))]
		public void CtorNull1 ()
		{
			new UniqueId ((string) null);
		}

	}
}
