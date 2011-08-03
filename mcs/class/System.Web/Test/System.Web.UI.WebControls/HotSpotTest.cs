//
// Tests for System.Web.UI.WebControls.ImageMap.cs
//
// Author:
//  Hagit Yidov (hagity@mainsoft.com
//
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
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
using NUnit.Framework;
using System;
using System.IO;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using MonoTests.stand_alone.WebHarness;
using MonoTests.SystemWeb.Framework;

namespace MonoTests.System.Web.UI.WebControls

{
    class PokerHotSpot : HotSpot
    {
        // View state Stuff
        public PokerHotSpot()
        {
            TrackViewState();
        }

        public object SaveState()
        {
            return SaveViewState();
        }

        public void LoadState(object o)
        {
            LoadViewState(o);
        }

        public StateBag StateBag
        {
            get { return base.ViewState; }
        }

        // Protected Properties
        public bool IsTrackingState
        {
            get { return base.IsTrackingViewState; }
        }

        // Implementation for abstract members
        protected internal override string MarkupName
        {
            get { return (""); }
        }
        public override string GetCoordinates()
        {
            return ("");
        }
    }

    [TestFixture]
    public class HotSpotTest
    {

        [Test]
        public void HotSpot_DefaultProperties()
        {
            PokerHotSpot hotSpot = new PokerHotSpot();
            Assert.AreEqual(0, hotSpot.StateBag.Count, "ViewState.Count");
            // Public Properties
            Assert.AreEqual(string.Empty, hotSpot.AccessKey, "AccessKey");
            Assert.AreEqual(string.Empty, hotSpot.AlternateText, "AlternateText");
            Assert.AreEqual(HotSpotMode.NotSet, hotSpot.HotSpotMode, "HotSpotMode");
            Assert.AreEqual(string.Empty, hotSpot.NavigateUrl, "NavigateUrl");
            Assert.AreEqual(string.Empty, hotSpot.PostBackValue, "PostBackValue");
            Assert.AreEqual(0, hotSpot.TabIndex, "TabIndex");
            Assert.AreEqual(string.Empty, hotSpot.Target, "Target");
            // Protected Properties
            Assert.AreEqual(true, hotSpot.IsTrackingState, "IsTrackingState");
        }

        [Test]
        public void HotSpot_AssignToDefaultProperties()
        {
            PokerHotSpot hotSpot = new PokerHotSpot();

            Assert.AreEqual(0, hotSpot.StateBag.Count, "ViewState.Count");

            hotSpot.AccessKey = string.Empty;
            Assert.AreEqual(string.Empty, hotSpot.AccessKey, "AccessKey");
            Assert.AreEqual(1, hotSpot.StateBag.Count, "ViewState.Count-1");

            hotSpot.AlternateText = string.Empty;
            Assert.AreEqual(string.Empty, hotSpot.AlternateText, "AlternateText");
            Assert.AreEqual(2, hotSpot.StateBag.Count, "ViewState.Count-2");

            hotSpot.HotSpotMode = HotSpotMode.NotSet;
            Assert.AreEqual(HotSpotMode.NotSet, hotSpot.HotSpotMode, "HotSpotMode");
            Assert.AreEqual(3, hotSpot.StateBag.Count, "ViewState.Count-3");

            hotSpot.NavigateUrl = string.Empty;
            Assert.AreEqual(string.Empty, hotSpot.NavigateUrl, "NavigateUrl");
            Assert.AreEqual(4, hotSpot.StateBag.Count, "ViewState.Count-4");

            hotSpot.PostBackValue = string.Empty;
            Assert.AreEqual(string.Empty, hotSpot.PostBackValue, "PostBackValue");
            Assert.AreEqual(5, hotSpot.StateBag.Count, "ViewState.Count-5");

            hotSpot.TabIndex = 0;
            Assert.AreEqual(0, hotSpot.TabIndex, "TabIndex");
            Assert.AreEqual(6, hotSpot.StateBag.Count, "ViewState.Count-6");

            hotSpot.Target = string.Empty;
            Assert.AreEqual(string.Empty, hotSpot.Target, "Target");
            Assert.AreEqual(7, hotSpot.StateBag.Count, "ViewState.Count-7");
        }

        [Test]
        public void HotSpot_ViewState()
        {
            PokerHotSpot hotSpot = new PokerHotSpot();
            hotSpot.AccessKey = string.Empty;
            Assert.AreEqual(string.Empty, hotSpot.AccessKey, "AccessKey-beforecopy");
            hotSpot.AlternateText = string.Empty;
            Assert.AreEqual(string.Empty, hotSpot.AlternateText, "AlternateText-beforecopy");
            hotSpot.HotSpotMode = HotSpotMode.NotSet;
            Assert.AreEqual(HotSpotMode.NotSet, hotSpot.HotSpotMode, "HotSpotMode-beforecopy");
            hotSpot.NavigateUrl = string.Empty;
            Assert.AreEqual(string.Empty, hotSpot.NavigateUrl, "NavigateUrl-beforecopy");
            hotSpot.PostBackValue = string.Empty;
            Assert.AreEqual(string.Empty, hotSpot.PostBackValue, "PostBackValue-beforecopy");
            hotSpot.TabIndex = 0;
            Assert.AreEqual(0, hotSpot.TabIndex, "TabIndex-beforecopy");
            hotSpot.Target = string.Empty;
            Assert.AreEqual(string.Empty, hotSpot.Target, "Target-beforecopy");
            Assert.AreEqual(true, hotSpot.IsTrackingState, "IsTrackingState-beforecopy");
            object state = hotSpot.SaveState();
            PokerHotSpot copy = new PokerHotSpot();
            copy.LoadState(state);
            Assert.AreEqual(string.Empty, hotSpot.AccessKey, "AccessKey-aftercopy");
            Assert.AreEqual(string.Empty, hotSpot.AlternateText, "AlternateText-aftercopy");
            Assert.AreEqual(HotSpotMode.NotSet, hotSpot.HotSpotMode, "HotSpotMode-aftercopy");
            Assert.AreEqual(string.Empty, hotSpot.NavigateUrl, "NavigateUrl-aftercopy");
            Assert.AreEqual(string.Empty, hotSpot.PostBackValue, "PostBackValue-aftercopy");
            Assert.AreEqual(0, hotSpot.TabIndex, "TabIndex-aftercopy");
            Assert.AreEqual(string.Empty, hotSpot.Target, "Target-aftercopy");
            Assert.AreEqual(true, hotSpot.IsTrackingState, "IsTrackingState-aftercopy");
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void HotSpot_ExpAccessKey()
        {
            // The specified access key is neither a a null reference, 
            // an empty string (""), nor a single character string. 
            PokerHotSpot hotSpot = new PokerHotSpot();
            hotSpot.AccessKey = "abc"; 
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void HotSpot_ExpHotSpotMode()
        {
            // The specified type is not one of the HotSpotMode enumeration values.
            PokerHotSpot hotSpot = new PokerHotSpot();
            hotSpot.HotSpotMode = (HotSpotMode)10;
        }

        //[Test]
        //[ExpectedException(typeof(ArgumentOutOfRangeException))]
        //public void HotSpot_ExpTabIndex()
        //{
        //    // The specified tab index is not between -32768 and 32767
        //    PokerHotSpot hotSpot = new PokerHotSpot();
        //    short bignum = Convert.ToInt16(40000);
        //    hotSpot.TabIndex = bignum;
        //}
    }
}


#endif
