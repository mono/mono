using System;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.UI;
using NUnit.Framework;

namespace MonoTests.System.Web.UI {

    [TestFixture]
    public class PageEncryptionTest {

        [Test]
        public void ValidateEncryptedString() {
            var pageType = typeof(Page);

            var testString = "?page=test.js&page2=test2.js&page3=test3.js&page4=test4.js";
			
            var encVal = pageType.GetMethod("EncryptString", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[]{testString, null});
            var decVal = pageType.GetMethod("DecryptString", BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, new object[]{encVal, null});

            Assert.AreEqual(testString, decVal, "ValidateEncryptedString");
        }
    }
}