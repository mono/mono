// ***********************************************************************
// Copyright (c) 2007 Charlie Poole
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
// ***********************************************************************

using System;
using NUnit.Framework;

namespace NUnit.TestData.CultureAttributeData
{
	[TestFixture, Culture( "en,fr,de" )]
	public class FixtureWithCultureAttribute
	{
		[Test, Culture("en,de")]
		public void EnglishAndGermanTest() { }

		[Test, Culture("fr")]
		public void FrenchTest() { }

		[Test, Culture("fr-CA")]
		public void FrenchCanadaTest() { }
	}

#if !NETCF
    [TestFixture, SetCulture("xx-XX")]
    public class FixtureWithInvalidSetCultureAttribute
    {
        [Test]
        public void SomeTest() { }
    }

    [TestFixture]
    public class FixtureWithInvalidSetCultureAttributeOnTest
    {
        [Test, SetCulture("xx-XX")]
        public void InvalidCultureSet() { }
    }
#endif
}