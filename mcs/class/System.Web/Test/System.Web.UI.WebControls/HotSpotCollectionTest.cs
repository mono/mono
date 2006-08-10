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
    public class HotSpotCollectionTest
    {

        [Test]
        public void HotSpotCollection_Add()
         {
             HotSpotCollection spots = new HotSpotCollection();
             Assert.AreEqual(0, spots.Count, "BeforeAdd");
             CircleHotSpot circle = new CircleHotSpot();
             spots.Add(circle);
             Assert.AreEqual(1, spots.Count, "AfterAdd");
             Assert.AreEqual(circle.ToString(), spots[0].ToString(), "AfterAdd");
        }

        [Test]
        public void HotSpotCollection_Insert()
        {
            HotSpotCollection spots = new HotSpotCollection();
            spots.Add(new CircleHotSpot());
            spots.Add(new CircleHotSpot());
            Assert.AreEqual(2, spots.Count, "BeforeInsert");
            RectangleHotSpot rect = new RectangleHotSpot();
            spots.Insert(1,rect);
            Assert.AreEqual(3, spots.Count, "AfterInsert");
            Assert.AreEqual(rect.ToString(), spots[1].ToString(), "AfterInsert");
        }

        [Test]
        public void HotSpotCollection_Remove()
        {
            HotSpotCollection spots = new HotSpotCollection();
            spots.Add(new CircleHotSpot());
            RectangleHotSpot rect = new RectangleHotSpot();
            spots.Add(rect);
            spots.Add(new CircleHotSpot());
            Assert.AreEqual(3, spots.Count, "BeforeRemove");
            spots.Remove(rect);
            Assert.AreEqual(2, spots.Count, "AfterRemove");
        }

        [Test]
        public void HotSpotCollection_RemoveAt()
        {
            HotSpotCollection spots = new HotSpotCollection();
            CircleHotSpot circle1 = new CircleHotSpot();
            spots.Add(circle1);
            RectangleHotSpot rect = new RectangleHotSpot();
            spots.Add(rect);
            CircleHotSpot circle2 = new CircleHotSpot();
            spots.Add(circle2);
            Assert.AreEqual(3, spots.Count, "BeforeRemoveAt");
            spots.RemoveAt(1);
            Assert.AreEqual(2, spots.Count, "AfterRemoveAt");
            Assert.AreEqual(circle1.ToString(), spots[0].ToString(), "AfterRemoveAt");
            Assert.AreEqual(circle2.ToString(), spots[1].ToString(), "AfterRemoveAt");
        }
    }
}


#endif
