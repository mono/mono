//
// HttpWebRequestElementTest.cs - NUnit Test Cases for System.Net.Configuration.HttpWebRequestElement
//
// Authors:
//   Gert Driesen (drieseng@users.sourceforge.net)
//
// (C) 2005 Novell
//

#if !MOBILE && !XAMMAC_4_5

using System.Net.Configuration;

using NUnit.Framework;

namespace MonoTests.System.Net
{
	[TestFixture]
	public class HttpWebRequestElementTest
	{
		[Test]
		public void DefaultValues ()
		{
			HttpWebRequestElement element = new HttpWebRequestElement ();

			Assert.AreEqual (64, element.MaximumErrorResponseLength, "#1");
			Assert.AreEqual (64, element.MaximumResponseHeadersLength, "#2");
			Assert.AreEqual (-1, element.MaximumUnauthorizedUploadLength, "#3");
			Assert.AreEqual (false, element.UseUnsafeHeaderParsing, "#4");
		}
	}
}

#endif
