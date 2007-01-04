using System;
using NUnit.Framework;
using System.Windows.Forms;
using System.Drawing;

namespace MonoTests.System.Windows.Forms
{
	[TestFixture]
	public class LinkAreaTest
	{	
#if NET_2_0
		[Test]
		public void LinkAreaToString ()
		{
			LinkArea la = new LinkArea ();
			Assert.AreEqual ("{Start=0, Length=0}", la.ToString (), "A1");

			la = new LinkArea (0, 0);
			Assert.AreEqual ("{Start=0, Length=0}", la.ToString (), "A2");

			la = new LinkArea (4, 75);
			Assert.AreEqual ("{Start=4, Length=75}", la.ToString (), "A3");		
		}
#endif
	}
}
