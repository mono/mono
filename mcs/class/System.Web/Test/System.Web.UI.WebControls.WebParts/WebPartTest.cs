//
// Tests for System.Web.UI.WebControls.WebParts.PartTest
//
// Author:
//	Chris Toshok (toshok@novell.com)
//

//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System;
using NUnit.Framework;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

namespace MonoTests.System.Web.UI.WebControls.WebParts {

  [TestFixture]
  public class WebPartTest {
  	class Poker : WebPart {
		public void TrackState ()
		{
			TrackViewState();
		}

		public object SaveState ()
		{
			return SaveViewState();
		}

		public void LoadState (object o)
		{
			LoadViewState(o);
		}
	}

  	[Test]
    [Category ("NotWorking")]
	public void Defaults ()
  	{
		Poker p = new Poker ();

		Assert.AreEqual (p.AllowClose, true, "A1");
		Assert.AreEqual (p.AllowConnect, true, "A2");
		Assert.AreEqual (p.AllowEdit, true, "A3");
		Assert.AreEqual (p.AllowHide, true, "A4");
		Assert.AreEqual (p.AllowMinimize, true, "A5");
		Assert.AreEqual (p.AllowZoneChange, true, "A6");
		Assert.AreEqual (p.AuthorizationFilter, String.Empty, "A7");
		Assert.AreEqual (p.CatalogIconImageUrl, String.Empty, "A8");
		Assert.AreEqual (p.ChromeState, PartChromeState.Normal, "A9");
		Assert.AreEqual (p.ChromeType, PartChromeType.Default, "A10");
		Assert.AreEqual (p.ConnectErrorMessage, String.Empty, "A11");
		Assert.AreEqual (p.Description, String.Empty, "A12");
		/* Direction - A13 */
		Assert.AreEqual (p.DisplayTitle, "Untitled", "A14");
		Assert.AreEqual (p.ExportMode, WebPartExportMode.None, "A15");
		Assert.AreEqual (p.HasSharedData, false, "A16");
		Assert.AreEqual (p.HasUserData, false, "A17");
		Assert.AreEqual (p.Height, Unit.Empty, "A18");
		Assert.AreEqual (p.HelpMode, WebPartHelpMode.Navigate, "A19");
		Assert.AreEqual (p.HelpUrl, String.Empty, "A20");
		Assert.AreEqual (p.Hidden, false, "A21");
		Assert.AreEqual (p.ImportErrorMessage, "Cannot import this Web Part.", "A22");
		Assert.AreEqual (p.IsClosed, false, "A23");
		Assert.AreEqual (p.IsShared, false, "A24");
		Assert.AreEqual (p.IsStandalone, true, "A25");
		/* this next isn't really a default - it's true
		 * because the part was created programmatically */
		Assert.AreEqual (p.IsStatic, true, "A26"); 
		Assert.AreEqual (p.Subtitle, String.Empty, "A27");
		Assert.AreEqual (p.Title, String.Empty, "A28");
		Assert.AreEqual (p.TitleIconImageUrl, String.Empty, "A29");
		Assert.AreEqual (p.TitleUrl, String.Empty, "A30");
		Assert.IsNotNull (p.Verbs, "A31");
#if IWebEditableInterface
		Assert.AreEqual (p.WebBrowsableObject, null, "A32");
#endif
#if notyet
		Assert.AreEqual (p.WebPartManager, null, "A33");
#endif		
		Assert.AreEqual (p.Width, Unit.Empty, "A34");
		Assert.AreEqual (p.ZoneIndex, 0, "A35");
	}
  }
}

#endif
