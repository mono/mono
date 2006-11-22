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
    public class CircleHotSpotTest
    {

        [Test]
        public void CircleHotSpot_DefaultProperties()
        {
            CircleHotSpot circle = new CircleHotSpot();
            Assert.AreEqual(0, circle.Radius, "Radius");
            Assert.AreEqual(0, circle.X, "X-coordinate");
            Assert.AreEqual(0, circle.Y, "Y-coordinate");
        }

        [Test]
        public void CircleHotSpot_AssignToDefaultProperties()
        {
            CircleHotSpot circle = new CircleHotSpot();
            circle.Radius = 0;
            Assert.AreEqual(0, circle.Radius, "Radius");
            circle.X = 0;
            Assert.AreEqual(0, circle.X, "X-coordinate");
            circle.Y = 0;
            Assert.AreEqual(0, circle.Y, "Y-coordinate");
        }

        [Test]
		[ExpectedException (typeof (ArgumentOutOfRangeException))]	
        public void CircleHotSpot_ExpRadius()
        {
            // The specified value is less than 0
            CircleHotSpot circle = new CircleHotSpot();
            circle.Radius = -1;
        }

        [Test]
        public void CircleHotSpot_GetCoordinates()
         {
            CircleHotSpot circle = new CircleHotSpot();
            circle.Radius = 20;
            circle.X = 50;
            circle.Y = 40;
            Assert.AreEqual(20, circle.Radius, "BeforeGetCoordinates-Radius");
            Assert.AreEqual(50, circle.X, "BeforeGetCoordinates-X");
            Assert.AreEqual(40, circle.Y, "BeforeGetCoordinates-Y");
            Assert.AreEqual("50,40,20", circle.GetCoordinates(), "AfterGetCoordinates");
        }

        [Test]
        public void CircleHotSpot_ToString()
        {
            CircleHotSpot circle = new CircleHotSpot();
            Assert.AreEqual("CircleHotSpot", circle.ToString(), "After-ToString");
        }
    }
}


#endif
