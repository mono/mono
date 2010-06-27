//
// MatchTest.cs - Unit tests for System.Text.RegularExpressions.Match
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
using System.Text.RegularExpressions;

namespace MonoTests.System.Text.RegularExpressions
{
[TestFixture]
public class MatchTest
{
    [Test]
    public void Synchronized_Inner_Null ()
    {
        try {
            Match.Synchronized (null);
            Assert.Fail ("#1");
        } catch (ArgumentNullException ex) {
            Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
            Assert.IsNull (ex.InnerException, "#3");
            Assert.IsNotNull (ex.Message, "#4");
            Assert.IsNotNull (ex.ParamName, "#5");
            Assert.AreEqual ("inner", ex.ParamName, "#6");
        }
    }

    [Test]
    public void Synchronized_Empty ()
    {
        Match em = Match.Empty;
        Match sm = Match.Synchronized (em);
        Assert.AreSame (em, sm, "Synchronized");
    }

    [Test]
    public void Result_Replacement_Null ()
    {
        try {
            Match.Empty.Result (null);
            Assert.Fail ("#1");
        } catch (ArgumentNullException ex) {
            Assert.AreEqual (typeof (ArgumentNullException), ex.GetType (), "#2");
            Assert.IsNull (ex.InnerException, "#3");
            Assert.IsNotNull (ex.Message, "#4");
            Assert.IsNotNull (ex.ParamName, "#5");
            Assert.AreEqual ("replacement", ex.ParamName, "#6");
        }
    }

    [Test]
    public void Result_Replacement_Empty ()
    {
        Regex email = new Regex ("(?<user>[^@]+)@(?<domain>.+)");
        Match m = email.Match ("mono@go-mono.com");
        string exp = m.Result (string.Empty);
        Assert.AreEqual (string.Empty, exp);
    }

    [Test]
    public void Result_Match_Empty ()
    {
        try {
            Match.Empty.Result ("whatever");
            Assert.Fail ("#1");
        } catch (NotSupportedException ex) {
            // Result cannot be called on failed Match
            Assert.AreEqual (typeof (NotSupportedException), ex.GetType (), "#2");
            Assert.IsNull (ex.InnerException, "#3");
            Assert.IsNotNull (ex.Message, "#4");
        }
    }

    [Test]
    public void Match_Backref ()
    {
        Assert.IsTrue (Regex.IsMatch ("F2345678910LL1", @"(F)(2)(3)(4)(5)(6)(7)(8)(9)(10)(L)\11"), "ltr");
        // FIXME
        //Assert.IsTrue (Regex.IsMatch ("F2345678910LL1", @"(F)(2)(3)(4)(5)(6)(7)(8)(9)(10)(L)\11", RegexOptions.RightToLeft), "rtl");
    }
}
}
