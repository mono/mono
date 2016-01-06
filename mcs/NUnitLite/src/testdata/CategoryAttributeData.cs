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

namespace NUnit.TestData.CategoryAttributeData
{
	[TestFixture, InheritableCategory("MyCategory")]
	public abstract class AbstractBase { }
	
	[TestFixture, Category( "DataBase" )]
	public class FixtureWithCategories : AbstractBase
	{
		[Test, Category("Long")]
		public void Test1() { }

		[Test, Critical]
		public void Test2() { }

        [Test, Category("Top")]
        [TestCaseSource("Test3Data")]
        public void Test3(int x) { }

        [Test, Category("A-B")]
        public void Test4() { }

        internal TestCaseData[] Test3Data = new TestCaseData[] {
            new TestCaseData(5).SetCategory("Bottom")
        };
    }

	[AttributeUsage(AttributeTargets.Method, AllowMultiple=false, Inherited=false)]
	public class CriticalAttribute : CategoryAttribute { }
	
	[AttributeUsage(AttributeTargets.Class, AllowMultiple=true, Inherited=true)]
	public class InheritableCategoryAttribute : CategoryAttribute
	{
		public InheritableCategoryAttribute(string name) : base(name) { }
	}
}