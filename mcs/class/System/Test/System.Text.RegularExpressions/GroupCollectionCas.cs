//
// GroupCollectionCas.cs
//	- CAS unit tests for System.Text.RegularExpressions.GroupCollection
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
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

using NUnit.Framework;

using System;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Text.RegularExpressions;

namespace MonoCasTests.System.Text.RegularExpressions {

[TestFixture]
[Category ("CAS")]
public class GroupCollectionCas {

    private GroupCollection coll;

    [TestFixtureSetUp]
    public void FixtureSetUp ()
    {
        coll = Match.Empty.Groups;
    }

    [SetUp]
    public void SetUp ()
    {
        if (!SecurityManager.SecurityEnabled)
            Assert.Ignore ("SecurityManager.SecurityEnabled is OFF");
    }

    [Test]
    [PermissionSet (SecurityAction.Deny, Unrestricted = true)]
    public void Deny_Unrestricted ()
    {
        Assert.AreEqual (1, coll.Count, "Count");
        Assert.IsTrue (coll.IsReadOnly, "IsReadOnly");
        Assert.IsFalse (coll.IsSynchronized, "IsSynchronized");
        Assert.IsNotNull (coll.SyncRoot, "SyncRoot");
        Assert.IsNotNull (coll[0], "this[int]");

        Assert.IsNotNull (coll.GetEnumerator (), "GetEnumerator");
        Group[] groups = new Group[1];
        coll.CopyTo (groups, 0);
    }

    [Test]
    [PermissionSet (SecurityAction.Deny, Unrestricted = true)]
    public void LinkDemand_Deny_Unrestricted ()
    {
        MethodInfo mi = typeof (GroupCollection).GetProperty ("Count").GetGetMethod ();
        Assert.IsNotNull (mi, "Count");
        Assert.AreEqual (1, (int)mi.Invoke (coll, null), "invoke");
    }
}
}
