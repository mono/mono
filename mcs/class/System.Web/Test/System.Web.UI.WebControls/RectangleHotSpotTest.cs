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
    [TestFixture]
    public class RectangleHotSpotTest
    {

        [Test]
        public void RectangleHotSpot_DefaultProperties()
        {
            RectangleHotSpot rect = new RectangleHotSpot();
            Assert.AreEqual(0, rect.Bottom, "Bottom");
            Assert.AreEqual(0, rect.Top, "Top");
            Assert.AreEqual(0, rect.Left, "Left");
            Assert.AreEqual(0, rect.Right, "Right");
        }

        [Test]
        public void RectangleHotSpot_AssignToDefaultProperties()
        {
            RectangleHotSpot rect = new RectangleHotSpot();
            rect.Bottom = 0;
            Assert.AreEqual(0, rect.Bottom, "Bottom");
            rect.Top = 0;
            Assert.AreEqual(0, rect.Top, "Top");
            rect.Left = 0;
            Assert.AreEqual(0, rect.Left, "Left");
            rect.Right = 0;
            Assert.AreEqual(0, rect.Right, "Right");
        }

        [Test]
        public void RectangleHotSpot_GetCoordinates()
         {
            RectangleHotSpot rect = new RectangleHotSpot();
            rect.Bottom = 10;
            rect.Top = 20;
            rect.Left = 30;
            rect.Right = 50;
            Assert.AreEqual(10, rect.Bottom, "BeforeGetCoordinates-Bottom");
            Assert.AreEqual(20, rect.Top, "BeforeGetCoordinates-Top");
            Assert.AreEqual(30, rect.Left, "BeforeGetCoordinates-Left");
            Assert.AreEqual(50, rect.Right, "BeforeGetCoordinates-Right");
            Assert.AreEqual("30,20,50,10", rect.GetCoordinates(), "AfterGetCoordinates");
        }

        [Test]
        public void RectangleHotSpot_ToString()
        {
            RectangleHotSpot rect = new RectangleHotSpot();
            Assert.AreEqual("RectangleHotSpot", rect.ToString(), "After-ToString");
        }
    }
}


#endif
