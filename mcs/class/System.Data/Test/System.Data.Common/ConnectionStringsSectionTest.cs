//
// MonoTests.System.Configuration.ConnectionStringsSectionTest.cs
//
// Author:
//   Sureshkumar T <tsureshkumar@novell.com>
//
//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

#if !MOBILE && !MONOMAC

using System;
using System.Configuration;

using NUnit.Framework;

namespace MonoTests.System.Configuration
{
    [TestFixture]
    public class ConnectionStringsSectionTest
    {
        [Test]
        public void GetConfigTest ()
        {
    
            object o = ConfigurationSettings.GetConfig ("connectionStrings_test");
            Assert.IsNotNull (o, "getconfig returns null");

            ConnectionStringsSection css = (ConnectionStringsSection) o;
            ConnectionStringSettings cs= css.ConnectionStrings ["Publications"];
            Assert.IsNotNull (cs, "connectionstringsettings is null");
            
            Assert.AreEqual ("Publications", cs.Name, "name wrong");
            Assert.AreEqual ("System.Data.SqlClient", cs.ProviderName, "ProviderName wrong");
            Assert.AreEqual ("Data Source=MyServer;Initial Catalog=pubs;integrated security=SSPI", cs.ConnectionString, "ConnectionString wrong");
           
        }
    }


}
#endif
