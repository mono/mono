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
    public class PolygonHotSpotTest
    {

        [Test]
        public void PolygonHotSpot_DefaultProperties()
        {
            PolygonHotSpot polygon = new PolygonHotSpot();
            Assert.AreEqual(string.Empty, polygon.Coordinates, "Coordinates");
        }

        [Test]
        public void PolygonHotSpot_AssignToDefaultProperties()
        {
            PolygonHotSpot polygon = new PolygonHotSpot();
            polygon.Coordinates = string.Empty;
            Assert.AreEqual(string.Empty, polygon.Coordinates, "Coordinates");
        }

        [Test]
        public void PolygonHotSpot_GetCoordinates()
         {
            PolygonHotSpot polygon = new PolygonHotSpot();
            polygon.Coordinates = "10,20,30,40,50,60,70,80";
            Assert.AreEqual("10,20,30,40,50,60,70,80", polygon.Coordinates, "BeforeGetCoordinates");
            Assert.AreEqual("10,20,30,40,50,60,70,80", polygon.GetCoordinates(), "AfterGetCoordinates");
        }

        [Test]
        public void PolygonHotSpot_ToString()
        {
            PolygonHotSpot polygon = new PolygonHotSpot();
            Assert.AreEqual("PolygonHotSpot", polygon.ToString(), "After-ToString");
        }
    }
}


#endif
