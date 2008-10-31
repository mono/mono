using System;
using NUnit.Framework;
using System.Windows.Forms;
using System.Drawing;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class HelpProviderTest : TestHelper
	{	
#if NET_2_0
		[Test]
		public void HelpProviderPropertyTag ()
		{
			HelpProvider md = new HelpProvider ();
			object s = "MyString";

			Assert.AreEqual (null, md.Tag, "A1");

			md.Tag = s;
			Assert.AreSame (s, md.Tag, "A2");
		}
#endif
	}
}
