//
// NTAccountTest.cs - NUnit Test Cases for NTAccount
//
// Author:
//	Kenneth Bell
//

using System;
using System.Security.Principal;
using System.Text;
using NUnit.Framework;

namespace MonoTests.System.Security.Principal
{
    [TestFixture]
    public class NTAccountTest
    {
        [Test]
        public void ConstructorOneString()
        {
            Assert.AreEqual(@"Everyone", new NTAccount("Everyone").Value);
            Assert.AreEqual(@"EVERYONE", new NTAccount("EVERYONE").Value);
            Assert.AreEqual(@"DoMaIn\uSeR", new NTAccount(@"DoMaIn\uSeR").Value);
        }

        [Test]
        public void ConstructorTwoString()
        {
            Assert.AreEqual(@"DoMaIn\uSeR", new NTAccount("DoMaIn", "uSeR").Value);
            Assert.AreEqual(@"uSeR", new NTAccount(null, "uSeR").Value);
        }

        [Test]
        public void Translate()
        {
            NTAccount acct = new NTAccount("Everyone");
            SecurityIdentifier sid = (SecurityIdentifier)acct.Translate(typeof(SecurityIdentifier));
            Assert.AreEqual("S-1-1-0", sid.Value);
        }

        [Test]
        [ExpectedException(typeof(IdentityNotMappedException))]
        public void TranslateUnknown()
        {
            NTAccount acct = new NTAccount(@"UnknownDomain\UnknownUser");
            acct.Translate(typeof(SecurityIdentifier));
        }
    }
}
